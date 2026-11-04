using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface INoticeRepository
    {
        Task<List<Notice>> GetAllNoticesAsync(Guid orgId);
        Task<Notice?> GetNoticeByIdAsync(Guid id, Guid orgId);
        Task<Notice> CreateNoticeAsync(Notice notice);
        Task UpdateNoticeAsync(Notice notice);
        Task<NoticeTemplate?> GetTemplateByIdAsync(Guid templateId);
        Task UpdateNoticeWithTranslationsAsync(Notice notice, List<NoticeTranslation> newTranslations);
    }
}