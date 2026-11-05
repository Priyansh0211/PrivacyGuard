using System;
using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    // Enum for Dropdown:
    public enum PrivacyRequestType
    {
        Access,
        Correction,
        Completion,
        Update,
        Erasure,
        Portability,
        ProcessingSummary
    }

    // Enum for Dropdown Status
    public enum PrivacyRequestStatus
    {
        Submitted,
        IdentityPending,
        Verified,
        InProgress,
        AwaitingPrincipal,
        PartiallyFulfilled,
        Fulfilled,
        Rejected,
        Withdrawn,
        Expired
    }

    public class PrivacyRequest
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public Guid DataPrincipalId { get; set; }

        // NAYA: Har request ka ek unique ID hona chahiye (e.g., DSR-2026-000024)
        [Required]
        [MaxLength(30)]
        public string ReferenceId { get; set; } = string.Empty;

        // UPDATED: String ki jagah Enum use kiya taaki UI dropdown se direct map ho
        [Required]
        public PrivacyRequestType RequestType { get; set; }

        [Required]
        public string RequestDetails { get; set; } = string.Empty;

        // UPDATED: Enum for Strict Status Tracking
        [Required]
        public PrivacyRequestStatus Status { get; set; } = PrivacyRequestStatus.Submitted;

        // NAYA: UI (Screenshot 152) mein 'Assigned To' tha (e.g., Rahul Kulkarni)
        public Guid? AssignedToUserId { get; set; }

        // NAYA: UI mein 'Tasks' column tha (e.g., 0 of 5, 5 of 5)
        public int TotalTasks { get; set; } = 0;
        public int CompletedTasks { get; set; } = 0;

        public string? ResolutionRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // SLA tracking: Default 30 days
        public DateTime TargetResolutionDate { get; set; }

        public DataPrincipal? DataPrincipal { get; set; }

        // Constructor to set TargetResolutionDate based on CreatedAt
        public PrivacyRequest()
        {
            TargetResolutionDate = CreatedAt.AddDays(30);
        }
    }
}