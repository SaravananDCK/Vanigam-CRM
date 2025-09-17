using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Serilog;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Controllers;

[Route("odata/VanigamAccountingService/ApplicationTenants")]
public partial class ApplicationTenantsController : ODataController
{
    private VanigamAccountingDbContext context;
    private IWebHostEnvironment env;

    public ApplicationTenantsController(IWebHostEnvironment env, VanigamAccountingDbContext context)
    {
        this.context = context;
        this.env = env;
    }

    partial void OnTenantsRead(ref IQueryable<ApplicationTenant> tenants);

    [EnableQuery]
    [HttpGet]
    public IEnumerable<ApplicationTenant> Get()
    {
        try
        {
            if (!(HttpContext.User.IsInRole(ApplicationRole.SuperUserRole) || HttpContext.User.Identity.Name == ApplicationUser.TenantsAdmin))
            {
                return Enumerable.Empty<ApplicationTenant>();
            }

            var items = this.context.Tenants.AsQueryable<ApplicationTenant>();

            OnTenantsRead(ref items);

            return items;
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            return null;
        }
    }

    [EnableQuery]
    [HttpGet("{Id}")]
    public async Task<IActionResult> GetApplicationTenant(int key)
    {
        try
        {
            var item = this.context.Tenants.FirstOrDefault(i => i.Id == key);
            return new ObjectResult(item);
        }
        catch(Exception ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }
    [EnableQuery]
    [HttpGet("GetTenantUsers(tenantId={tenantId})")]
    public async Task<IActionResult> GetTenantUsers(int tenantId)
    {
        try
        {
            var users = this.context.ApplicationTenantUsers.Where(i => i.TenantId == tenantId);
            var userList = users.Select(u => u.UserId);
            var result= this.context.Users.Where(u => userList.Contains(u.Id));
            return new ObjectResult(result);
        }
        catch(Exception ex)
        {
            Log.Error(ex, ex.Message);
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    partial void OnTenantDeleted(ApplicationTenant tenant);

    [HttpDelete("{Id}")]
    public async Task<IActionResult> Delete(int key)
    {
        try
        {
            if (HttpContext.User.Identity.Name != ApplicationUser.TenantsAdmin)
            {
                return new UnauthorizedResult();
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = this.context.Tenants
                .FirstOrDefault(i => i.Id == key);

            if (item == null)
            {
                ModelState.AddModelError("", "Item no longer available");
                return BadRequest(ModelState);
            }

            this.OnTenantDeleted(item);
            this.context.Tenants.Remove(item);
            await this.context.SaveChangesAsync();

            return new NoContentResult();
        }
        catch(Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    partial void OnTenantUpdated(ApplicationTenant tenant);

    [HttpPatch("{Id}")]
    public async Task<IActionResult> Patch(int key, [FromBody]Delta<ApplicationTenant> patch)
    {
        if (HttpContext.User.Identity.Name != ApplicationUser.TenantsAdmin)
        {
            return new UnauthorizedResult();
        }

        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = this.context.Tenants.FirstOrDefault(i => i.Id == key);

        if (item == null)
        {
            ModelState.AddModelError("", "Item no longer available");
            return BadRequest(ModelState);
        }

        if (env.EnvironmentName != "Development")
        {
            if (context.Tenants.ToList().Any(t => t.Hosts.Split(',').Any(h => item.Hosts.Split(',').Contains(h))))
            {
                ModelState.AddModelError("", "Tenant with the same host already exist.");
                return BadRequest(ModelState);
            }
        }

        patch.Patch(item);

        this.OnTenantUpdated(item);
        this.context.Tenants.Update(item);
        await this.context.SaveChangesAsync();

        var itemToReturn = this.context.Tenants.Where(i => i.Id == key);
        return new ObjectResult(SingleResult.Create(itemToReturn));
    }

    partial void OnTenantCreated(ApplicationTenant tenant);

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ApplicationTenant item)
    {
        if (HttpContext.User.Identity.Name != ApplicationUser.TenantsAdmin)
        {
            return new UnauthorizedResult();
        }

        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (item == null)
        {
            return BadRequest();
        }

        if (env.EnvironmentName != "Development")
        {
            if (context.Tenants.ToList().Any(t => t.Hosts.Split(',').Any(h => item.Hosts.Split(',').Contains(h))))
            {
                ModelState.AddModelError("", "Tenant with the same host already exist.");
                return BadRequest(ModelState);
            }
        }
            
        this.OnTenantCreated(item);
        this.context.Tenants.Add(item);
        await this.context.SaveChangesAsync();

        var key = item.Id;

        var itemToReturn = this.context.Tenants.FirstOrDefault(i => i.Id == key);

        return Created($"auth/ApplicationTenants({itemToReturn?.Id})", itemToReturn);
    }
}
