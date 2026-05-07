using System.ComponentModel.DataAnnotations;

namespace FinderLink.Models
{
    public class CreateClaimViewModel
    {
        [Required]
        public int ItemId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string ContactInfo { get; set; } = string.Empty;

        public string? ProofOfOwnership { get; set; }
    }
}
