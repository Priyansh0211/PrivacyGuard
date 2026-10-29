using System.Threading.Tasks;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Repositories
{
    public interface IConsumerRepository
    {
        Task<Consumer> AddConsumerAsync(Consumer consumer);
        Task SaveChangesAsync();
    }
}