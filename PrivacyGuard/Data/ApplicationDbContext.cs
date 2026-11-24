using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrivacyGuard.Models;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PrivacyGuard.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly HashSet<string> SensitiveAuditProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(DataPrincipal.FullName),
            nameof(DataPrincipal.DisplayName),
            nameof(DataPrincipal.Email),
            nameof(DataPrincipal.Mobile),
            nameof(DataPrincipal.ConsumerNo),
            nameof(DataPrincipal.DateOfBirth),
            nameof(DataPrincipal.PasswordHash),
            nameof(PrincipalIdentifier.Value)
        };

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PrivacyRequest> PrivacyRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DataInventory> DataInventories { get; set; }
        public DbSet<DataBreachIncident> DataBreachIncidents { get; set; }
        public DbSet<CampaignResponse> CampaignResponses { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<DpiaRecord> DpiaRecords { get; set; }
        public DbSet<OrgUnit> OrgUnits { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Grievance> Grievances { get; set; }
        public DbSet<GrievanceMessage> GrievanceMessages { get; set; }
        public DbSet<BulkImportBatch> BulkImportBatches { get; set; }
        public DbSet<BulkImportRow> BulkImportRows { get; set; }
        public DbSet<DataPrincipal> DataPrincipals { get; set; }
        public DbSet<PrincipalIdentifier> PrincipalIdentifiers { get; set; }
        public DbSet<ConsentPurpose> ConsentPurposes { get; set; }
        public DbSet<ConsentNoticeTemplate> ConsentNoticeTemplates { get; set; }
        public DbSet<ConsentRecord> ConsentRecords { get; set; }
        public DbSet<ConsentDetail> ConsentDetails { get; set; }
        public DbSet<DataCategory> DataCategories { get; set; }
        public DbSet<DataProcessor> DataProcessors { get; set; }
        public DbSet<ConsentPurposeDataCategory> ConsentPurposeDataCategories { get; set; }
        public DbSet<ConsentPurposeDataProcessor> ConsentPurposeDataProcessors { get; set; }
        public DbSet<NoticeTemplate> NoticeTemplates { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<NoticeTranslation> NoticeTranslations { get; set; }
        public DbSet<CampaignAudienceMapping> CampaignAudienceMappings { get; set; }
        public DbSet<CampaignAudienceGroup> CampaignAudienceGroups { get; set; }
        public DbSet<CampaignAudienceGroupMember> CampaignAudienceGroupMembers { get; set; }
        public DbSet<RetentionPolicy> RetentionPolicies { get; set; }
        public DbSet<ReportJob> ReportJobs { get; set; }
        public DbSet<GeneratedReport> GeneratedReports { get; set; }
        public DbSet<RuleAndAct> RulesAndActs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RuleAndAct>()
                .HasIndex(r => r.OrganizationId);

            modelBuilder.Entity<DataInventory>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrgUnit>()
                .HasOne<OrgUnit>()
                .WithMany()
                .HasForeignKey(o => o.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<NoticeTranslation>()
                .HasOne(nt => nt.Notice)
                .WithMany(n => n.Translations)
                .HasForeignKey(nt => nt.NoticeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrievanceMessage>()
                .HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrievanceMessage>()
                .HasOne(message => message.SenderDataPrincipal)
                .WithMany()
                .HasForeignKey(message => message.SenderDataPrincipalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GrievanceMessage>()
                .ToTable(table => table.HasCheckConstraint(
                    "CK_GrievanceMessages_ExactlyOneSender",
                    "([SenderId] IS NOT NULL AND [SenderDataPrincipalId] IS NULL) OR ([SenderId] IS NULL AND [SenderDataPrincipalId] IS NOT NULL)"));


            // Unique database index on OrganizationId + UnitCode

            modelBuilder.Entity<PrivacyGuard.Models.OrgUnit>()
                .HasIndex(o => new { o.OrganizationId, o.UnitCode })
                .IsUnique();

            modelBuilder.Entity<ConsentPurposeDataCategory>()
                .HasKey(pc => new { pc.ConsentPurposeId, pc.DataCategoryId });

            modelBuilder.Entity<ConsentPurposeDataProcessor>()
                .HasKey(pp => new { pp.ConsentPurposeId, pp.DataProcessorId });

            modelBuilder.Entity<CampaignAudienceGroupMember>()
                .HasOne(member => member.Group)
                .WithMany(group => group.Members)
                .HasForeignKey(member => member.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampaignAudienceGroup>()
                .HasIndex(group => new { group.OrganizationId, group.Name })
                .IsUnique();

            modelBuilder.Entity<CampaignAudienceGroupMember>()
                .HasIndex(member => new { member.GroupId, member.MemberId, member.UserType })
                .IsUnique();

            modelBuilder.Entity<AuditLog>()
                .HasIndex(log => new { log.OrganizationId, log.SequenceNumber })
                .HasFilter("[OrganizationId] IS NOT NULL")
                .IsUnique();

            modelBuilder.Entity<AuditLog>()
                .HasIndex(log => log.SequenceNumber)
                .HasFilter("[OrganizationId] IS NULL")
                .IsUnique();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var transaction = Database.CurrentTransaction;
            var ownsTransaction = transaction == null;

            if (ownsTransaction)
            {
                transaction = await Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
            }

            try
            {
                var chainTails = new Dictionary<Guid, AuditLog>();
                var auditEntries = OnBeforeSaveChanges(chainTails);
                var result = await base.SaveChangesAsync(cancellationToken);

                OnAfterSaveChanges(auditEntries, chainTails);

                if (auditEntries.Count > 0)
                {
                    result += await base.SaveChangesAsync(cancellationToken);
                }

                if (ownsTransaction)
                {
                    await transaction!.CommitAsync(cancellationToken);
                }

                return result;
            }
            catch
            {
                if (ownsTransaction)
                {
                    await transaction!.RollbackAsync(cancellationToken);
                }

                throw;
            }
            finally
            {
                if (ownsTransaction)
                {
                    await transaction!.DisposeAsync();
                }
            }
        }

        private static object? SanitizeAuditValue(string entityName, string propertyName, object? value)
        {
            if (value == null) return null;

            if ((entityName is nameof(DataPrincipal) or nameof(PrincipalIdentifier)) &&
                SensitiveAuditProperties.Contains(propertyName))
            {
                return "***REDACTED***";
            }

            return value;
        }

        private List<AuditEntry> OnBeforeSaveChanges(Dictionary<Guid, AuditLog> chainTails)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orgIdString = user?.FindFirst("OrganizationId")?.Value;
            var actorEmail = user?.FindFirst(ClaimTypes.Email)?.Value;
            var actorRole = user?.FindFirst(ClaimTypes.Role)?.Value;
            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            Guid? currentUserId = Guid.TryParse(userIdString, out var parsedUserId) ? parsedUserId : null;
            Guid? currentOrgId = Guid.TryParse(orgIdString, out var parsedOrgId) ? parsedOrgId : null;

            bool isSystemActor = currentUserId == null && string.IsNullOrWhiteSpace(actorEmail);
            string resolvedActorEmail = !string.IsNullOrWhiteSpace(actorEmail) ? actorEmail : "System";
            string resolvedActorRole = !string.IsNullOrWhiteSpace(actorRole) ? actorRole : (isSystemActor ? "System" : "User");
            string resolvedIp = !string.IsNullOrWhiteSpace(clientIp) ? clientIp : (isSystemActor ? "System" : "Unknown");

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                string entityName = entry.Entity.GetType().Name;

                // Fallback OrganizationId from entity when running in background job without HTTP context
                Guid? entryOrgId = currentOrgId;
                if (entryOrgId == null && entry.Entity is DataPrincipal dpEntity)
                {
                    entryOrgId = dpEntity.OrganizationId;
                }
                else if (entryOrgId == null && entry.Entity is RetentionPolicy rpEntity)
                {
                    entryOrgId = rpEntity.OrganizationId;
                }

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entityName,
                    UserId = currentUserId,
                    OrganizationId = entryOrgId,
                    ActorEmail = resolvedActorEmail,
                    ActorRole = resolvedActorRole,
                    IpAddress = resolvedIp,
                    ActionType = entry.State.ToString()
                };

                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = SanitizeAuditValue(entityName, propertyName, property.CurrentValue);
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = SanitizeAuditValue(entityName, propertyName, property.OriginalValue);
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = SanitizeAuditValue(entityName, propertyName, property.OriginalValue);
                                auditEntry.NewValues[propertyName] = SanitizeAuditValue(entityName, propertyName, property.CurrentValue);
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.TemporaryProperties.Any()))
            {
                var auditLog = auditEntry.ToAudit();
                ApplyHashChaining(auditLog, chainTails);
                AuditLogs.Add(auditLog);
            }

            return auditEntries.Where(_ => _.TemporaryProperties.Any()).ToList();
        }

        private void OnAfterSaveChanges(
            List<AuditEntry> auditEntries,
            Dictionary<Guid, AuditLog> chainTails)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            foreach (var auditEntry in auditEntries)
            {
                string entityName = auditEntry.Entry.Entity.GetType().Name;

                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = SanitizeAuditValue(entityName, prop.Metadata.Name, prop.CurrentValue);
                    }
                }
                var auditLog = auditEntry.ToAudit();
                ApplyHashChaining(auditLog, chainTails);
                AuditLogs.Add(auditLog);
            }
        }

        private void ApplyHashChaining(AuditLog newLog, Dictionary<Guid, AuditLog> chainTails)
        {
            AuditLog? lastLog = null;
            var organizationKey = newLog.OrganizationId ?? Guid.Empty;

            chainTails.TryGetValue(organizationKey, out lastLog);

            if (lastLog == null)
            {
                var persistedLastLog = AuditLogs
                    .AsNoTracking()
                    .Where(a => a.OrganizationId == newLog.OrganizationId)
                    .OrderByDescending(a => a.SequenceNumber)
                    .FirstOrDefault();

                var trackedLastLog = AuditLogs.Local
                    .Where(a => a.OrganizationId == newLog.OrganizationId)
                    .OrderByDescending(a => a.SequenceNumber)
                    .FirstOrDefault();

                lastLog = persistedLastLog;
                if (trackedLastLog != null &&
                    (lastLog == null || trackedLastLog.SequenceNumber > lastLog.SequenceNumber))
                {
                    lastLog = trackedLastLog;
                }
            }

            newLog.SequenceNumber = (lastLog?.SequenceNumber ?? 0) + 1;
            newLog.PreviousHash = lastLog?.EntryHash;

            var rawData = $"{newLog.SequenceNumber}{newLog.OrganizationId}{newLog.ActionType}{newLog.TableName}{newLog.PrimaryKey}{newLog.NewValues}{newLog.PreviousHash}";

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(rawData);
                var hashBytes = sha256.ComputeHash(bytes);
                newLog.EntryHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            chainTails[organizationKey] = newLog;
        }
    }
}