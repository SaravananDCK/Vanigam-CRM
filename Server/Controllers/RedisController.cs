using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Redis;

namespace Vanigam.CRM.Server.Controllers;

[Route("Redis/[action]")]
[Authorize]
public class RedisController(RedisService redisService) : Controller
{
    public RedisService RedisService { get; } = redisService;

    [HttpGet]
    public async Task<IActionResult> HasPermission(string userId,string tableName,PermissionType permissionType)
    {
        if (string.IsNullOrEmpty(userId))
        {
            // For example, throw an exception or return an error response
            throw new InvalidOperationException("User ID not found in claims.");

        }
        var claims = await RedisService.GetUserClaimsAsync(userId);

        var readPermissions = claims.Where(c => c.Type == "Permission." +  tableName)
            .Select(c => JsonSerializer.Deserialize<AuthorizationPermission>(c.Value)).ToList();
        if (permissionType.HasFlag(PermissionType.Read) && readPermissions.Any(role => role.Read))
        {
            return Ok(true);
        }
        if (permissionType.HasFlag(PermissionType.Create) && readPermissions.Any(role => role.Create))
        {
            return Ok(true);
        }
        if (permissionType.HasFlag(PermissionType.Update) && readPermissions.Any(role => role.Update))
        {
            return Ok(true);
        }
        if (permissionType.HasFlag(PermissionType.Delete) && readPermissions.Any(role => role.Delete))
        {
            return Ok(true);
        }

        return Ok(false);
    }
}
