using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.DTOs
{
    public class RolePermissionDto
    {

        //For checkbox
        public class PermissionActionDto
        {
            public int PermissionId { get; set; }
            public string ActionName { get; set; } = string.Empty;
        }

       
        // category name
        public class ModulePermissionDto
        {
            public string ModuleName { get; set; } = string.Empty;
            public List<PermissionActionDto> Actions { get; set; } = new List<PermissionActionDto>();
        }
        public class CreateRoleDto
        {
            [Required]
            public string RoleName { get; set; } = string.Empty;

            public string? Description { get; set; }

           
            [Required]
            public List<int> PermissionIds { get; set; } = new List<int>();
        }
    }

}
