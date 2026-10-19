using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IBreachRepository
    {
        Task<DataBreachIncident> CreateIncidentAsync(DataBreachIncident incident);
        Task<List<DataBreachIncident>> GetAllIncidentsAsync(Guid organizationId);
        Task<DataBreachIncident?> GetIncidentByIdAsync(Guid incidentId, Guid organizationId);
        Task UpdateIncidentAsync(DataBreachIncident incident);
    }
}
