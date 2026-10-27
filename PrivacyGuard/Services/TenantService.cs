using Microsoft.IdentityModel.Tokens;
using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PrivacyGuard.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IConfiguration _configuration;

        public TenantService(ITenantRepository tenantRepository, IConfiguration configuration)
        {
            _tenantRepository = tenantRepository;
            _configuration = configuration;
        }

        public async Task<(bool IsSuccess, Guid? OrganizationId, Guid? AdminId, string ErrorMessage)> RegisterTenantAsync(RegisterTenantDto request)
        {
            var currentTime = DateTime.UtcNow;

            var orgId = Guid.NewGuid();

            var newOrg = new Organization
            {
                OrganizationId = orgId,
                Name = request.OrganizationName,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };

            var adminRole = new Role
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                Name = "Admin",
                Description = "System Administrator (Maker)",
                CreatedAt = currentTime
            };

            var newAdmin = new User
            {
                OrganizationId = orgId,
                FullName = request.AdminFullName,
                ReferenceId = PrivacyHelper.GenerateUserReferenceId(),
                Email = request.AdminEmail,
                PhoneNumber = request.AdminPhoneNumber,
                PasswordHash = request.AdminPassword, // Future mein BCrypt se hash karna hoga

                RoleId = adminRole.Id,
                UserRole = adminRole,

                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = currentTime,
                LastLoginDate = null,
                LastLoginIP = null
            };

            try
            {
                var result = await _tenantRepository.CreateTenantWithAdminAsync(newOrg, newAdmin);
                return (true, result.org.OrganizationId, result.admin.UserId, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, null, "Database error: " + ex.Message);
            }
        }

        public async Task<(bool IsSuccess, string Token, string ErrorMessage)> LoginAsync(LoginDto request, string? loginIp)
        {
            var user = await _tenantRepository.GetUserByEmailAsync(request.Email);

            if (user == null || user.PasswordHash != request.Password)
            {
                return (false, string.Empty, "Invalid email or password");
            }

            if (!user.IsActive)
            {
                return (false, string.Empty, "Account is disabled");
            }

            var roleName = user.UserRole != null ? user.UserRole.Name : "NoRoleAssigned";

            // 1. Ek hi baar claims list banayenge
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("OrganizationId", user.OrganizationId.ToString())
            };

            // Teammate ka feature: Permissions add karna
            var userPermissions = await _tenantRepository.GetPermissionsByRoleIdAsync(user.RoleId);
            if (userPermissions != null && userPermissions.Any())
            {
                foreach (var systemName in userPermissions)
                {
                    claims.Add(new Claim("Permission", systemName));
                }
            }

            // Teammate ka feature: OrgUnitId check
            if (user.OrgUnitId.HasValue && user.OrgUnitId.Value != Guid.Empty)
            {
                claims.Add(new Claim("OrgUnitId", user.OrgUnitId.Value.ToString()));
            }

            // 2. Ek hi baar Security Key banayenge null check ke sath
            var securityKey = _configuration["JwtSettings:SecurityKey"];
            if (string.IsNullOrEmpty(securityKey))
            {
                return (false, string.Empty, "Server Configuration Error: JWT Key is missing.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Expiry handle ki gayi
            double expiryMinutes = 60; // default value
            if (double.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out double parsedExpiry))
            {
                expiryMinutes = parsedExpiry;
            }
            var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            user.LastLoginDate = DateTime.UtcNow;
            user.LastLoginIP = loginIp;

            await _tenantRepository.UpdateUserAsync(user);

            return (true, tokenString, string.Empty);
        }
    }
}