using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IDataPrincipalService
    {
        Task<DataPrincipal> CreateSingleDataPrincipalAsync(DataPrincipalDto dto, Guid organizationId, Guid? orgUnitId = null);

        Task<DataPrincipal360Dto?> Get360ViewAsync(Guid dataPrincipalId, Guid organizationId, string? callerRole, Guid callerUnitId);

        Task<DataPrincipalListResponseDto> GetDataPrincipalsListAsync(
            Guid organizationId, string search, string classType, int page, int pageSize,
            string? callerRole, Guid callerUnitId);

        // DSAR (Privacy Request) flow ke liye: branch check nahi
        Task<bool> AnonymizeDataPrincipalAsync(Guid dataPrincipalId, Guid organizationId);

        // Data Principal page ke liye: branch scoped
        Task<bool> AnonymizeDataPrincipalAsync(Guid dataPrincipalId, Guid organizationId, string? callerRole, Guid callerUnitId);

        Task<(int SuccessCount, string Message)> BulkImportPrincipalsAsync(List<DataPrincipalDto> dtos, Guid organizationId, Guid? orgUnitId = null);

        Task<int> GetTotalPrincipalsCountAsync(Guid organizationId, Guid? orgUnitId, string? callerRole, Guid callerUnitId);

        Task<bool> UpdateDataPrincipalAsync(Guid id, DataPrincipalDto dto, Guid organizationId, string? callerRole, Guid callerUnitId);
    }
}