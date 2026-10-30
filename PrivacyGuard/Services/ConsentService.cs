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

        // ==========================================
        // UI Filters aur Grid Data
        // ==========================================
        public async Task<IEnumerable<object>> GetFilteredRecordsAsync(Guid organizationId, string? reference, string? status, string? purpose, string? channel, DateTime? fromDate, DateTime? toDate)
        {
            var records = await _repository.GetFilteredRecordsAsync(organizationId, reference, status, purpose, channel, fromDate, toDate);
            return records.Select(c => MapToConsentArtifact(c));
        }

        public async Task<IEnumerable<ConsentPurpose>> GetPurposesAsync(Guid organizationId)
        {
            return await _repository.GetPurposesByOrgAsync(organizationId);
        }

        // Helper Method: SHA-256 Hash generate karne ke liye
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
                ReferenceNumber = $"CNS-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}",
                ReceiptId = $"RCP-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}",
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };

            bool hasYes = false, hasNo = false;

            foreach (var purpose in activePurposes)
            {
                var userDecision = request.Decisions.FirstOrDefault(d => d.ConsentPurposeId == purpose.ConsentPurposeId);
                string decision = userDecision?.Decision ?? "No";

                if (purpose.IsMandatory) decision = "Yes";

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

            if (hasYes && hasNo) record.OverallStatus = ConsentStatus.PartiallyGranted;
            else if (hasYes) record.OverallStatus = ConsentStatus.Granted;
            else record.OverallStatus = ConsentStatus.Withdrawn;

            var payload = new { record.ReceiptId, record.DataPrincipalId, Timestamp = record.CreatedAt };
            record.ArtifactHash = GenerateHash(payload);
            record.KeyId = "ED25519-KEY-001";
            record.DigitalSignature = "SIGNED_DEMO";

            await _repository.AddAsync(record);
            await _repository.SaveChangesAsync();

            return (true, "Success", record.ReceiptId);
        }

        public async Task<bool> WithdrawConsentAsync(Guid consentRecordId, Guid organizationId)
        {
            var oldRecord = await _repository.GetByIdAsync(consentRecordId, organizationId);
            if (oldRecord == null) return false;

            var newRecord = new ConsentRecord
            {
                ConsentRecordId = Guid.NewGuid(),
                OrganizationId = organizationId,
                DataPrincipalId = oldRecord.DataPrincipalId,
                Channel = oldRecord.Channel,
                Language = oldRecord.Language,
                ReferenceNumber = oldRecord.ReferenceNumber,
                ReceiptId = $"RCP-{DateTime.UtcNow.Year}-{new Random().Next(100000, 999999)}",
                PreviousRecordId = oldRecord.ConsentRecordId,
                Version = oldRecord.Version + 1,
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

        // ==========================================
        // LATEST & HISTORY MAPS
        // ==========================================
        public async Task<object?> GetLatestConsentAsync(Guid dataPrincipalId, Guid organizationId)
        {
            var consent = await _repository.GetLatestConsentByPrincipalIdAsync(dataPrincipalId, organizationId);
            if (consent == null) return null;

            return MapToConsentArtifact(consent);
        }

        public async Task<IEnumerable<object>> GetHistoryAsync(Guid dataPrincipalId, Guid organizationId)
        {
            var history = await _repository.GetHistoryAsync(dataPrincipalId, organizationId);
            return history.Select(c => MapToConsentArtifact(c));
        }

        public async Task<ConsentPurpose> CreatePurposeAsync(CreatePurposeDto dto, Guid organizationId)
        {
            var purpose = new ConsentPurpose
            {
                ConsentPurposeId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Code = dto.Code,
                ShortName = dto.ShortName,
                Description = dto.Description,
                IsMandatory = dto.IsMandatory,
                LegalBasis = dto.LegalBasis,
                RetentionPeriod = dto.RetentionPeriod,
                RiskLevel = dto.RiskLevel,
                DoesConsentLapse = dto.DoesConsentLapse,
                IsActive = dto.IsActive, 
                Categories = new List<ConsentPurposeDataCategory>(),
                Processors = new List<ConsentPurposeDataProcessor>()
            };

            foreach (var catId in dto.CategoryIds)
            {
                purpose.Categories.Add(new ConsentPurposeDataCategory
                {
                    ConsentPurposeId = purpose.ConsentPurposeId,
                    DataCategoryId = catId
                });
            }

            foreach (var procId in dto.ProcessorIds)
            {
                purpose.Processors.Add(new ConsentPurposeDataProcessor
                {
                    ConsentPurposeId = purpose.ConsentPurposeId,
                    DataProcessorId = procId
                });
            }

            await _repository.AddPurposeAsync(purpose);
            return purpose;
        }

        public async Task<List<PurposeCheckboxItemDto>> GetPurposesForCheckboxesAsync(Guid orgId)
        {
            var purposes = await _repository.GetAllPurposesByOrgAsync(orgId);

            return purposes.Select(p => new PurposeCheckboxItemDto
            {
                Code = p.Code,
                Name = p.ShortName
            }).ToList();
        }

        // ==========================================
        // MASTER DATA (Categories & Processors)
        // ==========================================
        public async Task<IEnumerable<DataCategory>> GetCategoriesAsync(Guid organizationId)
        {
            return await _repository.GetCategoriesAsync(organizationId);
        }

        public async Task<DataCategory> AddCategoryAsync(CreateDataCategoryDto request, Guid organizationId)
        {
            var category = new DataCategory
            {
                DataCategoryId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Sensitivity = request.Sensitivity
            };

            return await _repository.AddCategoryAsync(category);
        }

        public async Task<IEnumerable<DataProcessor>> GetProcessorsAsync(Guid organizationId)
        {
            return await _repository.GetProcessorsAsync(organizationId);
        }

        public async Task<DataProcessor> AddProcessorAsync(CreateDataProcessorDto request, Guid organizationId)
        {
            var processor = new DataProcessor
            {
                DataProcessorId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Code = request.Code,
                LegalName = request.LegalName,
                Relationship = request.Relationship,
                Country = request.Country,
                DataLeavesIndia = request.DataLeavesIndia
            };

            return await _repository.AddProcessorAsync(processor);
        }

        public async Task<object?> GetConsentRecordByIdAsync(Guid consentRecordId, Guid organizationId)
        {
            var consent = await _repository.GetByIdAsync(consentRecordId, organizationId);
            if (consent == null) return null;

            // Wahi same formatter use hoga jo history/latest ke liye hai
            return MapToConsentArtifact(consent);
        }

        // ==========================================
        // HELPER METHOD: Modal aur Grid ki fields map karne ke liye
        // ==========================================
        private object MapToConsentArtifact(ConsentRecord c)
        {
            return new
            {
                c.ConsentRecordId,
                Reference = c.ReferenceNumber,
                Receipt = c.ReceiptId, // UI Modal uses 'Receipt'
                ReceiptId = c.ReceiptId,
                c.Version,
                Status = c.OverallStatus.ToString(),
                DataPrincipal = c.DataPrincipalId,

                Notice = "general_service_notice_v1",
                Channel = c.Channel.ToString(),
                Language = c.Language ?? "en-IN",
                Given = c.CreatedAt,
                Expires = c.ConsentDetails.Any() ? c.ConsentDetails.Max(d => (DateTime?)d.ExpiryDate) : null,
                Hash = c.ArtifactHash,
                ArtifactHash = c.ArtifactHash,
                Signature = c.DigitalSignature ?? "Not recorded",

                Purposes = c.ConsentDetails.Select(d => new
                {
                    Purpose = d.ConsentPurpose?.ShortName ?? "Unknown",
                    Decision = d.Decision,
                    Status = d.Status
                }),

                Decisions = c.ConsentDetails.Select(d => new
                {
                    Purpose = d.ConsentPurpose?.ShortName ?? "Unknown",
                    Decision = d.Decision,
                    Status = d.Status
                })
            };
        }
    }
}