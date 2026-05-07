using System.ComponentModel.DataAnnotations;

namespace FinderLink.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Item> ItemsCreated { get; set; } = new List<Item>();
        public virtual ICollection<Item> ItemsFound { get; set; } = new List<Item>();
        public virtual ICollection<Claim> ClaimsVerified { get; set; } = new List<Claim>();
        public virtual ICollection<AdminLog> AdminLogs { get; set; } = new List<AdminLog>();
        public virtual ICollection<Release> ReleasedItems { get; set; } = new List<Release>();
        public virtual ICollection<Release> ReceivedReleases { get; set; } = new List<Release>();
    }
}
