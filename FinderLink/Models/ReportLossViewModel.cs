using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FinderLink.Models
{
    public class ReportLossViewModel
    {
        [Required]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string LocationFound { get; set; } = string.Empty;

        [Required]
        public DateTime DateFound { get; set; } = DateTime.Today;

        [Required]
        public string FoundByName { get; set; } = string.Empty;

        [Required]
        public string FoundByContact { get; set; } = string.Empty;

        [Required]
        public IFormFile? ItemPhoto { get; set; }
    }
}
