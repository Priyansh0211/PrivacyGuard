using System.Threading.Tasks;
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class ConsumerRepository : IConsumerRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor add kiya taaki _context setup ho sake
        public ConsumerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Consumer> AddConsumerAsync(Consumer consumer)
        {
            await _context.Consumers.AddAsync(consumer);
            return consumer;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}