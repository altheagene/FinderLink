using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinderLink.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Others";

        [Required]
        [StringLength(200)]
        public string LocationFound { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "unclaimed"; // "unclaimed", "pending", "claimed", "released"

        [Required]
        public DateTime DateFound { get; set; }

        // Finder info - FK if registered admin, text fallback if not
        [ForeignKey("FoundByAdmin")]
        public int? FoundByAdminId { get; set; }

        [StringLength(100)]
        public string? FoundByName { get; set; } // For non-admin finders

        [StringLength(100)]
        public string? FoundByContact { get; set; } // Email/phone for non-admin finders

        [Required]
        [ForeignKey("CreatedByAdmin")]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FoundByAdminId")]
        public virtual Admin? FoundByAdmin { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual Admin CreatedByAdmin { get; set; } = null!;

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public virtual ICollection<Release> Releases { get; set; } = new List<Release>();
    }
}
