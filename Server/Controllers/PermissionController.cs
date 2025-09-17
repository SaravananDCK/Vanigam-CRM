using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.OData.ModelBuilder.Core.V1;
using Serilog;
using System.Security.Claims;
using Vanigam.CRM.Server.Permissions;
using Vanigam.CRM.Objects.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Vanigam.CRM.Server.Controllers
{
    [Route("Permission")]
    [Authorize]
    public class PermissionController : Controller
    {
        public PermissionService PermissionService { get; }
        
        public PermissionController(PermissionService permissionService)
        {
            PermissionService = permissionService;
        }

       

        [HttpGet]
        [Route("GetClaims(roleId='{roleId}')")]
        public async Task<IEnumerable<AuthorizationPermission>> GetClaims(string roleId)
        {
            try
            {
               var result = await PermissionService.GetClaims(roleId);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        [HttpPost]
        [Route("UpdatePermissions")]
        public async Task<IActionResult> UpdatePermissions([FromBody] AuthorizationPermission permission)
        {
            try
            {
                var result = await PermissionService.UpdatePermissions(permission);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}

