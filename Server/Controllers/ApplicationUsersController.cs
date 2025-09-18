using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData.Results;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Server.Controllers
{
    [Authorize]
    [Route("odata/VanigamAccountingService/ApplicationUsers")]
    public partial class ApplicationUsersController : ODataController
    {
        private readonly VanigamAccountingDbContext context;
        private readonly UserManager<ApplicationUser> userManager;


        private readonly IWebHostEnvironment env;

        public ApplicationUsersController(IWebHostEnvironment env, VanigamAccountingDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;

            this.env = env;
        }

        partial void OnUsersRead(ref IQueryable<ApplicationUser> users);

        [EnableQuery]
        [HttpGet]
        public IEnumerable<ApplicationUser> Get()
        {
            var users = userManager.Users;

            if (env.EnvironmentName != "Development")
            {
                var tenant = context.Tenants.ToList().FirstOrDefault(t => t.Hosts.Split(',').Any(h => h.Contains(HttpContext.Request.Host.Value)));
                if (tenant != null)
                {
                    users = users.Where(r => r.TenantId == tenant.Id);
                }
            }
            OnUsersRead(ref users);

            return users;
        }

        [EnableQuery]
        [HttpGet("{Id}")]
        public SingleResult<ApplicationUser> GetApplicationUser(string key)
        {
            var user = context.Users.Where(i => i.Id.ToString() == key);

            return SingleResult.Create(user);
        }

        partial void OnUserDeleted(ApplicationUser user);

        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(string key)
        {
            var user = await userManager.FindByIdAsync(key);

            if (user == null)
            {
                return NotFound();
            }

            OnUserDeleted(user);

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return IdentityError(result);
            }

            return new NoContentResult();
        }

        partial void OnUserUpdated(ApplicationUser user);

        [HttpPatch("{Id}")]
        public async Task<IActionResult> Patch(string key, [FromBody]ApplicationUser data)
        {
            var user = await userManager.FindByIdAsync(key);

            if (user == null)
            {
                return NotFound();
            }

            OnUserUpdated(data);

            IdentityResult result = null;


            userManager.UserValidators.Clear();
            user.Roles = null;
            user.TenantId = data.TenantId;
            result = await userManager.UpdateAsync(user);

            if (data.Roles != null)
            {
                result = await userManager.RemoveFromRolesAsync(user, await userManager.GetRolesAsync(user));

                if (result.Succeeded) 
                {
                    result = await userManager.AddToRolesAsync(user, data.Roles.Select(r => r.Name));
                }
            }

            if (!string.IsNullOrEmpty(data.Password))
            {
                result = await userManager.RemovePasswordAsync(user);

                if (result.Succeeded)
                {
                    result = await userManager.AddPasswordAsync(user, data.Password);
                }

                if (!result.Succeeded)
                {
                    return IdentityError(result);
                }
            }

            if (result != null && !result.Succeeded)
            {
                return IdentityError(result);
            }

            return new NoContentResult();
        }

        partial void OnUserCreated(ApplicationUser user);

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApplicationUser user)
        {
            user.UserName = user.Email;
            //user.EmailConfirmed = true;
            var password = user.Password;
            var roles = user.Roles;
            user.Roles = null;

            userManager.UserValidators.Clear();

            if (context.Users.Any(u => u.TenantId == user.TenantId && u.UserName == user.Name))
            {
                ModelState.AddModelError("", "User with the same name already exist for this tenant.");
                return BadRequest(ModelState);
            }
            IdentityResult result = await userManager.CreateAsync(user, password);

            if (result.Succeeded && roles != null)
            {
                result = await userManager.AddToRolesAsync(user, roles.Select(r => r.Name));
            }

            user.Roles = roles;

            if (result.Succeeded)
            {
                OnUserCreated(user);

                return Created($"odata/Identity/Users('{user.Id}')", user);
            }
            else
            {
                return IdentityError(result);
            }
        }

        private IActionResult IdentityError(IdentityResult result)
        {
            var message = string.Join(", ", result.Errors.Select(error => error.Description));

            return BadRequest(new { error = new { message } });
        }
    }
}
