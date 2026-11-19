using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // .Include() ke liye ye bahut zaroori hai
using PrivacyGuard.Data;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;

namespace PrivacyGuard.Repositories
{
    public class DataPrincipalRepository : IDataPrincipalRepository
    {
        private readonly ApplicationDbContext _context;

        public DataPrincipalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DataPrincipal> AddDataPrincipalAsync(DataPrincipal dataPrincipal)
        {
            await _context.DataPrincipals.AddAsync(dataPrincipal);
            return dataPrincipal;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ==========================================
        // NAYA METHOD: EF Core Query for 360-View
        // ==========================================
        public async Task<DataPrincipal?> Get360ViewEntityAsync(Guid dataPrincipalId, Guid organizationId)
        {
            // Ek hi query mein DataPrincipal ke sath uski saari related lists aur nested tables fetch kar li
            return await _context.DataPrincipals
                .Include(dp => dp.Role)
                .Include(dp => dp.Identifiers)
                .Include(dp => dp.PrivacyRequests)
                .Include(dp => dp.ConsentRecords)
                    .ThenInclude(cr => cr.ConsentDetails)
                        .ThenInclude(cd => cd.ConsentPurpose) // Ye include real purpose name laane ke liye zaroori hai
                .FirstOrDefaultAsync(dp => dp.DataPrincipalId == dataPrincipalId
                                        && dp.OrganizationId == organizationId);
        }
        public async Task<(List<DataPrincipal> Principals, int TotalCount)> GetPaginatedDataPrincipalsAsync(
            Guid organizationId, string search, string classType, int page, int pageSize)
        {
            var query = _context.DataPrincipals.AsQueryable()
                                .Where(dp => dp.OrganizationId == organizationId);

            // Filters apply karna
            if (!string.IsNullOrEmpty(classType))
            {
                query = query.Where(dp => dp.Class.ToLower() == classType.ToLower());
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(dp =>
                    (dp.FullName != null && dp.FullName.ToLower().Contains(search)) ||
                    (dp.Email != null && dp.Email.ToLower().Contains(search)) ||
                    (dp.Mobile != null && dp.Mobile.Contains(search)) ||
                    dp.ReferenceId.ToLower().Contains(search)
                );
            }

            int totalCount = await query.CountAsync();

            var principals = await query
                .OrderByDescending(dp => dp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (principals, totalCount);
        }
        // ==========================================
        // FAST DASHBOARD COUNT (Optimized for Millions of rows)
        // ==========================================
        public async Task<int> GetTotalPrincipalsCountAsync(Guid organizationId, Guid? orgUnitId = null)
        {
            var query = _context.DataPrincipals.Where(dp => dp.OrganizationId == organizationId);

            // Agar circle filter pass hua hai, toh sirf us circle ka count nikalo
            if (orgUnitId.HasValue)
            {
                query = query.Where(dp => dp.OrgUnitId == orgUnitId.Value);
            }

            // CountAsync DB engine level par chalta hai, memory mein data nahi laata (Ultra fast)
            return await query.CountAsync();
        }

        // ==========================================
        // BULK IMPORT (CSV / EXCEL DATA KE LIYE)
        // ==========================================
        public async Task AddBulkDataPrincipalsAsync(List<DataPrincipal> dataPrincipals)
        {
            // AddRangeAsync 1000s of records ko ek sath batch banakar DB mein bhejta hai
            await _context.DataPrincipals.AddRangeAsync(dataPrincipals);
            await _context.SaveChangesAsync();
        }
    }
}