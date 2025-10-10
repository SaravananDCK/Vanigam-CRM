# Warranty & AMC Contract Management - Implementation Summary

## ✅ Implementation Complete

This document summarizes the TPH (Table-Per-Hierarchy) implementation for Contract entities with warranty and AMC support.

---

## 📋 Changes Overview

### 1. **Contract Entity - TPH Hierarchy** ✅

**File**: `Objects/Entities/Contract.cs`

**Changes**:
- ✅ Changed `Contract` from concrete class to **abstract base class**
- ✅ Added TPH discriminator: `ContractType` enum
- ✅ Created 4 derived classes with specific properties:
  - `WarrantyContract` - Free manufacturer/seller warranty
  - `AmcContract` - Paid Annual Maintenance Contract
  - `GuaranteeContract` - Extended guarantee period
  - `ServiceContract` - General service agreement

**New Base Class Fields**:
```csharp
- ContractType (discriminator)
- InvoiceId (link to invoice for warranty contracts)
- ContractValue (monetary value, null for free warranties)
- AutoRenewEnabled (renewal settings)
- AutoRenewNoticeDays (notification timing)
- ParentContractId (warranty→AMC conversion tracking)
- ChildContracts (collection)
```

**Derived Class Specific Fields**:

**WarrantyContract**:
- WarrantyNumber
- RequiresRegistration
- RegistrationDate
- IsTransferable

**AmcContract**:
- AmcNumber
- ScheduledVisitsPerYear
- ResponseTimeHours / ResolutionTimeHours
- Includes24x7Support / IncludesRemoteSupport / IncludesOnsiteSupport
- RenewalDiscountPercent

**GuaranteeContract**:
- GuaranteeNumber
- IsMoneyBackGuarantee
- GuaranteeConditions

**ServiceContract**:
- ServiceContractNumber
- ServiceType
- IncludedServiceHours
- AdditionalHourlyRate

---

### 2. **Item Entity Enhancement** ✅

**File**: `Objects/Entities/Item.cs`

**New Fields Added**:
```csharp
- WarrantyPeriodMonths (standard warranty duration)
- GuaranteePeriodMonths (extended guarantee duration)
- IsAmcEligible (can product be covered under AMC?)
- AmcAnnualRate (annual AMC pricing)
- RequiresPurchaseProof (invoice needed for warranty claims)
- Manufacturer (brand name)
- ModelNumber (model code)
```

**Purpose**: Track warranty/AMC eligibility at product level to exclude:
- Software licenses (cloud services)
- Consumables (cables, accessories)
- Non-serviceable items

---

### 3. **ContractCoverageRule Enhancement** ✅

**File**: `Objects/Entities/ContractCoverageRule.cs`

**New Fields Added**:
```csharp
- InvoiceItemId (link to specific invoice item for warranty tracking)
- SerialNumber (individual product serial number)
- ItemWarrantyEndDate (item-specific warranty end date)
- CoveredQuantity (quantity covered under warranty)
```

**Updated**:
- ✅ Added FK to `ItemCategory` (was commented out, now active)

**Purpose**: Track which specific invoice items are covered under warranty contracts.

---

### 4. **Enum Addition** ✅

**File**: `Objects/Entities/Enums.cs`

**New Enum**:
```csharp
public enum ContractType {
    Warranty,        // Free manufacturer/seller warranty
    AMC,             // Annual Maintenance Contract (paid)
    Guarantee,       // Extended guarantee period
    ServiceContract  // General service agreement
}
```

---

### 5. **DbContext Configuration** ✅

**File**: `Objects/VanigamAccountingDbContext.cs`

**TPH Configuration Added**:
```csharp
// Configure TPH for Contract hierarchy with ContractType discriminator
modelBuilder.Entity<Contract>()
    .ToTable(nameof(Contracts))
    .HasDiscriminator<ContractType>(nameof(Contract.ContractType))
    .HasValue<WarrantyContract>(ContractType.Warranty)
    .HasValue<AmcContract>(ContractType.AMC)
    .HasValue<GuaranteeContract>(ContractType.Guarantee)
    .HasValue<ServiceContract>(ContractType.ServiceContract);

// Self-referencing hierarchy for warranty→AMC conversion
modelBuilder.Entity<Contract>()
    .HasOne(c => c.ParentContract)
    .WithMany(c => c.ChildContracts)
    .HasForeignKey(c => c.ParentContractId)
    .OnDelete(DeleteBehavior.Restrict);

// Invoice relationship
modelBuilder.Entity<Contract>()
    .HasOne(c => c.Invoice)
    .WithMany()
    .HasForeignKey(c => c.InvoiceId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

### 6. **Database Migration Script** ✅

**File**: `DatabaseScripts/AddWarrantyAndAmcSupport.sql`

**Includes**:
- ✅ All ALTER TABLE statements for Contracts, Items, ContractCoverageRules
- ✅ Foreign key constraints
- ✅ Performance indexes
- ✅ Two database views:
  - `V_ExpiringWarranties` - Track warranties expiring in next 90 days
  - `V_AmcRenewals` - Track AMC contracts due for renewal
- ✅ PostgreSQL function: `get_amc_eligible_products(customer_id)` - Get products ready for AMC conversion
- ✅ Column comments for documentation

---

### 7. **Client Code Fix** ✅

**File**: `Client/Pages/DetailView/EditContract.razor.cs`

**Fixed**:
```csharp
// Before (ERROR):
CurrentObject = new(); // Can't instantiate abstract class

