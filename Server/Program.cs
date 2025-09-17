using System.Data.Common;
using System.Text;
using Blazr.RenderState.Server;
using Hangfire;
using Hangfire.PostgreSql;
using Radzen;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.XtraReports.Web.Extensions;
using DevExpress.XtraReports.Security;
using DevExpress.XtraReports.Web.WebDocumentViewer;
using Microsoft.AspNetCore.Authentication.Cookies;
using NpgsqlTypes;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using QuestPDF.Infrastructure;
using Twilio;
using Vanigam.Objects.Helpers;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Server.Extensions;
using Vanigam.CRM.Server.Services;
using Vanigam.CRM.Objects.Helpers;
using Vanigam.Objects.DTOs;
using Vanigam.CRM.Objects.Redis;
using Vanigam.CRM.Server.Permissions;
using Vanigam.CRM.Server.Session;
using Vanigam.CRM.Objects.Services;
using Vanigam.CRM.Server.Helpers;
using Vanigam.CRM.Server.Components;
using Vanigam.CRM.Server.HangFire;
using Vanigam.CRM.Server.Services.Reporting;
using Vanigam.CRM.Client;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var isPostGreSql = builder.Configuration.GetValue<bool>("IsPostGreSQL");
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableDebugging=true;
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024)
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
//-----swagger-----
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(
//    s =>
//    {
//        s.DocumentFilter<SwaggerFilterOutControllers>();
//        s.SchemaFilter<SwaggerExcludeClrTypesFilter>();
//    });
//-----swagger-----
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "VanigamAccountingTheme";
    options.Duration = TimeSpan.FromDays(365);
});

builder.Services.AddScoped<DialogService, VanigamAccountingDialogService>();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<Vanigam.CRM.Objects.VanigamAccountingDbContext>(options =>
{
    if (isPostGreSql)
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("EntitiesConnection"));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("EntitiesConnection"));
    }
});
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>().AddEntityFrameworkStores<VanigamAccountingDbContext>().AddDefaultTokenProviders();
builder.Services.AddControllers().AddOData(opt =>
{
    opt.InitOData();
    opt.TimeZone = TimeZoneInfo.Utc;
});

//builder.Services.AddInheritedClasses(typeof(BaseApiService<>));
builder.Services.AddInheritedClasses(typeof(BaseService<>));
builder.Services.AddAuthentication();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/unauthorized";
    options.Cookie.Name = "VanigamAccountingCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.LoginPath = "/Account/Login";
    // ReturnUrlParameter requires 
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            //JWT token life time is validated manually by JwtTokenLifetimeAuthFilter
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Jwt:Issuer"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Key")))
        };
    });
foreach (var policyDefinition in PolicyDefinition.PolicyDefinitions)
{
    builder.Services.AddAuthorizationBuilder().AddPolicy(policyDefinition.PolicyName, policy => policy.RequireAssertion(policyDefinition.Requirement));
}

builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddSingleton<RedisService>();
builder.Services.AddScoped<SecurityService>();

//builder.Services.AddTransient<IUserStore<ApplicationUser>, MultiTenancyUserStore>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<SessionManager>();

// User Session Tracking Services
builder.Services.AddScoped<IRedisSessionService, RedisSessionService>();
//builder.Services.AddScoped<IUserSessionService, UserSessionService>();

