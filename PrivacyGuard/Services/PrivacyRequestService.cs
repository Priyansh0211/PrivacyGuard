using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

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
            // DTO se data nikal kar naya PrivacyRequest object banaya
            var newRequest = new PrivacyRequest
            {
                DataPrincipalId = requestDto.DataPrincipalId,
                OrganizationId = organizationId,
                RequestType = requestDto.RequestType, // e.g., "Erasure", "Correction"
                RequestDetails = requestDto.RequestDetails,
                Status = "Pending", // By default nayi request Pending hogi
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newRequest);
            await _repository.SaveChangesAsync();

            return newRequest;
        }

        public async Task<bool> ResolveRequestAsync(Guid requestId, ResolvePrivacyRequestDto resolveDto, Guid organizationId)
        {
            // Pehle check karo request DB mein exist karti hai ya nahi
            var existingRequest = await _repository.GetByIdAsync(requestId, organizationId);

            if (existingRequest == null)
                return false;

            // Admin ne jo status aur remarks bheje hain, wo update kardo
            existingRequest.Status = resolveDto.Status;
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