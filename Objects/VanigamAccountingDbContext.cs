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

namespace Vanigam.CRM.Objects
{
    public partial class VanigamAccountingDbContext(DbContextOptions<VanigamAccountingDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
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
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<MaterialUsage> MaterialUsages => Set<MaterialUsage>();
        public DbSet<Quote> Quotes => Set<Quote>();
        public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // enums -> string
            modelBuilder.Entity<Lead>().Property(l => l.Status).HasConversion<string>().IsRequired();
            modelBuilder.Entity<Opportunity>().Property(o => o.Stage).HasConversion<string>().IsRequired();
            modelBuilder.Entity<Job>().Property(j => j.Status).HasConversion<string>().IsRequired();
            modelBuilder.Entity<Appointment>().Property(a => a.Status).HasConversion<string>().IsRequired();
            modelBuilder.Entity<JobAssignment>().Property(a => a.Status).HasConversion<string>().IsRequired();
            modelBuilder.Entity<Technician>().Property(t => t.Status).HasConversion<string>().IsRequired();
            modelBuilder.Entity<Invoice>().Property(i => i.Status).HasConversion<string>().IsRequired();

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
                Hosts = "https://localhost:5001/,http://localhost:5270/"

            };
            Tenants.Add(tekSpearTenant);
            await this.SaveChangesAsync();

            var demoTenant = new ApplicationTenant
            {
                Id = 2,
                Name = "TekSpear Solutions demo",
                //Currency = "INR",
                //TimeZone = "Asia/Kolkata",
                Hosts = "https://localhost:5001/,http://localhost:5270/"

            };
            Tenants.Add(demoTenant);
            await this.SaveChangesAsync();

            var roleStore = new RoleStore<ApplicationRole>(this);
            await roleStore.CreateAsync(new ApplicationRole { Name = ApplicationRole.SuperUserRole, NormalizedName = ApplicationRole.SuperUserRole.ToUpper() });
            await roleStore.CreateAsync(new ApplicationRole { Name = ApplicationRole.AdminRole, NormalizedName = ApplicationRole.AdminRole.ToUpper() });

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
                var userStore = new UserStore<SuperUser>(this);
                await userStore.CreateAsync(tenantsAdmin);
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
            admin.Roles = new List<ApplicationRole>();
            admin.Roles.Add(Roles.FirstOrDefault(r => r.Name == ApplicationRole.SuperUserRole));
            if (!this.Users.Any(u => u.UserName == admin.UserName))
            {
                var password = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(admin, ApplicationUser.Admin + "@123");
                admin.PasswordHash = hashed;
                var userStore = new UserStore<SuperUser>(this);
                await userStore.CreateAsync(admin);
            }
            var systemAdmin = new SuperUser()
            {
                Id = ApplicationUser.SystemUserId,
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
            systemAdmin.Roles = new List<ApplicationRole>();
            systemAdmin.Roles.Add(Roles.FirstOrDefault(r => r.Name == ApplicationRole.SuperUserRole));
            if (!this.Users.Any(u => u.UserName == systemAdmin.UserName))
            {
                var password = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(systemAdmin, ApplicationUser.SystemUserName + "@123");
                systemAdmin.PasswordHash = hashed;
                var userStore = new UserStore<SuperUser>(this);
                await userStore.CreateAsync(systemAdmin);
            }
            await this.SaveChangesAsync();
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
                        entry.Entity.CreatedAtUtc = DateTimeOffset.UtcNow;
                        if (!string.IsNullOrEmpty(currentUserService.UserId))
                        {
                            entry.Entity.CreatedByUserId = currentUserService.UserId;
                            entry.Entity.CreatedByUserName = currentUserService.FullName;
                            entry.Entity.CreatedAtString = DateTimeOffset.UtcNow.ToHumanDateTime();
                        }
                        else
                        {
                            entry.Entity.CreatedByUserId = ApplicationUser.SystemUserId;
                            entry.Entity.CreatedByUserName = ApplicationUser.SystemUserName;
                            entry.Entity.CreatedAtString = DateTimeOffset.UtcNow.ToHumanDateTime();
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(currentUserService.UserId))
                        {
                            entry.Entity.UpdatedByUserId = currentUserService.UserId;
                            entry.Entity.UpdatedByUserName = currentUserService.FullName;
                            entry.Entity.UpdatedAtString = DateTimeOffset.UtcNow.ToHumanDateTime();
                        }
                        else
                        {
                            entry.Entity.UpdatedByUserId = ApplicationUser.SystemUserId;
                            entry.Entity.CreatedByUserName = ApplicationUser.SystemUserName;
                            entry.Entity.CreatedAtString = DateTimeOffset.UtcNow.ToHumanDateTime();
                        }
                        break;
                }
            }
        }
    }
}

