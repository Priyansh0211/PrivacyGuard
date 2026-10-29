using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    // Interface declare kar rahe hain repository ke liye
    public interface IConsentRepository
    {
        Task<IEnumerable<ConsentPurpose>> GetPurposesByOrgAsync(Guid organizationId);
        Task<ConsentRecord?> GetLatestConsentByPrincipalIdAsync(Guid dataPrincipalId, Guid organizationId);
        // Naya consent record database me add karne ke liye
        Task<ConsentRecord> AddAsync(ConsentRecord consent);

        // Kisi specific consent record ko uski ID aur OrganizationId se dhundhne ke liye
        Task<ConsentRecord?> GetByIdAsync(Guid consentRecordId, Guid organizationId);

        // Kisi existing record ko update karne ke liye (jaise withdraw karte waqt)
        void Update(ConsentRecord consent);

        // Kisi ek user (Data Principal) ki saari consent history nikalne ke liye
        Task<IEnumerable<ConsentRecord>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId);

        // Database mein actual changes save karne ke liye
        Task<int> SaveChangesAsync();

        Task AddPurposeAsync(ConsentPurpose purpose);
    }
}