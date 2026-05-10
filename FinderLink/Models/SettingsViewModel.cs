using System.ComponentModel.DataAnnotations;

namespace FinderLink.Models
{
    public class SettingsViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = "Administrator";

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
        public List<Location> Locations { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}