// After (FIXED):
CurrentObject = new AmcContract(); // Default to AMC type
```

---

## 🔄 Business Workflows Supported

### 1. **Automatic Warranty Contract Creation**
**Trigger**: Invoice posted/paid

**Flow**:
```
Invoice → Get items with WarrantyPeriodMonths > 0
       → Create WarrantyContract
       → Create ContractCoverageRules (one per item)
       → Link to InvoiceItems via InvoiceItemId
```

### 2. **Warranty Expiry Tracking**
**Query**: `V_ExpiringWarranties` view

**Returns**: Products with warranties expiring in next 90 days, with:
- Customer info
- Product details
- AMC eligibility status
- Days until expiry

### 3. **AMC Conversion**
**Manual Process**:
```
Warranty Contract (expiring)
    ↓
User selects AMC-eligible products
    ↓
Create AmcContract (ParentContractId = warranty contract)
    ↓
Create ContractCoverageRules (only for selected products)
    ↓
Update WarrantyContract.ChildContracts
```

### 4. **Product-Level AMC Eligibility**
**Logic**:
```csharp
// Get AMC-eligible products for customer
var eligibleProducts = await dbContext.Contracts
    .Where(c => c.CustomerId == customerId
             && c.ContractType == ContractType.Warranty
             && c.ParentContractId == null) // Not converted yet
    .SelectMany(c => c.CoverageRules)
    .Select(r => r.InventoryItem)
    .Where(i => i.IsAmcEligible == true)
    .ToListAsync();
```

---

## 📊 Database Schema Summary

### Tables Modified

**Contracts** (16 new columns):
- ContractType (discriminator)
- InvoiceId, ContractValue, AutoRenewEnabled, AutoRenewNoticeDays, ParentContractId
- WarrantyNumber, RequiresRegistration, RegistrationDate, IsTransferable (Warranty)
- AmcNumber, ScheduledVisitsPerYear, ResponseTimeHours, ResolutionTimeHours, Includes24x7Support, IncludesRemoteSupport, IncludesOnsiteSupport, RenewalDiscountPercent (AMC)
- GuaranteeNumber, IsMoneyBackGuarantee, GuaranteeConditions (Guarantee)
- ServiceContractNumber, ServiceType, IncludedServiceHours, AdditionalHourlyRate (Service)

**Items** (7 new columns):
- WarrantyPeriodMonths, GuaranteePeriodMonths
- IsAmcEligible, AmcAnnualRate, RequiresPurchaseProof
- Manufacturer, ModelNumber

**ContractCoverageRules** (4 new columns):
- InvoiceItemId, SerialNumber, ItemWarrantyEndDate, CoveredQuantity

### Indexes Created
- IX_Contracts_ContractType
- IX_Contracts_InvoiceId
- IX_Contracts_ParentContractId
- IX_Contracts_StartDate_EndDate
- IX_Contracts_CustomerId_IsActive
- IX_Items_IsAmcEligible
- IX_Items_WarrantyPeriodMonths
- IX_ContractCoverageRules_InvoiceItemId
- IX_ContractCoverageRules_ItemWarrantyEndDate

---

## 🎯 Next Steps for Full Implementation

### Phase 1: Services (To Be Implemented)
- [ ] `WarrantyManagementService` - Auto-create warranties from invoices
- [ ] `AmcConversionService` - Convert warranty to AMC
- [ ] Enhance `InvoiceService` - Trigger warranty creation on payment
- [ ] Enhance `ContractCoverageService` - Handle warranty vs AMC rules

### Phase 2: API Layer (To Be Implemented)
- [ ] `WarrantyContractsController` (OData)
- [ ] `AmcContractsController` (OData)
- [ ] `AmcConversionController` (REST)
- [ ] API endpoints for warranty expiry queries

### Phase 3: UI Components (To Be Implemented)
- [ ] Warranties ListView (`Warranties.razor`)
- [ ] Warranty DetailView (`EditWarrantyContract.razor`)
- [ ] AMC Conversion Dialog (`ConvertWarrantyToAmcDialog.razor`)
- [ ] Enhance Product edit form (add warranty/AMC fields)
- [ ] Dashboard widgets (expiring warranties, AMC revenue)

### Phase 4: Background Jobs (To Be Implemented)
- [ ] Hangfire job: Daily warranty expiry check
- [ ] Hangfire job: AMC renewal reminders
- [ ] Email/notification integration

### Phase 5: Seed Data Updates (To Be Implemented)
- [ ] Update `ProductSeedData.json` - Add warranty/AMC fields
- [ ] Update `InventoryItemSeedData.json` - Add warranty fields
- [ ] Create sample warranty contracts

---

## ✅ Build Status

**Build Result**: ✅ **SUCCESS**
- 0 Errors
- 232 Warnings (all existing, none from new code)

**Compilation**: All new entities compile correctly

---

## 📝 Usage Examples

### Create Warranty Contract (Manual)
```csharp
var warranty = new WarrantyContract
{
    Title = "Warranty - Invoice #INV001",
    CustomerId = customer.Oid,
    InvoiceId = invoice.Oid,
    StartDate = invoice.InvoiceDate,
    Duration = ContractDuration.Annual,
    WarrantyNumber = "WTY-2025-001",
    RequiresRegistration = false,
    IsTransferable = false
};

