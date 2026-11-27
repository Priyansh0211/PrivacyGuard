using System;
using System.Linq;

namespace PrivacyGuard.Helpers
{
    public static class RoleGuard
    {
        private static readonly string[] PrivilegedRoles = { "SuperAdmin", "Admin", "SystemAdmin", "DPO" };
        private static readonly string[] SuperRoles = { "SuperAdmin", "SystemAdmin" };
        private const string DataPrincipalRoleName = "DataPrincipal";

        // Admin level roles (inhe assign/edit/deactivate karna restricted hai)
        public static bool IsPrivilegedRole(string? roleName) =>
            !string.IsNullOrWhiteSpace(roleName) &&
            PrivilegedRoles.Any(r => r.Equals(roleName.Trim(), StringComparison.OrdinalIgnoreCase));

        // Sirf ye roles privileged roles assign/edit kar sakte hain
        public static bool IsSuperRole(string? roleName) =>
            !string.IsNullOrWhiteSpace(roleName) &&
            SuperRoles.Any(r => r.Equals(roleName.Trim(), StringComparison.OrdinalIgnoreCase));

        // System role jo kisi ke liye create/rename nahi ho sakta
        public static bool IsDataPrincipalRole(string? roleName) =>
            string.Equals(roleName?.Trim(), DataPrincipalRoleName, StringComparison.OrdinalIgnoreCase);

        // Branch-scoped = har non-admin user. Unit assigned nahi hai to kuch bhi access nahi (deny by default).
        public static bool IsBranchScoped(string? roleName, Guid orgUnitId) =>
            !IsPrivilegedRole(roleName);
    }
}