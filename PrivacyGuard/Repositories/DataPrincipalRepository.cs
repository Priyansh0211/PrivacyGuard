using System.Threading.Tasks;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class DataPrincipalRepository : IDataPrincipalRepository
    {
        private readonly ApplicationDbContext _context;

        public DataPrincipalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataPrincipal> AddDataPrincipalAsync(DataPrincipal dataPrincipal)
        {
            await _context.DataPrincipals.AddAsync(dataPrincipal);
            return dataPrincipal;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}