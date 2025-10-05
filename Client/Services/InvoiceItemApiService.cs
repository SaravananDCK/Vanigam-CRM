using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class InvoiceItemApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<InvoiceItem>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.InvoiceItems));
