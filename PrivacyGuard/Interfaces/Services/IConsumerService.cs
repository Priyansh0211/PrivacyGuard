using System;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IConsumerService
    {
        Task<Consumer> CreateSingleConsumerAsync(CreateConsumerDto dto, Guid organizationId);
    }
}