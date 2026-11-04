using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class 
        
        NoticeService : INoticeService
    {
        private readonly INoticeRepository _noticeRepo;

        public NoticeService(INoticeRepository noticeRepo)
        {
            _noticeRepo = noticeRepo;
        }

        public async Task<List<NoticeListDto>> GetAllNoticesAsync(Guid orgId)
        {
            var notices = await _noticeRepo.GetAllNoticesAsync(orgId);
            return notices.Select(n =>
            {
                int pCount = 0;
                if (!string.IsNullOrWhiteSpace(n.PurposesJson) && n.PurposesJson != "[]")
                {
                    try
                    {
                        var list = JsonSerializer.Deserialize<List<string>>(n.PurposesJson);
                        pCount = list?.Count ?? 0;
                    }
                    catch { pCount = 0; }
                }

                return new NoticeListDto
                {
                    Id = n.Id,
                    Code = n.Code,
                    Title = n.Translations.FirstOrDefault()?.Title ?? string.Empty,
                    Status = n.Status,
                    Version = n.Version,
                    Sha256Hash = n.Sha256Hash,
                    PurposesCount = pCount,
                    CreatedAt = n.CreatedAt
                };
            }).ToList();
        }

        public async Task<Guid> CreateNoticeAsync(CreateNoticeDto dto, Guid orgId)
        {
            var notice = new Notice
            {
                OrganizationId = orgId,
                Code = dto.Code,
                Status = "Draft",
                Version = 1,
                Translations = new List<NoticeTranslation>
                {
                    new NoticeTranslation
                    {
                        LanguageCode = dto.PrimaryLanguage,
                        Title = dto.Title,
                        Summary = dto.Summary,
                        IsTranslated = true
                    }
                }
            };

            var createdNotice = await _noticeRepo.CreateNoticeAsync(notice);
            return createdNotice.Id;
        }

        public async Task<NoticeDetailDto?> GetNoticeDetailsAsync(Guid id, Guid orgId)
        {
            var notice = await _noticeRepo.GetNoticeByIdAsync(id, orgId);
            if (notice == null) return null;

            return new NoticeDetailDto
            {
                Id = notice.Id,
                Code = notice.Code,
                Status = notice.Status,
                Version = notice.Version,
                Sha256Hash = notice.Sha256Hash,
                Layout = notice.Layout,
                PrimaryColor = notice.PrimaryColor,
                LogoUrl = notice.LogoUrl,
                PurposesJson = notice.PurposesJson,
                Translations = notice.Translations.Select(t => new TranslationDto
                {
                    LanguageCode = t.LanguageCode,
                    Title = t.Title,
                    Summary = t.Summary,
                    IsTranslated = t.IsTranslated
                }).ToList()
            };
        }

        public async Task<bool> UpdateNoticeAsync(Guid id, UpdateNoticeDto dto, Guid orgId)
        {
            var notice = await _noticeRepo.GetNoticeByIdAsync(id, orgId);
            if (notice == null) return false;

            if (notice.Status == "Published")
                throw new InvalidOperationException("Published notice cannot be edited directly. Create a new version.");

            // 1. Template Mapping & Purpose Merging
            var template = await _noticeRepo.GetTemplateByIdAsync(dto.NoticeTemplateId);
            if (template != null)
            {
                notice.NoticeTemplateId = template.Id;
                notice.LogoUrl = template.LogoUrl;
                notice.PrimaryColor = template.PrimaryColor;
                notice.Layout = template.Layout;

                List<string> finalPurposes = new List<string>();

                // Add Template default purposes
                if (!string.IsNullOrWhiteSpace(template.PurposesJson) && template.PurposesJson != "[]")
                {
                    try
                    {
                        var templatePurposes = JsonSerializer.Deserialize<List<string>>(template.PurposesJson);
                        if (templatePurposes != null) finalPurposes.AddRange(templatePurposes);
                    }
                    catch { }
                }

                // Add user selected purposes
                if (dto.Purposes != null && dto.Purposes.Any())
                {
                    finalPurposes.AddRange(dto.Purposes);
                }

                // Remove duplicates and save
                finalPurposes = finalPurposes.Distinct().ToList();
                notice.PurposesJson = JsonSerializer.Serialize(finalPurposes);
            }

            var newTranslations = dto.Translations.Select(t => new NoticeTranslation
            {
                LanguageCode = t.LanguageCode,
                Title = t.Title,
                Summary = t.Summary,
                IsTranslated = t.IsTranslated
            }).ToList();

            await _noticeRepo.UpdateNoticeWithTranslationsAsync(notice, newTranslations);

           
            return true;
        }

public async Task<bool> PublishNoticeAsync(Guid id, Guid orgId)
    {
        var notice = await _noticeRepo.GetNoticeByIdAsync(id, orgId);

        if (notice == null || notice.Status == "Published") return false;

        string rawData = $"{notice.Code}-{notice.NoticeTemplateId}-{notice.PurposesJson}-{notice.Version}";

        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(rawData);
            var hashBytes = sha256.ComputeHash(bytes);
            notice.Sha256Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        notice.Status = "Published";

        await _noticeRepo.UpdateNoticeAsync(notice);

        return true;
    }
        public async Task<Guid?> CreateNewVersionAsync(Guid originalId, Guid orgId)
        {
            var oldNotice = await _noticeRepo.GetNoticeByIdAsync(originalId, orgId);

            if (oldNotice == null || oldNotice.Status != "Published")
                return null;

            var newNotice = new Notice
            {
                OrganizationId = orgId,
                Code = oldNotice.Code, 
                NoticeTemplateId = oldNotice.NoticeTemplateId,
                Status = "Draft",   
                Version = oldNotice.Version + 1,
                Layout = oldNotice.Layout,
                PrimaryColor = oldNotice.PrimaryColor,
                LogoUrl = oldNotice.LogoUrl,
                PurposesJson = oldNotice.PurposesJson,
                Sha256Hash = null
            };

            newNotice.Translations = oldNotice.Translations.Select(t => new NoticeTranslation
            {
                LanguageCode = t.LanguageCode,
                Title = t.Title,
                Summary = t.Summary,
                IsTranslated = t.IsTranslated
            }).ToList();

            var createdNotice = await _noticeRepo.CreateNoticeAsync(newNotice);

            return createdNotice.Id;
        }

        public async Task<List<PublicNoticeListDto>> GetAllActiveNoticesAsync(Guid orgId, string languageCode)
        {
            // 1. Repo se data mangaya
            var allPublished = await _noticeRepo.GetPublishedNoticesByOrgAsync(orgId);

            // 2. Business Logic: Har code ka sirf latest version filter karo
            var latestNotices = allPublished
                .GroupBy(n => n.Code)
                .Select(g => g.OrderByDescending(n => n.Version).First())
                .ToList();

            var result = new List<PublicNoticeListDto>();

            // 3. Mapping: Bhasha ke hisaab se translation lagao
            foreach (var notice in latestNotices)
            {
                var translation = notice.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)
                                  ?? notice.Translations.FirstOrDefault(t => t.LanguageCode == "en");

                result.Add(new PublicNoticeListDto
                {
                    Code = notice.Code,
                    Version = notice.Version,
                    LogoUrl = notice.LogoUrl,
                    PrimaryColor = notice.PrimaryColor,
                    //UpdatedAt = notice.UpdatedAt,
                    Title = translation?.Title ?? "Privacy Notice",
                    Summary = translation?.Summary ?? "Click to read more."
                });
            }

            return result;
        }

        public async Task<PublicNoticeResponseDto> GetActiveNoticeForPrincipalAsync(string code, string languageCode, Guid orgId)
        {
            // 1. Repo se ek specific notice mangaya
            var notice = await _noticeRepo.GetLatestPublishedNoticeByCodeAsync(orgId, code);

            if (notice == null) return null;

            // 2. Language filter
            var translation = notice.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)
                              ?? notice.Translations.FirstOrDefault(t => t.LanguageCode == "en");

            // 3. Map to DTO
            return new PublicNoticeResponseDto
            {
                Code = notice.Code,
                Version = notice.Version,
                Layout = notice.Layout,
                PrimaryColor = notice.PrimaryColor,
                LogoUrl = notice.LogoUrl,
                PurposesJson = notice.PurposesJson,
                Sha256Hash = notice.Sha256Hash,
                Title = translation?.Title ?? "Privacy Notice",
                Summary = translation?.Summary ?? "Notice details are currently unavailable."
            };
        }
    }
}