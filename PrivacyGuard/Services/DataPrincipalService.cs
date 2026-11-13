using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Helpers;
using System;
using System.Collections.Generic;
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

        // ==========================================
        // 1. ADD DATA PRINCIPAL (Fixed Identifiers & Reference ID)
        // ==========================================
        public async Task<DataPrincipal> CreateSingleDataPrincipalAsync(DataPrincipalDto dto, Guid organizationId)
        {
            bool requiresGuardian = !string.IsNullOrEmpty(dto.Class) &&
                (dto.Class.ToLower() == "child" || dto.Class.ToLower() == "person_with_disability");

            var dpId = Guid.NewGuid();

            // FIX: Unique Reference ID to prevent Random() collisions
            string uniqueRef = $"PSD-{DateTime.UtcNow:yyMM}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

            var dataPrincipal = new DataPrincipal
            {
                DataPrincipalId = dpId,
                OrganizationId = organizationId,
                RoleId = dto.RoleId,
                ReferenceId = uniqueRef,
                Mobile = dto.Mobile,
                Email = dto.Email,
                PasswordHash = dto.Password,
                ConsumerNo = dto.ConsumerNo,
                FullName = dto.FullName,
                DateOfBirth = string.IsNullOrEmpty(dto.DateOfBirth) ? null : DateTime.Parse(dto.DateOfBirth),
                Class = dto.Class,
                PreferredLocale = dto.PreferredLocale,
                RequiresGuardianConsent = requiresGuardian,
                CreatedAt = DateTime.UtcNow,

                // FIX: Initialize Identifiers list immediately so 360-view isn't blank
                Identifiers = new List<PrincipalIdentifier>()
            };

            // Populate Identifiers for the 360-Degree View
            if (!string.IsNullOrEmpty(dto.Mobile))
                dataPrincipal.Identifiers.Add(new PrincipalIdentifier { DataPrincipalId = dpId, Kind = "Mobile", Value = dto.Mobile, IsPrimary = true });

            if (!string.IsNullOrEmpty(dto.Email))
                dataPrincipal.Identifiers.Add(new PrincipalIdentifier { DataPrincipalId = dpId, Kind = "Email", Value = dto.Email, IsPrimary = false });

            if (!string.IsNullOrEmpty(dto.ConsumerNo))
                dataPrincipal.Identifiers.Add(new PrincipalIdentifier { DataPrincipalId = dpId, Kind = "Consumer No", Value = dto.ConsumerNo, IsPrimary = false });

            await _repository.AddDataPrincipalAsync(dataPrincipal);
            await _repository.SaveChangesAsync();

            return dataPrincipal;
        }

        // ==========================================
        // 2. NAYA METHOD: Get Paginated & Masked List
        // ==========================================
        public async Task<DataPrincipalListResponseDto> GetDataPrincipalsListAsync(
            Guid organizationId, string search, string classType, int page, int pageSize)
        {
            var (principals, totalCount) = await _repository.GetPaginatedDataPrincipalsAsync(organizationId, search, classType, page, pageSize);

            var listDto = new DataPrincipalListResponseDto
            {
                TotalCount = totalCount,
                Data = principals.Select(dp => new DataPrincipalRowDto
                {
                    DataPrincipalId = dp.DataPrincipalId,
                    ReferenceId = dp.ReferenceId,
                    Name = PiiMaskingHelper.MaskName(dp.FullName ?? dp.DisplayName ?? "N/A"),
                    Mobile = PiiMaskingHelper.MaskMobile(dp.Mobile ?? ""),
                    Email = PiiMaskingHelper.MaskEmail(dp.Email ?? ""),
                    ConsumerNo = PiiMaskingHelper.MaskConsumerNo(dp.ConsumerNo ?? ""),
                    Class = dp.Class,
                    PreferredLocale = dp.PreferredLocale,
                    RequiresGuardianConsent = dp.RequiresGuardianConsent,
                    Status = "Active",
                    CreatedAt = dp.CreatedAt,
                    Source = "Manual"
                }).ToList()
            };

            return listDto;
        }

        // ==========================================
        // 3. FETCH 360-VIEW DATA (Fixed Active Consents)
        // ==========================================
        public async Task<DataPrincipal360Dto?> Get360ViewAsync(Guid dataPrincipalId, Guid organizationId)
        {
            var dp = await _repository.Get360ViewEntityAsync(dataPrincipalId, organizationId);

            if (dp == null) return null;

            var dto = new DataPrincipal360Dto
            {
                Record = new RecordDto
                {
                    ReferenceId = dp.ReferenceId,
                    Name = PiiMaskingHelper.MaskName(dp.FullName ?? dp.DisplayName ?? "N/A"),
                    Class = dp.Class,
                    Status = "Active",
                    PreferredLanguage = dp.PreferredLocale,
                    RegisteredDate = dp.CreatedAt
                },
                Identifiers = dp.Identifiers.Select(i => new IdentifierDto
                {
                    Kind = i.Kind,
                    Value = MaskIdentifierValue(i.Kind, i.Value),
                    IsPrimary = i.IsPrimary,
                    IsVerified = i.IsVerified
                }).ToList(),
                RightsRequests = dp.PrivacyRequests.Select(pr => new RightRequestSummaryDto
                {
                    RequestId = pr.RequestId,
                    // Status property jo tumne model me update ki hai, usko string me map kiya
                    Reference = pr.ReferenceId ?? $"DSR-{pr.CreatedAt.Year}-{pr.RequestId.ToString().Substring(0, 6).ToUpper()}",
                    RightExercised = pr.RequestType.ToString(),
                    Status = pr.Status.ToString(),
                    RaisedOn = pr.CreatedAt,
                    Deadline = pr.TargetResolutionDate
                }).ToList(),

                // FIX: Active Consents mapped with real Purpose Names from ConsentDetails
                ActiveConsents = dp.ConsentRecords
            .Where(cr => cr.OverallStatus == ConsentStatus.Granted || cr.OverallStatus == ConsentStatus.PartiallyGranted)
            .Select(cr => new ConsentStateDto
            {
                // ConsentDetails ke andar se saare purposes nikal kar comma (,) se join kar rahe hain
                PurposeName = cr.ConsentDetails != null && cr.ConsentDetails.Any()
                    ? string.Join(", ", cr.ConsentDetails
                        .Where(cd => cd.Decision == "Yes" || cd.Status == "Active") // Sirf active purposes
                        .Select(cd => cd.ConsentPurpose != null && !string.IsNullOrEmpty(cd.ConsentPurpose.ShortName)
                                      ? cd.ConsentPurpose.ShortName
                                      : "General Purpose"))
                    : "No specific purpose recorded",

                GrantedOn = cr.CreatedAt,
                Status = cr.OverallStatus.ToString()
            }).ToList()
            };

            return dto;
        }

        // ==========================================
        // DSAR: Right to Erasure (Soft Delete / Anonymize)
        // ==========================================
        public async Task<bool> AnonymizeDataPrincipalAsync(Guid dataPrincipalId, Guid organizationId)
        {
            var dp = await _repository.Get360ViewEntityAsync(dataPrincipalId, organizationId);
            if (dp == null) return false;

            // DPDP Act Compliance: PII data ko scramble/anonymize kar do
            dp.FullName = "Anonymized User";
            dp.Email = "erased@anonymized.local";
            dp.Mobile = "0000000000";
            dp.ConsumerNo = "ERASED";
            //dp.Status = "Erased"; // Status change kar diya

            // Identifiers ko bhi scramble kar do
            foreach (var identifier in dp.Identifiers)
            {
                identifier.Value = "ERASED";
            }

            // DB mein save
            await _repository.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // BULK IMPORT PROCESSOR (For CSV Uploads)
        // ==========================================
        public async Task<(int SuccessCount, string Message)> BulkImportPrincipalsAsync(
            List<DataPrincipalDto> dtos, Guid organizationId, Guid? orgUnitId = null)
        {
            var newPrincipals = new List<DataPrincipal>();
            var currentTime = DateTime.UtcNow;

            foreach (var dto in dtos)
            {
                var dpId = Guid.NewGuid();
                string uniqueRef = $"PSD-{currentTime:yyMM}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                var dp = new DataPrincipal
                {
                    DataPrincipalId = dpId,
                    OrganizationId = organizationId,
                    OrgUnitId = orgUnitId, // Circle / Branch mapping
                    ReferenceId = uniqueRef,
                    FullName = dto.FullName,
                    Email = dto.Email,
                    Mobile = dto.Mobile,
                    ConsumerNo = dto.ConsumerNo,
                    Class = string.IsNullOrEmpty(dto.Class) ? "adult" : dto.Class,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                    Identifiers = new List<PrincipalIdentifier>()
                };

                // Identifiers Mapping
                if (!string.IsNullOrEmpty(dto.Mobile))
                    dp.Identifiers.Add(new PrincipalIdentifier { DataPrincipalId = dpId, Kind = "Mobile", Value = dto.Mobile, IsPrimary = true });

                if (!string.IsNullOrEmpty(dto.Email))
                    dp.Identifiers.Add(new PrincipalIdentifier { DataPrincipalId = dpId, Kind = "Email", Value = dto.Email, IsPrimary = false });

                newPrincipals.Add(dp);
            }

            // Ek hi baar mein saare records DB mein insert kardo (Hang nahi hoga)
            await _repository.AddBulkDataPrincipalsAsync(newPrincipals);

            return (newPrincipals.Count, $"{newPrincipals.Count} Principals successfully imported.");
        }

        // ==========================================
        // NAYA METHOD: Get Total Principals Count
        // ==========================================
        public async Task<int> GetTotalPrincipalsCountAsync(Guid organizationId, Guid? orgUnitId = null)
        {
            return await _repository.GetTotalPrincipalsCountAsync(organizationId, orgUnitId);
        }

        // ==========================================
        // 4. HELPER METHOD
        // ==========================================
        private string MaskIdentifierValue(string kind, string value)
        {
            return kind.ToLower() switch
            {
                "email" => PiiMaskingHelper.MaskEmail(value),
                "mobile" => PiiMaskingHelper.MaskMobile(value),
                "consumer no" => PiiMaskingHelper.MaskConsumerNo(value),
                _ => value
            };
        }
    }
}