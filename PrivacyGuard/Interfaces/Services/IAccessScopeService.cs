namespace PrivacyGuard.Interfaces.Services
{
    public interface IAccessScopeService
    {
        // null = org-wide (koi restriction nahi), warna allowed unit IDs (apni unit + child units)
        Task<HashSet<Guid>?> GetAllowedUnitIdsAsync(Guid organizationId, string? callerRole, Guid callerUnitId);

        // Role ki saari permissions caller ke paas hain? (SuperAdmin/SystemAdmin ke liye hamesha true)
        Task<bool> CanAssignRoleAsync(Guid callerUserId, string? callerRole, Guid roleId);
    }
}