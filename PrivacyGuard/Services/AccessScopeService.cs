using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class AccessScopeService : IAccessScopeService
    {
        private readonly ApplicationDbContext _context;

        public AccessScopeService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<HashSet<Guid>?> GetAllowedUnitIdsAsync(Guid organizationId, string? callerRole, Guid callerUnitId)
        {
            if (!RoleGuard.IsBranchScoped(callerRole, callerUnitId))
                return null; // org-wide

            var units = await _context.OrgUnits
                .AsNoTracking()
                .Where(o => o.OrganizationId == organizationId)
                .Select(o => new { o.Id, o.ParentId })
                .ToListAsync();

            var result = new HashSet<Guid>();

            // Caller ki unit isi org me nahi mili to kuch bhi allowed nahi
            if (!units.Any(u => u.Id == callerUnitId))
                return result;

            var childrenMap = units
                .Where(u => u.ParentId.HasValue)
                .GroupBy(u => u.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var stack = new Stack<Guid>();
            stack.Push(callerUnitId);

            while (stack.Count > 0)
            {
                var id = stack.Pop();
                if (!result.Add(id)) continue; // cycle guard

                if (childrenMap.TryGetValue(id, out var kids))
                {
                    foreach (var kid in kids) stack.Push(kid);
                }
            }

            return result;
        }
        public async Task<bool> CanAssignRoleAsync(Guid callerUserId, string? callerRole, Guid roleId)
        {
            if (RoleGuard.IsSuperRole(callerRole))
                return true;

            var rolePermissionIds = _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId);

            var callerPermissionIds =
                from u in _context.Users
                where u.UserId == callerUserId
                join rp in _context.RolePermissions on u.RoleId equals rp.RoleId
                select rp.PermissionId;

            // Role me aisi koi permission nahi honi chahiye jo caller ke paas nahi hai
            return !await rolePermissionIds.Except(callerPermissionIds).AnyAsync();
        }
    }
}