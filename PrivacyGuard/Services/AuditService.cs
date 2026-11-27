using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;

namespace PrivacyGuard.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

     
           public async Task<List<AuditLogDto>> GetOrganizationLogsAsync(Guid organizationId)
        {
            var logs = await _auditRepository.GetLogsByOrganizationAsync(organizationId);

            return logs.Select(l => new AuditLogDto
            {
                Seq = l.SequenceNumber,
                When = l.Timestamp,
                Action = l.ActionType,
                Entity = $"{l.TableName} {l.PrimaryKey}", // Dikhane ke liye merge kiya
                Actor = $"{l.ActorEmail}\n{l.ActorRole}", // UI ke hisaab se 2 lines
                IpAddress = l.IpAddress,
                Outcome = l.Outcome,
                EntryHash = l.EntryHash // 🚨 Screenshot ke hisaab se sabse important[cite: 2]
            }).ToList();
        }

        public Task RecordAuthenticationEventAsync(Guid userId, Guid organizationId, string actorEmail, string actorRole, string? ipAddress, string action)
        {
            return _auditRepository.RecordAuthenticationEventAsync(userId, organizationId, actorEmail, actorRole, ipAddress, action);
        }
    }
    
}
