using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces
{
    public interface IDpiaService
    {
        Task<List<DpiaListDto>> GetAllDpiaAsync(Guid orgId);
        Task<DpiaRecord?> GetDpiaByIdAsync(Guid id, Guid orgId);
        Task<DpiaRecord> CreateDpiaAsync(CreateDpiaDto dto, string createdByUserId, Guid orgId);
        Task<bool> ReviewDpiaAsync(Guid id, Guid orgId, ReviewDpiaDto dto, string reviewedByUserId);
    }
}