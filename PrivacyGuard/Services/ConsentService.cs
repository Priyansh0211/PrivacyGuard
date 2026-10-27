using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class ConsentService : IConsentService
    {
        private readonly IConsentRepository _repository;

        public ConsentService(IConsentRepository repository)
        {
            _repository = repository;
        }

        // NAYA LOGIC: Ek sath master record aur granular details (checkboxes) save karna
        public async Task<ConsentRecord> SubmitGranularConsentAsync(SubmitConsentDto request, Guid organizationId)
        {
            // 1. Master Record (Parent) banaya jo form submission track karega
            var consentMaster = new ConsentRecord
            {
                DataPrincipalId = request.DataPrincipalId.ToString(), // DTO se aayi User Id
                OrganizationId = organizationId,           // Token se aayi Org Id
                NoticeVersion = "1.0",                     // Default notice version
                SubmittedAt = DateTime.UtcNow              // Form kab submit hua
            };

            // 2. Har Checkbox (Child) ka loop lagakar master mein add kiya
            foreach (var pref in request.Preferences)
            {
                consentMaster.ConsentDetails.Add(new ConsentDetail
                {
                    DataInventoryId = pref.DataInventoryId, // DTO se specific purpose ki ID
                    ConsentStatus = pref.Status             // "Granted" ya "Denied"
                });
            }

            // 3. Repository ko bola Master (with saari Details) DB mein save karo
            await _repository.AddAsync(consentMaster);
            await _repository.SaveChangesAsync();

            return consentMaster;
        }

        // NAYA LOGIC: Granular consent (kisi ek specific purpose) ko wapas lena
        public async Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid dataInventoryId, Guid organizationId)
        {
            // Repository se master record mangwaya (Dhyan rahe, Repo mein .Include() laga hona chahiye)
            var consentMaster = await _repository.GetByIdAsync(consentRecordId, organizationId);

            if (consentMaster == null) return false;

            // Master ke andar se wo specific checkbox dhoondo jise withdraw karna hai
            var detailToWithdraw = consentMaster.ConsentDetails
                .FirstOrDefault(d => d.DataInventoryId == dataInventoryId);

            // Agar wo pehle se Granted tha, toh usko Withdrawn mark kar do
            if (detailToWithdraw != null && detailToWithdraw.ConsentStatus == "Granted")
            {
                detailToWithdraw.ConsentStatus = "Withdrawn";
                detailToWithdraw.WithdrawnAt = DateTime.UtcNow;

                _repository.Update(consentMaster);
                await _repository.SaveChangesAsync();

                return true;
            }

            return false; // Agar record nahi mila ya pehle se Denied tha
        }

        // User ki history nikalne ka logic (Same rahega)
        public async Task<IEnumerable<ConsentRecord>> GetHistoryAsync(string dataPrincipalId, Guid organizationId)
        {
            return await _repository.GetHistoryAsync(dataPrincipalId, organizationId);
        }
    }
}