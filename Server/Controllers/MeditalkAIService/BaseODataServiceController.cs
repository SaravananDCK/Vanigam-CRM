using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Data;
using Vanigam.CRM.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Vanigam.CRM.Server.ApiAuth;
using Vanigam.CRM.Objects.Enums;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Exceptions;


namespace Vanigam.CRM.Server.Controllers;

//[Route("odata/MeditalkAIService/{Controller}")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//[JwtTokenLifetimeAuthorize]
public abstract class BaseODataServiceController<T, K> : ODataController where T : BaseClass where K : BaseService<T>
{
    public LoginUserType? UserType { get; private set; }
    public int? TenantId { get; private set; }
    public ApplicationUser CurrentUser { get; private set; }
    protected BaseODataServiceController(VanigamAccountingDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager, K service,
        string expandProperties)
    {
        Context = context;
        UserManager = userManager;
        Service = service;
        RoleManager = roleManager;
        ExpandProperties = expandProperties;
        TenantId = service?.TenantId;
    }

    protected VanigamAccountingDbContext Context { get; set; }
    public K Service { get; }
    protected UserManager<ApplicationUser> UserManager { get; }
    protected RoleManager<ApplicationRole> RoleManager { get; }

    public string ExpandProperties { get; }

    //[Authorize(Policy = "IsRead")]
    [HttpGet]
    [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
    public async Task<IEnumerable<T>> GetAll(bool expandUserInfo = false)
    {
        try
        {
            return await Service.GetAllAsync(request: Request);
            //var items = await Service.GetAllAsync(request:Request);
            //if (!expandUserInfo)
            //{
            //    return items;
            //}
            //var query = from b in items
            //            join createdByUser in Context.Set<User>()
            //                on b.CreatedById equals createdByUser.Oid into createdByGroup
            //            from createdBy in createdByGroup.DefaultIfEmpty()
            //            join modifiedByUser in Context.Set<User>()
            //                on b.ModifiedById equals modifiedByUser.Oid into modifiedByGroup
            //            from modifiedBy in modifiedByGroup.DefaultIfEmpty()
            //            select new
            //            {
            //                BaseClass = b,
            //                CreatedByUser = createdBy,
            //                ModifiedByUser = modifiedBy
            //            };

            //foreach (var item in query)
            //{
            //    item.BaseClass.SetCreatedByAndModifiedBy(item.CreatedByUser?.FullName, item.ModifiedByUser?.FullName);
            //}
            //return query.Select(q => q.BaseClass);
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            return null;
        }

    }

    //[Authorize(Policy = "IsRead")]
    [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
    [HttpGet("/odata/MeditalkAIService/{Controller}(Oid={Oid})")]
    public async Task<SingleResult<T>> Get(Guid key)
    {
        try
        {
            var items = await Service.GetAsync(key, Request);
            var result = SingleResult.Create(items);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            return null;
        }

    }

    //[Authorize(Policy = "IsDelete")]
    [HttpDelete("/odata/MeditalkAIService/{Controller}(Oid={Oid})")]
    //[Authorize(Policy = Vanigam.CRM.Objects.ApplicationPolicy.IsSuperUser)]
    public async Task<IActionResult> Delete(Guid key)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var items = await Service.GetAsync(key,Request);
            if (Request != null)
                items = EntityPatch.ApplyTo(Request, items);
            var item = items.FirstOrDefault();
            if (item == null) return StatusCode((int)HttpStatusCode.PreconditionFailed);
            await Service.DeleteAsync(item);
            return new NoContentResult();
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    //[Authorize(Policy = "IsUpdate")]
    [HttpPut("/odata/MeditalkAIService/{Controller}(Oid={Oid})")]
    [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
    public async Task<IActionResult> Put(Guid key, [FromBody] T item)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var items = await Service.GetAsync(key, Request);
            if (Request != null)
                items = (DbSet<T>)EntityPatch.ApplyTo(Request, items);
            var firstItem = items.FirstOrDefault();

            if (firstItem == null) return StatusCode((int)HttpStatusCode.PreconditionFailed);

            var itemToReturn = await Service.UpdateAsync(item, GetSkipAuditLog());
            if (Request != null)
                Request.QueryString = Request.QueryString.Add("$expand", ExpandProperties);
            return new ObjectResult(SingleResult.Create(itemToReturn));
        }
        catch (NotUniqueException ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return new ObjectResult(ModelState)
            {
                StatusCode = (int)HttpStatusCode.Conflict
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    //[Authorize(Policy = "IsUpdate")]
    [HttpPatch("/odata/MeditalkAIService/{Controller}(Oid={Oid})")]
    [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
    public async Task<IActionResult> Patch(Guid key, [FromBody] Delta<T> patch)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var items = await Service.GetAsync(key, Request);
            if (Request != null)
                items = EntityPatch.ApplyTo(Request, items);

            var item = items.FirstOrDefault();

            if (item == null) return StatusCode((int)HttpStatusCode.PreconditionFailed);

            patch.Patch(item);
            var itemToReturn = await Service.UpdateAsync(item, GetSkipAuditLog());
            if (Request != null)
                Request.QueryString = Request.QueryString.Add("$expand", ExpandProperties);
            return new ObjectResult(SingleResult.Create(itemToReturn));
        }
        catch (NotUniqueException ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return new ObjectResult(ModelState)
            {
                StatusCode = (int)HttpStatusCode.Conflict
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    //[Authorize(Policy = "IsCreate")]
    [HttpPost]
    [EnableQuery(MaxExpansionDepth = 10, MaxAnyAllExpressionDepth = 10, MaxNodeCount = 1000)]
    public async Task<IActionResult> Post([FromBody] T item)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (item == null) return BadRequest();
            var itemToReturn = await Service.CreateAsync(item, GetSkipAuditLog());
            if (Request != null)
            {
                Request.QueryString = Request.QueryString.Add("$expand", ExpandProperties);
            }
            var value = SingleResult.Create(itemToReturn);
            return new ObjectResult(value)
            {
                StatusCode = 201
            };
        }
        catch (NotUniqueException ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return new ObjectResult(ModelState)
            {
                StatusCode = (int)HttpStatusCode.Conflict
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    private bool GetSkipAuditLog()
    {
        if (Request==null)
        {
            return false;
        }
        Request.Headers.TryGetValue("SkipAuditLog", out var skipAuditLogStr);
        var val = skipAuditLogStr.FirstOrDefault();
        if (string.IsNullOrEmpty(val)) return false;
        return bool.Parse(val);
    }
}
