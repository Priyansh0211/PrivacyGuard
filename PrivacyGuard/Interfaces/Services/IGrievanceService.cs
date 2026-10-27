using PrivacyGuard.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IGrievanceService
    {
        Task<string> CreateGrievanceAsync(Guid userId, Guid orgId, CreateGrievanceDto dto);
        Task<List<GrievanceDetailDto>> GetAllGrievancesAsync(Guid orgId);
        Task<GrievanceDetailDto?> GetGrievanceDetailsAsync(Guid id, Guid orgId, bool isAdmin);
        Task<bool> AssignTicketAsync(Guid id, Guid orgId, Guid adminId);
        Task<bool> AddReplyAsync(Guid grievanceId, Guid userId, Guid orgId, AddGrievanceMessageDto dto, bool isAdmin);
    }
}