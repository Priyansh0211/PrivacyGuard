using PrivacyGuard.Models;

namespace PrivacyGuard.Data
{
    public static class DataSeeder
    {
        public static void Initialize(ApplicationDbContext context)
        {

            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
    {
        // 🟢 Module 1: User & Role Management (Settings)
        new Permission { ModuleName = "User & Role Management", ActionName = "View Users", SystemName = "Users.View" },
        new Permission { ModuleName = "User & Role Management", ActionName = "Add New User", SystemName = "Users.Create" },
        new Permission { ModuleName = "User & Role Management", ActionName = "Edit User Status", SystemName = "Users.Edit" },
        new Permission { ModuleName = "User & Role Management", ActionName = "Manage Roles & Permissions", SystemName = "Roles.Manage" },

        // 🟢 Module 2: DPIA Management
        new Permission { ModuleName = "DPIA Management", ActionName = "View DPIA Records", SystemName = "DPIA.View" },
        new Permission { ModuleName = "DPIA Management", ActionName = "Create New DPIA", SystemName = "DPIA.Create" },
        new Permission { ModuleName = "DPIA Management", ActionName = "Approve / Reject DPIA", SystemName = "DPIA.Approve" },

        // 🟢 Module 3: Privacy & Grievance Requests
        new Permission { ModuleName = "Privacy Requests", ActionName = "View Requests", SystemName = "PrivacyRequest.View" },
        new Permission { ModuleName = "Privacy Requests", ActionName = "Resolve Requests", SystemName = "PrivacyRequest.Resolve" },

        // 🟢 Module 4: Consent Management
        new Permission { ModuleName = "Consent Management", ActionName = "View Consent Records", SystemName = "Consent.View" },
        
        // 🟢 Module 5: Data Inventory
        new Permission { ModuleName = "Data Inventory", ActionName = "View Inventory", SystemName = "Inventory.View" },
        new Permission { ModuleName = "Data Inventory", ActionName = "Manage Inventory", SystemName = "Inventory.Manage" }
    };

                context.Permissions.AddRange(permissions);
                context.SaveChanges();
            }
            

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
                Name = "City Care Hospital",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Organizations.Add(org);

            var adminRoleId = Guid.NewGuid();
            var normalUserRoleId = Guid.NewGuid();
            var dpoRoleId = Guid.NewGuid();

            context.Roles.AddRange(
                new Role { Id = adminRoleId, OrganizationId = orgId, Name = "Admin", Description = "System Administrator (Maker)", CreatedAt = DateTime.UtcNow },
                new Role { Id = dpoRoleId, OrganizationId = orgId, Name = "DPO", Description = "Data Protection Officer (Checker)", CreatedAt = DateTime.UtcNow },
                new Role { Id = normalUserRoleId, OrganizationId = orgId, Name = "NormalUser", Description = "Data Principal / Patient", CreatedAt = DateTime.UtcNow }
            );

            // 2. Create Users (Role string ki jagah RoleId pass kiya gaya hai)
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            context.Users.AddRange(
                new User
                {
                    UserId = adminId,
                    OrganizationId = orgId,
                    FullName = "Admin Sir",
                    Email = "admin@citycare.com",
                    RoleId = adminRoleId, 
                    PasswordHash = "dummyhash",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = userId,
                    OrganizationId = orgId,
                    FullName = "Rahul Patient",
                    Email = "rahul@gmail.com",
                    RoleId = normalUserRoleId,
                    PasswordHash = "dummyhash",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
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
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = userId.ToString(), RequestType = "Data Access", RequestDetails = "I need my complete medical history", Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-28) }
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