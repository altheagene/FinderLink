using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinderLink.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        [ForeignKey("Item")]
        public int ItemId { get; set; }

        [Required]
        [ForeignKey("Admin")]
        public int AdminId { get; set; }

        [StringLength(500)]
        public string? ClaimDescription { get; set; }

        [Required]
        [StringLength(150)]
        public string ClaimerName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string ClaimerContact { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "pending"; // "pending", "verified", "rejected", "released"

        [Required]
        public DateTime DateClaimed { get; set; } = DateTime.UtcNow;

        [ForeignKey("VerifiedByAdmin")]
        public int? VerifiedBy { get; set; }

        public DateTime? DateVerified { get; set; }

        // Navigation properties
        public virtual Item Item { get; set; } = null!;
        public virtual Admin Admin { get; set; } = null!;

        [ForeignKey("VerifiedBy")]
        public virtual Admin? VerifiedByAdmin { get; set; }

        public virtual Release? Release { get; set; }
    }
}
