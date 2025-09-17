using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class LanguageApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Language>(navigationManager, httpClient, authenticationStateProvider, configuration, "Languages");
