using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class FileCategoryApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<FileCategory>(navigationManager, httpClient, authenticationStateProvider, configuration,
        "FileCategories");
