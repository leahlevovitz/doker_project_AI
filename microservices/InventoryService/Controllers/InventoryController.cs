using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IDatabase _redis;

    public InventoryController(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    // GET api/inventory/{giftId}
    [HttpGet("{giftId}")]
    public async Task<IActionResult> Get(int giftId)
    {
        var quantity = await _redis.StringGetAsync($"gift:{giftId}:quantity");
        var isLocked = await _redis.StringGetAsync($"gift:{giftId}:locked");

        return Ok(new
        {
            GiftId = giftId,
            Quantity = quantity.HasValue ? (int)quantity : 0,
            IsLocked = isLocked == "true"
        });
    }

    // PUT api/inventory/{giftId}/quantity
    [HttpPut("{giftId}/quantity")]
    public async Task<IActionResult> SetQuantity(int giftId, [FromBody] int quantity)
    {
        await _redis.StringSetAsync($"gift:{giftId}:quantity", quantity);
        return Ok(new { GiftId = giftId, Quantity = quantity });
    }

    // POST api/inventory/{giftId}/decrement
    [HttpPost("{giftId}/decrement")]
    public async Task<IActionResult> Decrement(int giftId)
    {
        var key = $"gift:{giftId}:quantity";
        var newQty = await _redis.StringDecrementAsync(key);
        if (newQty < 0)
        {
            // Rollback - restore the value
            await _redis.StringIncrementAsync(key);
            return BadRequest(new { message = "Out of stock" });
        }
        return Ok(new { GiftId = giftId, Quantity = newQty });
    }

    // PUT api/inventory/{giftId}/lock
    [HttpPut("{giftId}/lock")]
    public async Task<IActionResult> SetLock(int giftId, [FromBody] bool isLocked)
    {
        await _redis.StringSetAsync($"gift:{giftId}:locked", isLocked ? "true" : "false");
        return Ok(new { GiftId = giftId, IsLocked = isLocked });
    }
}
