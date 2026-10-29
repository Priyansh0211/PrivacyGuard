using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class GrievanceService : IGrievanceService
    {
        private readonly IGrievanceRepository _grievanceRepo;
        public GrievanceService(IGrievanceRepository grievanceRepo) { _grievanceRepo = grievanceRepo; }

        public async Task<string> CreateGrievanceAsync(Guid userId, Guid orgId, CreateGrievanceDto dto)
        {
            var newGrievance = new Grievance
            {
                OrganizationId = orgId,
                DataPrincipalId = userId,
                // Assume PrivacyHelper is a utility you made to generate IDs
                ReferenceId = $"GRV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Subject = dto.Subject,
                Description = dto.Description,
                Category = dto.Category,
                Status = "Open",
                Severity = "Medium"
            };
            await _grievanceRepo.CreateGrievanceAsync(newGrievance);
            return newGrievance.ReferenceId;
        }

        public async Task<List<GrievanceDetailDto>> GetAllGrievancesAsync(Guid orgId)
        {
            var grievances = await _grievanceRepo.GetAllGrievancesAsync(orgId);
            return MapToDtoList(grievances);
        }

        public async Task<List<GrievanceDetailDto>> GetMyGrievancesAsync(Guid userId, Guid orgId)
        {
            var grievances = await _grievanceRepo.GetMyGrievancesAsync(userId, orgId);
            return MapToDtoList(grievances);
        }

        public async Task<GrievanceDetailDto?> GetGrievanceDetailsAsync(Guid id, Guid orgId, bool isAdmin)
        {
            var grievance = await _grievanceRepo.GetGrievanceByIdAsync(id, orgId);
            if (grievance == null) return null;

            var filteredMessages = grievance.Messages.AsEnumerable();
            if (!isAdmin) filteredMessages = filteredMessages.Where(m => m.IsInternal == false);

            return new GrievanceDetailDto
            {
                Id = grievance.Id,
                ReferenceId = grievance.ReferenceId,
                Subject = grievance.Subject,
                Category = grievance.Category,
                Severity = grievance.Severity,
                Status = grievance.Status,
                RaisedByReferenceId = grievance.DataPrincipal?.ReferenceId ?? "Unknown",
                CreatedAt = grievance.CreatedAt,
                Messages = filteredMessages.Select(m => new GrievanceMessageDto
                {
                    MessageText = m.MessageText,
                    IsInternal = m.IsInternal,
                    SenderType = m.SenderId == grievance.DataPrincipalId ? "User" : "System",
                    CreatedAt = m.CreatedAt
                }).OrderBy(m => m.CreatedAt).ToList()
            };
        }

        public async Task<bool> AssignTicketAsync(Guid id, Guid orgId, Guid adminId)
        {
            var grievance = await _grievanceRepo.GetGrievanceByIdAsync(id, orgId);
            if (grievance == null) return false;

            grievance.AssignedToId = adminId;
            grievance.Status = "In Progress";
            grievance.UpdatedAt = DateTime.UtcNow;
            await _grievanceRepo.UpdateGrievanceAsync(grievance);
            return true;
        }

       
        public async Task<bool> ResolveGrievanceAsync(Guid id, Guid orgId, Guid adminId)
        {
            var grievance = await _grievanceRepo.GetGrievanceByIdAsync(id, orgId);
            if (grievance == null) return false;

            // Optional check: Sirf wahi resolve kare jisko assign hui hai
            if (grievance.AssignedToId != adminId) throw new Exception("You are not assigned to this ticket.");

            grievance.Status = "Resolved";
            grievance.UpdatedAt = DateTime.UtcNow;
            await _grievanceRepo.UpdateGrievanceAsync(grievance);
            return true;
        }

        public async Task<bool> AddReplyAsync(Guid grievanceId, Guid userId, Guid orgId, AddGrievanceMessageDto dto, bool isAdmin)
        {
            var grievance = await _grievanceRepo.GetGrievanceByIdAsync(grievanceId, orgId);
            if (grievance == null) return false;

            var newMessage = new GrievanceMessage
            {
                GrievanceId = grievanceId,
                SenderId = userId,
                MessageText = dto.MessageText,
                IsInternal = isAdmin ? dto.IsInternal : false,
                CreatedAt = DateTime.UtcNow
            };
            await _grievanceRepo.AddMessageAsync(newMessage);
            return true;
        }

        private List<GrievanceDetailDto> MapToDtoList(List<Grievance> grievances)
        {
            return grievances.Select(g => new GrievanceDetailDto
            {
                Id = g.Id,
                ReferenceId = g.ReferenceId,
                Subject = g.Subject,
                Category = g.Category,
                Severity = g.Severity,
                Status = g.Status,
                RaisedByReferenceId = g.DataPrincipal?.ReferenceId ?? "Unknown",
                CreatedAt = g.CreatedAt
            }).ToList();
        }
    }
}