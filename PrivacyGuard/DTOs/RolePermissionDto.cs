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



        // Har ek checkbox ke liye data
        public class PermissionCheckboxDto
        {
            public int Id { get; set; }
            public string ModuleName { get; set; } = string.Empty;
            public string ActionName { get; set; } = string.Empty;
            public bool IsAssigned { get; set; } // True = Ticked, False = Unticked
        }

        // Poore Edit page ka data jo frontend ko jayega
        public class RolePermissionResponseDto
        {
            public Guid RoleId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<PermissionCheckboxDto> Permissions { get; set; } = new List<PermissionCheckboxDto>();
        }

        // ==========================================
        //  PUT API KE LIYE (Save / Update karne ke liye)
        // ==========================================

        public class UpdateRoleDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            // Frontend yahan un sabhi Checkboxes ki ID bhejega jo tick (true) hain
            public List<int> PermissionIds { get; set; } = new List<int>();
        }


       
    }



}
