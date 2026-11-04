using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class PrivacyRequestService : IPrivacyRequestService
    {
        private readonly IPrivacyRequestRepository _repository;

        public PrivacyRequestService(IPrivacyRequestRepository repository)
        {
            _repository = repository;
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

            // Parse String from DTO to Enum for Status Update
            if (Enum.TryParse<PrivacyRequestStatus>(resolveDto.Status, true, out var parsedStatus))
            {
                existingRequest.Status = parsedStatus;
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
    }
}