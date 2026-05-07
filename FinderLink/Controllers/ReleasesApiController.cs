using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReleasesController : ControllerBase
    {
        private readonly IReleaseService _releaseService;
        private readonly ILogger<ReleasesController> _logger;

        public ReleasesController(
            IReleaseService releaseService,
            ILogger<ReleasesController> logger)
        {
            _releaseService = releaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get all releases
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Release>>> GetReleases()
        {
            try
            {
                var releases = await _releaseService.GetAllReleasesAsync();
                return Ok(releases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving releases");
                return StatusCode(500, "Error retrieving releases");
            }
        }

        /// <summary>
        /// Get release by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Release>> GetRelease(int id)
        {
            try
            {
                var release = await _releaseService.GetReleaseByIdAsync(id);
                if (release == null)
                    return NotFound("Release not found");

                return Ok(release);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving release");
                return StatusCode(500, "Error retrieving release");
            }
        }

        /// <summary>
        /// Get releases for a specific user (as receiver or admin)
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Release>>> GetReleasesByUser(int userId)
        {
            try
            {
                var releases = await _releaseService.GetReleasesByAdminAsync(userId);
                return Ok(releases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user releases");
                return StatusCode(500, "Error retrieving releases");
            }
        }

        /// <summary>
        /// Get releases for a specific item
        /// </summary>
        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<List<Release>>> GetReleasesByItem(int itemId)
        {
            try
            {
                var releases = await _releaseService.GetReleasesByItemAsync(itemId);
                return Ok(releases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item releases");
                return StatusCode(500, "Error retrieving releases");
            }
        }

        /// <summary>
        /// Release an item to a claimant (admin action with transaction)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Release>> ReleaseItem([FromBody] ReleaseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var release = await _releaseService.ReleaseItemAsync(
                    request.ItemId,
                    request.ClaimId,
                    request.ReleasedBy,
                    request.ReleasedTo,
                    request.Proof
                );

                return CreatedAtAction(nameof(GetRelease), new { id = release.ReleaseId }, release);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing item");
                return StatusCode(500, "Error releasing item");
            }
        }
    }

    /// <summary>
    /// Request model for releasing an item
    /// </summary>
    public class ReleaseRequest
    {
        public int ItemId { get; set; }
        public int ClaimId { get; set; }
        public int ReleasedBy { get; set; } // Admin user ID
        public int ReleasedTo { get; set; } // Claimant user ID
        public string? Proof { get; set; } // URL to proof document
    }
}
