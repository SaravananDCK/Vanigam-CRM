# GST (Goods and Services Tax) Implementation

This document outlines the Indian GST compliance features implemented in the Vanigam CRM system.

## Overview

The GST implementation provides comprehensive support for Indian tax compliance, including:
- Separate tracking of CGST, SGST, IGST, and Cess components
- Automated ledger posting with proper GST accounting
- GST compliance reports (GSTR-1, GSTR-3B, HSN Summary)
- JSON export for GST portal upload

## Database Changes

### Voucher Entity GST Fields

Added to `Objects/Entities/Voucher.cs` (lines 46-57):

```csharp
// GST Breakdown (India-specific tax components)
[Column(TypeName = "decimal(18,2)")]
public decimal CGSTAmount { get; set; } = 0;

[Column(TypeName = "decimal(18,2)")]
public decimal SGSTAmount { get; set; } = 0;

[Column(TypeName = "decimal(18,2)")]
public decimal IGSTAmount { get; set; } = 0;

[Column(TypeName = "decimal(18,2)")]
public decimal CessAmount { get; set; } = 0;
```

**Required Database Migration**:
```sql
-- Add GST breakdown columns to Vouchers table
ALTER TABLE Vouchers ADD CGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
ALTER TABLE Vouchers ADD SGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
ALTER TABLE Vouchers ADD IGSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
ALTER TABLE Vouchers ADD CessAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
```

## Ledger Posting Logic

### Sales Invoice Posting

**Location**: `Server/Services/LedgerPostingService.cs` (lines 29-224)

**Accounting Entries**:
1. **Debit: Customer Account** = Total Amount (including tax)
2. **Credit: Sales Account** = Gross Sales (before discount)
3. **Debit: Sales Discount Account** = Discount Amount (contra-revenue)
4. **Credit: SGST Payable Account** = SGST Amount
5. **Credit: CGST Payable Account** = CGST Amount
6. **Credit: IGST Payable Account** = IGST Amount

**Example**:
```
Invoice Total: ₹11,800
Gross Sales: ₹10,000
Discount: ₹1,000
Net Sales: ₹9,000
CGST (9%): ₹810
SGST (9%): ₹810
IGST: ₹0

Journal Entry:
Dr. Customer Account          ₹10,620
Dr. Sales Discount           ₹1,000
    Cr. Sales Account                 ₹10,000
    Cr. CGST Payable                     ₹810
    Cr. SGST Payable                     ₹810
```

### Purchase Invoice Posting

**Location**: `Server/Services/LedgerPostingService.cs` (lines 307-502)

**Accounting Entries**:
1. **Credit: Vendor Account** = Total Amount (including tax)
2. **Debit: Purchase Account** = Gross Purchases (before discount)
3. **Credit: Purchase Discount Account** = Discount Amount (contra-expense)
4. **Debit: SGST ITC Account** = SGST Amount (Input Tax Credit)
5. **Debit: CGST ITC Account** = CGST Amount (Input Tax Credit)
6. **Debit: IGST ITC Account** = IGST Amount (Input Tax Credit)

**Balance Validation**: Both posting methods validate that total debits equal total credits before saving.

## GST Reports System

### Reports Dashboard

**Location**: `Client/Pages/Reports/GSTReports.razor`
**Route**: `/gst-reports`

**Available Reports**:
1. **GSTR-1** - Outward Supplies (B2B, B2C Large, B2C Small, Exports)
2. **GSTR-3B** - Monthly Summary Return (Tax Liability & ITC)
3. **HSN Summary** - Product-wise tax summary
4. **Tax Liability** - Detailed tax payable report
5. **ITC Report** - Input Tax Credit analysis
6. **GSTR-9** - Annual Return

### GSTR-1 Report Service

**Location**: `Server/Services/GSTR1ReportService.cs`

**Features**:
- B2B Invoice listing with customer GSTIN
- B2C Large invoices (₹2.5 lakh and above)
- B2C Small invoices (below ₹2.5 lakh) aggregated by state
- HSN-wise summary with proportional tax allocation

**HSN Summary Algorithm**:
```csharp
// Calculate proportion of invoice value for each HSN
var proportion = invoice.TotalAmount > 0
    ? hsnLinesValue / invoice.TotalAmount
    : 0;

// Allocate GST proportionally
hsn.CGSTAmount += invoice.CGSTAmount * proportion;
hsn.SGSTAmount += invoice.SGSTAmount * proportion;
hsn.IGSTAmount += invoice.IGSTAmount * proportion;
```

