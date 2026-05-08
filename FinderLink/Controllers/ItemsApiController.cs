 using FinderLink.Models;
using FinderLink.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IClaimService _claimService;
        private readonly IAdminLogService _adminLogService;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(
            IItemService itemService,
            IClaimService claimService,
            IAdminLogService adminLogService,
            ILogger<ItemsController> logger)
        {
            _itemService = itemService;
            _claimService = claimService;
            _adminLogService = adminLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get all items or filter by status
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Item>>> GetItems([FromQuery] string? status = null)
        {
            try
            {
                var items = string.IsNullOrEmpty(status)
                    ? await _itemService.GetAllItemsAsync()
                    : await _itemService.GetItemsByStatusAsync(status);

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items");
                return StatusCode(500, "Error retrieving items");
            }
        }

        /// <summary>
        /// Get item by ID with all related claims and releases
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            try
            {
                var item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                    return NotFound("Item not found");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item");
                return StatusCode(500, "Error retrieving item");
            }
        }

        /// <summary>
        /// Create a new item (found item report)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem([FromBody] Item item)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdItem = await _itemService.CreateItemAsync(item);

                // Log the action
                await _adminLogService.LogActionAsync(
                    item.CreatedBy,
                    "add_item",
                    createdItem.ItemId,
                    null,
                    $"Item added: {createdItem.ItemName}"
                );

                return CreatedAtAction(nameof(GetItem), new { id = createdItem.ItemId }, createdItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                return StatusCode(500, "Error creating item");
            }
        }

        /// <summary>
        /// Update item details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Item>> UpdateItem(int id, [FromBody] Item item)
        {
            try
            {
                var existingItem = await _itemService.GetItemByIdAsync(id);
                if (existingItem == null)
                    return NotFound("Item not found");

                item.ItemId = id;
                var updatedItem = await _itemService.UpdateItemAsync(item);

                // Log the action
                await _adminLogService.LogActionAsync(
                    existingItem.CreatedBy,
                    "update_item",
                    id,
                    null,
                    $"Item updated: {updatedItem.ItemName}"
                );

                return Ok(updatedItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item");
                return StatusCode(500, "Error updating item");
            }
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var success = await _itemService.DeleteItemAsync(id);
                if (!success)
                    return NotFound("Item not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item");
                return StatusCode(500, "Error deleting item");
            }
        }

        /// <summary>
        /// Search items by name, description, or location
        /// </summary>
        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<List<Item>>> SearchItems(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return BadRequest("Search term cannot be empty");

                var items = await _itemService.SearchItemsAsync(searchTerm);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching items");
                return StatusCode(500, "Error searching items");
            }
        }
    }
}
