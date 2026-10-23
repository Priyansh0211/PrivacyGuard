namespace PrivacyGuard.DTOs
{
    public class CreateOrgUnitDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
    }

    public class OrgUnitResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; } // Naya field add kiya
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; }
        public List<OrgUnitResponseDto> Children { get; set; } = new List<OrgUnitResponseDto>();
    }
}