**Why Proportional Allocation?**
- VoucherLine entities don't store tax amounts
- Only Invoice totals have GST breakdown
- Proportional allocation ensures accurate HSN-wise reporting

### GSTR-3B Report Service

**Location**: `Server/Services/GSTR3BReportService.cs`

**Features**:
- Outward supplies calculation (taxable value + tax)
- Inward supplies with ITC eligibility
- Net tax payable calculation (Output Tax - ITC)
- Ineligible ITC tracking

**Tax Payable Calculation**:
```csharp
var taxPayable = new TaxAmounts
{
    CGSTAmount = Math.Max(0, outward.CGSTAmount - itc.NetITC.CGSTAmount),
    SGSTAmount = Math.Max(0, outward.SGSTAmount - itc.NetITC.SGSTAmount),
    IGSTAmount = Math.Max(0, outward.IGSTAmount - itc.NetITC.IGSTAmount)
};
```

### API Endpoints

**Location**: `Server/Controllers/GSTReportsController.cs`

**Endpoints**:
- `GET /api/GSTReports/gstr1?month={month}&year={year}` - GSTR-1 report data
- `GET /api/GSTReports/gstr3b?month={month}&year={year}` - GSTR-3B report data
- `GET /api/GSTReports/hsn-summary?month={month}&year={year}` - HSN summary data
- `GET /api/GSTReports/gstr1/export/json?month={month}&year={year}` - Export GSTR-1 as JSON
- `GET /api/GSTReports/gstr3b/export/json?month={month}&year={year}` - Export GSTR-3B as JSON

**Authentication**: All endpoints require JWT Bearer authentication

### DTOs (Data Transfer Objects)

**Location**: `Objects/DTOs/GSTReportDTOs.cs`

**Key Classes**:
- `GSTR1Report` - Complete GSTR-1 structure
- `B2BInvoice` - Business-to-business invoice details
- `B2CLInvoice` - B2C large invoice (₹2.5L+)
- `B2CSInvoice` - B2C small invoice aggregated by state
- `HSNSummary` - HSN/SAC code-wise summary
- `GSTR3BReport` - Complete GSTR-3B structure
- `OutwardSupplies` - Total outward supplies breakdown
- `ITCAvailable` - Input tax credit details
- `TaxPayable` - Net tax liability

### Client Services

**Location**: `Client/Services/GSTReportsApiService.cs`

**Methods**:
- `GetGSTR1ReportAsync(int month, int year)` - Fetch GSTR-1 data
- `GetGSTR3BReportAsync(int month, int year)` - Fetch GSTR-3B data
- `GetHSNSummaryAsync(int month, int year)` - Fetch HSN summary
- `GetGSTR1JsonDownloadUrl(int month, int year)` - Get JSON export URL
- `GetGSTR3BJsonDownloadUrl(int month, int year)` - Get JSON export URL

### Report Dialog

**Location**: `Client/Pages/Reports/GSTR1ReportDialog.razor`

**Features**:
- Tabbed interface (B2B Invoices, HSN Summary, Summary)
- Data grids with pagination
- Export to JSON (GST portal compatible)
- Export to Excel (placeholder for future)

## Service Registration

### Server-Side (Server/Program.cs)

```csharp
// Register GST Report services
builder.Services.AddScoped<GSTR1ReportService>();
builder.Services.AddScoped<GSTR3BReportService>();
```

### Client-Side (Client/Program.cs)

```csharp
builder.Services.AddScoped<GSTReportsApiService>();
```

## Navigation Menu

**Location**: `Client/Layout/MainLayout.razor.cs` (lines 210-222)

**Menu Structure**:
```
Reports
├── GST Reports (/gst-reports)
├── Financial Reports (placeholder)
├── Sales Reports (placeholder)
└── Purchase Reports (placeholder)
```

## Implementation Status

### Completed ✅
- [x] GST breakdown fields in Voucher entity
- [x] Ledger posting logic for sales invoices with GST
- [x] Ledger posting logic for purchase invoices with GST
- [x] GSTR-1 report service
- [x] GSTR-3B report service
- [x] HSN summary generation
- [x] GST Reports dashboard UI
- [x] GSTR-1 report dialog with data grids
- [x] API controller with all endpoints
- [x] Client API service
- [x] JSON export functionality
- [x] Navigation menu integration
- [x] Service registration (DI)

