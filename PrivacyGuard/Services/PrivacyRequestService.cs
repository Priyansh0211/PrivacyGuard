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
    public class PrivacyRequestService : IPrivacyRequestService
    {
        private readonly IPrivacyRequestRepository _repository;
        private readonly IDataPrincipalService _dataPrincipalService;

        // UPDATED: Constructor ab IDataPrincipalService ko inject kar raha hai Data Erasure ke liye
        public PrivacyRequestService(
            IPrivacyRequestRepository repository,
            IDataPrincipalService dataPrincipalService)
        {
            _repository = repository;
            _dataPrincipalService = dataPrincipalService;
        }

        public async Task<PrivacyRequest> RaiseRequestAsync(CreatePrivacyRequestDto requestDto, Guid organizationId)
        {
            // Parse String from DTO to Enum for Model
            Enum.TryParse<PrivacyRequestType>(requestDto.RequestType, true, out var parsedType);

            var newRequest = new PrivacyRequest
            {
                DataPrincipalId = Guid.Parse(requestDto.DataPrincipalId),
                OrganizationId = organizationId,
                ReferenceId = $"DSR-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                RequestType = parsedType,
                RequestDetails = requestDto.RequestDetails,
                Status = PrivacyRequestStatus.Submitted, // Enum instead of "Pending"
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newRequest);
            await _repository.SaveChangesAsync();

            return newRequest;
        }

        public async Task<bool> ResolveRequestAsync(Guid requestId, ResolvePrivacyRequestDto resolveDto, Guid organizationId)
        {
            var existingRequest = await _repository.GetByIdAsync(requestId, organizationId);

            if (existingRequest == null)
                return false;

            // DTO string ko wapas Enum mein convert kar rahe hain
            if (Enum.TryParse<PrivacyRequestStatus>(resolveDto.Status, true, out var parsedStatus))
            {
                existingRequest.Status = parsedStatus;

                // ==============================================================
                // 🚨 THE MASTER WORKFLOW: If ERASURE is APPROVED (Fulfilled) 🚨
                // ==============================================================
                if (parsedStatus == PrivacyRequestStatus.Fulfilled && existingRequest.RequestType == PrivacyRequestType.Erasure)
                {
                    // Data principal ko automatically anonymize kar do
                    await _dataPrincipalService.AnonymizeDataPrincipalAsync(existingRequest.DataPrincipalId, organizationId);

                    // NOTE: Yahan IAuditService call karke log daalna best practice hai 
                    // e.g., _auditService.LogActionAsync(orgId, "Data Erasure", "User data anonymized due to DSAR Request.");
                }
            }

            existingRequest.ResolutionRemarks = resolveDto.ResolutionRemarks;
            existingRequest.ResolvedAt = DateTime.UtcNow;

            _repository.Update(existingRequest);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<PrivacyRequest>> GetMyRequestsAsync(string dataPrincipalId, Guid organizationId)
        {
            return await _repository.GetAllByUserAsync(dataPrincipalId, organizationId);
        }

        public async Task<PrivacyRequestListResponseDto> GetFilteredRequestsAsync(
            Guid organizationId, string? search, string? statusStr, string? typeStr, bool overdueOnly, int page, int pageSize)
        {
            // Convert strings to Enums for Repository
            PrivacyRequestStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(statusStr) && Enum.TryParse<PrivacyRequestStatus>(statusStr, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            PrivacyRequestType? typeEnum = null;
            if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<PrivacyRequestType>(typeStr, true, out var parsedType))
            {
                typeEnum = parsedType;
            }

            var (requests, totalCount) = await _repository.GetFilteredRequestsAsync(
                organizationId, search, statusEnum, typeEnum, overdueOnly, page, pageSize);

            // Map Entity to DTO
            var listDto = new PrivacyRequestListResponseDto
            {
                TotalCount = totalCount,
                Data = requests.Select(r => new PrivacyRequestRowDto
                {
                    RequestId = r.RequestId,
                    ReferenceId = r.ReferenceId ?? "N/A",
                    DataPrincipalId = r.DataPrincipalId,

                    // Mask the name for safety on the admin grid
                    PrincipalName = Helpers.PiiMaskingHelper.MaskName(r.DataPrincipal?.FullName ?? "Unknown"),

                    RequestType = r.RequestType.ToString(),
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt,
                    TargetResolutionDate = r.TargetResolutionDate,

                    // Overdue logic calculation
                    IsOverdue = r.TargetResolutionDate < DateTime.UtcNow &&
                                (r.Status != PrivacyRequestStatus.Fulfilled && r.Status != PrivacyRequestStatus.Rejected && r.Status != PrivacyRequestStatus.Withdrawn)
                }).ToList()
            };

            return listDto;
        }
        public async Task<PrivacyRequestDetailDto?> GetRequestDetailsAsync(Guid requestId, Guid organizationId)
        {
            var request = await _repository.GetRequestDetailsAsync(requestId, organizationId);
            if (request == null) return null;

            return new PrivacyRequestDetailDto
            {
                RequestId = request.RequestId,
                ReferenceId = request.ReferenceId ?? "N/A",
                DataPrincipalId = request.DataPrincipalId,

                // Use PiiMaskingHelper from your Helpers namespace
                PrincipalName = Helpers.PiiMaskingHelper.MaskName(request.DataPrincipal?.FullName ?? "Unknown"),
                PrincipalEmail = Helpers.PiiMaskingHelper.MaskEmail(request.DataPrincipal?.Email ?? ""),
                PrincipalMobile = Helpers.PiiMaskingHelper.MaskMobile(request.DataPrincipal?.Mobile ?? ""),

                RequestType = request.RequestType.ToString(),
                RequestDetails = request.RequestDetails ?? "No description provided.",
                Status = request.Status.ToString(),

                CreatedAt = request.CreatedAt,
                TargetResolutionDate = request.TargetResolutionDate,
                ResolvedAt = request.ResolvedAt,
                ResolutionRemarks = request.ResolutionRemarks ?? string.Empty,

                IsOverdue = request.TargetResolutionDate < DateTime.UtcNow &&
                            (request.Status != PrivacyRequestStatus.Fulfilled && request.Status != PrivacyRequestStatus.Rejected && request.Status != PrivacyRequestStatus.Withdrawn)
            };
        }
    }
}