using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinderLink.Models
{
    public class Release
    {
        [Key]
        public int ReleaseId { get; set; }

        [Required]
        [ForeignKey("Item")]
        public int ItemId { get; set; }

        [ForeignKey("Claim")]
        public int? ClaimId { get; set; }

        [Required]
        [ForeignKey("ReleasedToAdmin")]
        public int ReleasedTo { get; set; }

        [Required]
        [ForeignKey("ReleasedByAdmin")]
        public int ReleasedBy { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? Proof { get; set; } // URL to proof document/image

        // Navigation properties
        public virtual Item Item { get; set; } = null!;
        public virtual Claim Claim { get; set; } = null!;

        [ForeignKey("ReleasedTo")]
        public virtual Admin ReleasedToAdmin { get; set; } = null!;

        [ForeignKey("ReleasedBy")]
        public virtual Admin ReleasedByAdmin { get; set; } = null!;
    }
}
