namespace PrivacyGuard.DTOs
{
    public class AssignOrgUnitDto
    {
        // value = us unit me daalo, null ya Guid.Empty = unit se hata do
        public Guid? OrgUnitId { get; set; }
    }
}