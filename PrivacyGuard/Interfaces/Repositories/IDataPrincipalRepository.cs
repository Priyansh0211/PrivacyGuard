using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IDataPrincipalRepository
    {
        Task<DataPrincipal> AddDataPrincipalAsync(DataPrincipal dataPrincipal);
        Task SaveChangesAsync();
    }
}