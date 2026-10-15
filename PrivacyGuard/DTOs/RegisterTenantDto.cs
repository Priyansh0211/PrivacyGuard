namespace PrivacyGuard.DTOs
{
    public class RegisterTenantDto
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPhoneNumber { get; set; } = string.Empty; 
        public string AdminPassword { get; set; } = string.Empty;
    }
}
