using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class CampaignAudienceGroupService : ICampaignAudienceGroupService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICampaignAudienceGroupRepository _repository;

        public CampaignAudienceGroupService(ApplicationDbContext dbContext, ICampaignAudienceGroupRepository repository)
        {
            _dbContext = dbContext;
            _repository = repository;
        }

        public async Task<CampaignAudienceGroupDto> CreateAsync(Guid organizationId, CreateCampaignAudienceGroupDto dto)
        {
            Validate(dto.Name, dto.Members);
            await ValidateMembersBelongToOrganizationAsync(organizationId, dto.Members);
            await EnsureUniqueNameAsync(organizationId, dto.Name, null);
            return await _repository.CreateAsync(organizationId, dto);
        }

        public Task<IEnumerable<CampaignAudienceGroupDto>> GetAllAsync(Guid organizationId) => _repository.GetAllAsync(organizationId);

        public Task<CampaignAudienceGroupDto?> GetByIdAsync(Guid groupId, Guid organizationId) => _repository.GetByIdAsync(groupId, organizationId);

        public async Task<CampaignAudienceGroupDto?> UpdateAsync(Guid groupId, Guid organizationId, UpdateCampaignAudienceGroupDto dto)
        {
            Validate(dto.Name, dto.Members);
            await ValidateMembersBelongToOrganizationAsync(organizationId, dto.Members);
            await EnsureUniqueNameAsync(organizationId, dto.Name, groupId);
            return await _repository.UpdateAsync(groupId, organizationId, dto);
        }

        public Task<bool> DeleteAsync(Guid groupId, Guid organizationId) => _repository.DeleteAsync(groupId, organizationId);

        public Task ApplyToCampaignAsync(Guid groupId, Guid campaignId, Guid organizationId) => _repository.ApplyToCampaignAsync(groupId, campaignId, organizationId);

        private async Task ValidateMembersBelongToOrganizationAsync(Guid organizationId, IEnumerable<CampaignAudienceGroupMemberRequestDto> members)
        {
            var normalizedMembers = members.Select(member => new
            {
                member.MemberId,
                UserType = NormalizeUserType(member.UserType)
            }).ToList();

            if (normalizedMembers.GroupBy(member => new { member.MemberId, member.UserType }).Any(group => group.Count() > 1))
                throw new ArgumentException("The same audience member cannot be added more than once.");

            foreach (var member in normalizedMembers)
            {
                var exists = member.UserType == "InternalUser"
                    ? await _dbContext.Users.AnyAsync(user => user.UserId == member.MemberId && user.OrganizationId == organizationId && user.IsActive)
                    : await _dbContext.DataPrincipals.AnyAsync(principal => principal.DataPrincipalId == member.MemberId && principal.OrganizationId == organizationId);

                if (!exists) throw new ArgumentException($"Audience member '{member.MemberId}' is invalid for this organization.");
            }
        }

        private async Task EnsureUniqueNameAsync(Guid organizationId, string name, Guid? excludedGroupId)
        {
            var normalizedName = name.Trim().ToLower();
            var exists = await _dbContext.CampaignAudienceGroups.AnyAsync(group =>
                group.OrganizationId == organizationId &&
                group.Name.ToLower() == normalizedName &&
                (!excludedGroupId.HasValue || group.GroupId != excludedGroupId.Value));

            if (exists) throw new ArgumentException("An audience group with this name already exists.");
        }

        private static void Validate(string name, IEnumerable<CampaignAudienceGroupMemberRequestDto> members)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name is required.");
            if (members == null || !members.Any()) throw new ArgumentException("At least one audience member is required.");
        }

        private static string NormalizeUserType(string userType)
        {
            return userType.Trim().ToLowerInvariant() switch
            {
                "internaluser" or "internal" or "staff" => "InternalUser",
                "dataprincipal" or "customer" => "DataPrincipal",
                _ => throw new ArgumentException("UserType must be InternalUser or DataPrincipal.")
            };
        }
    }
}
