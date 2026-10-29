using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

        public async Task<IEnumerable<ConsentPurpose>> GetPurposesAsync(Guid organizationId)
        {
            return await _repository.GetPurposesByOrgAsync(organizationId);
        }

        // Helper Method: SHA-256 Hash generate karne ke liye (PDF Req)[cite: 3]
        private string GenerateHash(object payload)
        {
            string jsonPayload = JsonSerializer.Serialize(payload);
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonPayload));
                var builder = new StringBuilder();
                foreach (var b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public async Task<(bool IsSuccess, string Message, string ReceiptId)> RecordConsentAsync(CreateConsentRequestDto request, Guid organizationId)
        {
            var activePurposes = await _repository.GetPurposesByOrgAsync(organizationId);
            if (!activePurposes.Any()) return (false, "No purposes found.", string.Empty);

            var record = new ConsentRecord
            {
                ConsentRecordId = Guid.NewGuid(),
                OrganizationId = organizationId,
                DataPrincipalId = request.DataPrincipalId,
                Channel = request.Channel,
                Language = request.Language,
                ReferenceNumber = $"CNS-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}", // CNS Generator[cite: 2]
                ReceiptId = $"RCP-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}",     // RCP Generator[cite: 3]
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };

            bool hasYes = false, hasNo = false;

            foreach (var purpose in activePurposes)
            {
                var userDecision = request.Decisions.FirstOrDefault(d => d.ConsentPurposeId == purpose.ConsentPurposeId);
                string decision = userDecision?.Decision ?? "No";

                if (purpose.IsMandatory) decision = "Yes"; // Mandatory Force[cite: 4]

                if (decision == "Yes") hasYes = true;
                if (decision == "No") hasNo = true;

                record.ConsentDetails.Add(new ConsentDetail
                {
                    ConsentDetailId = Guid.NewGuid(),
                    ConsentRecordId = record.ConsentRecordId,
                    ConsentPurposeId = purpose.ConsentPurposeId,
                    Decision = decision,
                    Status = "Active",
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                });
            }

            // PDF Fix: Overall Status Logic[cite: 2]
            if (hasYes && hasNo) record.OverallStatus = ConsentStatus.PartiallyGranted;
            else if (hasYes) record.OverallStatus = ConsentStatus.Granted;
            else record.OverallStatus = ConsentStatus.Withdrawn;

            // PDF Fix: Hash Generation[cite: 3]
            var payload = new { record.ReceiptId, record.DataPrincipalId, Timestamp = record.CreatedAt };
            record.ArtifactHash = GenerateHash(payload);
            record.KeyId = "ED25519-KEY-001";
            record.DigitalSignature = "SIGNED_DEMO";

            await _repository.AddAsync(record);
            await _repository.SaveChangesAsync();

            return (true, "Success", record.ReceiptId);
        }

        // PDF Fix: Versioned Withdrawal[cite: 4]
        public async Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId)
        {
            var oldRecord = await _repository.GetByIdAsync(consentRecordId, organizationId);
            if (oldRecord == null) return false;

            // Withdrawal pe naya record ban raha hai (Versioning)
            var newRecord = new ConsentRecord
            {
                ConsentRecordId = Guid.NewGuid(),
                OrganizationId = organizationId,
                DataPrincipalId = oldRecord.DataPrincipalId,
                Channel = oldRecord.Channel,
                Language = oldRecord.Language,
                ReferenceNumber = oldRecord.ReferenceNumber,
                ReceiptId = $"RCP-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}",
                PreviousRecordId = oldRecord.ConsentRecordId, // Chain link[cite: 3]
                Version = oldRecord.Version + 1, // Version++[cite: 4]
                OverallStatus = ConsentStatus.Withdrawn,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var detail in oldRecord.ConsentDetails)
            {
                newRecord.ConsentDetails.Add(new ConsentDetail
                {
                    ConsentDetailId = Guid.NewGuid(),
                    ConsentRecordId = newRecord.ConsentRecordId,
                    ConsentPurposeId = detail.ConsentPurposeId,
                    Decision = "No",
                    Status = "Withdrawn"
                });
            }

            var payload = new { newRecord.ReceiptId, newRecord.DataPrincipalId, Action = "Withdrawal" };
            newRecord.ArtifactHash = GenerateHash(payload);

            await _repository.AddAsync(newRecord);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<object?> GetLatestConsentAsync(Guid dataPrincipalId, Guid organizationId)
        {
            var consent = await _repository.GetLatestConsentByPrincipalIdAsync(dataPrincipalId, organizationId);
            if (consent == null) return null;

            return new
            {
                consent.ConsentRecordId,
                consent.ReceiptId,
                consent.ReferenceNumber,
                Status = consent.OverallStatus.ToString(),
                consent.ArtifactHash,
                consent.Version,
                Decisions = consent.ConsentDetails.Select(d => new { d.ConsentPurpose.ShortName, d.Decision })
            };
        }

        public async Task<IEnumerable<object>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId)
        {
            var history = await _repository.GetHistoryAsync(dataPrincipalId, organizationId);
            return history.Select(c => new
            {
                c.Version,
                c.CreatedAt,
                Status = c.OverallStatus.ToString(),
                c.ArtifactHash,
                c.ReceiptId,
                Decisions = c.ConsentDetails.Select(d => new { d.ConsentPurpose.ShortName, d.Decision })
            });
        }
        public async Task<ConsentPurpose> CreatePurposeAsync(string code, string shortName, bool isMandatory, Guid organizationId)
        {
            var purpose = new ConsentPurpose
            {
                ConsentPurposeId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Code = code,
                ShortName = shortName,
                IsMandatory = isMandatory,
                LegalBasis = isMandatory ? "Legitimate Use" : "Consent"
            };
            await _repository.AddPurposeAsync(purpose);
            return purpose;
        }
    }
}