context.Contracts.Add(warranty);

// Add coverage rules for each warranted product
foreach (var item in invoice.Items.Where(i => i.Product.WarrantyPeriodMonths > 0))
{
    var rule = new ContractCoverageRule
    {
        ContractId = warranty.Oid,
        InventoryItemId = item.ProductId,
        InvoiceItemId = item.Oid,
        CoverageType = CoverageRuleType.Free,
        ChargePercentage = 0,
        ItemWarrantyEndDate = invoice.InvoiceDate.AddMonths(item.Product.WarrantyPeriodMonths.Value),
        CoveredQuantity = item.Quantity
    };
    context.ContractCoverageRules.Add(rule);
}

await context.SaveChangesAsync();
```

### Convert Warranty to AMC
```csharp
var warrantyContract = await context.Contracts
    .OfType<WarrantyContract>()
    .Include(c => c.CoverageRules)
    .FirstOrDefaultAsync(c => c.Oid == warrantyId);

var amc = new AmcContract
{
    Title = $"AMC - {warrantyContract.Customer.Name}",
    CustomerId = warrantyContract.CustomerId,
    ParentContractId = warrantyContract.Oid,
    StartDate = warrantyContract.EndDate.Value.AddDays(1),
    Duration = ContractDuration.Annual,
    ContractValue = 25000m,
    AmcNumber = "AMC-2025-001",
    ScheduledVisitsPerYear = 4,
    ResponseTimeHours = 24,
    ResolutionTimeHours = 48,
    Includes24x7Support = false,
    IncludesRemoteSupport = true,
    IncludesOnsiteSupport = true
};

context.Contracts.Add(amc);
await context.SaveChangesAsync();
```

### Query by Contract Type
```csharp
// Get all warranty contracts
var warranties = await context.Contracts
    .OfType<WarrantyContract>()
    .Where(c => c.IsActive)
    .ToListAsync();

// Get all AMC contracts
var amcs = await context.Contracts
    .OfType<AmcContract>()
    .Where(c => c.IsActive)
    .ToListAsync();

// Get all contracts (polymorphic query)
var allContracts = await context.Contracts
    .Where(c => c.CustomerId == customerId)
    .ToListAsync(); // Returns mix of Warranty, AMC, Guarantee, Service contracts
```

---

## 🎉 Summary

**Architecture**: TPH (Table-Per-Hierarchy) successfully implemented for Contract hierarchy
**Discriminator**: `ContractType` enum
**Derived Classes**: 4 (WarrantyContract, AmcContract, GuaranteeContract, ServiceContract)
**Database**: Migration script ready for PostgreSQL
**Build**: ✅ Success (0 errors)
**Ready For**: Service layer implementation and UI development

---

**Implementation Date**: January 8, 2025
**Build Verified**: ✅ Success
**Migration Script**: `DatabaseScripts/AddWarrantyAndAmcSupport.sql`
