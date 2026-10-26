using System.ComponentModel.DataAnnotations;

namespace PrivacyGuard.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; } 

        [Required, MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty; 

        [Required, MaxLength(100)]
        public string ActionName { get; set; } = string.Empty; 

        [Required, MaxLength(100)]
        public string SystemName { get; set; } = string.Empty;
    }
}