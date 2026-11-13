using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Linq;
using System.Security.Claims;

namespace PrivacyGuard.Filters
{
    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permission;
        private readonly IRoleService _roleService;

        public PermissionFilter(string permission, IRoleService roleService)
        {
            _permission = permission;
            _roleService = roleService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Permissions are already issued in the JWT at login time. This is
            // important for DataPrincipals because NameIdentifier is their
            // DataPrincipalId, not a Users.UserId.
            var tokenPermissions = context.HttpContext.User
                .FindAll("Permission")
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (tokenPermissions.Contains(_permission))
                return;

            // Backward-compatible fallback for older internal-user tokens.
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Result = new UnauthorizedResult(); // 401 Unauthorized
                return;
            }

            var userId = Guid.Parse(userIdClaim);

            // 2. हमारी पुरानी Service से यूज़र की सारी Permissions लाना
            var userPermissions = await _roleService.GetUserPermissionsAsync(userId);

            // 3. चेक करना कि क्या इस यूज़र की लिस्ट में मांगी गई Permission है?
            if (userPermissions == null || !userPermissions.Contains(_permission))
            {
                context.Result = new ForbidResult(); // 403 Forbidden (एक्सेस मना है)
                return;
            }
        }
    }
}