### Pending ⏳
- [ ] Database migration to add GST columns
- [x] Invoice calculation logic to populate GST fields from TaxCode
- [x] PurchaseInvoice bulk save with GST calculation
- [x] Job bulk save with GST calculation
- [ ] UI implementation for PurchaseInvoice line items with GST
- [ ] UI implementation for Job material usage with GST
- [ ] Localization keys for all report labels
- [ ] GSTR-3B report dialog UI
- [ ] HSN Summary standalone dialog
- [ ] Excel export functionality
- [ ] PDF export for reports
- [ ] GSTR-9 annual return implementation
- [ ] Tax liability report
- [ ] ITC report detailed view

## Invoice Processing Workflow

### Implemented Workflow

1. **Invoice Creation**:
   - User adds invoice items with TaxCode selection
   - Each item has TaxCode with GST component rates (CGSTRate, SGSTRate, IGSTRate, CessRate)
   - System stores TaxCodeId on each InvoiceItem (VoucherLine)

2. **GST Calculation** (Client-Side):
   - **Location**: `Client/Pages/DetailView/EditInvoice.razor.cs` - `CalculateTotalAmount()` method
   - For each invoice item:
     - Calculate taxable amount: `Total - DiscountAmount`
     - Calculate CGST: `taxableAmount * (CGSTRate / 100)`
     - Calculate SGST: `taxableAmount * (SGSTRate / 100)`
     - Calculate IGST: `taxableAmount * (IGSTRate / 100)`
     - Calculate Cess: `taxableAmount * (CessRate / 100)`
   - Aggregate all items to get invoice-level GST totals
   - Populate: `CGSTAmount`, `SGSTAmount`, `IGSTAmount`, `CessAmount` in Invoice

3. **Data Flow**:
   - **InvoiceItemDTO Enhancement**: Added GST rate fields (lines 53-57 in InvoiceBulkSaveDTO.cs)
   - **API Enhancement**: Server endpoint includes TaxCode data when loading items (InvoicesController.cs lines 53, 63-66)
   - **Bulk Save**: GST amounts passed to server via InvoiceBulkSaveDTO (lines 21-27)
   - **Service Layer**: InvoiceService saves GST breakdown to database (lines 169-172 for update, 214-217 for create)

4. **Ledger Posting**:
   - Triggered on invoice approval/posting
   - `LedgerPostingService.PostInvoiceToLedger(invoice)` called
   - Uses pre-calculated GST amounts for journal entries

### State-Based Tax Logic

**Intra-State Supply** (Supplier and Customer in same state):
```
Tax = CGST + SGST
Example: 18% GST = 9% CGST + 9% SGST
```

**Inter-State Supply** (Supplier and Customer in different states):
```
Tax = IGST
Example: 18% GST = 18% IGST
```

## Key Design Decisions

### 1. Invoice-Level Tax Posting (Not Line-Level)

**Rationale**:
- GST filing requirements aggregate at invoice level
- GSTR-1 shows invoice totals, not line items
- HSN summary is separate from B2B/B2C invoice listing
- Reduces ledger entry volume
- Improves posting performance

**Trade-off**:
- Cannot directly trace tax to individual products
- HSN summary uses proportional allocation

### 2. Separate CGST/SGST/IGST Fields

**Rationale**:
- Required for accurate GST return filing
- GSTR-3B requires separate reporting of each component
- ITC claims need component-wise tracking
- Audit trail compliance

**Alternative Rejected**:
- Single `TaxAmount` field with computed breakdown
- Would require complex reverse calculations for reporting

### 3. HSN Proportional Tax Allocation

**Rationale**:
- VoucherLine doesn't store tax amounts
- Avoids data duplication
- Accurate enough for GST filing purposes
- Simplifies invoice data entry

**Limitation**:
- If different lines have different tax rates, allocation is approximate
- Acceptable because HSN summary is for informational purposes

### 4. Balance Validation in Posting

**Rationale**:
- Ensures accounting integrity
- Catches calculation errors before database commit
- Prevents unbalanced ledgers

**Implementation**:
```csharp
if (totalDebits != totalCredits)
{
    throw new InvalidOperationException(
        $"Ledger entries do not balance. Debits: {totalDebits:N2}, Credits: {totalCredits:N2}");
}
```

## Testing Checklist

### Unit Testing
- [ ] Test GSTR1ReportService with sample invoices
- [ ] Test GSTR3BReportService calculations
- [ ] Test HSN proportional allocation logic
- [ ] Test ledger posting balance validation
- [ ] Test intra-state vs inter-state tax logic

