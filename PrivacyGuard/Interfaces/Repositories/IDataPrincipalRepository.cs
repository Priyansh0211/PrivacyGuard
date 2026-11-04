using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IDataPrincipalRepository
    {
        Task<DataPrincipal> AddDataPrincipalAsync(DataPrincipal dataPrincipal);
        Task SaveChangesAsync();

        Task<DataPrincipal?> Get360ViewEntityAsync(Guid dataPrincipalId, Guid organizationId);
    }
}