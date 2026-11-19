using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IDataPrincipalRepository
    {
        Task<DataPrincipal> AddDataPrincipalAsync(DataPrincipal dataPrincipal);
        Task SaveChangesAsync();

        Task<DataPrincipal?> Get360ViewEntityAsync(Guid dataPrincipalId, Guid organizationId);
        Task<(List<DataPrincipal> Principals, int TotalCount)> GetPaginatedDataPrincipalsAsync(
        Guid organizationId, string search, string classType, int page, int pageSize);
        Task<int> GetTotalPrincipalsCountAsync(Guid organizationId, Guid? orgUnitId = null);
        Task AddBulkDataPrincipalsAsync(List<DataPrincipal> dataPrincipals);
    }
}