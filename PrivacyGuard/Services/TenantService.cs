using Microsoft.IdentityModel.Tokens;
using PrivacyGuard.DTOs;
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

        public TenantService(ITenantRepository tenantRepository , IConfiguration configuration)
        {
            _tenantRepository = tenantRepository;
            _configuration = configuration;
        }

        public async Task<(bool IsSuccess, Guid? OrganizationId, Guid? AdminId, string ErrorMessage)> RegisterTenantAsync(RegisterTenantDto request)
        {
            var currentTime = DateTime.UtcNow;

            var newOrg = new Organization
            {
                Name = request.OrganizationName,
                IsActive = true,
                CreatedAt = currentTime,
                UpdatedAt = currentTime
            };

            var newAdmin = new User
            {
                
                FullName = request.AdminFullName,
                Email = request.AdminEmail,
                PhoneNumber = request.AdminPhoneNumber,
                PasswordHash = request.AdminPassword, // Future mein BCrypt se hash karna hoga

               
                Role = "Admin",
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


        public async Task<(bool IsSuccess, string Token, string ErrorMessage)> LoginAsync( LoginDto request,string? loginIp)
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

            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("OrganizationId", user.OrganizationId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]));

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
