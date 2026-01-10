using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class FileCategoryApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<FileCategory>(navigationManager, httpClient, authenticationStateProvider, configuration,
        nameof(VanigamAccountingDbContext.FileCategories));
