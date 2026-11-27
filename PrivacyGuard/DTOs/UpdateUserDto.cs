namespace PrivacyGuard.DTOs
{
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid RoleId { get; set; }
        // null = unit mein koi change nahi, Guid.Empty = unit se hata do, value = nayi unit
        public Guid? OrgUnitId { get; set; }
    }
}