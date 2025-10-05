using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class ContractCoverageRuleApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<ContractCoverageRule>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.ContractCoverageRules));
