using PrivacyGuard.DTOs;

namespace PrivacyGuard.Interfaces.Services
{
    public interface INoticeTemplateService
    {
        Task<Guid> CreateTemplateAsync(CreateTemplateDto dto, Guid orgId);
        Task<List<TemplateListDto>> GetAllTemplatesAsync(Guid orgId);
        Task<TemplateListDto?> GetTemplateByIdAsync(Guid id, Guid orgId);
        Task<bool> UpdateTemplateAsync(Guid id, Guid orgId, UpdateTemplateDto dto);
        Task<bool> DeleteTemplateAsync(Guid id, Guid orgId);
    }
}
