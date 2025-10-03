using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class LedgerAccountApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<LedgerAccount>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.LedgerAccounts));
