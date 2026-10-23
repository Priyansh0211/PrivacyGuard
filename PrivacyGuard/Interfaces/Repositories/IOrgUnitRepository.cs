using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IOrgUnitRepository
    {
        Task<OrgUnit> AddAsync(OrgUnit orgUnit);
        Task<OrgUnit?> GetByIdAsync(Guid id, Guid organizationId); // Updated
        Task<IEnumerable<OrgUnit>> GetAllByOrgIdAsync(Guid organizationId); // Updated
        void Update(OrgUnit orgUnit);
        Task<int> SaveChangesAsync();
    }
}