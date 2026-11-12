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

                    // Module 3 & 14 & 15: Privacy & Grievance Requests
                    new Permission { ModuleName = "Privacy Requests", ActionName = "View Requests", SystemName = "PrivacyRequest.View" },
                    new Permission { ModuleName = "Privacy Requests", ActionName = "Resolve Requests", SystemName = "PrivacyRequest.Resolve" },
                    new Permission { ModuleName = "Privacy Requests", ActionName = "Manage Requests", SystemName = "PrivacyRequest.Manage" },
                    new Permission { ModuleName = "Privacy Requests", ActionName = "Create Own Requests", SystemName = "PrivacyRequest.CreatePublic" },
                    new Permission { ModuleName = "Privacy Requests", ActionName = "Read Own Requests", SystemName = "PrivacyRequest.ReadPublic" },

                    // Module 4: Consent Management
                    new Permission { ModuleName = "Consent Management", ActionName = "View Consent Records", SystemName = "Consent.View" },
                    new Permission { ModuleName = "Consent Management", ActionName = "Manage Consent Records", SystemName = "Consent.Manage" },
                    
                    // Module 5: Data Inventory
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

                    // Audit & Notice Templates
                    new Permission { ModuleName = "Audit Management", ActionName = "Audit View", SystemName = "Audit.View" },
                    new Permission { ModuleName = "Notice Template", ActionName = "Notice Template View", SystemName = "NoticeTemplates.View" },
                    new Permission { ModuleName = "Notice Template", ActionName = "Notice Template Manage", SystemName = "NoticeTemplates.Manage" },
                    new Permission { ModuleName = "Notice Template", ActionName = "Read Public Notices", SystemName = "Notice.ReadPublic" },
                    
                    // Module 9: Organization & OrgUnit Management
                    new Permission { ModuleName = "Organization", ActionName = "Organization View", SystemName = "OrgUnit.View" },
                    new Permission { ModuleName = "Organization", ActionName = "Organization Manage", SystemName = "OrgUnit.Manage" },
                    
                    // Module 10: Retention Policy Management
                    new Permission { ModuleName = "Retention Policy Management", ActionName = "View Retention Policies", SystemName = "RetentionPolicy.View" },
                    new Permission { ModuleName = "Retention Policy Management", ActionName = "Manage Retention Policies", SystemName = "RetentionPolicy.Manage" },

                    // Module 11: Data Principal Management
                    new Permission { ModuleName = "Data Principal", ActionName = "View Data Principals", SystemName = "DataPrincipal.View" },
                    new Permission { ModuleName = "Data Principal", ActionName = "Manage Data Principals", SystemName = "DataPrincipal.Manage" },

                    // Module 13: Bulk Import Management
                    new Permission { ModuleName = "Bulk Import", ActionName = "View Imports", SystemName = "Import.View" },
                    new Permission { ModuleName = "Bulk Import", ActionName = "Manage Imports", SystemName = "Import.Manage" }
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
                new User { UserId = adminId, OrganizationId = orgId, ReferenceId = "EMP-ADMIN01", FullName = "Smt. Anjali Deshpande", Email = "anjali@mahavitaran.in", Department = "DPO", RoleId = adminRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = staff1Id, OrganizationId = orgId, ReferenceId = "EMP-STAFF01", FullName = "Rahul Kulkarni", Email = "rahul@mahavitaran.in", Department = "Support", RoleId = staffRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "EMP-STAFF02", FullName = "Sneha Jadhav", Email = "sneha@mahavitaran.in", Department = "Compliance", RoleId = staffRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = user1Id, OrganizationId = orgId, ReferenceId = "MSD-7GADEHTJ", FullName = "Amit Sharma", Email = "amit@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-3S99X4MF", FullName = "Priya Singh", Email = "priya@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-CAMSVD2E", FullName = "Karan Patel", Email = "karan@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-2SB98FFH", FullName = "Ravi Kumar", Email = "ravi@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-BBSH64GS", FullName = "Neha Verma", Email = "neha@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-A5F9R25M", FullName = "Vikas Gupta", Email = "vikas@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "MSD-H5M6SHA", FullName = "Pooja Joshi", Email = "pooja@user.com", RoleId = userRoleId, PasswordHash = dummyHash, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            // 5. 🟢 SEED ROLE PERMISSIONS (Fixed Order)
            var allPermissionsList = context.Permissions.ToList();

            // A. Admin ko saari permissions
            foreach (var perm in allPermissionsList)
            {
                context.RolePermissions.Add(new RolePermission { Id = Guid.NewGuid(), RoleId = adminRoleId, PermissionId = perm.Id });
            }

            // B. Staff ko specific permissions (Grievances & Breaches)
            var staffPerms = allPermissionsList.Where(p => p.SystemName.StartsWith("Grievances") || p.SystemName.StartsWith("Breaches")).ToList();
            foreach (var perm in staffPerms)
            {
                context.RolePermissions.Add(new RolePermission { Id = Guid.NewGuid(), RoleId = staffRoleId, PermissionId = perm.Id });
            }

            // C. Data Principal ko sirf Public permissions
            var publicPermissionNames = new List<string> { "Notice.ReadPublic", "PrivacyRequest.CreatePublic", "PrivacyRequest.ReadPublic" };
            var publicPermissions = allPermissionsList.Where(p => publicPermissionNames.Contains(p.SystemName)).ToList();
            foreach (var perm in publicPermissions)
            {
                context.RolePermissions.Add(new RolePermission { Id = Guid.NewGuid(), RoleId = userRoleId, PermissionId = perm.Id });
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

            // 7. 🟢 BAAKI PURANA DUMMY DATA
            var inventoryId = Guid.NewGuid();
            context.DataInventories.Add(new DataInventory { DataInventoryId = inventoryId, OrganizationId = orgId, Name = "Patient Medical Records", DataCategory = "Health Data", Purpose = "Treatment & Diagnosis", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

            context.Campaigns.Add(new Campaign { CampaignId = Guid.NewGuid(), OrganizationId = orgId, Title = "New Health Data Policy 2026", CampaignType = "Awareness", TargetRole = "All", Status = "Active", StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(15), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DataInventoryId = inventoryId });

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

            context.PrivacyRequests.AddRange(
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = dummyPrincipalId, ReferenceId = "DSR-2026-0001", RequestType = PrivacyRequestType.Erasure, RequestDetails = "Please delete my old X-Ray reports", Status = PrivacyRequestStatus.Submitted, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new PrivacyRequest { RequestId = Guid.NewGuid(), OrganizationId = orgId, DataPrincipalId = dummyPrincipalId, ReferenceId = "DSR-2026-0002", RequestType = PrivacyRequestType.Access, RequestDetails = "I need my complete medical history", Status = PrivacyRequestStatus.Submitted, CreatedAt = DateTime.UtcNow.AddDays(-28) }
            );

            context.DataBreachIncidents.Add(new DataBreachIncident { Id = Guid.NewGuid(), OrganizationId = orgId, Title = "Unauthorized Access to Lab Results", Description = "An intern accessed reports without permission.", Severity = "High", IncidentDate = DateTime.UtcNow.AddDays(-3), DiscoveryDate = DateTime.UtcNow.AddDays(-2), EstimatedAffectedRecords = 150, Status = "Open", CreatedAt = DateTime.UtcNow });
            context.SaveChanges();

            // 8. 🟢 SEED RETENTION POLICY & METRICS DATA
            if (!context.RetentionPolicies.Any())
            {
                var policyId = Guid.NewGuid();
                context.RetentionPolicies.Add(new RetentionPolicy
                {
                    PolicyId = policyId,
                    OrganizationId = orgId,
                    PolicyName = "Standard Inactive User Policy",
                    Description = "Anonymize users older than 30 days",
                    RetentionPeriodInDays = 30,
                    TriggerEvent = "AccountCreated",
                    ActionType = "Anonymize",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

                context.DataPrincipals.AddRange(
                    new DataPrincipal { DataPrincipalId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "PSD-OVERDUE-01", FullName = "Overdue User", Email = "overdue@test.com", Mobile = "1111111111", Class = "adult", PreferredLocale = "en-IN", CreatedAt = DateTime.UtcNow.AddDays(-35) },
                    new DataPrincipal { DataPrincipalId = Guid.NewGuid(), OrganizationId = orgId, ReferenceId = "PSD-QUEUE-01", FullName = "Queue User", Email = "queue@test.com", Mobile = "2222222222", Class = "adult", PreferredLocale = "en-IN", CreatedAt = DateTime.UtcNow.AddDays(-25) }
                );

                context.SaveChanges();
            }

            // 9. 🟢 SEED NOTICE TEMPLATE & PUBLISHED NOTICE
            if (!context.Notices.Any())
            {
                var templateId = Guid.NewGuid();
                var noticeId = Guid.NewGuid();

                context.NoticeTemplates.Add(new NoticeTemplate
                {
                    Id = templateId,
                    OrganizationId = orgId,
                    Code = "PRIV-2026",
                    Name = "Standard DPDP Privacy Policy",
                    Description = "Default privacy notice for all new users.",
                    PurposesJson = "[\"Service Delivery\", \"Analytics\"]",
                    LogoUrl = "/images/default-logo.png",
                    PrimaryColor = "#0F62FE",
                    Layout = "Standard",
                    CreatedAt = DateTime.UtcNow
                });

                context.Notices.Add(new Notice
                {
                    Id = noticeId,
                    OrganizationId = orgId,
                    NoticeTemplateId = templateId,
                    Code = "PRIV-2026",
                    Status = "Published",
                    Version = 1,
                    Sha256Hash = "dummy-hash-1234567890abcdef",
                    PurposesJson = "[\"Service Delivery\", \"Analytics\"]",
                    Layout = "Standard",
                    PrimaryColor = "#0F62FE",
                    LogoUrl = "/images/default-logo.png",
                    CreatedAt = DateTime.UtcNow,
                    PublishedAt = DateTime.UtcNow
                });

                context.NoticeTranslations.AddRange(
                    new NoticeTranslation { Id = Guid.NewGuid(), NoticeId = noticeId, LanguageCode = "en", Title = "Privacy Notice", Summary = "We collect your data to provide better services.", IsTranslated = true },
                    new NoticeTranslation { Id = Guid.NewGuid(), NoticeId = noticeId, LanguageCode = "hi", Title = "गोपनीयता सूचना", Summary = "हम बेहतर सेवाएं प्रदान करने के लिए आपका डेटा एकत्र करते हैं।", IsTranslated = true }
                );

                context.SaveChanges();
            }
        }
    }
}