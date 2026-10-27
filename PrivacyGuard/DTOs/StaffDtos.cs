using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class CreateStaffDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public Guid RoleId { get; set; }
        public Guid? OrgUnitId { get; set; } // Branch ID
    }

    public class StaffResponseDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid OrganizationId { get; set; }
        public Guid? OrgUnitId { get; set; }
        public string OrgUnitName { get; set; } = string.Empty; 
        public bool IsActive { get; set; }
    }
}