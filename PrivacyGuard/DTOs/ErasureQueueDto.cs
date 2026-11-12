using System;

namespace PrivacyGuard.DTOs
{
    public class ErasureQueueDto
    {
        public Guid RecordId { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}