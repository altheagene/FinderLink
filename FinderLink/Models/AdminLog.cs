using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinderLink.Models
{
    public class AdminLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [ForeignKey("Admin")]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // "verify_claim", "release_item", "add_item", "update_item"

        [ForeignKey("Item")]
        public int? ItemId { get; set; }

        [ForeignKey("Claim")]
        public int? ClaimId { get; set; }

        [Required]
        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Navigation properties
        public virtual Admin Admin { get; set; } = null!;
        public virtual Item? Item { get; set; }
        public virtual Claim? Claim { get; set; }
    }
}
