using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class ConsentService : IConsentService
    {
        // Ab hum DbContext nahi, balki Repository use kar rahe hain
        private readonly IConsentRepository _repository;

        // Constructor mein repository inject ki
        public ConsentService(IConsentRepository repository)
        {
            _repository = repository;
        }

        // Consent dene ka logic
        public async Task<ConsentRecords> GrantConsentAsync(GrantConsentDto request, Guid organizationId)
        {
            // Naya ConsentRecord model object banaya
            var consent = new ConsentRecords                
            {
                DataPrincipalId = request.DataPrincipalId, // DTO se aayi User Id
                DataInventoryId = request.DataInventoryId, // DTO se aayi Data Id
                OrganizationId = organizationId,           // Token se aayi Org Id
                ConsentStatus = "Granted",                 // Default status 'Granted'
                NoticeVersion = request.NoticeVersion,     // DTO se aaya Notice version
                GrantedAt = DateTime.UtcNow                // Current date/time
            };

            // Repository ko bola data add karo
            await _repository.AddAsync(consent);
            // Repository ko bola SQL mein save karo
            await _repository.SaveChangesAsync();

            // Save hone ke baad data wapas bhej diya
            return consent;
        }

        // Consent wapas lene ka logic
        public async Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId)
        {
            // Repository ko bol kar purana record DB se mangwaya
            var consent = await _repository.GetByIdAsync(consentRecordId, organizationId);

            // Agar record nahi mila toh false return karo
            if (consent == null) return false;

            // Agar mil gaya toh uska status change kar do
            consent.ConsentStatus = "Withdrawn";
            // Withdraw karne ka date/time daal diya
            consent.WithdrawnAt = DateTime.UtcNow;

            // Repository ko bola is record ko update kar do
            _repository.Update(consent);
            // Repository ko bola SQL mein save karo
            await _repository.SaveChangesAsync();

            // Success return kar diya
            return true;
        }

        // User ki history nikalne ka logic
        public async Task<IEnumerable<ConsentRecords>> GetHistoryAsync(string dataPrincipalId, Guid organizationId)
        {
            // Seedha repository se history maangi aur wapas kar di
            return await _repository.GetHistoryAsync(dataPrincipalId, organizationId);
        }
    }
}