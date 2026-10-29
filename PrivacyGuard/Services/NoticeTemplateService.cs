using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class NoticeTemplateService : INoticeTemplateService
    {
        private readonly INoticeTemplateRepository _templateRepo;

        public NoticeTemplateService(INoticeTemplateRepository templateRepo)
        {
            _templateRepo = templateRepo;
        }

        public async Task<Guid> CreateTemplateAsync(CreateTemplateDto dto, Guid orgId)
        {
            var template = new NoticeTemplate
            {
                OrganizationId = orgId,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                PurposesJson = JsonSerializer.Serialize(dto.Purposes),
                LogoUrl = dto.LogoUrl,
                PrimaryColor = dto.PrimaryColor
            };

            var createdTemplate = await _templateRepo.CreateTemplateAsync(template);
            return createdTemplate.Id;
        }

        public async Task<List<TemplateListDto>> GetAllTemplatesAsync(Guid orgId)
        {
            var templates = await _templateRepo.GetAllTemplatesAsync(orgId);

            return templates.Select(t => new TemplateListDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Purposes = string.IsNullOrEmpty(t.PurposesJson)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(t.PurposesJson) ?? new List<string>(),
                CreatedAt = t.CreatedAt
            }).ToList();
        }

        public async Task<TemplateListDto?> GetTemplateByIdAsync(Guid id, Guid orgId)
        {
            var t = await _templateRepo.GetTemplateByIdAsync(id, orgId);
            if (t == null) return null;

            return new TemplateListDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Purposes = string.IsNullOrEmpty(t.PurposesJson)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(t.PurposesJson) ?? new List<string>(),
                CreatedAt = t.CreatedAt
            };
        }

 
        public async Task<bool> UpdateTemplateAsync(Guid id, Guid orgId, UpdateTemplateDto dto)
        {
            var existingTemplate = await _templateRepo.GetTemplateByIdAsync(id, orgId);
            if (existingTemplate == null) return false;

            // Naye values assign karo
            existingTemplate.Code = dto.Code;
            existingTemplate.Name = dto.Name;
            existingTemplate.Description = dto.Description;
            existingTemplate.PurposesJson = JsonSerializer.Serialize(dto.Purposes);
            existingTemplate.LogoUrl = dto.LogoUrl;
            existingTemplate.PrimaryColor = dto.PrimaryColor;

            await _templateRepo.UpdateTemplateAsync(existingTemplate);
            return true;
        }

        public async Task<bool> DeleteTemplateAsync(Guid id, Guid orgId)
        {
            return await _templateRepo.DeleteTemplateAsync(id, orgId);
        }
    }
}