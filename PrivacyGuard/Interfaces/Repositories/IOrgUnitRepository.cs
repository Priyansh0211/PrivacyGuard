using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IOrgUnitRepository
    {
        Task<OrgUnit> AddAsync(OrgUnit orgUnit);
        Task<OrgUnit?> GetByIdAsync(Guid id, Guid organizationId);
        Task<IEnumerable<OrgUnit>> GetAllByOrgIdAsync(Guid organizationId);
        void Update(OrgUnit orgUnit);
        Task<int> SaveChangesAsync();

        Task<bool> IsUnitCodeUniqueAsync(string unitCode, Guid organizationId, Guid? excludeId = null);
        Task<int> GetPrincipalsCountAsync(Guid orgUnitId);
        Task<bool> HasChildrenAsync(Guid orgUnitId);
        Task<bool> HasAssignedUsersAsync(Guid orgUnitId);
    }
}