using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IPrivacyRequestRepository
    {
        // Nayi request DB mein add karne ke liye
        Task<PrivacyRequest> AddAsync(PrivacyRequest request);

        // Single request fetch karne ke liye (Resolution ke time kaam aayega)
        Task<PrivacyRequest?> GetByIdAsync(Guid requestId, Guid organizationId);

        // User ki saari requests fetch karne ke liye
        Task<IEnumerable<PrivacyRequest>> GetAllByUserAsync(string dataPrincipalId, Guid organizationId);

        // Request ko update karne ke liye (Status change karte waqt)
        void Update(PrivacyRequest request);

        // Changes SQL mein save karne ke liye
        Task<int> SaveChangesAsync();
    }
}