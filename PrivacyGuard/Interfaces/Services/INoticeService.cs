using PrivacyGuard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface INoticeService
    {
        Task<List<NoticeListDto>> GetAllNoticesAsync(Guid orgId);
        Task<Guid> CreateNoticeAsync(CreateNoticeDto dto, Guid orgId);
        Task<NoticeDetailDto?> GetNoticeDetailsAsync(Guid id, Guid orgId);
        Task<bool> UpdateNoticeAsync(Guid id, UpdateNoticeDto dto, Guid orgId);

        Task<bool> PublishNoticeAsync(Guid id, Guid orgId);
        Task<Guid?> CreateNewVersionAsync(Guid originalId, Guid orgId);
        Task<List<PublicNoticeListDto>> GetAllActiveNoticesAsync(Guid orgId, string languageCode);
        Task<PublicNoticeResponseDto> GetActiveNoticeForPrincipalAsync(string code, string languageCode, Guid orgId);
    }
}