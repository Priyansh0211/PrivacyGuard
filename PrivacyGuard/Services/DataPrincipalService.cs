using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Helpers; // Masking Helper ke liye
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class DataPrincipalService : IDataPrincipalService
    {
        private readonly IDataPrincipalRepository _repository;

        public DataPrincipalService(IDataPrincipalRepository repository)
        {
            _repository = repository;
        }

        public async Task<DataPrincipal> CreateSingleDataPrincipalAsync(DataPrincipalDto dto, Guid organizationId)
        {
            bool requiresGuardian = !string.IsNullOrEmpty(dto.Class) &&
                (dto.Class.ToLower() == "child" || dto.Class.ToLower() == "person_with_disability");

            var dataPrincipal = new DataPrincipal
            {
                DataPrincipalId = Guid.NewGuid(),
                OrganizationId = organizationId,
                ReferenceId = $"PSD-{new Random().Next(10000000, 99999999)}",
                Mobile = dto.Mobile,
                Email = dto.Email,
                ConsumerNo = dto.ConsumerNo,
                FullName = dto.FullName,
                DateOfBirth = string.IsNullOrEmpty(dto.DateOfBirth) ? null : DateTime.Parse(dto.DateOfBirth),
                Class = dto.Class,
                PreferredLocale = dto.PreferredLocale,
                RequiresGuardianConsent = requiresGuardian,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddDataPrincipalAsync(dataPrincipal);
            await _repository.SaveChangesAsync();

            return dataPrincipal;
        }

        // ==========================================
        // NAYA METHOD: Fetch & Mask 360-View Data
        // ==========================================
        public async Task<DataPrincipal360Dto?> Get360ViewAsync(Guid dataPrincipalId, Guid organizationId)
        {
            // Ye method hum repository mein banayenge jo Include() karega
            var dp = await _repository.Get360ViewEntityAsync(dataPrincipalId, organizationId);

            if (dp == null) return null;

            // DTO Mapping aur Data Masking
            var dto = new DataPrincipal360Dto
            {
                Record = new RecordDto
                {
                    ReferenceId = dp.ReferenceId,
                    Name = PiiMaskingHelper.MaskName(dp.FullName ?? dp.DisplayName), // Helper Call
                    Class = dp.Class,
                    Status = "Active",
                    PreferredLanguage = dp.PreferredLocale,
                    RegisteredDate = dp.CreatedAt
                },
                Identifiers = dp.Identifiers.Select(i => new IdentifierDto
                {
                    Kind = i.Kind,
                    Value = MaskIdentifierValue(i.Kind, i.Value), // Helper Call for each type
                    IsPrimary = i.IsPrimary,
                    IsVerified = i.IsVerified
                }).ToList(),
                RightsRequests = dp.PrivacyRequests.Select(pr => new RightRequestSummaryDto
                {
                    RequestId = pr.RequestId,
                    Reference = $"DSR-{pr.CreatedAt.Year}-{pr.RequestId.ToString().Substring(0, 6).ToUpper()}",
                    RightExercised = pr.RequestType,
                    Status = pr.Status,
                    RaisedOn = pr.CreatedAt,
                    Deadline = pr.TargetResolutionDate
                }).ToList(),
                ActiveConsents = dp.ConsentRecords.Select(cr => new ConsentStateDto
                {
                    PurposeName = "Mapped Purpose", // Note: Agar ConsentRecord me Purpose.Name hai toh cr.Purpose.Name use karein
                    GrantedOn = cr.CreatedAt,
                    Status = cr.OverallStatus.ToString()
                }).ToList()
            };

            return dto;
        }

        // Switch case jo correct helper method call karega
        private string MaskIdentifierValue(string kind, string value)
        {
            return kind.ToLower() switch
            {
                "email" => PiiMaskingHelper.MaskEmail(value),
                "mobile" => PiiMaskingHelper.MaskMobile(value),
                "consumer no" => PiiMaskingHelper.MaskConsumerNo(value),
                _ => value // agar koi unmapped kind hai toh default return
            };
        }
    }
}