builder.Services.AddScoped<ReconcilePermissionService>();
builder.Services.AddScoped<PermissionService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddControllers().AddOData(o =>
{
    var oDataBuilder = new ODataConventionModelBuilder();
    oDataBuilder.EntitySet<ApplicationUser>("ApplicationUsers");
    var usersType = oDataBuilder.StructuralTypes.First(x => x.ClrType == typeof(ApplicationUser));
    usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.Password)));
    usersType.AddProperty(typeof(ApplicationUser).GetProperty(nameof(ApplicationUser.ConfirmPassword)));
    oDataBuilder.EntitySet<ApplicationRole>("ApplicationRoles");
    oDataBuilder.EntitySet<ApplicationTenantUser>("ApplicationTenantUsers");
    var entity = oDataBuilder.EntitySet<ApplicationTenant>("ApplicationTenants");
    var functionRefreshSummary = entity.EntityType.Collection.Function("GetTenantUsers")
        .ReturnsCollectionFromEntitySet<ApplicationUser>("ApplicationUsers");
    functionRefreshSummary.Parameter<int>("tenantId");
    o.AddRouteComponents("odata/Identity", oDataBuilder.GetEdmModel()).Count().Filter().OrderBy().Expand().Select().SetMaxTop(null).TimeZone = TimeZoneInfo.Utc;
    o.TimeZone = TimeZoneInfo.Utc;
});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, Vanigam.CRM.Client.ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped <ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<ApplicationAuthenticationStateProvider>());


builder.Services.AddLocalization();
//builder.Services.AddScoped<ILocalStorageService, BrowserLocalStorageService>();
//builder.Services.AddScoped<ILocalStorageService, RedisLocalStorageService>();
builder.Services.AddDevExpressControls();
builder.Services.AddScoped<ReportStorageWebExtension, CustomReportStorageWebExtension>();
builder.Services.AddScoped<IWebDocumentViewerReportResolver, WebDocumentViewerReportResolver>();
builder.Services.AddScoped<IObjectDataSourceInjector, ObjectDataSourceInjector>();


builder.Services.ConfigureReportingServices(configurator =>
{
    configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
    {
        viewerConfigurator.UseCachedReportSourceBuilder();
    });
    configurator.UseAsyncEngine();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://vanigamAccounting.tekspear.com")
                .AllowAnyHeader()
                .AllowAnyMethod();
            policyBuilder.WithOrigins("https://*:5001/")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.AddBlazrRenderStateServerServices();

ScriptPermissionManager.GlobalInstance = new ScriptPermissionManager(ExecutionMode.Unrestricted);
DevExpress.Utils.DeserializationSettings.RegisterTrustedAssembly(typeof(DbContext).Assembly);
DevExpress.Utils.DeserializationSettings.RegisterTrustedAssembly(typeof(Language).Assembly);

//builder.Services.AddScoped<FireBaseMessagingService>();

if (builder.Configuration.GetValue<bool>("EnableHangFire"))
{
    builder.Services.AddHangfire(configuration => configuration.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("EntitiesConnection")));
    //builder.Services.AddHangfire(configuration => configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("EntitiesConnection")));
    builder.Services.AddHangfireServer();
}

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

try
{
    app.UseHttpsRedirection();
    app.MapControllers();
    //app.UseHeaderPropagation();

    app.UseRequestLocalization(options =>
        options.AddSupportedCultures("en", "es").AddSupportedUICultures("en", "es").SetDefaultCulture("en"));
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<SessionValidationMiddleware>();
    app.UseAntiforgery();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Vanigam.CRM.Client._Imports).Assembly);
    app.UseCors("MyAllowSpecificOrigins");
    app.UseSerilogIngestion();
    //app.UseSerilogUi(option => option.WithRoutePrefix("log-dashboard"));
    //app.UseSwagger();
    //app.UseSwaggerUI();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        IServiceProvider provider = scope.ServiceProvider;
        VanigamAccountingDbContext vanigamDbContext = provider.GetRequiredService<VanigamAccountingDbContext>();
        vanigamDbContext.SeedInitialData().Wait();
    }

    if (builder.Configuration.GetValue<bool>("EnableHangFire"))
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new NoAuthorizationFilter() }
        });
        //app.UseHangfireDashboard();
    }

    await app.StartAsync();
    if (builder.Configuration.GetValue<bool>("EnableHangFire"))
    {
        builder.AddOrUpdateHangFireJobs();
    }

    await app.WaitForShutdownAsync();

}
catch (Exception ex)
{
    // Log the exception if the application startup fails
    Log.Fatal(ex, "An error occurred while starting the application");
    throw; // Optionally re-throw the exception
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are flushed to the log sink
}


