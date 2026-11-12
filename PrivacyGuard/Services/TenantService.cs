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

            
            // 🚨 Nayi company ban rahi hai, toh Default SuperAdmin Role set karo
            var dpoRole = new Role
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                Name = "SuperAdmin", 
                Description = "(Full Access)",
                CreatedAt = currentTime
            };

            var newAdmin = new User
            {
                OrganizationId = orgId,
                FullName = request.AdminFullName,
                ReferenceId = PrivacyHelper.GenerateUserReferenceId(),
                Email = request.AdminEmail,
                PhoneNumber = request.AdminPhoneNumber,
                PasswordHash = request.AdminPassword, 


                
                RoleId = dpoRole.Id,
                UserRole = dpoRole,


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

            Guid organizationId;
            string email;
            string roleName;
            string userType;
            User? internalUser = null;
            DataPrincipal? dataPrincipal = null;

            var isLegacyDataPrincipalUser =
                user?.UserRole?.Name.Equals("DataPrincipal", StringComparison.OrdinalIgnoreCase) == true;

            if (user != null && !isLegacyDataPrincipalUser && user.PasswordHash == request.Password)
            {
                if (!user.IsActive) return (false, string.Empty, "Account is disabled");

                internalUser = user;
                organizationId = user.OrganizationId;
                email = user.Email;
                roleName = user.UserRole?.Name ?? "NoRoleAssigned";
                userType = "InternalUser";
            }
            else
            {
                dataPrincipal = await _tenantRepository.GetDataPrincipalByEmailAsync(request.Email);

                if (dataPrincipal == null || dataPrincipal.PasswordHash != request.Password)
                    return (false, string.Empty, "Invalid email or password");

                organizationId = dataPrincipal.OrganizationId;
                email = dataPrincipal.Email!;
                roleName = dataPrincipal.Role?.Name ?? "DataPrincipal";
                userType = "DataPrincipal";
            }

            var identityId = internalUser?.UserId ?? dataPrincipal!.DataPrincipalId;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("UserType", userType),
                new Claim("OrganizationId", organizationId.ToString())
            };

            

            // Teammate ka feature: Permissions add karna
            var permissionsRoleId = internalUser?.RoleId ?? dataPrincipal?.RoleId;
            var userPermissions = await _tenantRepository.GetPermissionsByRoleIdAsync(permissionsRoleId);
            if (userPermissions != null && userPermissions.Any())
            {
                foreach (var systemName in userPermissions)
                {
                    claims.Add(new Claim("Permission", systemName));
                }
            }

            // Teammate ka feature: OrgUnitId check
            if (internalUser?.OrgUnitId.HasValue == true && internalUser.OrgUnitId.Value != Guid.Empty)
            {
                claims.Add(new Claim("OrgUnitId", internalUser.OrgUnitId.Value.ToString()));
            }

            // 2. Ek hi baar Security Key banayenge null check ke sath
            var securityKey = _configuration["JwtSettings:SecurityKey"];
            if (string.IsNullOrEmpty(securityKey)) return (false, string.Empty, "Server Configuration Error: JWT Key is missing.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            double expiryMinutes = 60;
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

            if (internalUser != null)
            {
                internalUser.LastLoginDate = DateTime.UtcNow;
                internalUser.LastLoginIP = loginIp;
                await _tenantRepository.UpdateUserAsync(internalUser);
            }

            return (true, tokenString, string.Empty);
        }

        public async Task<List<RoleDropdownDto>> GetRolesForDropdownAsync(Guid organizationId)
        {
            return await _tenantRepository.GetRolesForDropdownAsync(organizationId);
        }
    }
}