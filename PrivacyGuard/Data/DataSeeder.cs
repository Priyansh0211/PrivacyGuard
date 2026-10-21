using PrivacyGuard.Models;

namespace PrivacyGuard.Data
{
    public static class DataSeeder
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Agar pehle se data hai, toh kuch mat karo
            if (context.Organizations.Any())
            {
                return;
            }

            // 1. Create Organization (Client Demo - Hospital ya School)
            var orgId = Guid.NewGuid();
            var org = new Organization
            {
                OrganizationId = orgId,
                Name = "City Care Hospital", // Tum ise badal kar "Delhi Public School" bhi kar sakte ho
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Organizations.Add(org);

            // 2. Create Users (1 Admin, 1 Normal User)
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            context.Users.AddRange(
                new User { UserId = adminId, OrganizationId = orgId, FullName = "Admin Sir", Email = "admin@citycare.com", Role = "Admin", PasswordHash = "dummyhash", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = userId, OrganizationId = orgId, FullName = "Rahul Patient", Email = "rahul@gmail.com", Role = "NormalUser", PasswordHash = "dummyhash", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // 3. Create Data Inventory
            var inventoryId = Guid.NewGuid();
            context.DataInventories.Add(new DataInventory
            {
                DataInventoryId = inventoryId,
                OrganizationId = orgId,
                Name = "Patient Medical Records",
                DataCategory = "Health Data",
                Purpose = "Treatment & Diagnosis",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // 4. Create Campaign
            var campaignId = Guid.NewGuid();
            context.Campaigns.Add(new Campaign
            {
                CampaignId = campaignId,
                OrganizationId = orgId,
                Title = "New Health Data Policy 2026",
                CampaignType = "Awareness",
                TargetRole = "All",
                Status = "Active",
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(15),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DataInventoryId = inventoryId
            });

            // 5. Create Consent Record (Active)
            context.ConsentRecords.Add(new ConsentRecords
            {
                ConsentRecordId = Guid.NewGuid(),
                OrganizationId = orgId,
                DataPrincipalId = userId.ToString(),
                DataInventoryId = inventoryId,
                ConsentStatus = "Granted",
                NoticeVersion = "v1.0",
                GrantedAt = DateTime.UtcNow
            });

            // 6. Create Privacy Requests (Ek Pending, Ek SLA Risk wali)
            context.PrivacyRequests.AddRange(
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = userId.ToString(), RequestType = "Data Deletion", RequestDetails = "Please delete my old X-Ray reports", Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = userId.ToString(), RequestType = "Data Access", RequestDetails = "I need my complete medical history", Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-28) } // Yeh SLA Risk mein dikhegi (25 din purani)
            );  

            // 7. Create Data Breach Incident
            context.DataBreachIncidents.Add(new DataBreachIncident
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                Title = "Unauthorized Access to Lab Results",
                Description = "An intern accessed reports without permission.",
                Severity = "High",
                IncidentDate = DateTime.UtcNow.AddDays(-3),
                DiscoveryDate = DateTime.UtcNow.AddDays(-2),
                EstimatedAffectedRecords = 150,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            });

            // Save sab kuch ek sath!
            context.SaveChanges();
        }
    }
}