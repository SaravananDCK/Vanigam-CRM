using Vanigam.CRM.Objects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Vanigam.CRM.Objects.Configurations;
using Vanigam.CRM.Objects.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Helpers;
using Vanigam.CRM.Objects.Services;
using Vanigam.CRM.Objects.SeedData;
using System.Reflection;
using NodaTime;

namespace Vanigam.CRM.Objects
{
    public partial class VanigamAccountingDbContext(DbContextOptions<VanigamAccountingDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
    {
        
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            //configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Technician> Technicians => Set<Technician>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<TimeSheet> TimeSheets => Set<TimeSheet>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
        public DbSet<MaterialUsage> MaterialUsages => Set<MaterialUsage>();
        public DbSet<Quote> Quotes => Set<Quote>();
        public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<JobReport> JobReports => Set<JobReport>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<GPSPoint> GPSPoints => Set<GPSPoint>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<Sla> Slas => Set<Sla>();
        public DbSet<RecurringJob> RecurringJobs => Set<RecurringJob>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<CustomField> CustomFields => Set<CustomField>();
        public DbSet<ApplicationUser>? ApplicationUsers { get; set; }
        public DbSet<ApplicationTenant> Tenants { get; set; }
        public DbSet<ApplicationTenantUser> ApplicationTenantUsers { get; set; }

        public DbSet<Language>? Languages { get; set; }

        #region ReportsBased
        public DbSet<FileDocument>? FileDocuments { get; set; }
        public DbSet<FileCategory> FileCategories { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<PdfField> PdfFields { get; set; }
        public DbSet<SignPdfField> SignPdfFields { get; set; }
        public DbSet<DocxMacroTemplate> DocxMacroTemplates { get; set; }
        public DbSet<DocxTemplate> DocxTemplates { get; set; }
        public DbSet<PdfTemplate> PdfTemplates { get; set; }
        public DbSet<ReportTemplate> ReportTemplates { get; set; }

        #endregion

        #region Accounting Entities
        public DbSet<AccountGroup> AccountGroups { get; set; }
        public DbSet<LedgerAccount> LedgerAccounts { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherLine> VoucherLines { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<StockLedgerEntry> StockLedgerEntries { get; set; }
        public DbSet<NumberSeries> NumberSeries { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
          .HasMany(u => u.Roles)
          .WithMany(r => r.Users)
          .UsingEntity<IdentityUserRole<string>>();


            modelBuilder.Entity<ApplicationUser>()
                .HasOne(i => i.ApplicationTenant)
                .WithMany(i => i.Users)
                .HasForeignKey(i => i.TenantId)
                .HasPrincipalKey(i => i.Id);

            modelBuilder.Entity<ApplicationRole>()
                .HasOne(i => i.ApplicationTenant)
                .WithMany(i => i.Roles)
                .HasForeignKey(i => i.TenantId)
                .HasPrincipalKey(i => i.Id);


            modelBuilder.Entity<ApplicationUser>().ToTable(nameof(this.ApplicationUsers));
            modelBuilder.Entity<SuperUser>().ToTable(nameof(VanigamAccountingDbContext.ApplicationUsers));
            modelBuilder.Entity<Admin>().ToTable(nameof(VanigamAccountingDbContext.ApplicationUsers));

            modelBuilder.Entity<Item>()
                .ToTable(nameof(Items))
                .HasDiscriminator<ItemType>(nameof(Item.Type))
                .HasValue<InventoryItem>(ItemType.InventoryItem)
                .HasValue<Product>(ItemType.Product)
                .HasValue<ServiceItem>(ItemType.ServiceItem);

            // Configure AccountGroup self-referencing hierarchy
            modelBuilder.Entity<AccountGroup>()
                .HasOne(g => g.ParentGroup)
                .WithMany(g => g.ChildGroups)
                .HasForeignKey(g => g.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AccountGroup>()
                .Property(g => g.Nature)
                .HasConversion<string>();

            // Configure TPH for LedgerAccount hierarchy
            modelBuilder.Entity<LedgerAccount>()
                .ToTable(nameof(LedgerAccounts))
                .HasDiscriminator<AccountType>(nameof(LedgerAccount.AccountType))
                .HasValue<Customer>(AccountType.Customer)
                .HasValue<Vendor>(AccountType.Vendor)
                .HasValue<BankAccount>(AccountType.BankAccount)
                .HasValue<LedgerAccount>(AccountType.LedgerAccount);

            // Configure LedgerAccount to AccountGroup relationship
            modelBuilder.Entity<LedgerAccount>()
                .HasOne(l => l.AccountGroup)
                .WithMany(g => g.LedgerAccounts)
                .HasForeignKey(l => l.AccountGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure LedgerEntry to LedgerAccount relationship
            modelBuilder.Entity<LedgerEntry>()
                .HasOne(e => e.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure EntryType enum to be stored as string
            modelBuilder.Entity<LedgerEntry>()
                .Property(e => e.EntryType)
                .HasConversion<string>();

            // Configure TPH for Voucher hierarchy
            modelBuilder.Entity<Voucher>()
                .ToTable(nameof(Vouchers))
                .HasDiscriminator<VoucherType>(nameof(Voucher.VoucherType))
                .HasValue<Quote>(VoucherType.Quote)
                .HasValue<Invoice>(VoucherType.Invoice)
                .HasValue<PurchaseOrder>(VoucherType.PurchaseOrder)
                .HasValue<PurchaseInvoice>(VoucherType.PurchaseInvoice);

            var configurations = typeof(FileCategory).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(BaseClassConfiguration<>))
            .Select(Activator.CreateInstance);

            foreach (var configuration in configurations)
            {
                modelBuilder.ApplyConfiguration((dynamic)configuration);
            }

        }
        public async Task SeedInitialData()
        {
            var beforeUpdate = new string[]
            {
            };
            var functions = new string[]
            {

            };
            var triggers = new string[]
            {

            };
            var afterUpdate = functions.Concat(triggers).ToArray();

            if (!await this.Database.CanConnectAsync())
            {
                await this.Database.EnsureCreatedAsync();
                await this.SeedTenantsAdmin();
                await this.SeedRoleClaims();
                await this.SeedAccountGroupData();
                await this.SeedBankAccountData();
                await this.SeedNumberSeriesData();
                await this.SeedLeadData();
                await this.SeedCustomerData();
                await this.SeedOpportunityData();
                await this.SeedJobData();
                await this.SeedActivityData();
                await this.SeedContactData();
                await this.SeedProductData();
                await this.SeedInventoryItemData();
                await this.SeedServiceItemData();
                await this.SeedQuoteData();
                //foreach (var fn in beforeUpdate)
                //{
                //    await using var stream = typeof(Party).Assembly.GetManifestResourceStream(fn);
                //    if (stream != null)
                //    {
                //        using var reader = new StreamReader(stream);
                //        await this.Database.ExecuteSqlRawAsync(await reader.ReadToEndAsync());
                //    }
                //}

                //foreach (var fn in afterUpdate)
                //{
                //    await using var stream = typeof(Party).Assembly.GetManifestResourceStream(fn);
                //    if (stream != null)
                //    {
                //        using var reader = new StreamReader(stream);
                //        await this.Database.ExecuteSqlRawAsync(await reader.ReadToEndAsync());
                //    }
                //}
            }

        }

        public async Task SeedTenantsAdmin()
        {
            var tekSpearTenant = new ApplicationTenant
            {
                Id = 1,
                Name = "TekSpear Solutions",
                //Currency = "INR",
                //TimeZone = "Asia/Kolkata",
                Hosts = "https://localhost:5001/,http://localhost:5270/,https://localhost:61564/"

            };
            Tenants.Add(tekSpearTenant);
            await this.SaveChangesAsync();

            var demoTenant = new ApplicationTenant
            {
                Id = 2,
                Name = "TekSpear Solutions demo",
                //Currency = "INR",
                //TimeZone = "Asia/Kolkata",
                Hosts = "https://localhost:5001/,http://localhost:5270/,https://localhost:61564/"

            };
            Tenants.Add(demoTenant);
            await this.SaveChangesAsync();

            var roleStore = new RoleStore<ApplicationRole, VanigamAccountingDbContext,Guid>(this);
            var superUserRole = new ApplicationRole { Name = ApplicationRole.SuperUserRole, NormalizedName = ApplicationRole.SuperUserRole.ToUpper() };
            var adminRole = new ApplicationRole { Name = ApplicationRole.AdminRole, NormalizedName = ApplicationRole.AdminRole.ToUpper() };

            await roleStore.CreateAsync(superUserRole);
            await roleStore.CreateAsync(adminRole);

            await this.SaveChangesAsync();

            var tenantsAdmin = new SuperUser
            {
                UserName = ApplicationUser.TenantsAdmin,
                NormalizedUserName = ApplicationUser.TenantsAdmin.ToUpper(),
                Email = ApplicationUser.TenantsAdmin,
                NormalizedEmail = ApplicationUser.TenantsAdmin.ToUpper(),
                EmailConfirmed = false,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            if (!this.Users.Any(u => u.UserName == tenantsAdmin.UserName))
            {
                var password = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(tenantsAdmin, ApplicationUser.TenantsAdmin + "@123");
                tenantsAdmin.PasswordHash = hashed;
                var userStore = new UserStore<SuperUser,ApplicationRole,VanigamAccountingDbContext,Guid>(this);
                await userStore.CreateAsync(tenantsAdmin);

                // Add role claim to tenantsAdmin
                await userStore.AddToRoleAsync(tenantsAdmin, ApplicationRole.SuperUserRole.ToUpper());
            }
            var admin = new SuperUser()
            {
                Name = "Saravanan Chandra Krishnan",
                FullName = "Saravanan Chandra Krishnan",
                TenantId = tekSpearTenant.Id,
                Email = "Saravanan@tekspear.com",
                UserType = LoginUserType.Admin,
                UserName = "Saravanan@tekspear.com",
                NormalizedUserName = "Saravanan@tekspear.com".ToUpper(),
                NormalizedEmail = "Saravanan@tekspear.com".ToUpper(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            if (!this.Users.Any(u => u.UserName == admin.UserName))
            {
                var password = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(admin, ApplicationUser.Admin + "@123");
                admin.PasswordHash = hashed;
                var userStore = new UserStore<SuperUser, ApplicationRole, VanigamAccountingDbContext, Guid>(this);
                await userStore.CreateAsync(admin);

                // Add role claim to admin
                await userStore.AddToRoleAsync(admin, ApplicationRole.SuperUserRole.ToUpper());
            }
            var systemAdmin = new SuperUser()
            {
                Id = Guid.Parse(ApplicationUser.SystemUserId),
                Name = "System",
                FullName = "System",
                TenantId = demoTenant.Id,
                Email = "System@tekspear.com",
                UserType = LoginUserType.Admin,
                UserName = "System@tekspear.com",
                NormalizedUserName = "System@tekspear.com".ToUpper(),
                NormalizedEmail = "System@tekspear.com".ToUpper(),
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            if (!this.Users.Any(u => u.UserName == systemAdmin.UserName))
            {
                var password = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(systemAdmin, ApplicationUser.SystemUserName + "@123");
                systemAdmin.PasswordHash = hashed;
                var userStore = new UserStore<SuperUser, ApplicationRole, VanigamAccountingDbContext, Guid>(this);
                await userStore.CreateAsync(systemAdmin);

                // Add role claim to systemAdmin
                await userStore.AddToRoleAsync(systemAdmin, ApplicationRole.SuperUserRole.ToUpper());
            }
            await this.SaveChangesAsync();
        }

        public async Task SeedRoleClaims()
        {
            var roleStore = new RoleStore<ApplicationRole, VanigamAccountingDbContext, Guid>(this);

            // Get all roles
            var superUserRole = await roleStore.FindByNameAsync(ApplicationRole.SuperUserRole.ToUpper());
            var adminRole = await roleStore.FindByNameAsync(ApplicationRole.AdminRole.ToUpper());

            if (superUserRole == null || adminRole == null)
                return;

            // Dynamically get all DbSet property names from the DbContext
            var dbSetProperties = this.GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();

            var entities = dbSetProperties;

            // Create claims for SuperUser role (full permissions)
            foreach (var entity in entities)
            {
                var claimType = $"Permission.{entity}";
                var claimValue = $"{{\"Role\": \"{ApplicationRole.SuperUserRole}\", \"Read\": true, \"Create\": true, \"Update\": true, \"Delete\": true, \"Priority\": 1}}";

                // Check if claim already exists
                var existingClaim = RoleClaims.FirstOrDefault(rc => rc.RoleId == superUserRole.Id && rc.ClaimType == claimType);
                if (existingClaim == null)
                {
                    RoleClaims.Add(new IdentityRoleClaim<Guid>
                    {
                        RoleId = superUserRole.Id,
                        ClaimType = claimType,
                        ClaimValue = claimValue
                    });
                }
            }

            // Create claims for Admin role (full permissions except tenant management)
            var businessEntities = entities.Where(e => !e.Equals("ApplicationUsers") &&
                                                       !e.Equals("Tenants") &&
                                                       !e.Equals("ApplicationTenantUsers"));

            foreach (var entity in businessEntities)
            {
                var claimType = $"Permission.{entity}";
                var claimValue = $"{{\"Role\": \"{ApplicationRole.AdminRole}\", \"Read\": true, \"Create\": true, \"Update\": true, \"Delete\": true, \"Priority\": 2}}";

                // Check if claim already exists
                var existingClaim = RoleClaims.FirstOrDefault(rc => rc.RoleId == adminRole.Id && rc.ClaimType == claimType);
                if (existingClaim == null)
                {
                    RoleClaims.Add(new IdentityRoleClaim<Guid>
                    {
                        RoleId = adminRole.Id,
                        ClaimType = claimType,
                        ClaimValue = claimValue
                    });
                }
            }

            // Add limited permissions for Admin role on Application entities (read-only)
            var applicationEntities = entities.Where(e => e.Equals("ApplicationUsers"));
            foreach (var entity in applicationEntities)
            {
                var claimType = $"Permission.{entity}";
                var claimValue = $"{{\"Role\": \"{ApplicationRole.AdminRole}\", \"Read\": true, \"Create\": false, \"Update\": false, \"Delete\": false, \"Priority\": 2}}";

                // Check if claim already exists
                var existingClaim = RoleClaims.FirstOrDefault(rc => rc.RoleId == adminRole.Id && rc.ClaimType == claimType);
                if (existingClaim == null)
                {
                    RoleClaims.Add(new IdentityRoleClaim<Guid>
                    {
                        RoleId = adminRole.Id,
                        ClaimType = claimType,
                        ClaimValue = claimValue
                    });
                }
            }

            await this.SaveChangesAsync();
        }

        public async Task SeedAccountGroupData()
        {
            // Check if AccountGroup data already exists
            if (await AccountGroups.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "AccountGroupsSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "AccountGroupsSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var seedData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);

                if (!seedData.TryGetProperty("AccountGroups", out var accountGroupsElement))
                    return;

                var accountGroupSeedData = System.Text.Json.JsonSerializer.Deserialize<List<AccountGroupSeedModel>>(accountGroupsElement.GetRawText());

                if (accountGroupSeedData?.Any() != true)
                    return;

                // Dictionary to track created groups by their Code for hierarchy linking
                var createdGroups = new Dictionary<string, AccountGroup>();

                // First pass: Create all parent groups (no ParentCode)
                var parentGroups = accountGroupSeedData.Where(g => string.IsNullOrEmpty(g.ParentCode)).ToList();
                foreach (var seedGroup in parentGroups)
                {
                    if (!Enum.TryParse<AccountNature>(seedGroup.Nature, out var nature))
                    {
                        Console.WriteLine($"Invalid Nature value: {seedGroup.Nature}");
                        continue;
                    }

                    var accountGroup = new AccountGroup
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Code = seedGroup.Code,
                        Name = seedGroup.Name,
                        Nature = nature,
                        ParentGroupId = null,
                        IsActive = seedGroup.IsActive,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    await AccountGroups.AddAsync(accountGroup);
                    createdGroups[seedGroup.Code] = accountGroup;
                }

                await SaveChangesAsync();

                // Second pass: Create all child groups (with ParentCode)
                var childGroups = accountGroupSeedData.Where(g => !string.IsNullOrEmpty(g.ParentCode)).ToList();
                foreach (var seedGroup in childGroups)
                {
                    if (!Enum.TryParse<AccountNature>(seedGroup.Nature, out var nature))
                    {
                        Console.WriteLine($"Invalid Nature value: {seedGroup.Nature}");
                        continue;
                    }

                    // Find parent group
                    AccountGroup? parentGroup = null;
                    if (!string.IsNullOrEmpty(seedGroup.ParentCode) && createdGroups.ContainsKey(seedGroup.ParentCode))
                    {
                        parentGroup = createdGroups[seedGroup.ParentCode];
                    }

                    var accountGroup = new AccountGroup
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Code = seedGroup.Code,
                        Name = seedGroup.Name,
                        Nature = nature,
                        ParentGroupId = parentGroup?.Oid,
                        IsActive = seedGroup.IsActive,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    await AccountGroups.AddAsync(accountGroup);
                    createdGroups[seedGroup.Code] = accountGroup;
                }

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding AccountGroup data: {ex.Message}");
            }
        }

        public async Task SeedBankAccountData()
        {
            // Check if BankAccount data already exists
            if (await BankAccounts.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "LedgerAccountsSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "LedgerAccountsSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var seedData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);

                if (!seedData.TryGetProperty("BankAccounts", out var bankAccountsElement))
                    return;

                var bankAccountSeedData = System.Text.Json.JsonSerializer.Deserialize<List<BankAccountSeedModel>>(bankAccountsElement.GetRawText());

                if (bankAccountSeedData?.Any() != true)
                    return;

                var bankAccounts = new List<BankAccount>();
                foreach (var seedBankAccount in bankAccountSeedData)
                {
                    // Find AccountGroup by Code
                    var accountGroup = await AccountGroups.FirstOrDefaultAsync(ag => ag.Code == seedBankAccount.AccountGroupCode);
                    if (accountGroup == null)
                    {
                        Console.WriteLine($"AccountGroup not found for Code: {seedBankAccount.AccountGroupCode}");
                        continue;
                    }

                    var bankAccount = new BankAccount
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Code = seedBankAccount.Code,
                        Name = seedBankAccount.Name,
                        AccountType = Enum.TryParse<AccountType>(seedBankAccount.AccountType, out var accountType) ? accountType : AccountType.BankAccount,
                        AccountGroupId = accountGroup.Oid,
                        Description = seedBankAccount.Description,
                        Currency = seedBankAccount.Currency,
                        Balance = seedBankAccount.Balance,
                        CreditLimit = seedBankAccount.CreditLimit,
                        IsActive = seedBankAccount.IsActive,
                        BankName = seedBankAccount.BankName,
                        AccountNumber = seedBankAccount.AccountNumber,
                        RoutingNumber = seedBankAccount.RoutingNumber,
                        IFSCCode = seedBankAccount.IFSCCode,
                        BranchName = seedBankAccount.BranchName,
                        SwiftCode = seedBankAccount.SwiftCode,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    bankAccounts.Add(bankAccount);
                }

                await BankAccounts.AddRangeAsync(bankAccounts);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding BankAccount data: {ex.Message}");
            }
        }

        public async Task SeedNumberSeriesData()
        {
            // Check if NumberSeries data already exists
            if (await NumberSeries.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "NumberSeriesSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "NumberSeriesSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var seedData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);

                if (!seedData.TryGetProperty("NumberSeries", out var numberSeriesElement))
                    return;

                var numberSeriesSeedData = System.Text.Json.JsonSerializer.Deserialize<List<NumberSeriesSeedModel>>(numberSeriesElement.GetRawText());

                if (numberSeriesSeedData?.Any() != true)
                    return;

                var numberSeriesList = new List<NumberSeries>();
                foreach (var seedNumberSeries in numberSeriesSeedData)
                {
                    var numberSeries = new NumberSeries
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        EntityType = seedNumberSeries.EntityType,
                        Prefix = seedNumberSeries.Prefix,
                        Suffix = seedNumberSeries.Suffix,
                        StartNo = seedNumberSeries.StartNo,
                        CurrentNo = seedNumberSeries.CurrentNo,
                        PaddingLength = seedNumberSeries.PaddingLength,
                        IsActive = seedNumberSeries.IsActive,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    numberSeriesList.Add(numberSeries);
                }

                await NumberSeries.AddRangeAsync(numberSeriesList);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding NumberSeries data: {ex.Message}");
            }
        }

        public async Task SeedLeadData()
        {
            // Check if Lead data already exists
            if (await Leads.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "LeadSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "LeadSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var leadSeedData = System.Text.Json.JsonSerializer.Deserialize<List<LeadSeedModel>>(jsonContent);

                if (leadSeedData?.Any() != true)
                    return;

                var leads = new List<Lead>();
                foreach (var seedLead in leadSeedData)
                {
                    var lead = new Lead
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Name = seedLead.Name,
                        Email = seedLead.Email,
                        Phone = seedLead.Phone,
                        SecondaryPhone = seedLead.SecondaryPhone,
                        Organization = seedLead.Organization,
                        JobTitle = seedLead.JobTitle,
                        Industry = seedLead.Industry,
                        CompanySize = seedLead.CompanySize,
                        Website = seedLead.Website,
                        Address = seedLead.Address,
                        City = seedLead.City,
                        State = seedLead.State,
                        PostalCode = seedLead.PostalCode,
                        Country = seedLead.Country,
                        ProductOfInterest = seedLead.ProductOfInterest,
                        EstimatedBudget = seedLead.EstimatedBudget,
                        Timeline = seedLead.Timeline,
                        LeadScore = seedLead.LeadScore,
                        Source = seedLead.Source,
                        CampaignSource = seedLead.CampaignSource,
                        ReferredBy = seedLead.ReferredBy,
                        LinkedInProfile = seedLead.LinkedInProfile,
                        Description = seedLead.Description,
                        Status = Enum.TryParse<LeadStatus>(seedLead.Status, out var status) ? status : LeadStatus.New,
                        Comments = seedLead.Comments,
                        LastContactDate = DateTime.TryParse(seedLead.LastContactDate, out var lastContact) ? DateTime.SpecifyKind(lastContact, DateTimeKind.Utc) : null,
                        NextFollowUpDate = DateTime.TryParse(seedLead.NextFollowUpDate, out var nextFollowUp) ? DateTime.SpecifyKind(nextFollowUp, DateTimeKind.Utc) : null,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    leads.Add(lead);
                }

                await Leads.AddRangeAsync(leads);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding Lead data: {ex.Message}");
            }
        }

        public async Task SeedCustomerData()
        {
            // Check if Customer data already exists
            if (await Customers.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "CustomerSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "CustomerSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var customerSeedData = System.Text.Json.JsonSerializer.Deserialize<List<CustomerSeedModel>>(jsonContent);

                if (customerSeedData?.Any() != true)
                    return;

                var customers = new List<Customer>();
                foreach (var seedCustomer in customerSeedData)
                {
                    var customer = new Customer
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Name = seedCustomer.Name,
                        Type = Enum.TryParse<CustomerType>(seedCustomer.Type, out var type) ? type : CustomerType.Company,
                        Email = seedCustomer.Email,
                        Phone = seedCustomer.Phone,
                        Address = seedCustomer.Address,
                        City = seedCustomer.City,
                        State = seedCustomer.State,
                        PostalCode = seedCustomer.PostalCode,
                        Country = seedCustomer.Country,
                        Website = seedCustomer.Website,
                        Industry = seedCustomer.Industry,
                        AnnualRevenue = seedCustomer.AnnualRevenue,
                        EmployeeCount = seedCustomer.EmployeeCount,
                        CustomerSince = DateTime.TryParse(seedCustomer.CustomerSince, out var customerSince) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(customerSince, DateTimeKind.Utc)).ToDateTimeOffset() : null,
                        Status = Enum.TryParse<CustomerStatus>(seedCustomer.Status, out var status) ? status : CustomerStatus.Active,
                        Rating = seedCustomer.Rating,
                        Description = seedCustomer.Description,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    customers.Add(customer);
                }

                await Customers.AddRangeAsync(customers);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding Customer data: {ex.Message}");
            }
        }

        public async Task SeedOpportunityData()
        {
            // Check if Opportunity data already exists
            if (await Opportunities.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "OpportunitySeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "OpportunitySeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var opportunitySeedData = System.Text.Json.JsonSerializer.Deserialize<List<OpportunitySeedModel>>(jsonContent);

                if (opportunitySeedData?.Any() != true)
                    return;

                var opportunities = new List<Opportunity>();
                foreach (var seedOpportunity in opportunitySeedData)
                {
                    var opportunity = new Opportunity
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Title = seedOpportunity.Title,
                        Description = seedOpportunity.Description,
                        EstimatedValue = seedOpportunity.EstimatedValue,
                        ExpectedCloseDate = DateTime.TryParse(seedOpportunity.ExpectedCloseDate, out var expectedCloseDate) ? DateTime.SpecifyKind(expectedCloseDate, DateTimeKind.Utc) : null,
                        Stage = Enum.TryParse<OpportunityStage>(seedOpportunity.Stage, out var stage) ? stage : OpportunityStage.Prospecting,
                        Probability = seedOpportunity.Probability,
                        Source = seedOpportunity.Source,
                        Comments = seedOpportunity.Notes,
                        Notes = seedOpportunity.Notes,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedOpportunity.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    opportunities.Add(opportunity);
                }

                await Opportunities.AddRangeAsync(opportunities);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding Opportunity data: {ex.Message}");
            }
        }

        public async Task SeedJobData()
        {
            // Check if Job data already exists
            if (await Jobs.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "JobSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "JobSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var jobSeedData = System.Text.Json.JsonSerializer.Deserialize<List<JobSeedModel>>(jsonContent);

                if (jobSeedData?.Any() != true)
                    return;

                var jobs = new List<Job>();
                foreach (var seedJob in jobSeedData)
                {
                    var customer = Customers.FirstOrDefault(c => c.Name == seedJob.CustomerName);
                    var job = new Job
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Title = seedJob.Title,
                        CustomerId = customer?.Oid,
                        Description = seedJob.Description,
                        Status = Enum.TryParse<JobStatus>(seedJob.Status, out var status) ? status : JobStatus.Pending,
                        Priority = Enum.TryParse<Priority>(seedJob.Priority, out var priority) ? priority : Priority.Normal,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedJob.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    jobs.Add(job);
                }

                await Jobs.AddRangeAsync(jobs);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding Job data: {ex.Message}");
            }
        }

        public async Task SeedActivityData()
        {
            // Check if Activity data already exists
            if (await Activities.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "ActivitySeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "ActivitySeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var activitySeedData = System.Text.Json.JsonSerializer.Deserialize<List<ActivitySeedModel>>(jsonContent);

                if (activitySeedData?.Any() != true)
                    return;

                var activities = new List<Activity>();
                foreach (var seedActivity in activitySeedData)
                {
                    var activity = new Activity
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Type = Enum.TryParse<ActivityType>(seedActivity.Type, out var type) ? type : ActivityType.Call,
                        Subject = seedActivity.Subject,
                        Description = seedActivity.Description,
                        ActivityDate = DateTime.TryParse(seedActivity.ActivityDate, out var activityDate) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(activityDate, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        Duration = seedActivity.Duration,
                        Status = Enum.TryParse<ActivityStatus>(seedActivity.Status, out var status) ? status : ActivityStatus.Pending,
                        Priority = Enum.TryParse<Priority>(seedActivity.Priority, out var priority) ? priority : Priority.Normal,
                        Outcome = seedActivity.Outcome,
                        Notes = seedActivity.Notes,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedActivity.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    activities.Add(activity);
                }

                await Activities.AddRangeAsync(activities);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding Activity data: {ex.Message}");
            }
        }

        public async Task SeedContactData()
        {
            // Check if Contact data already exists
            if (await Contacts.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "ContactSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "ContactSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var contactSeedData = System.Text.Json.JsonSerializer.Deserialize<List<ContactSeedModel>>(jsonContent);

                if (contactSeedData?.Any() != true)
                    return;

                var contacts = new List<Contact>();
                foreach (var seedContact in contactSeedData)
                {
                    var customer = Customers.FirstOrDefault(c => c.Name == seedContact.CustomerName);
                    var contact = new Contact
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        CustomerId = customer.Oid,
                        FirstName = seedContact.FirstName,
                        LastName = seedContact.LastName,
                        JobTitle = seedContact.JobTitle,
                        Department = seedContact.Department,
                        Email = seedContact.Email,
                        Phone = seedContact.Phone,
                        Mobile = seedContact.Mobile,
                        LinkedInProfile = seedContact.LinkedInProfile,
                        Address = seedContact.Address,
                        City = seedContact.City,
                        State = seedContact.State,
                        PostalCode = seedContact.PostalCode,
                        Country = seedContact.Country,
                        IsPrimary = seedContact.IsPrimary,
                        Status = Enum.TryParse<ContactStatus>(seedContact.Status, out var status) ? status : ContactStatus.Active,
                        Notes = seedContact.Notes,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedContact.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        UpdatedByUserId = ApplicationUser.SystemUserId,
                        UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        IsNotDeleted = true
                    };

                    contacts.Add(contact);
                }

                await Contacts.AddRangeAsync(contacts);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid breaking the seeding process
                Console.WriteLine($"Error seeding Contact data: {ex.Message}");
            }
        }

        public async Task SeedProductData()
        {
            // Check if Product data already exists
            if (await Products.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "ProductSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "ProductSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var productSeedData = System.Text.Json.JsonSerializer.Deserialize<List<ProductSeedModel>>(jsonContent);

                if (productSeedData?.Any() != true)
                    return;

                var products = new List<Product>();
                foreach (var seedProduct in productSeedData)
                {
                    var product = new Product
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Name = seedProduct.Name,
                        SKU = seedProduct.SKU,
                        Type = Enum.TryParse<ItemType>(seedProduct.Type, out var type) ? type : ItemType.Product,
                        UnitPrice = seedProduct.UnitPrice,
                        Cost = seedProduct.Cost,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedProduct.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()
                    };

                    products.Add(product);
                }

                await Products.AddRangeAsync(products);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to prevent seeding from failing completely
                Console.WriteLine($"Error seeding Product data: {ex.Message}");
            }
        }

        public async Task SeedInventoryItemData()
        {
            // Check if InventoryItem data already exists
            if (await InventoryItems.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "InventoryItemSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "InventoryItemSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var inventoryItemSeedData = System.Text.Json.JsonSerializer.Deserialize<List<InventoryItemSeedModel>>(jsonContent);

                if (inventoryItemSeedData?.Any() != true)
                    return;

                var inventoryItems = new List<InventoryItem>();
                foreach (var seedInventoryItem in inventoryItemSeedData)
                {
                    var inventoryItem = new InventoryItem
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Name = seedInventoryItem.Name,
                        SKU = seedInventoryItem.SKU,
                        Type = Enum.TryParse<ItemType>(seedInventoryItem.Type, out var type) ? type : ItemType.InventoryItem,
                        UnitPrice = seedInventoryItem.UnitPrice,
                        Cost = seedInventoryItem.Cost,
                        QuantityOnHand = seedInventoryItem.QuantityOnHand,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedInventoryItem.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()
                    };

                    inventoryItems.Add(inventoryItem);
                }

                await InventoryItems.AddRangeAsync(inventoryItems);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to prevent seeding from failing completely
                Console.WriteLine($"Error seeding InventoryItem data: {ex.Message}");
            }
        }

        public async Task SeedServiceItemData()
        {
            // Check if ServiceItem data already exists
            if (await ServiceItems.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "ServiceItemSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "ServiceItemSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var serviceItemSeedData = System.Text.Json.JsonSerializer.Deserialize<List<ServiceItemSeedModel>>(jsonContent);

                if (serviceItemSeedData?.Any() != true)
                    return;

                var serviceItems = new List<ServiceItem>();
                foreach (var seedServiceItem in serviceItemSeedData)
                {
                    var serviceItem = new ServiceItem
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Name = seedServiceItem.Name,
                        SKU = seedServiceItem.SKU,
                        Type = Enum.TryParse<ItemType>(seedServiceItem.Type, out var type) ? type : ItemType.ServiceItem,
                        UnitPrice = seedServiceItem.UnitPrice,
                        Cost = seedServiceItem.Cost,
                        HourlyRate = seedServiceItem.HourlyRate,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedServiceItem.CreatedAtUtc, out var createdAt) ?
                            Instant.FromDateTimeUtc(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset()
                    };

                    serviceItems.Add(serviceItem);
                }

                await ServiceItems.AddRangeAsync(serviceItems);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to prevent seeding from failing completely
                Console.WriteLine($"Error seeding ServiceItem data: {ex.Message}");
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            UpdateCreatedByAndModifiedBy();
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        private void UpdateCreatedByAndModifiedBy()
        {
            var currentUserService = this.GetService<ICurrentUserService>();

            foreach (var entry in ChangeTracker.Entries<BaseClass>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
                        if (!string.IsNullOrEmpty(currentUserService.UserId))
                        {
                            entry.Entity.CreatedByUserId = currentUserService.UserId;
                            entry.Entity.CreatedByUserName = currentUserService.FullName;
                            entry.Entity.CreatedAtString = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToHumanDateTime();
                        }
                        else
                        {
                            entry.Entity.CreatedByUserId = ApplicationUser.SystemUserId;
                            entry.Entity.CreatedByUserName = ApplicationUser.SystemUserName;
                            entry.Entity.CreatedAtString = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToHumanDateTime();
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();
                        if (!string.IsNullOrEmpty(currentUserService.UserId))
                        {
                            entry.Entity.UpdatedByUserId = currentUserService.UserId;
                            entry.Entity.UpdatedByUserName = currentUserService.FullName;
                            entry.Entity.UpdatedAtString = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToHumanDateTime();
                        }
                        else
                        {
                            entry.Entity.UpdatedByUserId = ApplicationUser.SystemUserId;
                            entry.Entity.CreatedByUserName = ApplicationUser.SystemUserName;
                            entry.Entity.CreatedAtString = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().ToHumanDateTime();
                        }
                        break;
                }
            }
        }

        public async Task SeedQuoteData()
        {
            // Check if Quote data already exists
            if (await Quotes.AnyAsync())
                return;

            try
            {
                // Get the demo tenant ID
                var demoTenant = await Tenants.FirstOrDefaultAsync(t => t.Name == "TekSpear Solutions");
                if (demoTenant == null)
                    return;

                // Read the JSON file
                var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "QuoteSeedData.json");
                if (!File.Exists(seedDataPath))
                {
                    // Try alternative path (development environment)
                    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    while (projectPath != null && !Directory.GetFiles(projectPath, "*.csproj").Any())
                    {
                        projectPath = Directory.GetParent(projectPath)?.FullName;
                    }
                    if (projectPath != null)
                    {
                        seedDataPath = Path.Combine(Directory.GetParent(projectPath)?.FullName ?? "", "Objects", "SeedData", "QuoteSeedData.json");
                    }
                }

                if (!File.Exists(seedDataPath))
                    return;

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                var quoteSeedData = System.Text.Json.JsonSerializer.Deserialize<List<QuoteSeedModel>>(jsonContent);

                if (quoteSeedData?.Any() != true)
                    return;

                var quotes = new List<Quote>();
                foreach (var seedQuote in quoteSeedData)
                {
                    // Find customer by name
                    var customer = await Customers.FirstOrDefaultAsync(c => c.Name == seedQuote.CustomerName);

                    // Find job by title
                    var job = await Jobs.FirstOrDefaultAsync(j => j.Title == seedQuote.JobTitle);

                    var quote = new Quote
                    {
                        Oid = Guid.NewGuid(),
                        TenantId = demoTenant.Id,
                        Number = seedQuote.Title,
                        Status = Enum.TryParse<QuoteStatus>(seedQuote.Status, out var status) ? status : QuoteStatus.Draft,
                        PartyId = customer?.Oid,
                        JobId = job?.Oid,
                        TotalAmount = seedQuote.TotalAmount,
                        CreatedByUserId = ApplicationUser.SystemUserId,
                        CreatedAtUtc = DateTime.TryParse(seedQuote.CreatedAtUtc, out var createdAt) ?
                            DateTime.SpecifyKind(createdAt, DateTimeKind.Utc).ToDateTimeOffset() :
                            SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                        Items = new List<QuoteItem>()
                    };

                    // Add quote items
                    foreach (var seedItem in seedQuote.Items)
                    {
                        // Try to find existing inventory item by name
                        var inventoryItem = await Items.FirstOrDefaultAsync(i => i.Name == seedItem.ItemName);

                        var quoteItem = new QuoteItem
                        {
                            Oid = Guid.NewGuid(),
                            TenantId = demoTenant.Id,
                            QuoteId = quote.Oid,
                            InventoryItemId = inventoryItem?.Oid,
                            Quantity = seedItem.Quantity,
                            UnitPrice = seedItem.UnitPrice,
                            CreatedByUserId = ApplicationUser.SystemUserId,
                            CreatedAtUtc = quote.CreatedAtUtc
                        };

                        quote.Items.Add(quoteItem);
                    }

                    quotes.Add(quote);
                }

                await Quotes.AddRangeAsync(quotes);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to prevent seeding from failing completely
                Console.WriteLine($"Error seeding Quote data: {ex.Message}");
            }
        }
    }
}

