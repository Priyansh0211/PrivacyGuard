using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IGrievanceRepository
    {
        Task<Grievance> CreateGrievanceAsync(Grievance grievance);
        Task<List<Grievance>> GetAllGrievancesAsync(Guid organizationId);
        Task<List<Grievance>> GetMyGrievancesAsync(Guid userId, Guid organizationId); // 🚨 Naya
        Task<Grievance?> GetGrievanceByIdAsync(Guid id, Guid organizationId);
        Task AddMessageAsync(GrievanceMessage message);
        Task UpdateGrievanceAsync(Grievance grievance);
    }
}