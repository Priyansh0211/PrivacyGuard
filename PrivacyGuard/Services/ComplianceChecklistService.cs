using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Data;
using PrivacyGuard.DTOs;
using PrivacyGuard.Interfaces.Services;
using PrivacyGuard.Models;

namespace PrivacyGuard.Services
{
    public class ComplianceChecklistService : IComplianceChecklistService
    {
        private const string Complete = "Complete";
        private const string NeedsAttention = "NeedsAttention";
        private const string NotTracked = "NotTracked";

        private readonly ApplicationDbContext _context;

        public ComplianceChecklistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ComplianceChecklistDto> GetChecklistAsync(Guid organizationId)
        {
            var inventoryCount = await _context.DataInventories
                .CountAsync(item => item.OrganizationId == organizationId);
            var publishedNoticeCount = await _context.Notices
                .CountAsync(item => item.OrganizationId == organizationId &&
                    item.PublishedAt != null && item.Status.ToLower() == "published");
            var consentRecordCount = await _context.ConsentRecords
                .CountAsync(item => item.OrganizationId == organizationId);
            var processorCount = await _context.DataProcessors
                .CountAsync(item => item.OrganizationId == organizationId);
            var activeRetentionPolicyCount = await _context.RetentionPolicies
                .CountAsync(item => item.OrganizationId == organizationId && item.IsActive);
            var privacyRequestCount = await _context.PrivacyRequests
                .CountAsync(item => item.OrganizationId == organizationId);
            var openPrivacyRequestCount = await _context.PrivacyRequests
                .CountAsync(item => item.OrganizationId == organizationId &&
                    item.Status != PrivacyRequestStatus.Fulfilled &&
                    item.Status != PrivacyRequestStatus.Rejected &&
                    item.Status != PrivacyRequestStatus.Withdrawn &&
                    item.Status != PrivacyRequestStatus.Expired);
            var grievanceCount = await _context.Grievances
                .CountAsync(item => item.OrganizationId == organizationId);
            var openGrievanceCount = await _context.Grievances
                .CountAsync(item => item.OrganizationId == organizationId &&
                    item.Status.ToLower() != "closed" && item.Status.ToLower() != "resolved");
            var breachCount = await _context.DataBreachIncidents
                .CountAsync(item => item.OrganizationId == organizationId);
            var openBreachCount = await _context.DataBreachIncidents
                .CountAsync(item => item.OrganizationId == organizationId && item.Status.ToLower() != "closed");
            var dpiaCount = await _context.DpiaRecords
                .CountAsync(item => item.OrganizationId == organizationId);
            var approvedDpiaCount = await _context.DpiaRecords
                .CountAsync(item => item.OrganizationId == organizationId && item.Status.ToLower() == "approved");
            var overdueDpiaCount = await _context.DpiaRecords
                .CountAsync(item => item.OrganizationId == organizationId &&
                    item.Status.ToLower() == "approved" && item.NextReviewDate < DateTime.UtcNow);
            var auditEventCount = await _context.AuditLogs
                .CountAsync(item => item.OrganizationId == organizationId);

            var items = new List<ComplianceChecklistItemDto>
            {
                CreateItem("data-inventory", "Data Governance", "Maintain a data inventory",
                    inventoryCount > 0 ? Complete : NeedsAttention,
                    $"{inventoryCount} inventory record(s) found.", inventoryCount,
                    "Add the personal data your organization collects, its purpose, and retention details.", "DataInventory"),
                CreateItem("privacy-notice", "Transparency", "Publish a privacy notice",
                    publishedNoticeCount > 0 ? Complete : NeedsAttention,
                    $"{publishedNoticeCount} published notice(s) found.", publishedNoticeCount,
                    "Create and publish an organization privacy notice.", "Notice"),
                CreateItem("consent-records", "Consent", "Maintain consent records",
                    consentRecordCount > 0 ? Complete : NeedsAttention,
                    $"{consentRecordCount} consent record(s) found.", consentRecordCount,
                    "Capture consent records when processing relies on consent.", "Consent"),
                CreateItem("processor-register", "Third Parties", "Maintain a processor register",
                    processorCount > 0 ? Complete : NeedsAttention,
                    $"{processorCount} processor record(s) found.", processorCount,
                    "Add the third-party processors used by your organization.", "Consent"),
                CreateItem("retention-policies", "Data Lifecycle", "Configure active retention policies",
                    activeRetentionPolicyCount > 0 ? Complete : NeedsAttention,
                    $"{activeRetentionPolicyCount} active retention policy/policies found.", activeRetentionPolicyCount,
                    "Create and activate retention policies for the data you hold.", "RetentionPolicy"),
                CreateItem("privacy-requests", "Data Principal Rights", "Review the privacy request queue",
                    openPrivacyRequestCount == 0 ? Complete : NeedsAttention,
                    $"{privacyRequestCount} request(s) recorded; {openPrivacyRequestCount} open.", privacyRequestCount,
                    openPrivacyRequestCount == 0 ? "No open requests are recorded." : "Review and resolve open privacy requests.", "PrivacyRequest"),
                CreateItem("grievances", "Grievances", "Review the grievance queue",
                    openGrievanceCount == 0 ? Complete : NeedsAttention,
                    $"{grievanceCount} grievance(s) recorded; {openGrievanceCount} open.", grievanceCount,
                    openGrievanceCount == 0 ? "No open grievances are recorded." : "Review and resolve open grievances.", "Grievance"),
                CreateItem("breach-register", "Incident Management", "Review the breach incident register",
                    openBreachCount == 0 ? Complete : NeedsAttention,
                    $"{breachCount} incident(s) recorded; {openBreachCount} open.", breachCount,
                    openBreachCount == 0 ? "No open incidents are recorded; verify response procedures separately." : "Review open incidents and document response actions.", "Breach"),
                CreateItem("dpia-reviews", "Risk Assessment", "Maintain reviewed DPIA assessments",
                    approvedDpiaCount > 0 && overdueDpiaCount == 0 ? Complete : NeedsAttention,
                    $"{dpiaCount} DPIA assessment(s) recorded; {approvedDpiaCount} approved, {overdueDpiaCount} overdue for review.", dpiaCount,
                    overdueDpiaCount > 0 ? "Review overdue DPIA assessments." : "Create and approve DPIA assessments where needed.", "Dpia"),
                CreateItem("audit-trail", "Governance", "Retain an audit trail",
                    auditEventCount > 0 ? Complete : NeedsAttention,
                    $"{auditEventCount} audit event(s) found.", auditEventCount,
                    "Verify that relevant organization activity is being recorded in the audit trail.", "Audit"),
                CreateItem("staff-training", "Staff Training", "Record privacy training evidence",
                    NotTracked, "Training completion evidence is not stored in PrivacyGuard.", 0,
                    "Track training completion in your organization's training system.", string.Empty),
                CreateItem("processor-agreements", "Third Parties", "Track processor agreement evidence",
                    NotTracked, "Processor agreement evidence is not stored in PrivacyGuard.", 0,
                    "Maintain and review processor agreements outside this checklist.", string.Empty)
            };

            var trackedItems = items.Count(item => item.Status != NotTracked);
            var completeItems = items.Count(item => item.Status == Complete);
            var needsAttentionItems = items.Count(item => item.Status == NeedsAttention);

            return new ComplianceChecklistDto
            {
                TotalItems = items.Count,
                TrackedItems = trackedItems,
                CompleteItems = completeItems,
                NeedsAttentionItems = needsAttentionItems,
                NotTrackedItems = items.Count(item => item.Status == NotTracked),
                ProgressPercent = trackedItems == 0 ? 0 : completeItems * 100 / trackedItems,
                Items = items
            };
        }

        private static ComplianceChecklistItemDto CreateItem(
            string id,
            string category,
            string requirement,
            string status,
            string evidenceSummary,
            int evidenceCount,
            string nextAction,
            string module)
        {
            return new ComplianceChecklistItemDto
            {
                Id = id,
                Category = category,
                Requirement = requirement,
                Status = status,
                EvidenceSummary = evidenceSummary,
                EvidenceCount = evidenceCount,
                NextAction = nextAction,
                Module = module
            };
        }
    }
}