### Integration Testing
- [ ] Create test invoice with GST
- [ ] Verify ledger entries are correct
- [ ] Generate GSTR-1 report and verify data
- [ ] Generate GSTR-3B report and verify calculations
- [ ] Export JSON and validate format
- [ ] Test with multiple customers in different states

### User Acceptance Testing
- [ ] Test full invoice-to-report workflow
- [ ] Verify JSON export works with GST portal
- [ ] Test report generation for different periods
- [ ] Verify discount handling in ledger
- [ ] Test ITC calculations for purchase invoices

## Compliance Notes

### GSTR-1 Filing Requirements
- **Frequency**: Monthly (or Quarterly for small businesses)
- **Due Date**: 11th of following month
- **Format**: JSON upload to GST portal
- **Content**: All outward supplies (B2B, B2C, Exports, etc.)

### GSTR-3B Filing Requirements
- **Frequency**: Monthly
- **Due Date**: 20th of following month
- **Format**: Online form on GST portal
- **Content**: Summary return with tax liability and ITC

### HSN Summary Requirements
- **Threshold**: Mandatory if turnover > ₹5 crore
- **Detail Level**: 4-digit HSN code minimum (6-digit preferred)
- **Included In**: Both GSTR-1 and annual return

## References

- **GST Act**: Central Goods and Services Tax Act, 2017
- **GST Portal**: https://www.gst.gov.in/
- **GSTR-1 Format**: [GST Portal - GSTR-1 Help](https://tutorial.gst.gov.in/userguide/returns/index.htm)
- **GSTR-3B Format**: [GST Portal - GSTR-3B Help](https://tutorial.gst.gov.in/userguide/returns/gstr3b.htm)

## Support and Maintenance

### Common Issues

**Issue**: Ledger entries don't balance
- **Cause**: Rounding errors or incorrect discount calculation
- **Fix**: Check invoice totals and GST amounts match

**Issue**: HSN summary totals don't match invoice totals
- **Cause**: Proportional allocation approximation
- **Fix**: This is expected behavior when different lines have different tax rates

**Issue**: JSON export fails validation on GST portal
- **Cause**: Missing required fields or incorrect format
- **Fix**: Verify all customer GSTIN numbers are present

### Future Enhancements

1. **Real-time GST Validation**: Verify GSTIN numbers via GST API
2. **E-Way Bill Integration**: Generate e-way bills for interstate supplies
3. **E-Invoice Integration**: Generate IRN (Invoice Reference Number)
4. **Reverse Charge Mechanism**: Support for RCM transactions
5. **Composition Scheme**: Support for composition taxpayers
6. **Credit/Debit Notes**: Proper handling in GST returns
7. **Amendments**: Support for amendment returns (GSTR-1A)

## Code Changes for Invoice GST Calculation

### 1. InvoiceItemDTO Enhancement
**File**: `Objects/DTOs/InvoiceBulkSaveDTO.cs` (lines 53-57)

Added GST rate properties to InvoiceItemDTO:
```csharp
// GST component rates from TaxCode (for calculation purposes)
public double CGSTRate { get; set; } = 0;
public double SGSTRate { get; set; } = 0;
public double IGSTRate { get; set; } = 0;
public double CessRate { get; set; } = 0;
```

**Purpose**: Allows client-side calculation of GST components without additional API calls.

### 2. Server API Enhancement
**File**: `Server/Controllers/MeditalkAIService/InvoicesController.cs` (lines 44-77)

Modified `GetInvoiceItemsForEditing` endpoint to include TaxCode data:
```csharp
var items = await context.InvoiceItems
    .Where(i => i.VoucherId == invoiceId)
    .Include(i => i.Item)
    .Include(i => i.TaxCode)  // Added
    .Select(i => new InvoiceItemDTO
    {
        // ... existing fields
        TaxCodeId = i.TaxCodeId,
        CGSTRate = i.TaxCode != null ? i.TaxCode.CGSTRate : 0,
        SGSTRate = i.TaxCode != null ? i.TaxCode.SGSTRate : 0,
        IGSTRate = i.TaxCode != null ? i.TaxCode.IGSTRate : 0,
        CessRate = i.TaxCode != null ? i.TaxCode.CessRate : 0,
        // ...
    })
    .ToListAsync();
```

**Purpose**: Provides TaxCode GST rates to client for calculation.

### 3. Client-Side GST Calculation
**File**: `Client/Pages/DetailView/EditInvoice.razor.cs` (lines 111-143)

Enhanced `CalculateTotalAmount` method:
```csharp
private void CalculateTotalAmount()
{
    var subTotal = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
    var totalDiscount = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
    var totalTax = invoiceItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

    // Calculate GST breakdown from invoice items
    decimal cgstAmount = 0;
    decimal sgstAmount = 0;
    decimal igstAmount = 0;
    decimal cessAmount = 0;

    foreach (var item in invoiceItems.Where(i => !i.IsDeleted))
    {
        // Calculate taxable amount for this line (after discount)
        var taxableAmount = item.Total - item.DiscountAmount;

        // Calculate GST components based on rates from TaxCode
        cgstAmount += taxableAmount * (decimal)(item.CGSTRate / 100);
        sgstAmount += taxableAmount * (decimal)(item.SGSTRate / 100);
        igstAmount += taxableAmount * (decimal)(item.IGSTRate / 100);
        cessAmount += taxableAmount * (decimal)(item.CessRate / 100);
    }

    CurrentObject.SubTotal = subTotal;
    CurrentObject.DiscountAmount = totalDiscount;
    CurrentObject.TaxAmount = totalTax;
    CurrentObject.CGSTAmount = cgstAmount;
    CurrentObject.SGSTAmount = sgstAmount;
    CurrentObject.IGSTAmount = igstAmount;
    CurrentObject.CessAmount = cessAmount;
    CurrentObject.TotalAmount = subTotal - totalDiscount + totalTax;
}
```

**Purpose**: Calculates GST breakdown from line items and populates invoice-level GST fields.

**Calculation Logic**:
- Taxable amount per line = Line Total - Line Discount
- CGST = Taxable Amount × (CGST Rate / 100)
- SGST = Taxable Amount × (SGST Rate / 100)
- IGST = Taxable Amount × (IGST Rate / 100)
- Cess = Taxable Amount × (Cess Rate / 100)

### 4. Bulk Save DTO Population
**File**: `Client/Pages/DetailView/EditInvoice.razor.cs` (lines 216-247)

Updated `SaveBulkInvoice` to include GST amounts:
```csharp
var bulkData = new InvoiceBulkSaveDTO
{
    Oid = IsCreateMode ? null : CurrentObject.Oid,
    Number = CurrentObject.Number,
    Status = CurrentObject.Status,
    PartyId = CurrentObject.PartyId,
    TotalAmount = CurrentObject.TotalAmount,
    SubTotal = CurrentObject.SubTotal,
    TaxAmount = CurrentObject.TaxAmount,
    CGSTAmount = CurrentObject.CGSTAmount,      // Added
    SGSTAmount = CurrentObject.SGSTAmount,      // Added
    IGSTAmount = CurrentObject.IGSTAmount,      // Added
    CessAmount = CurrentObject.CessAmount,      // Added
    DiscountAmount = CurrentObject.DiscountAmount,
    DiscountPercentage = CurrentObject.DiscountPercent,
    Items = invoiceItems.Select(i => new InvoiceItemDTO
    {
        // ... existing fields
        TaxCodeId = i.TaxCodeId,
        CGSTRate = i.CGSTRate,
        SGSTRate = i.SGSTRate,
        IGSTRate = i.IGSTRate,
        CessRate = i.CessRate,
        IsDeleted = i.IsDeleted
    }).ToList()
};
```

**Purpose**: Ensures calculated GST amounts are sent to server for persistence.

### 5. Server-Side Invoice Creation
**File**: `Server/Services/InvoiceService.cs` (lines 203-223)

Updated invoice creation to save GST amounts:
```csharp
invoice = new Invoice
{
    Oid = Guid.NewGuid(),
    Number = invoiceNumber,
    Status = invoiceData.Status,
    PartyId = invoiceData.PartyId,
    TotalAmount = invoiceData.TotalAmount,
    SubTotal = invoiceData.SubTotal,
    TaxAmount = invoiceData.TaxAmount,
    CGSTAmount = invoiceData.CGSTAmount,     // Added
    SGSTAmount = invoiceData.SGSTAmount,     // Added
    IGSTAmount = invoiceData.IGSTAmount,     // Added
    CessAmount = invoiceData.CessAmount,     // Added
    VoucherDate = invoiceData.VoucherDate,
    DiscountAmount = invoiceData.DiscountAmount,
    DiscountPercent = invoiceData.DiscountPercentage,
    DiscountType = invoiceData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount,
    TenantId = TenantId
};
```

**Note**: The update path (lines 169-172) already had GST amount saving implemented.

**Purpose**: Persists GST breakdown to database for ledger posting and reporting.

### Calculation Flow Diagram

```
[User Input]
    │
    ├─ Add Invoice Item with TaxCode
    │   └─ TaxCode contains: CGSTRate, SGSTRate, IGSTRate, CessRate
    │
    ├─ Item Quantity, UnitPrice, Discount
    │
    ↓
[Client Calculation - CalculateTotalAmount()]
    │
    ├─ For each item:
    │   ├─ LineTotal = Quantity × UnitPrice
    │   ├─ TaxableAmount = LineTotal - DiscountAmount
    │   ├─ LineCGST = TaxableAmount × (CGSTRate / 100)
    │   ├─ LineSGST = TaxableAmount × (SGSTRate / 100)
    │   ├─ LineIGST = TaxableAmount × (IGSTRate / 100)
    │   └─ LineCess = TaxableAmount × (CessRate / 100)
    │
    ├─ Invoice.CGSTAmount = Sum(all LineCGST)
    ├─ Invoice.SGSTAmount = Sum(all LineSGST)
    ├─ Invoice.IGSTAmount = Sum(all LineIGST)
    └─ Invoice.CessAmount = Sum(all LineCess)
    │
    ↓
[Save to Server - BulkSaveInvoiceWithItems]
    │
    ├─ Invoice entity saved with GST breakdown
    │
    └─ If Status = Posted:
        └─ LedgerPostingService.PostInvoiceToLedger()
            ├─ Uses Invoice.CGSTAmount for CGST Payable entry
            ├─ Uses Invoice.SGSTAmount for SGST Payable entry
            └─ Uses Invoice.IGSTAmount for IGST Payable entry
```

### Testing the Implementation

**Create Test Invoice**:
1. Create invoice with 2 line items
2. Item 1: Quantity=10, UnitPrice=100, TaxCode="GST 18%" (CGST 9% + SGST 9%)
3. Item 2: Quantity=5, UnitPrice=200, TaxCode="GST 12%" (CGST 6% + SGST 6%)

**Expected Calculations**:
- Item 1: Taxable=1000, CGST=90 (1000×9%), SGST=90
- Item 2: Taxable=1000, CGST=60 (1000×6%), SGST=60
- Invoice: CGSTAmount=150, SGSTAmount=150, IGSTAmount=0

**Verify**:
```sql
SELECT Number, SubTotal, CGSTAmount, SGSTAmount, IGSTAmount, TotalAmount
FROM Vouchers
WHERE VoucherType = 'Invoice'
ORDER BY CreatedAtUtc DESC;
```

## PurchaseInvoice and Job Bulk Save Implementation

### Overview

Similar to Invoice, both PurchaseInvoice and Job entities now support bulk save operations with line items and automatic GST calculation.

### PurchaseInvoice Bulk Save

**DTO Created**: `Objects/DTOs/PurchaseInvoiceBulkSaveDTO.cs`

**Key Components**:
- `PurchaseInvoiceBulkSaveDTO` - Main DTO with GST fields
- `PurchaseInvoiceItemDTO` - Line item DTO with TaxCode rates

**Service Method**: `Server/Services/PurchaseInvoiceService.cs` - `BulkSavePurchaseInvoiceWithItems()`

**Features**:
- Create/update purchase invoice with items in single transaction
- Automatic GST calculation from line item TaxCodes
- TaxCodeId stored on each PurchaseInvoiceItem for proper ITC tracking
- Automatic ledger posting when status = Posted
- Balance validation before commit

**GST Fields Supported**:
```csharp
public decimal CGSTAmount { get; set; } = 0;      // Input Tax Credit - CGST
public decimal SGSTAmount { get; set; } = 0;      // Input Tax Credit - SGST
public decimal IGSTAmount { get; set; } = 0;      // Input Tax Credit - IGST
public decimal CessAmount { get; set; } = 0;      // Input Tax Credit - Cess
```

**Ledger Posting**:
When PurchaseInvoice status = Posted:
1. Credits Vendor Account
2. Debits Purchase Account
3. Credits Purchase Discount (if applicable)
4. Debits CGST ITC Account
5. Debits SGST ITC Account
6. Debits IGST ITC Account

**Usage Pattern** (to be implemented in UI):
```csharp
var bulkData = new PurchaseInvoiceBulkSaveDTO
{
    Number = "PI-2025-001",
    Status = PurchaseInvoiceStatus.Draft,
    PartyId = vendorId,
    SubTotal = 10000,
    TaxAmount = 1800,
    CGSTAmount = 900,
    SGSTAmount = 900,
    IGSTAmount = 0,
    TotalAmount = 11800,
    Items = new List<PurchaseInvoiceItemDTO>
    {
        new() {
            InventoryItemId = itemId,
            Quantity = 10,
            UnitPrice = 1000,
            TaxCodeId = taxCodeId,
            CGSTRate = 9,
            SGSTRate = 9
        }
    }
};

var savedPurchaseInvoice = await PurchaseInvoiceService.BulkSavePurchaseInvoiceWithItems(bulkData);
```

### Job Bulk Save

**DTO Created**: `Objects/DTOs/JobBulkSaveDTO.cs`

**Key Components**:
- `JobBulkSaveDTO` - Main DTO with GST fields and job-specific properties
- `MaterialUsageDTO` - Material usage line item with TaxCode rates

**Service Method**: `Server/Services/JobService.cs` - `BulkSaveJobWithMaterials()`

**Features**:
- Create/update job with material usage in single transaction
- Automatic GST calculation from material TaxCodes
- TaxCodeId stored on each MaterialUsage for cost tracking
- Job-specific fields: Title, Description, Status, Priority

**GST Fields Supported**:
```csharp
public decimal CGSTAmount { get; set; } = 0;
public decimal SGSTAmount { get; set; } = 0;
public decimal IGSTAmount { get; set; } = 0;
public decimal CessAmount { get; set; } = 0;
```

**Usage Pattern** (to be implemented in UI):
```csharp
var bulkData = new JobBulkSaveDTO
{
    Number = "JOB-2025-001",
    Title = "AC Repair Service",
    Description = "Annual maintenance",
    Status = JobStatus.Pending,
    Priority = Priority.High,
    PartyId = customerId,
    ContactId = contactId,
    SubTotal = 5000,
    TaxAmount = 900,
    CGSTAmount = 450,
    SGSTAmount = 450,
    TotalAmount = 5900,
    Materials = new List<MaterialUsageDTO>
    {
        new() {
            InventoryItemId = sparePartId,
            Quantity = 2,
            UnitPrice = 2500,
            TaxCodeId = taxCodeId,
            CGSTRate = 9,
            SGSTRate = 9
        }
    }
};

var savedJob = await JobService.BulkSaveJobWithMaterials(bulkData);
```

### Common Patterns

Both implementations follow the same pattern as Invoice:

1. **Transaction Safety**: All operations wrapped in database transaction
2. **GST Calculation**: Line-level calculation aggregated to voucher level
3. **TaxCode Integration**: TaxCodeId stored on each line item for reference
4. **Soft Delete Support**: Items marked as IsDeleted are removed
5. **Update Support**: Handles both create and update scenarios
6. **Validation**: Entity validation before persistence

### Next Steps for UI Implementation

To complete the implementation, the following UI components need to be created:

**For PurchaseInvoice**:
1. Update `EditPurchaseInvoice.razor.cs` with:
   - `purchaseInvoiceItems` list management
   - `CalculateTotalAmount()` method (similar to Invoice)
   - `SaveBulkPurchaseInvoice()` method
2. Create `PurchaseInvoiceItemApiService.cs` for loading items with TaxCode
3. Add bulk save endpoint in `PurchaseInvoicesController.cs`

**For Job**:
1. Update `EditJob.razor.cs` with:
   - `materialUsages` list management
   - `CalculateTotalAmount()` method (similar to Invoice)
   - `SaveBulkJob()` method
2. Create endpoint for loading materials with TaxCode
3. Add bulk save endpoint in `JobsController.cs`

**Example CalculateTotalAmount for PurchaseInvoice**:
```csharp
private void CalculateTotalAmount()
{
    var subTotal = purchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.Total);
    var totalDiscount = purchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.DiscountAmount);
    var totalTax = purchaseInvoiceItems.Where(i => !i.IsDeleted).Sum(i => i.TaxAmount ?? 0);

    // Calculate GST breakdown from purchase invoice items
    decimal cgstAmount = 0;
    decimal sgstAmount = 0;
    decimal igstAmount = 0;
    decimal cessAmount = 0;

    foreach (var item in purchaseInvoiceItems.Where(i => !i.IsDeleted))
    {
        var taxableAmount = item.Total - item.DiscountAmount;
        cgstAmount += taxableAmount * (decimal)(item.CGSTRate / 100);
        sgstAmount += taxableAmount * (decimal)(item.SGSTRate / 100);
        igstAmount += taxableAmount * (decimal)(item.IGSTRate / 100);
        cessAmount += taxableAmount * (decimal)(item.CessRate / 100);
    }

    CurrentObject.SubTotal = subTotal;
    CurrentObject.DiscountAmount = totalDiscount;
    CurrentObject.TaxAmount = totalTax;
    CurrentObject.CGSTAmount = cgstAmount;
    CurrentObject.SGSTAmount = sgstAmount;
    CurrentObject.IGSTAmount = igstAmount;
    CurrentObject.CessAmount = cessAmount;
    CurrentObject.TotalAmount = subTotal - totalDiscount + totalTax;
}
```

### UI Implementation Status

#### PurchaseInvoice UI - COMPLETED ✅

**Components Created**:
1. **EditablePurchaseInvoiceItems.razor** - Editable grid component for purchase invoice items
2. **EditablePurchaseInvoiceItems.razor.cs** - Code-behind with GST calculation logic

**Features**:
- ✅ Add/Edit/Delete purchase invoice items
- ✅ Inventory item selection dropdown
- ✅ TaxCode-based GST rate assignment (CGST, SGST, IGST, Cess)
- ✅ Automatic GST calculation per line item
- ✅ Discount support (Amount or Percentage)
- ✅ Real-time totals calculation
- ✅ GST breakdown display in read-only mode
- ✅ Items data grid in read-only mode

**Integration**:
- EditPurchaseInvoice.razor updated to use EditablePurchaseInvoiceItems component
- Follows same pattern as EditInvoice with EditableInvoiceItems
- All event handlers wired up (ItemsChanged, TotalAmountChanged, etc.)

**Key Implementation Details**:
```csharp
// GST calculation in OnInventoryItemChanged
if (Item.TaxCode != null)
{
    item.CGSTRate = Item.TaxCode.CGSTRate;
    item.SGSTRate = Item.TaxCode.SGSTRate;
    item.IGSTRate = Item.TaxCode.IGSTRate;
    item.CessRate = Item.TaxCode.CessRate;

    var totalTaxRate = Item.TaxCode.CGSTRate + Item.TaxCode.SGSTRate +
                      Item.TaxCode.IGSTRate + Item.TaxCode.CessRate;
    item.TaxAmount = ((decimal)totalTaxRate / 100) * Item.UnitPrice;
}
```

#### Job UI - COMPLETED ✅

**Components Created**:
1. **EditableJobMaterials.razor** - Editable grid component for job material usage
2. **EditableJobMaterials.razor.cs** - Code-behind with GST calculation logic

**Features**:
- ✅ Add/Edit/Delete job materials
- ✅ Inventory item selection dropdown
- ✅ TaxCode-based GST rate assignment (CGST, SGST, IGST, Cess)
- ✅ Automatic GST calculation per material line
- ✅ Discount support (Amount or Percentage)
- ✅ Real-time totals calculation
- ✅ GST breakdown display in read-only mode
- ✅ Materials data grid in read-only mode
- ✅ Contract coverage fields (ChargedAmount, WaivedAmount)

**Integration**:
- EditJob.razor updated to use EditableJobMaterials component
- Follows same pattern as EditInvoice and EditPurchaseInvoice
- All event handlers wired up (MaterialsChanged, TotalAmountChanged, etc.)
- Materials displayed outside main form for better UX

**Key Implementation Details**:
```csharp
// GST calculation in OnInventoryItemChanged
if (Item.TaxCode != null)
{
    material.CGSTRate = Item.TaxCode.CGSTRate;
    material.SGSTRate = Item.TaxCode.SGSTRate;
    material.IGSTRate = Item.TaxCode.IGSTRate;
    material.CessRate = Item.TaxCode.CessRate;

    var totalTaxRate = Item.TaxCode.CGSTRate + Item.TaxCode.SGSTRate +
                      Item.TaxCode.IGSTRate + Item.TaxCode.CessRate;
    material.TaxAmount = ((decimal)totalTaxRate / 100) * Item.UnitPrice;
}
```

**MaterialUsage Specific Features**:
- ChargedAmount: Amount charged to customer (after contract discount/waiver)
- WaivedAmount: Amount waived/covered by contract
- Supports AMC contract coverage tracking

---

**Document Version**: 1.4
**Last Updated**: 2025-10-29
**Author**: Claude Code Implementation
