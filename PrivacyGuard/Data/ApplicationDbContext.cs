using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Models;
using System.Security.Claims;

namespace PrivacyGuard.Data
{

    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options , IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Organization> Organizations { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<ConsentRecords> ConsentRecords { get; set; }
        public DbSet<PrivacyRequest> PrivacyRequests { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<DataInventory> DataInventories { get; set; }

        public DbSet<DataBreachIncident> DataBreachIncidents { get; set; }

        public DbSet<CampaignResponse> CampaignResponses { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<DpiaRecord> DpiaRecords { get; set; }
        public DbSet<OrgUnit> OrgUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataInventory>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            // Token se user detail nikalna (Agar user logged in hai)
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orgIdString = _httpContextAccessor.HttpContext?.User?.FindFirst("OrganizationId")?.Value;

            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            Guid? currentUserId = !string.IsNullOrEmpty(userIdString) ? Guid.Parse(userIdString) : null;
            Guid? currentOrgId = !string.IsNullOrEmpty(orgIdString) ? Guid.Parse(orgIdString) : null;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = currentUserId,
                    OrganizationId = currentOrgId,
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

            // Jo records mein temporary properties nahi hain (matlab ID pehle se hai), unhe turant Audit table mein daal do
            foreach (var auditEntry in auditEntries.Where(_ => !_.TemporaryProperties.Any()))
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }

            return auditEntries.Where(_ => _.TemporaryProperties.Any()).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            // Agar koi nayi row (Create) bani thi jiski ID DB ne generate ki, usko update karna
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
                AuditLogs.Add(auditEntry.ToAudit());
            }

            return SaveChangesAsync();
        }
    }
}