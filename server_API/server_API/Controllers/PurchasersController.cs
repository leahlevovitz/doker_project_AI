using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server_API.BLL;
using server_API.DTO;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace server_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasersController : ControllerBase
    {
        private readonly IPurchaserBLL _bll;
        private readonly ILogger<PurchasersController> _logger;

        public PurchasersController(IPurchaserBLL bll, ILogger<PurchasersController> logger)
        {
            _bll = bll ?? throw new ArgumentNullException(nameof(bll));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<List<PurchaserDTO>>> GetAll([FromQuery] string? sortBy = null)
        {
            try
            {
                _logger.LogInformation("Fetching all purchases. SortBy: {SortBy}", sortBy ?? "None");

                var result = await _bll.GetAll(sortBy);

                _logger.LogInformation("Returned {Count} purchases.", result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all purchases.");
                throw;
            }
        }

        [HttpGet("by-gift")]
        public async Task<ActionResult<List<PurchaserDTO>>> GetByGift(int? giftId, [FromQuery] string? sortBy = null)
        {
            if (!giftId.HasValue || giftId <= 0)
            {
                _logger.LogWarning("Invalid giftId received: {GiftId}", giftId);
                return BadRequest("Invalid giftId.");
            }

            try
            {
                _logger.LogInformation("Fetching purchases for giftId: {GiftId}, SortBy: {SortBy}", giftId, sortBy ?? "None");

                var result = await _bll.GetByGift(giftId, sortBy);

                _logger.LogInformation("Returned {Count} purchases for giftId {GiftId}.", result.Count, giftId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching purchases for giftId {GiftId}", giftId);
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaserDTO>> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchase ID received: {Id}", id);
                return BadRequest("Invalid purchase ID.");
            }

            try
            {
                _logger.LogInformation("Fetching purchaser with ID: {Id}", id);

                var purchaser = await _bll.GetById(id);

                if (purchaser == null)
                {
                    _logger.LogWarning("Purchaser with ID: {Id} was not found", id);
                    return NotFound();
                }

                return Ok(purchaser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching purchaser ID {Id}", id);
                throw;
            }
        }

        [HttpGet("total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                _logger.LogInformation("Calculating total revenue");

                var result = await _bll.GetTotalRevenue();

                _logger.LogInformation("Total revenue calculated successfully.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calculating total revenue.");
                throw;
            }
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> AddMultiple([FromBody] IEnumerable<PurchaserDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
            {
                _logger.LogWarning("AddMultiple called with empty list");
                return BadRequest("The list of purchasers cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Adding {Count} purchases", dtos.Count());

                await _bll.Add(dtos);

                _logger.LogInformation("Purchases added successfully.");

                return Ok(new { message = "Purchases added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding multiple purchases.");
                throw;
            }
        }

        [HttpPost("Basket")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> AddToBasket([FromBody] PurchaserDTO dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("AddToBasket called with null DTO");
                return BadRequest("Invalid request body.");
            }

            try
            {
                _logger.LogInformation("Adding item to basket for gift: {GiftId}", dto.GiftId);

                await _bll.AddToBasket(dto);

                _logger.LogInformation("Item added to basket successfully.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding item to basket.");
                throw;
            }
        }

        [HttpGet("basket")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<List<PurchaserDTO>>> GetBasket()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Unauthorized basket access attempt - missing NameIdentifier claim.");
                    return Unauthorized();
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogError("Invalid userId claim format: {Claim}", userIdClaim);
                    return Unauthorized();
                }

                _logger.LogInformation("Fetching basket for userId: {UserId}", userId);

                var basketItems = await _bll.GetBasketByUser(userId);

                _logger.LogInformation("Returned {Count} basket items for userId {UserId}.", basketItems.Count, userId);

                return Ok(basketItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving basket.");
                throw;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid purchase ID for deletion: {Id}", id);
                return BadRequest("Invalid purchase ID.");
            }

            try
            {
                _logger.LogInformation("Deleting purchase (draft) ID: {Id}", id);

                await _bll.Delete(id);

                _logger.LogInformation("Purchase ID {Id} deleted successfully.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting purchase ID {Id}.", id);
                throw;
            }
        }
    }
}
