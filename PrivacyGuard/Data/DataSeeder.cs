using PrivacyGuard.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrivacyGuard.Data
{
    public static class DataSeeder
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // 1. 🟢 SEED PERMISSIONS
            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
                {
                    // Module 1: User & Role Management (Settings)
                    new Permission { ModuleName = "User & Role Management", ActionName = "View Users", SystemName = "Users.View" },
                    new Permission { ModuleName = "User & Role Management", ActionName = "Add New User", SystemName = "Users.Create" },
                    new Permission { ModuleName = "User & Role Management", ActionName = "Edit User Status", SystemName = "Users.Edit" },
                    new Permission { ModuleName = "User & Role Management", ActionName = "Manage Roles & Permissions", SystemName = "Roles.Manage" },

                    // Module 2: DPIA Management
                    new Permission { ModuleName = "DPIA Management", ActionName = "View DPIA Records", SystemName = "DPIA.View" },
                    new Permission { ModuleName = "DPIA Management", ActionName = "Create New DPIA", SystemName = "DPIA.Create" },
                    new Permission { ModuleName = "DPIA Management", ActionName = "Approve / Reject DPIA", SystemName = "DPIA.Approve" },

                    // Module 3: Privacy & Grievance Requests
                    new Permission { ModuleName = "Privacy Requests", ActionName = "View Requests", SystemName = "PrivacyRequest.View" },
                    new Permission { ModuleName = "Privacy Requests", ActionName = "Resolve Requests", SystemName = "PrivacyRequest.Resolve" },

                    // Module 4: Consent Management
                    new Permission { ModuleName = "Consent Management", ActionName = "View Consent Records", SystemName = "Consent.View" },
                    
                    // 🚨 Module 5: Data Inventory (FIXED: Controller ke HasPermission attribute se match kar diya)
                    new Permission { ModuleName = "Data Inventory", ActionName = "View Inventory", SystemName = "DataInventory.View" },
                    new Permission { ModuleName = "Data Inventory", ActionName = "Manage Inventory", SystemName = "DataInventory.Manage" },

                    // Module 6: Grievance Management
                    new Permission { ModuleName = "Grievance Management", ActionName = "View Grievances", SystemName = "Grievances.View" },
                    new Permission { ModuleName = "Grievance Management", ActionName = "Manage Grievances", SystemName = "Grievances.Manage" },

                    // Module 7: Breach Management
                    new Permission { ModuleName = "Breach Management", ActionName = "View Breaches", SystemName = "Breaches.View" },
                    new Permission { ModuleName = "Breach Management", ActionName = "Manage Breaches", SystemName = "Breaches.Manage" },

                    // Module 8: Campaign Management
                    new Permission { ModuleName = "Campaign Management", ActionName = "View Campaigns", SystemName = "Campaigns.View" },
                    new Permission { ModuleName = "Campaign Management", ActionName = "Create Campaigns", SystemName = "Campaigns.Create" },

                    new Permission { ModuleName = "Audit Management", ActionName = "Audit View", SystemName = "Audit.View" },
                    new Permission { ModuleName = "Notice Template", ActionName = "Notice Template View", SystemName = "NoticeTemplates.View" },
                    new Permission { ModuleName = "Notice Template", ActionName = "Notice Template Manage", SystemName = "NoticeTemplates.Manage" },
                    
                    // Module 9: Organization & OrgUnit Management
                    new Permission { ModuleName = "Organization", ActionName = "Organization View", SystemName = "OrgUnit.View" },
                    new Permission { ModuleName = "Organization", ActionName = "Organization Manage", SystemName = "OrgUnit.Manage" },
                    
                    // Module 10: Retention Policy Management
                    new Permission { ModuleName = "Retention Policy Management", ActionName = "View Retention Policies", SystemName = "RetentionPolicy.View" },
                    new Permission { ModuleName = "Retention Policy Management", ActionName = "Manage Retention Policies", SystemName = "RetentionPolicy.Manage" }
                };

                context.Permissions.AddRange(permissions);
                context.SaveChanges();
            }

            // Agar organization pehle se hai, toh aage ka data seed mat karo
            if (context.Organizations.Any()) return;

            // 2. 🟢 SEED ORGANIZATION
            var orgId = Guid.NewGuid();
            var org = new Organization
            {
                OrganizationId = orgId,
                Name = "Mahavitaran",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Organizations.Add(org);

            // 3. 🟢 SEED ROLES
            var adminRoleId = Guid.NewGuid();
            var staffRoleId = Guid.NewGuid();
            var userRoleId = Guid.NewGuid();

            context.Roles.AddRange(
                new Role { Id = adminRoleId, OrganizationId = orgId, Name = "SystemAdmin", Description = "Full System Access", CreatedAt = DateTime.UtcNow },
                new Role { Id = staffRoleId, OrganizationId = orgId, Name = "Staff", Description = "Handles Grievances and Breaches", CreatedAt = DateTime.UtcNow },
                new Role { Id = userRoleId, OrganizationId = orgId, Name = "DataPrincipal", Description = "Normal Users / Citizens", CreatedAt = DateTime.UtcNow }
            );

            // 4. 🟢 SEED 10 USERS
            var adminId = Guid.NewGuid();
            var staff1Id = Guid.NewGuid();
            var user1Id = Guid.NewGuid();
            var dummyHash = "dummyhash";

            context.Users.AddRange(
                // 1 Admin
                new User { UserId = adminId, OrganizationId = orgId, ReferenceId = "EMP-ADMIN01", FullName = "Smt. Anjali Deshpande", Email = "anjali@mahavitaran.in", Department = "DPO", RoleId = adminRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // 2 Staff
                new User { UserId = staff1Id, OrganizationId = orgId, ReferenceId = "EMP-STAFF01", FullName = "Rahul Kulkarni", Email = "rahul@mahavitaran.in", Department = "Support", RoleId = staffRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "EMP-STAFF02", FullName = "Sneha Jadhav", Email = "sneha@mahavitaran.in", Department = "Compliance", RoleId = staffRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // 7 Normal Users
                new User { UserId = user1Id, OrganizationId = orgId, ReferenceId = "MSD-7GADEHTJ", FullName = "Amit Sharma", Email = "amit@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-3S99X4MF", FullName = "Priya Singh", Email = "priya@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-CAMSVD2E", FullName = "Karan Patel", Email = "karan@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-2SB98FFH", FullName = "Ravi Kumar", Email = "ravi@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-BBSH64GS", FullName = "Neha Verma", Email = "neha@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-A5F9R25M", FullName = "Vikas Gupta", Email = "vikas@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-H5M6SHA", FullName = "Pooja Joshi", Email = "pooja@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            // 5. 🟢 SEED ROLE PERMISSIONS (Dynamic Map)
            var allPermissions = context.Permissions.ToList();

            foreach (var perm in allPermissions)
            {
                context.RolePermissions.Add(new RolePermission { Id = Guid.NewGuid(), RoleId = adminRoleId, PermissionId = perm.Id });
            }

            var staffPerms = allPermissions.Where(p => p.SystemName.StartsWith("Grievances") || p.SystemName.StartsWith("Breaches")).ToList();
            foreach (var perm in staffPerms)
            {
                context.RolePermissions.Add(new RolePermission { Id = Guid.NewGuid(), RoleId = staffRoleId, PermissionId = perm.Id });
            }
            context.SaveChanges();

            // 6. 🟢 DUMMY GRIEVANCE TICKET & MESSAGES
            var grievanceId = Guid.NewGuid();
            var grievance = new Grievance
            {
                Id = grievanceId,
                OrganizationId = orgId,
                DataPrincipalId = user1Id,
                ReferenceId = "GRV-20261026-1234",
                Subject = "My medical data was shared without consent",
                Description = "I found out my data is visible to an unauthorized portal.",
                Category = "Data Leak",
                Status = "Open",
                Severity = "High",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Grievances.Add(grievance);

            context.GrievanceMessages.AddRange(
                new GrievanceMessage { Id = Guid.NewGuid(), GrievanceId = grievanceId, SenderId = user1Id, MessageText = "Please help, my data is leaked.", IsInternal = false, CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new GrievanceMessage { Id = Guid.NewGuid(), GrievanceId = grievanceId, SenderId = staff1Id, MessageText = "We are looking into this urgently.", IsInternal = false, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new GrievanceMessage { Id = Guid.NewGuid(), GrievanceId = grievanceId, SenderId = adminId, MessageText = "Internal Note: Escalate this to DPO immediately. Possible breach.", IsInternal = true, CreatedAt = DateTime.UtcNow }
            );

            // 7. 🟢 BAAKI PURANA DUMMY DATA (Merged and Fixed)

            // 7.1 Data Inventory 
            var inventoryId = Guid.NewGuid();
            context.DataInventories.Add(new DataInventory { DataInventoryId = inventoryId, OrganizationId = orgId, Name = "Patient Medical Records", DataCategory = "Health Data", Purpose = "Treatment & Diagnosis", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

            // 7.2 Campaign
            context.Campaigns.Add(new Campaign { CampaignId = Guid.NewGuid(), OrganizationId = orgId, Title = "New Health Data Policy 2026", CampaignType = "Awareness", TargetRole = "All", Status = "Active", StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(15), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DataInventoryId = inventoryId });

            // 7.3 Consent Record 
            var dummyPrincipalId = Guid.NewGuid();

            if (!context.DataPrincipals.Any())
            {
                context.DataPrincipals.Add(new DataPrincipal
                {
                    DataPrincipalId = dummyPrincipalId,
                    OrganizationId = orgId,
                    ReferenceId = "PSD-DUMMY-001",
                    FullName = "Amit Dummy",
                    Email = "amit@dummy.com",
                    Mobile = "9999999999",
                    Class = "adult",
                    PreferredLocale = "en-IN",
                    CreatedAt = DateTime.UtcNow
                });

                context.SaveChanges();
            }
            else
            {
                dummyPrincipalId = context.DataPrincipals.First().DataPrincipalId;
            }

            context.ConsentRecords.Add(new ConsentRecord
            {
                ConsentRecordId = Guid.NewGuid(),
                OrganizationId = orgId,
                DataPrincipalId = dummyPrincipalId,
                OverallStatus = ConsentStatus.Granted,
                CreatedAt = DateTime.UtcNow
            });

            // 7.4 Privacy Requests (🚨 FIXED: user1Id ki jagah dummyPrincipalId use kiya hai taaki FK error na aaye)
            context.PrivacyRequests.AddRange(
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = dummyPrincipalId, ReferenceId = "DSR-2026-0001", RequestType = PrivacyRequestType.Erasure, RequestDetails = "Please delete my old X-Ray reports", Status = PrivacyRequestStatus.Submitted, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = dummyPrincipalId, ReferenceId = "DSR-2026-0002", RequestType = PrivacyRequestType.Access, RequestDetails = "I need my complete medical history", Status = PrivacyRequestStatus.Submitted, CreatedAt = DateTime.UtcNow.AddDays(-28) }
            );

            // 7.5 Data Breach
            context.DataBreachIncidents.Add(new DataBreachIncident { Id = Guid.NewGuid(), OrganizationId = orgId, Title = "Unauthorized Access to Lab Results", Description = "An intern accessed reports without permission.", Severity = "High", IncidentDate = DateTime.UtcNow.AddDays(-3), DiscoveryDate = DateTime.UtcNow.AddDays(-2), EstimatedAffectedRecords = 150, Status = "Open", CreatedAt = DateTime.UtcNow });

            context.SaveChanges();
        }
    }
}