using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Globalization;
using Blazr.RenderState.WASM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Vanigam.Objects.Helpers;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Helpers;
using Vanigam.CRM.Client.Services;
using Vanigam.Objects.DTOs;
using Vanigam.CRM.Client.Handlers;
using Vanigam.CRM.Objects.Services;
using Vanigam.CRM.Client.Components;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "VanigamAccountingTheme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddScoped<DialogService, VanigamAccountingDialogService>();
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ReportApiService>();
builder.Services.AddInheritedClasses(typeof(BaseApiService<>));
builder.Services.AddScoped<PermissionApiService>();
builder.Services.AddScoped<UserSessionApiService>();

builder.Services.AddAuthorizationCore(options =>
{
    foreach (var policyDefinition in PolicyDefinition.PolicyDefinitions)
    {
        options.AddPolicy(policyDefinition.PolicyName, policy => policy.RequireAssertion(policyDefinition.Requirement));
    }
});

builder.Services.AddHttpClient("Vanigam.CRM.Server",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<VanigamAccountingContextDelegatingHandler>();
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Vanigam.CRM.Server"));
builder.Services.AddScoped<Vanigam.CRM.Client.SecurityService>();
builder.Services.AddScoped<AuthenticationStateProvider, Vanigam.CRM.Client.ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<Vanigam.CRM.Client.ApplicationAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<Vanigam.CRM.Client.ApplicationAuthenticationStateProvider>());
builder.Services.AddLocalization();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();
builder.Services.AddScoped<ILocalStorageService, BrowserLocalStorageService>();
//builder.Services.AddScoped<ILocalStorageService, RedisLocalStorageService>();
builder.Services.AddTransient<VanigamAccountingContextDelegatingHandler>();
builder.AddBlazrRenderStateWASMServices();
builder.Services.AddDevExpressBlazor();

//Serilog client
var levelSwitch = new LoggingLevelSwitch();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .Enrich.WithProperty("InstanceId", Guid.NewGuid().ToString("n"))
    .WriteTo.BrowserHttp(endpointUrl: $"{builder.HostEnvironment.BaseAddress}ingest", controlLevelSwitch: levelSwitch)
    .CreateLogger();
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));


var host = builder.Build();

var jsRuntime = host.Services.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
var culture = await jsRuntime.InvokeAsync<string>("Radzen.getCulture");
if (!string.IsNullOrEmpty(culture))
{
    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
}
builder.Services.Configure<VanigamAccountingOptions>(builder.Configuration.GetSection(nameof(VanigamAccountingOptions)));

await host.RunAsync();
