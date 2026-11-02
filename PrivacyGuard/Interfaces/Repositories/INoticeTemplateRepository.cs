using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface INoticeTemplateRepository
    {
        Task<NoticeTemplate> CreateTemplateAsync(NoticeTemplate template);
        Task<List<NoticeTemplate>> GetAllTemplatesAsync(Guid organizationId);
        Task<NoticeTemplate?> GetTemplateByIdAsync(Guid id, Guid organizationId);
        Task<NoticeTemplate> UpdateTemplateAsync(NoticeTemplate template);
        Task<bool> DeleteTemplateAsync(Guid id, Guid organizationId);
    }
}