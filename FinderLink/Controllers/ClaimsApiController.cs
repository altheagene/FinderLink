using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;
        private readonly IAdminLogService _adminLogService;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(
            IClaimService claimService,
            IAdminLogService adminLogService,
            ILogger<ClaimsController> logger)
        {
            _claimService = claimService;
            _adminLogService = adminLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get all claims or filter by status
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Claim>>> GetClaims([FromQuery] string? status = null)
        {
            try
            {
                var claims = string.IsNullOrEmpty(status)
                    ? await _claimService.GetClaimsByStatusAsync("pending")
                    : await _claimService.GetClaimsByStatusAsync(status);

                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims");
                return StatusCode(500, "Error retrieving claims");
            }
        }

        /// <summary>
        /// Get claim by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Claim>> GetClaim(int id)
        {
            try
            {
                var claim = await _claimService.GetClaimByIdAsync(id);
                if (claim == null)
                    return NotFound("Claim not found");

                return Ok(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claim");
                return StatusCode(500, "Error retrieving claim");
            }
        }

        /// <summary>
        /// Get claims for a specific item
        /// </summary>
        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<List<Claim>>> GetClaimsByItem(int itemId)
        {
            try
            {
                var claims = await _claimService.GetClaimsByItemAsync(itemId);
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims for item");
                return StatusCode(500, "Error retrieving claims");
            }
        }

        /// <summary>
        /// Get claims for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Claim>>> GetClaimsByUser(int userId)
        {
            try
            {
                var claims = await _claimService.GetClaimsByAdminAsync(userId);
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user claims");
                return StatusCode(500, "Error retrieving claims");
            }
        }

        /// <summary>
        /// Create a new claim for an item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Claim>> CreateClaim([FromBody] Claim claim)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdClaim = await _claimService.CreateClaimAsync(claim);
                return CreatedAtAction(nameof(GetClaim), new { id = createdClaim.ClaimId }, createdClaim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim");
                return StatusCode(500, "Error creating claim");
            }
        }

        /// <summary>
        /// Verify a claim (admin action) - transitions to verified status and item to claimed
        /// </summary>
        [HttpPost("{claimId}/verify")]
        public async Task<ActionResult<Claim>> VerifyClaim(int claimId, [FromQuery] int verifiedBy)
        {
            try
            {
                var verifiedClaim = await _claimService.VerifyClaimAsync(claimId, verifiedBy);

                // Log the action
                await _adminLogService.LogActionAsync(
                    verifiedBy,
                    "verify_claim",
                    verifiedClaim.ItemId,
                    claimId,
                    "Claim verified"
                );

                return Ok(verifiedClaim);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying claim");
                return StatusCode(500, "Error verifying claim");
            }
        }

        /// <summary>
        /// Reject a claim (admin action)
        /// </summary>
        [HttpPost("{claimId}/reject")]
        public async Task<ActionResult<Claim>> RejectClaim(int claimId)
        {
            try
            {
                var rejectedClaim = await _claimService.RejectClaimAsync(claimId);
                return Ok(rejectedClaim);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim");
                return StatusCode(500, "Error rejecting claim");
            }
        }

        /// <summary>
        /// Delete a claim
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            try
            {
                var success = await _claimService.DeleteClaimAsync(id);
                if (!success)
                    return NotFound("Claim not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting claim");
                return StatusCode(500, "Error deleting claim");
            }
        }
    }
}
