using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class DpiaService : IDpiaService
    {
        private readonly IDpiaRepository _dpiaRepo;

        public DpiaService(IDpiaRepository dpiaRepo)
        {
            _dpiaRepo = dpiaRepo;
        }

        public async Task<List<DpiaListDto>> GetAllDpiaAsync(Guid orgId) =>
            await _dpiaRepo.GetAllDpiaAsync(orgId);

        public async Task<DpiaRecord?> GetDpiaByIdAsync(Guid id, Guid orgId) =>
            await _dpiaRepo.GetDpiaByIdAsync(id, orgId);

        public async Task<DpiaRecord> CreateDpiaAsync(CreateDpiaDto dto, string createdByUserId, Guid orgId) =>
            await _dpiaRepo.CreateDpiaAsync(dto, createdByUserId, orgId);

        public async Task<bool> ReviewDpiaAsync(Guid id, Guid orgId, ReviewDpiaDto dto, string reviewedByUserId) =>
            await _dpiaRepo.ReviewDpiaAsync(id, orgId, dto, reviewedByUserId);
    }
}