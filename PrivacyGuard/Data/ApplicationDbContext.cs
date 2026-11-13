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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // ==========================================
        // 1. DB SETS (All Tables)
        // ==========================================
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PrivacyRequest> PrivacyRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; } // 🚨 Naya Hash Chaining Wala Audit Log
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

        // ==========================================
        // 2. MODEL CREATING & MAPPINGS
        // ==========================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


          

            // DataInventory mapping
            modelBuilder.Entity<DataInventory>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrgUnit ka Parent-Child (Self-referencing) mapping
            modelBuilder.Entity<OrgUnit>()
                .HasOne<OrgUnit>()
                .WithMany()
                .HasForeignKey(o => o.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Global Cascade Delete restrict logic (Hamesha last mein)
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<NoticeTranslation>()
    .HasOne(nt => nt.Notice)
    .WithMany(n => n.Translations)
    .HasForeignKey(nt => nt.NoticeId)
    .OnDelete(DeleteBehavior.Cascade);


            // Unique database index on OrganizationId + UnitCode
            modelBuilder.Entity<PrivacyGuard.Models.OrgUnit>()
                .HasIndex(o => new { o.OrganizationId, o.UnitCode })
                .IsUnique();

            // Many-to-Many Keys
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

        // ==========================================
        // 3. AUDIT LOG & HASH CHAINING LOGIC
        // ==========================================
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

        private List<AuditEntry> OnBeforeSaveChanges(Dictionary<Guid, AuditLog> chainTails)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            // Token se User aur Actor details nikalna
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orgIdString = user?.FindFirst("OrganizationId")?.Value;
            var actorEmail = user?.FindFirst(ClaimTypes.Email)?.Value;
            var actorRole = user?.FindFirst(ClaimTypes.Role)?.Value;
            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            Guid? currentUserId = !string.IsNullOrEmpty(userIdString) ? Guid.Parse(userIdString) : null;
            Guid? currentOrgId = !string.IsNullOrEmpty(orgIdString) ? Guid.Parse(orgIdString) : null;

            foreach (var entry in ChangeTracker.Entries())
            {
                // Audit table aur unchanged rows ko skip karo
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = currentUserId,
                    OrganizationId = currentOrgId,
                    ActorEmail = actorEmail,
                    ActorRole = actorRole,
                    IpAddress = clientIp,
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
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            // Jinki IDs pehle se hain, unhe hash karke turant Audit table mein add karo
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

            // Nayi (Create) rows jinki ID DB ne generate ki, unko update karke hash lagana
            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                var auditLog = auditEntry.ToAudit();
                ApplyHashChaining(auditLog, chainTails);
                AuditLogs.Add(auditLog);
            }
        }

        private void ApplyHashChaining(AuditLog newLog, Dictionary<Guid, AuditLog> chainTails)
        {
            // 1. Pichla record nikalna (Taki chain jud sake)
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

            // 2. Naya Hash Banana: (Sequence + OrgId + Action + Entity + NewValues + PrevHash)
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