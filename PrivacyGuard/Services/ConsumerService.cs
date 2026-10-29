using System;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IConsumerRepository _repository;

        // Constructor add kiya taaki _repository setup ho sake
        public ConsumerService(IConsumerRepository repository)
        {
            _repository = repository;
        }

        public async Task<Consumer> CreateSingleConsumerAsync(CreateConsumerDto dto, Guid organizationId)
        {
            bool requiresGuardian = dto.Class.ToLower() == "child" || dto.Class.ToLower() == "person_with_disability";

            var consumer = new Consumer
            {
                ConsumerId = Guid.NewGuid(),
                OrganizationId = organizationId,
                ReferenceId = $"PSD-{new Random().Next(10000000, 99999999)}",
                Mobile = dto.Mobile,
                Email = dto.Email,
                ConsumerNo = dto.ConsumerNo,
                FullName = dto.FullName,
                DateOfBirth = string.IsNullOrEmpty(dto.DateOfBirth) ? null : DateTime.Parse(dto.DateOfBirth),
                Class = dto.Class,
                PreferredLocale = dto.PreferredLocale,
                RequiresGuardianConsent = requiresGuardian,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddConsumerAsync(consumer);
            await _repository.SaveChangesAsync();

            return consumer;
        }
    }
}