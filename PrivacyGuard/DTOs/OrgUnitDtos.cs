namespace PrivacyGuard.DTOs
{
    public class OrgUnitResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Naye changes (Service se match karne ke liye)
        public string UnitType { get; set; } = string.Empty;
        public Guid OrganizationId { get; set; }
        public Guid? ParentId { get; set; }

        public string UnitCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PrincipalsCount { get; set; }
        public List<OrgUnitResponseDto> Children { get; set; } = new();
    }

    public class CreateOrgUnitDto
    {
        public string Name { get; set; } = string.Empty;

        public string UnitType { get; set; } = string.Empty;

        public string UnitCode { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }

    }
}