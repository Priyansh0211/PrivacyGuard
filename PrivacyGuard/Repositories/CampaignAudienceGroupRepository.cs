using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Repositories
{
    public class CampaignAudienceGroupRepository : ICampaignAudienceGroupRepository
    {
        private const string InternalUserType = "InternalUser";
        private const string DataPrincipalType = "DataPrincipal";
        private readonly ApplicationDbContext _dbContext;

        public CampaignAudienceGroupRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CampaignAudienceGroupDto> CreateAsync(Guid organizationId, CreateCampaignAudienceGroupDto dto)
        {
            var group = new CampaignAudienceGroup
            {
                OrganizationId = organizationId,
                Name = dto.Name.Trim(),
                Members = dto.Members.Select(member => new CampaignAudienceGroupMember
                {
                    MemberId = member.MemberId,
                    UserType = NormalizeUserType(member.UserType)
                }).ToList()
            };

            await _dbContext.CampaignAudienceGroups.AddAsync(group);
            await _dbContext.SaveChangesAsync();
            return await GetByIdAsync(group.GroupId, organizationId) ?? throw new InvalidOperationException("Audience group could not be loaded after creation.");
        }

        public async Task<IEnumerable<CampaignAudienceGroupDto>> GetAllAsync(Guid organizationId)
        {
            var groups = await _dbContext.CampaignAudienceGroups
                .AsNoTracking()
                .Include(group => group.Members)
                .Where(group => group.OrganizationId == organizationId)
                .OrderBy(group => group.Name)
                .ToListAsync();

            return groups.Select(MapGroup).ToList();
        }

        public async Task<CampaignAudienceGroupDto?> GetByIdAsync(Guid groupId, Guid organizationId)
        {
            var group = await _dbContext.CampaignAudienceGroups
                .AsNoTracking()
                .Include(item => item.Members)
                .FirstOrDefaultAsync(item => item.GroupId == groupId && item.OrganizationId == organizationId);

            return group == null ? null : MapGroup(group);
        }

        public async Task<CampaignAudienceGroupDto?> UpdateAsync(Guid groupId, Guid organizationId, UpdateCampaignAudienceGroupDto dto)
        {
            var group = await _dbContext.CampaignAudienceGroups
                .Include(item => item.Members)
                .FirstOrDefaultAsync(item => item.GroupId == groupId && item.OrganizationId == organizationId);

            if (group == null) return null;

            group.Name = dto.Name.Trim();
            group.UpdatedAt = DateTime.UtcNow;
            _dbContext.CampaignAudienceGroupMembers.RemoveRange(group.Members);
            group.Members = dto.Members.Select(member => new CampaignAudienceGroupMember
            {
                GroupId = groupId,
                MemberId = member.MemberId,
                UserType = NormalizeUserType(member.UserType)
            }).ToList();
            await _dbContext.CampaignAudienceGroupMembers.AddRangeAsync(group.Members);
            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(groupId, organizationId);
        }

        public async Task<bool> DeleteAsync(Guid groupId, Guid organizationId)
        {
            var group = await _dbContext.CampaignAudienceGroups
                .FirstOrDefaultAsync(item => item.GroupId == groupId && item.OrganizationId == organizationId);

            if (group == null) return false;

            _dbContext.CampaignAudienceGroups.Remove(group);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task ApplyToCampaignAsync(Guid groupId, Guid campaignId, Guid organizationId)
        {
            var groupExists = await _dbContext.CampaignAudienceGroups
                .AnyAsync(group => group.GroupId == groupId && group.OrganizationId == organizationId);
            if (!groupExists) throw new KeyNotFoundException("Audience group not found.");

            var campaignExists = await _dbContext.Campaigns
                .AnyAsync(campaign => campaign.CampaignId == campaignId && campaign.OrganizationId == organizationId);
            if (!campaignExists) throw new KeyNotFoundException("Campaign not found.");

            var members = await _dbContext.CampaignAudienceGroupMembers
                .Where(member => member.GroupId == groupId)
                .ToListAsync();
            var existingMappings = await _dbContext.CampaignAudienceMappings
                .Where(mapping => mapping.CampaignId == campaignId)
                .ToListAsync();

            _dbContext.CampaignAudienceMappings.RemoveRange(existingMappings);
            await _dbContext.CampaignAudienceMappings.AddRangeAsync(members.Select(member => new CampaignAudienceMapping
            {
                CampaignId = campaignId,
                UserId = member.UserType == InternalUserType ? member.MemberId : null,
                DataPrincipalId = member.UserType == DataPrincipalType ? member.MemberId : null
            }));
            await _dbContext.SaveChangesAsync();
        }

        private static string NormalizeUserType(string userType)
        {
            return userType.Trim().ToLowerInvariant() switch
            {
                "internaluser" or "internal" or "staff" => InternalUserType,
                "dataprincipal" or "customer" => DataPrincipalType,
                _ => throw new ArgumentException("UserType must be InternalUser or DataPrincipal.")
            };
        }

        private static CampaignAudienceGroupDto MapGroup(CampaignAudienceGroup group)
        {
            return new CampaignAudienceGroupDto
            {
                GroupId = group.GroupId,
                Name = group.Name,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                Members = group.Members.Select(member => new CampaignAudienceGroupMemberDto
                {
                    MemberId = member.MemberId,
                    UserType = member.UserType
                }).ToList()
            };
        }
    }
}
