using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;
using Vanigam.CRM.Objects.Models;
using Vanigam.CRM.Objects.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Vanigam.CRM.Server.Permissions;
using Vanigam.CRM.Objects.Redis;
using DocumentFormat.OpenXml.InkML;
using Vanigam.CRM.Server.Session;
using Microsoft.AspNetCore.Authentication.Cookies;
using Vanigam.CRM.Server.Helpers;
using Vanigam.CRM.Server.Services;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Server.Controllers
{
    [Route("Account/[action]")]
    public partial class AccountController(
        IWebHostEnvironment env,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEmailSender<ApplicationUser> emailSender,
        IConfiguration configuration,
        IOptions<VanigamAccountingOptions> vanigamAccountingOptions,
        VanigamAccountingDbContext vanigamAccountingDbContext,
        RedisService redisService, 
        SessionManager sessionManager)
        : Controller
    {
        private const string ResetPasswordLoginProvider = "ResetPasswordToken";
        private const string ApiTokenLoginProvider = "ApiToken";
        private readonly IWebHostEnvironment _env = env;
        private readonly TimeSpan _tokenLifeSpan = new(0, 2, 0, 0);
        public IOptions<VanigamAccountingOptions> VanigamAccountingOptions { get; } = vanigamAccountingOptions;

        private IActionResult RedirectWithError(string error, string redirectUrl = null)
        {
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return Redirect($"~/Login?error={error}&redirectUrl={Uri.EscapeDataString(redirectUrl)}");
            }
            else
            {
                return Redirect($"~/Login?error={error}");
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AuthenticateToken([FromBody] UserLogin data)
        {
            if (data == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await AuthenticateUser(data);
            if (user != null)
            {
                var loginUser = GetLoginUser(user);
                var token = GenerateJSONWebToken(user, null);
                //await PermissionClaim.AddPermissionClaim(userManager, roleManager, user, redisService);
                var response = Ok(new { Token = token, Name = user.Name, UserId = user.Id, Message = "Login successful", UserType =  loginUser?.UserType.ToString(), Roles = user.Roles.Select(r => r.Name) });
                return response;
            }

            return BadRequest("Invalid login");

        }
        /// <summary>
        /// Get the AccessToken
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<IEnumerable<string>> GetAccessToken()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return new string[] { accessToken };
        }
        private string GenerateJSONWebToken(ApplicationUser user, int? expirationMinutes)
        {
            var loginUser = GetLoginUser(user);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>();
            claims.Add(new(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new(nameof(ApplicationUser.UserName), user.UserName));
            if (user.SessionId != null)
            {
                claims.Add(new(nameof(ApplicationUser.SessionId), user.SessionId));
            }
            if (loginUser != null)
            {
                if (!string.IsNullOrEmpty(user.TenantId?.ToString())) claims.Add(new(nameof(ApplicationUser.TenantId), user.TenantId.ToString()));
                if (!string.IsNullOrEmpty(loginUser?.FullName)) claims.Add(new(nameof(ApplicationUser.FullName), loginUser?.FullName));
                if (!string.IsNullOrEmpty(loginUser?.UserType?.ToString())) claims.Add(new(nameof(LoginUserType), loginUser?.UserType?.ToString()));
            }
           
            var token = new JwtSecurityToken(configuration["Jwt:Issuer"],
                configuration["Jwt:Issuer"],
                claims,
                expires: expirationMinutes != null ? DateTime.Now.AddMinutes(expirationMinutes.Value) : DateTime.Now.AddMinutes(_tokenLifeSpan.TotalMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SwitchTenantAndReAuthenticate(string userId, string tenantId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            {
                return BadRequest($"Invalid {nameof(userId)} or {nameof(tenantId)}");
            }

            try
            {
                //var user = await userManager.FindByIdAsync(userId);
                var user = userManager.Users.Include(u => u.Roles).FirstOrDefault(u => u.Id.ToString() == userId);
                if (user == null)
                    return Unauthorized();

                if (user.TenantId.ToString() != tenantId)
                    return Unauthorized();

                if (User.Identity.IsAuthenticated)
                {
                    // Sign out current session
                    await signInManager.SignOutAsync();
                    var claims = new List<Claim>();
                    if (user != null)
                    {
                        claims.Add(new Claim("Bearer_Token_Expiry", DateTime.UtcNow.AddMinutes(_tokenLifeSpan.TotalMinutes).ToString(new CultureInfo("en-US"))));
                        claims.Add(new Claim("Bearer_Token", GenerateJSONWebToken(user, null)));
                        //await PermissionClaim.AddPermissionClaim(userManager, roleManager, user, redisService);
                        claims.Add(new Claim(nameof(ApplicationUser.TenantId), user.TenantId.ToString()));
                    }

                    if (user != null)
                    {
                        //Sign in new session
                        await signInManager.SignInWithClaimsAsync(user, false, claims);
                        return Ok("Tenant switched successfully.");
                    }
                }

                await signInManager.SignOutAsync();
                return Unauthorized("User is not authenticated. Please log in again.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
        private ApplicationUser? GetLoginUser(ApplicationUser user)
        {
            if (user != null)
            {
                var loginUser = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id == user.Id);
                if (loginUser != null)
                {
                    return loginUser;
                }
            }
            return null;
        }   
        private async Task<ApplicationUser> AuthenticateUser(UserLogin login)
        {
            ApplicationUser user = userManager.Users.Include(u => u.Roles).FirstOrDefault(u => u.NormalizedUserName == login.UserName.ToUpper());
            var validLogin = await userManager.CheckPasswordAsync(user, login.Password);
            if (validLogin)
            {
                return user;
            }
            return null;
        }

        private async Task HandleLogOut()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userId);
            if (user != null && userId != null)
            {
                await sessionManager.LogOutLock(user);

                //// Record logout in session tracking
                //try
                //{
                //    await userSessionService.RecordLogoutAsync(userId);
                //}
                //catch (Exception ex)
                //{
                //    Log.Warning(ex, "Failed to record logout for {UserId}", userId);
                //}

                await redisService.RemoveUserClaimsAsync(userId.ToString());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVanigamAccountingOptions()
        {
            return Ok(VanigamAccountingOptions?.Value);
        }
        [HttpGet]
        public async Task<IActionResult> GetUserById(string id)
        {
            return Ok(userManager.Users.Include(u => u.Roles).FirstOrDefault(u => u.Id.ToString() == id));
        }
        [HttpGet]
        public async Task<IActionResult> GetLoginUser()
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var loginUser = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            return Ok(loginUser);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (returnUrl != "/" && !string.IsNullOrEmpty(returnUrl))
            {
                return Redirect($"~/Login?redirectUrl={Uri.EscapeDataString(returnUrl)}");
            }

            return Redirect("~/Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password, string redirectUrl)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {

                var user = await userManager.FindByNameAsync(userName);

                if (user == null)
                {
                    return RedirectWithError("Invalid user or password", redirectUrl);
                }

                //if (!user.EmailConfirmed)
                //{
                //    return RedirectWithError("User email not confirmed", redirectUrl);
                //}

                var claims = new List<Claim>();

                user = await AuthenticateUser(new UserLogin() { UserName = userName, Password = password });

                if (user != null)
                {
                    var userSession = await sessionManager.LogInLock(user);
                    if (userSession == false)
                    {
                        return RedirectWithError("User session id error.", redirectUrl);
                    }

                    // Track user session
                    try
                    {
                        //var deviceInfo = deviceInfoService.GetDeviceInfo(Request.Headers.UserAgent);
                        //var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                        //var (location, latitude, longitude) = await deviceInfoService.GetLocationFromIpAsync(ipAddress);

                        //await userSessionService.RecordLoginAsync(
                        //    user.Id, 
                        //    user.UserName, 
                        //    user.TenantId,
                        //    deviceInfo, 
                        //    ipAddress, 
                        //    Request.Headers.UserAgent,
                        //    location, 
                        //    latitude, 
                        //    longitude);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail login for session tracking issues
                        Log.Warning(ex, "Failed to record user session for {UserId}", user.Id);
                    }
                }

                if (user != null)
                {
                    claims.Add(new Claim("Bearer_Token_Expiry", DateTime.UtcNow.AddMinutes(_tokenLifeSpan.TotalMinutes).ToString(new CultureInfo("en-US"))));
                    claims.Add(new Claim("Bearer_Token", GenerateJSONWebToken(user, null)));
                    //await PermissionClaim.AddPermissionClaim(userManager, roleManager, user, redisService);

                }
                else
                {
                    return RedirectWithError("Incorrect password", redirectUrl);
                }
                //var result = await signInManager.PasswordSignInAsync(userName, password, false, false);
                if (user != null)
                    claims.Add(new Claim(nameof(ApplicationUser.TenantId), user.TenantId.ToString()));

                await signInManager.SignInWithClaimsAsync(user, false, claims);

                if (user != null)
                {
                    if (user.ChangePasswordOnLogon)
                    {
                        return Redirect($"/ChangePassword");
                    }
                    if (user?.Roles.FirstOrDefault(r => r.Name == ApplicationRole.SuperUserRole) == null)
                    {
                        var tenant = vanigamAccountingDbContext.Tenants.FirstOrDefault(t => t.Id == user.TenantId);
                        if (tenant != null && !tenant.Hosts.Split(',').Any(h => h.Contains(this.HttpContext.Request.Host.Value)))
                        {
                            await signInManager.SignOutAsync();
                            return RedirectWithError("Invalid Tenant", redirectUrl);
                        }
                    }

                    if (!string.IsNullOrEmpty(redirectUrl) && redirectUrl.StartsWith("/"))
                    {
                        redirectUrl = redirectUrl.Substring(1, redirectUrl.Length - 1);
                    }
                    var loginUser = vanigamAccountingDbContext.Users.Include(l => l.PreferredLanguage).FirstOrDefault(u => u.Id == user.Id);
                    if (loginUser != null && loginUser.PreferredLanguage != null)
                    {
                        Response.Cookies.Append(
                            CookieRequestCultureProvider.DefaultCookieName,
                            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(loginUser?.PreferredLanguage?.Code)));
                    }

                    return Redirect($"/{redirectUrl}");
                }
            }

            return RedirectWithError("Invalid user or password", redirectUrl);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Invalid password");
            }

            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await userManager.FindByIdAsync(id);

            var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            user.ChangePasswordOnLogon = false;
            await vanigamAccountingDbContext.SaveChangesAsync();
            if (result.Succeeded)
            {
                return Ok();
            }

            var message = string.Join(", ", result.Errors.Select(error => error.Description));

            return BadRequest(message);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserTenant(string userId, string tenantId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            {
                return BadRequest($"Invalid {nameof(userId)} or {nameof(tenantId)}");
            }

            try
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.TenantId = Convert.ToInt32(tenantId);
                    var loginUser = vanigamAccountingDbContext?.Users?.FirstOrDefault(u => u.Id.ToString() == userId);
                    if (loginUser != null)
                    {

                        loginUser.TenantId = Convert.ToInt32(tenantId);
                        await vanigamAccountingDbContext.SaveChangesAsync();
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }


            return BadRequest();
        }

        [HttpPost]
        public async Task<ApplicationAuthenticationState> CurrentUser()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return new ApplicationAuthenticationState
                {
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    Name = User.Identity.Name,
                    Claims = User.Claims.Select(c => new ApplicationClaim { Type = c.Type, Value = c.Value })
                };
            }
            var tokenClaimExpiry = User.Claims?.FirstOrDefault(c => c.Type == "Bearer_Token_Expiry");
            bool tokenExpired = false;
            var tenantId = int.Parse(User.Claims?.FirstOrDefault(c => c.Type == nameof(ApplicationUser.TenantId)).Value);
            if (tokenClaimExpiry != null)
            {
                if (DateTime.Parse(tokenClaimExpiry.Value.ToString(), new CultureInfo("en-US")) > DateTime.UtcNow)
                {
                    return new ApplicationAuthenticationState
                    {
                        Token = User.Claims?.FirstOrDefault(c => c.Type == "Bearer_Token")?.Value,
                        IsAuthenticated = User.Identity.IsAuthenticated,
                        Name = User.Identity.Name,
                        Claims = User.Claims.Select(c => new ApplicationClaim { Type = c.Type, Value = c.Value }),
                    };
                }
                else
                {
                    tokenExpired = true;
                }
            }
            else
            {
                tokenExpired = true;
            }
            return new ApplicationAuthenticationState
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Name = User.Identity.Name,
                TokenExpired = tokenExpired,
                Claims = User.Claims.Select(c => new ApplicationClaim { Type = c.Type, Value = c.Value }),
            };
        }

        //[HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            await HandleLogOut();
            return Redirect("~/");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Invalid user name or password.");
            }

            var user = new ApplicationUser { UserName = userName, Email = userName };

            var tenant = vanigamAccountingDbContext.Tenants.ToList().FirstOrDefault(t => t.Hosts.Split(',').Any(h => h.Contains(this.HttpContext.Request.Host.Value)));
            if (tenant != null)
            {
                userManager.UserValidators.Clear();

                if (vanigamAccountingDbContext.Users.Any(u => u.TenantId == tenant.Id && u.UserName == user.Name))
                {
                    return BadRequest("User with the same name already exist for this tenant.");
                }
            }
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                try
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);

                    var body = $@"<a href=""{callbackUrl}"">{"Please confirm your registration"}</a>";

                    await emailSender.SendConfirmationLinkAsync(user, user.Email, body);
                    //await SendEmailAsync(user.Email, "Confirm your registration", body);


                    var newUser = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.TenantId == null && u.UserName == userName);
                    if (newUser != null && tenant != null)
                    {
                        newUser.TenantId = tenant.Id;
                        vanigamAccountingDbContext.Users.Update(newUser);
                        await vanigamAccountingDbContext.SaveChangesAsync();
                    }

                    return Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }

            var message = string.Join(", ", result.Errors.Select(error => error.Description));

            return BadRequest(message);
        }

        [HttpPost]
        public async Task<IActionResult> ActivateUser(Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return BadRequest("Invalid userId");
            }

            var user = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id.ToString().ToUpper() == userId.ToString().ToUpper());
            if (user != null)
            {
                try
                {
                    vanigamAccountingDbContext.Users.Update(user);
                    await vanigamAccountingDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("");
        }
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return BadRequest("Invalid userId");
            }

            var user = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id.ToString().ToUpper() == userId.ToString().ToUpper());
            if (user != null)
            {
                try
                {
                    vanigamAccountingDbContext.Users.Update(user);
                    await vanigamAccountingDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("");
        }

        [HttpPost]
        public async Task<IActionResult> SendInvitation(Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return BadRequest("Invalid userId");
            }

            var user = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id.ToString().ToUpper() == userId.ToString().ToUpper());
            if (user != null)
            {
                try
                {
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);
                    var guid = Guid.NewGuid();
                    var result = await userManager.SetAuthenticationTokenAsync(user, ResetPasswordLoginProvider, guid.ToString(), code);
                    if (result.Succeeded)
                    {
                        var callbackUrl = $"{Request.Scheme}://{Request.Host}/resetpassword/{userId}/{guid}";
                        await emailSender.SendConfirmationLinkAsync(user, user.Email, callbackUrl);
                    }
                    return Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("");
        }
        public async Task<IActionResult> ResetPasswordById(Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return BadRequest("Invalid userId");
            }

            var user = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id.ToString().ToUpper() == userId.ToString().ToUpper());
            if (user != null)
            {
                try
                {
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);
                    var guid = Guid.NewGuid();
                    var result = await userManager.SetAuthenticationTokenAsync(user, ResetPasswordLoginProvider, guid.ToString(), code);
                    if (result.Succeeded)
                    {
                        var callbackUrl = $"{Request.Scheme}://{Request.Host}/resetpassword/{userId}/{guid}";
                        await emailSender.SendPasswordResetLinkAsync(user, user.Email, callbackUrl);
                    }
                    return Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("");
        }
        public async Task<IActionResult> GenerateApiTokenById(Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return BadRequest("Invalid userId");
            }

            var user = vanigamAccountingDbContext.Users.FirstOrDefault(u => u.Id.ToString().ToUpper() == userId.ToString().ToUpper());
            if (user != null)
            {
                try
                {
                    var loginUser = GetLoginUser(user);
                    if (loginUser != null)
                    {
                        loginUser.ApiAccountId = Guid.NewGuid();
                        loginUser.ApiAuthToken = GenerateApiToken(user);
                        vanigamAccountingDbContext.Users.Update(loginUser);
                        await vanigamAccountingDbContext.SaveChangesAsync();
                        return Ok();
                    }
                    else
                    {
                        throw new Exception($"Cant find Login User:{user}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest("");
        }
        private string GenerateApiToken(ApplicationUser user)
        {
            var loginUser = GetLoginUser(user);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>();
            claims.Add(new(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new(nameof(ApplicationUser.UserName), user.UserName));
            if (loginUser != null)
            {
                claims.Add(new(nameof(ApplicationUser.TenantId), user.TenantId.ToString()));
                claims.Add(new(nameof(Objects.Entities.ApplicationUser.FullName), loginUser?.FullName));
                claims.Add(new(nameof(LoginUserType), loginUser?.UserType?.ToString()));

            }
            var token = new JwtSecurityToken(configuration["Jwt:Issuer"],
                configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddYears(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var errors = string.Empty;
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return BadRequest("Cant find User");
                }
                //Confirm Email
                user.EmailConfirmed = true;
                var token = await userManager.GetAuthenticationTokenAsync(user, ResetPasswordLoginProvider, model.Token);
                var result = await userManager.ResetPasswordAsync(user, token, model.Password);
                await vanigamAccountingDbContext.SaveChangesAsync();
                if (result.Succeeded)
                {
                    return Ok("Password has been reset successfully.");
                }

                var message = string.Join(", ", result.Errors.Select(error => error.Description));

                return BadRequest(message);
            }
            return BadRequest(string.Join(", ", ModelState.Values.Where(v => v.ValidationState != ModelValidationState.Valid).Select(error => error.Errors.Select(error => error.ErrorMessage))));
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await userManager.FindByIdAsync(userId);

            var result = await userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                var password = GenerateRandomPassword();
                //code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //result = await _userManager.ResetPasswordAsync(user, code, password);

                if (result.Succeeded)
                {
                    var callbackUrl = Url.Action("ConfirmPasswordReset", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);
                    await SendEmailAsync(user.Email, "New password", $"<p>Your new password is: {password}</p><p>Please change it after login.</p>");

                    return Redirect("~/Login?info=Password reset successful. You will receive an email with your new password.");
                }
                return Redirect("~/Login?info=Your registration has been confirmed. You will receive an email with your new password.");
            }

            return RedirectWithError("Invalid user or confirmation code");
        }
        [HttpPost]

        //[HttpPost]
        //public async Task<IActionResult> ResetPassword(string userName)
        //{
        //    var user = await _userManager.FindByNameAsync(userName);

        //    if (user == null)
        //    {
        //        return BadRequest("Invalid user name.");
        //    }

        //    try
        //    {
        //        var code = await _userManager.GeneratePasswordResetTokenAsync(user);

        //        var callbackUrl = Url.Action("ConfirmPasswordReset", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);

        //        var body = string.Format(@"<a href=""{0}"">{1}</a>", callbackUrl, "Please confirm your password reset.");
        //        await _emailSender.SendPasswordResetLinkAsync(user, user.Email, callbackUrl);
        //        //await SendEmailAsync(user.Email, "Confirm your password reset", body);

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> ConfirmPasswordReset(string userId, string code)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Redirect("~/Login?error=Invalid user");
            }

            var password = GenerateRandomPassword();

            var result = await userManager.ResetPasswordAsync(user, code, password);

            if (result.Succeeded)
            {
                await SendEmailAsync(user.Email, "New password", $"<p>Your new password is: {password}</p><p>Please change it after login.</p>");

                return Redirect("~/Login?info=Password reset successful. You will receive an email with your new password.");
            }

            return Redirect("~/Login?error=Invalid user or confirmation code");
        }

        private static string GenerateRandomPassword(PasswordOptions options = null)
        {
            if (options == null)
            {
                options = new PasswordOptions
                {
                    RequiredLength = 8,
                    RequiredUniqueChars = 4,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true
                };
            }


            var randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };

            var rand = new Random(Environment.TickCount);
            var chars = new List<char>();

            if (options.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (options.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (options.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (options.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            for (int i = chars.Count; i < options.RequiredLength || chars.Distinct().Count() < options.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.From = new System.Net.Mail.MailAddress(configuration.GetValue<string>("Smtp:User"));
            mailMessage.Body = body;
            mailMessage.Subject = subject;
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            mailMessage.To.Add(to);

            var client = new System.Net.Mail.SmtpClient(configuration.GetValue<string>("Smtp:Host"))
            {
                UseDefaultCredentials = false,
                EnableSsl = configuration.GetValue<bool>("Smtp:Ssl"),
                Port = configuration.GetValue<int>("Smtp:Port"),
                Credentials = new System.Net.NetworkCredential(configuration.GetValue<string>("Smtp:User"), configuration.GetValue<string>("Smtp:Password"))
            };

            await client.SendMailAsync(mailMessage);
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> SendOTP(string phoneNumber)
        //{
        //    phoneNumber = phoneNumber.FormatPhoneNumber();
        //    var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        //    if (user == null)
        //    {
        //        return BadRequest("User with this phone number does not exist.");
        //    }

        //    // Generate OTP
        //    var otp = new Random().Next(100000, 999999).ToString(); // 6-digit OTP

        //    // Save OTP to the history table
        //    var otpEntry = new ApplicationUserOtp()
        //    {
        //        UserId = user.Id,
        //        OTP = otp,
        //        GeneratedAt = DateTime.UtcNow,
        //        ExpiredAt = DateTime.UtcNow.AddMinutes(5),
        //        IsUsed = false
        //    };
        //    emrDbContext.ApplicationUserOtpS.Add(otpEntry);
        //    await emrDbContext.SaveChangesAsync();

        //    // Send OTP via SMS//todo
        //    //await SmsService.SendSmsAsync(phoneNumber, otpEntry);
        //    return Ok(otpEntry);
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> VerifyOTP(string phoneNumber, string otp)
        //{
        //    phoneNumber = phoneNumber.FormatPhoneNumber();
        //    var user = await userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        //    if (user == null)
        //    {
        //        return BadRequest("User with this phone number does not exist.");
        //    }

        //    // Find the OTP in history
        //    var otpEntry = await emrDbContext.ApplicationUserOtpS
        //        .Where(o => o.UserId == user.Id && o.OTP == otp && !o.IsUsed && o.ExpiredAt > DateTime.UtcNow)
        //        .FirstOrDefaultAsync();

        //    if (otpEntry == null)
        //    {
        //        return BadRequest("Invalid or expired OTP.");
        //    }

        //    // Mark OTP as used
        //    //otpEntry.IsUsed = true;
        //    var loginUser = GetLoginUser(user);
        //    var token = GenerateJSONWebToken(user, 43200, loginUser);
        //    await PermissionClaim.AddPermissionClaim(userManager, roleManager, user, redisService);
        //    var patient = emrDbContext?.Patients?.FirstOrDefault(p => p.Oid == Guid.Parse(user.Id));
        //    if (patient != null)
        //    {
        //        var response = Ok(new
        //        {
        //            Token = token,
        //            Name = patient.FullName,
        //            PatientInfo = new {PatientId= patient.Oid, First = patient.FirstName, Last = patient.LastName, Photo = patient.Photo, AddressLine1 = patient.AddressLine1, AddressLine2 = patient.AddressLine2 },
        //            UserId = user.Id,
        //            Message = "Login successful",
        //            UserType = loginUser != null ? loginUser?.Type.ToString() : LoginUserType.Patient.ToString(),
        //            Roles = user.Roles.Select(r => r.Name)
        //        });
        //        await emrDbContext.SaveChangesAsync();
        //        return response;
        //    }
        //    return BadRequest("Cant find Patient");

        //}
    }
}
