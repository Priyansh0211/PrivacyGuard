using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Helpers;
using PrivacyGuard.Interfaces.Repositories;
using PrivacyGuard.Models;
using System;
using System.Threading.Tasks;

namespace PrivacyGuard.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ReportJob> CreateJobAsync(ReportJob job)
        {
            await _dbContext.ReportJobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();
            return job;
        }

        public async Task<ReportJob?> GetJobByIdAsync(Guid jobId)
        {
            return await _dbContext.ReportJobs.FindAsync(jobId);
        }

        public async Task<ReportJob> UpdateJobAsync(ReportJob job)
        {
            _dbContext.ReportJobs.Update(job);
            await _dbContext.SaveChangesAsync();
            return job;
        }

        public async Task SaveGeneratedReportAsync(GeneratedReport report)
        {
            await _dbContext.GeneratedReports.AddAsync(report);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReportJob>> GetJobsByOrganizationAsync(Guid organizationId)
        {
            return await _dbContext.ReportJobs
                .Where(j => j.OrganizationId == organizationId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<GeneratedReport?> GetReportByJobIdAsync(Guid jobId, Guid organizationId)
        {
            // Tenant isolation yahan bhi zaroori hai taaki koi dusri org ka report download na kar le
            return await _dbContext.GeneratedReports
                .FirstOrDefaultAsync(r => r.JobId == jobId && r.OrganizationId == organizationId);
        }

        public async Task<IEnumerable<GrievanceReportExportDto>> GetGrievanceReportDataAsync(Guid organizationId)
        {
            return await _dbContext.Grievances
                .Where(g => g.OrganizationId == organizationId)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GrievanceReportExportDto
                {
                    ReferenceId = g.ReferenceId,
                    Subject = g.Subject,    
                    Category = g.Category,
                    Severity = g.Severity,
                    Status = g.Status,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<GrievanceAuditExportDto>> GetGrievanceAuditReportDataAsync(Guid organizationId)
        {
            return await _dbContext.Grievances
                .Include(g => g.DataPrincipal)
                .Include(g => g.AssignedAdmin)
                .Where(g => g.OrganizationId == organizationId)
                .OrderByDescending(g => g.UpdatedAt)
                .Select(g => new GrievanceAuditExportDto
                {
                    ReferenceId = g.ReferenceId,
                    Subject = g.Subject,
                    Category = g.Category,
                    Severity = g.Severity,
                    Status = g.Status,
                    
                    DataPrincipalEmail = PiiMaskingHelper.MaskEmail(g.DataPrincipal != null ? g.DataPrincipal.Email : string.Empty),
                    AssignedAdminEmail = g.AssignedAdmin != null ? g.AssignedAdmin.Email : "Unassigned",
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();
        }

        
        public async Task<IEnumerable<ConsentAuditExportDto>> GetConsentAuditReportDataAsync(Guid organizationId)
        {
            return await _dbContext.ConsentDetails
                .Include(cd => cd.ConsentRecord)
                .Include(cd => cd.ConsentPurpose)
                .Where(cd => cd.ConsentRecord.OrganizationId == organizationId)
                .OrderByDescending(cd => cd.ExpiryDate)
                .Select(cd => new ConsentAuditExportDto
                {
                    ConsentDetailId = cd.ConsentDetailId,
                    ConsentRecordId = cd.ConsentRecordId,
                    ConsentPurposeId = cd.ConsentPurposeId,
                    Decision = cd.Decision,
                    Status = cd.Status,
                    ExpiryDate = cd.ExpiryDate.HasValue ? cd.ExpiryDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "No Expiry"
                })
                .ToListAsync();
        }

        
        public async Task<IEnumerable<DataBreachExportDto>> GetDataBreachReportDataAsync(Guid organizationId)
        {
            return await _dbContext.DataBreachIncidents
                .Where(b => b.OrganizationId == organizationId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new DataBreachExportDto
                {
                    Title = b.Title,
                    Severity = b.Severity,
                    Status = b.Status,
                    EstimatedAffectedRecords = b.EstimatedAffectedRecords,
                    IncidentDate = b.IncidentDate,
                    DiscoveryDate = b.DiscoveryDate,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DpiaReportExportDto>> GetDpiaReportDataAsync(Guid organizationId)
        {
            return await _dbContext.DpiaRecords
                .Where(d => d.OrganizationId == organizationId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DpiaReportExportDto
                {
                    ReferenceId = d.ReferenceId,
                    Title = d.Title,
                    PurposesCount = d.PurposesCount,
                    ResidualRisk = d.ResidualRisk,
                    Status = d.Status,
                    CreatedByUserId = d.CreatedByUserId,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DsarReportExportDto>> GetDsarReportDataAsync(Guid organizationId)
        {
            return await _dbContext.PrivacyRequests
                .AsNoTracking()
                .Where(r => r.OrganizationId == organizationId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new DsarReportExportDto
                {
                    ReferenceId = r.ReferenceId,
                    DataPrincipalReferenceId = r.DataPrincipal != null ? r.DataPrincipal.ReferenceId : string.Empty,
                    RequestType = r.RequestType.ToString(),
                    Status = r.Status.ToString(),
                    TotalTasks = r.TotalTasks,
                    CompletedTasks = r.CompletedTasks,
                    CreatedAt = r.CreatedAt,
                    TargetResolutionDate = r.TargetResolutionDate,
                    ResolvedAt = r.ResolvedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DataInventoryReportExportDto>> GetDataInventoryReportDataAsync(Guid organizationId)
        {
            return await _dbContext.DataInventories
                .AsNoTracking()
                .Where(d => d.OrganizationId == organizationId)
                .OrderBy(d => d.Name)
                .Select(d => new DataInventoryReportExportDto
                {
                    Name = d.Name,
                    DataCategory = d.DataCategory,
                    Purpose = d.Purpose,
                    DataSource = d.DataSource,
                    StorageLocation = d.StorageLocation,
                    RetentionPeriodDays = d.RetentionPeriodDays,
                    Status = d.Status,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DataProcessingReportExportDto>> GetDataProcessingReportDataAsync(Guid organizationId)
        {
            return await _dbContext.ConsentPurposeDataCategories
                .AsNoTracking()
                .Where(m => m.ConsentPurpose.OrganizationId == organizationId)
                .OrderBy(m => m.ConsentPurpose.ShortName)
                .Select(m => new DataProcessingReportExportDto
                {
                    Purpose = m.ConsentPurpose.ShortName,
                    DataCategory = m.DataCategory.Name,
                    LegalBasis = m.ConsentPurpose.LegalBasis,
                    Status = m.ConsentPurpose.IsActive ? "Active" : "Inactive",
                    CreatedAt = m.ConsentPurpose.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DataRetentionReportExportDto>> GetDataRetentionReportDataAsync(Guid organizationId)
        {
            var inventory = await _dbContext.DataInventories
                .AsNoTracking()
                .Where(d => d.OrganizationId == organizationId)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var today = DateTime.UtcNow;
            return inventory.Select(d => new DataRetentionReportExportDto
            {
                DataElement = d.Name,
                DataCategory = d.DataCategory,
                RetentionPeriodDays = d.RetentionPeriodDays,
                ExpectedDeletionDate = d.RetentionPeriodDays.HasValue ? d.CreatedAt.AddDays(d.RetentionPeriodDays.Value) : null,
                ComplianceStatus = !d.RetentionPeriodDays.HasValue
                    ? "Gap"
                    : d.CreatedAt.AddDays(d.RetentionPeriodDays.Value) < today ? "Review required" : "Configured",
                Status = d.Status
            }).ToList();
        }

        public async Task<IEnumerable<DataSharingReportExportDto>> GetDataSharingReportDataAsync(Guid organizationId)
        {
            return await _dbContext.DataProcessors
                .AsNoTracking()
                .Where(p => p.OrganizationId == organizationId)
                .OrderBy(p => p.LegalName)
                .Select(p => new DataSharingReportExportDto
                {
                    Processor = p.Processor,
                    LegalName = p.LegalName,
                    Relationship = p.Relationship,
                    Country = p.Country,
                    Location = p.Location,
                    DataLeavesIndia = p.DataLeavesIndia
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SecurityAuditReportExportDto>> GetSecurityAuditReportDataAsync(Guid organizationId)
        {
            return await _dbContext.AuditLogs
                .AsNoTracking()
                .Where(a => a.OrganizationId == organizationId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new SecurityAuditReportExportDto
                {
                    SequenceNumber = a.SequenceNumber,
                    ActorEmail = a.ActorEmail,
                    ActorRole = a.ActorRole,
                    TableName = a.TableName,
                    ActionType = a.ActionType,
                    Outcome = a.Outcome,
                    Timestamp = a.Timestamp,
                    EntryHash = a.EntryHash
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ComplianceGapReportExportDto>> GetComplianceGapReportDataAsync(Guid organizationId)
        {
            var inventoryCount = await _dbContext.DataInventories.CountAsync(d => d.OrganizationId == organizationId);
            var retentionConfiguredCount = await _dbContext.DataInventories.CountAsync(d => d.OrganizationId == organizationId && d.RetentionPeriodDays.HasValue);
            var activePurposeCount = await _dbContext.ConsentPurposes.CountAsync(p => p.OrganizationId == organizationId && p.IsActive);
            var processorCount = await _dbContext.DataProcessors.CountAsync(p => p.OrganizationId == organizationId);
            var auditCount = await _dbContext.AuditLogs.CountAsync(a => a.OrganizationId == organizationId);

            return new List<ComplianceGapReportExportDto>
            {
                new()
                {
                    Area = "Data inventory",
                    Requirement = "Maintain an inventory of personal data",
                    CurrentStatus = inventoryCount > 0 ? $"{inventoryCount} record(s)" : "Not configured",
                    Gap = inventoryCount > 0 ? "None identified" : "Inventory is empty",
                    RecommendedAction = inventoryCount > 0 ? "Review inventory regularly" : "Add personal data elements"
                },
                new()
                {
                    Area = "Retention",
                    Requirement = "Define retention periods for personal data",
                    CurrentStatus = $"{retentionConfiguredCount}/{inventoryCount} configured",
                    Gap = retentionConfiguredCount == inventoryCount ? "None identified" : "Retention period missing",
                    RecommendedAction = "Configure retention periods for every inventory item"
                },
                new()
                {
                    Area = "Consent and processing",
                    Requirement = "Document active processing purposes and legal basis",
                    CurrentStatus = $"{activePurposeCount} active purpose(s)",
                    Gap = activePurposeCount > 0 ? "None identified" : "No active processing purpose",
                    RecommendedAction = "Create and review processing purposes"
                },
                new()
                {
                    Area = "Third-party sharing",
                    Requirement = "Maintain a register of data processors",
                    CurrentStatus = $"{processorCount} processor(s)",
                    Gap = processorCount > 0 ? "None identified" : "Processor register is empty",
                    RecommendedAction = "Record and review third-party processors"
                },
                new()
                {
                    Area = "Security audit",
                    Requirement = "Maintain security and access audit evidence",
                    CurrentStatus = $"{auditCount} audit event(s)",
                    Gap = auditCount > 0 ? "None identified" : "No audit evidence available",
                    RecommendedAction = "Review audit logging configuration"
                }
            };
        }


    }
}