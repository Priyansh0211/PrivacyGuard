using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;
using PrivacyGuard.Repositories;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Services
{
    public class DataPrincipalService : IDataPrincipalService
    {
        private readonly IDataPrincipalRepository _repository;

        // Constructor update ho gaya IDataPrincipalRepository ke sath
        public DataPrincipalService(IDataPrincipalRepository repository)
        {
            _repository = repository;
        }

        public async Task<DataPrincipal> CreateSingleDataPrincipalAsync(DataPrincipalDto dto, Guid organizationId)
        {
            // Null check laga diya taaki error na aaye
            bool requiresGuardian = !string.IsNullOrEmpty(dto.Class) &&
                (dto.Class.ToLower() == "child" || dto.Class.ToLower() == "person_with_disability");

            var dataPrincipal = new DataPrincipal
            {
                DataPrincipalId = Guid.NewGuid(),
                OrganizationId = organizationId,
                ReferenceId = $"PSD-{new Random().Next(10000000, 99999999)}",
                Mobile = dto.Mobile,
                Email = dto.Email,
                ConsumerNo = dto.ConsumerNo, // DTO mein is property ka dhyan rakhna
                FullName = dto.FullName,
                DateOfBirth = string.IsNullOrEmpty(dto.DateOfBirth) ? null : DateTime.Parse(dto.DateOfBirth),
                Class = dto.Class,
                PreferredLocale = dto.PreferredLocale,
                RequiresGuardianConsent = requiresGuardian,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddDataPrincipalAsync(dataPrincipal);
            await _repository.SaveChangesAsync();

            return dataPrincipal;
        }
    }
}