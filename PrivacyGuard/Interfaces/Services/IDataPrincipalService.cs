using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrivacyGuard.DTOs;
using PrivacyGuard.Models;

namespace PrivacyGuard.Interfaces.Services
{
    public interface IDataPrincipalService
    {
        Task<DataPrincipal> CreateSingleDataPrincipalAsync(DataPrincipalDto dto, Guid organizationId);
        Task<DataPrincipal360Dto?> Get360ViewAsync(Guid dataPrincipalId, Guid organizationId);
        Task<DataPrincipalListResponseDto> GetDataPrincipalsListAsync(Guid organizationId, string search, string classType, int page, int pageSize);
        Task<bool> AnonymizeDataPrincipalAsync(Guid dataPrincipalId, Guid organizationId);
        Task<(int SuccessCount, string Message)> BulkImportPrincipalsAsync(List<DataPrincipalDto> dtos, Guid organizationId, Guid? orgUnitId = null);
        Task<int> GetTotalPrincipalsCountAsync(Guid organizationId, Guid? orgUnitId = null);
        Task<bool> UpdateDataPrincipalAsync(Guid id, DataPrincipalDto dto, Guid organizationId);
    }
}