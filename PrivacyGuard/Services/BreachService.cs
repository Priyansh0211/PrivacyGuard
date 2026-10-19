using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class BreachService : IBreachService
    {
        private readonly IBreachRepository _breachRepository;

        public BreachService(IBreachRepository breachRepository)
        {
            _breachRepository = breachRepository;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ReportIncidentAsync(Guid userId, ReportBreachDto dto)
        {
            var incident = new DataBreachIncident
            {
                OrganizationId = dto.OrganizationId,
                Title = dto.Title,
                Description = dto.Description,
                IncidentDate = dto.IncidentDate,
                DiscoveryDate = dto.DiscoveryDate,
                ReportedByUserId = userId,
                Status = "Open",
                Severity = "Unknown" // Default jab tak admin check na kare
            };

            await _breachRepository.CreateIncidentAsync(incident);
            return (true, string.Empty);
        }

        public async Task<List<BreachResponseDto>> GetAllIncidentsAsync(Guid organizationId)
        {
            var incidents = await _breachRepository.GetAllIncidentsAsync(organizationId);

            return incidents.Select(i => new BreachResponseDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Severity = i.Severity,
                EstimatedAffectedRecords = i.EstimatedAffectedRecords,
                Status = i.Status,
                DiscoveryDate = i.DiscoveryDate,
                ReporterName = i.ReportedByUser?.FullName,
                AssignedUserName = i.AssignedUser?.FullName,
                MitigationSteps = i.MitigationSteps,
                CreatedAt = i.CreatedAt,
                ClosedAt = i.ClosedAt
            }).ToList();
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AssignIncidentAsync(Guid incidentId, Guid organizationId, Guid adminUserId)
        {
            var incident = await _breachRepository.GetIncidentByIdAsync(incidentId, organizationId);
            if (incident == null) return (false, "Incident not found.");

            // Smart Trigger: Assign hote hi status Investigating ho jayega
            incident.AssignedToUserId = adminUserId;
            incident.Status = "Investigating";

            await _breachRepository.UpdateIncidentAsync(incident);
            return (true, string.Empty);
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateIncidentStatusAsync(Guid incidentId, Guid organizationId, Guid adminUserId, UpdateBreachStatusDto dto)
        {
            var incident = await _breachRepository.GetIncidentByIdAsync(incidentId, organizationId);
            if (incident == null) return (false, "Incident not found.");

            // Data mapping
            incident.Status = dto.Status;

            if (!string.IsNullOrEmpty(dto.Severity))
                incident.Severity = dto.Severity;

            if (dto.EstimatedAffectedRecords.HasValue)
                incident.EstimatedAffectedRecords = dto.EstimatedAffectedRecords.Value;

            if (!string.IsNullOrEmpty(dto.MitigationSteps))
                incident.MitigationSteps = dto.MitigationSteps;

            // Agar ticket close ho rahi hai toh timestamp set kar do
            if (dto.Status == "Closed" && incident.ClosedAt == null)
            {
                incident.ClosedAt = DateTime.UtcNow;
            }

            await _breachRepository.UpdateIncidentAsync(incident);
            return (true, string.Empty);
        }
    }
}
