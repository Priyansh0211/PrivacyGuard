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
                Id = l.Id,
                UserId = l.UserId,
                IpAddress = l.IpAddress,
                TableName = l.TableName,
                ActionType = l.ActionType,
                PrimaryKey = l.PrimaryKey,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                Timestamp = l.Timestamp
            }).ToList();
        }
    }
}
