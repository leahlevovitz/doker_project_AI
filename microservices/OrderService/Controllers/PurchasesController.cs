using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Messaging;
using OrderService.Models;
using Shared.Events;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly OrderEventPublisher _publisher;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(AppDbContext db, OrderEventPublisher publisher, ILogger<PurchasesController> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? sortBy = null)
    {
        var query = _db.Purchases.AsQueryable();
        query = sortBy switch
        {
            "date" => query.OrderByDescending(p => p.PurchaseDate),
            "gift" => query.OrderBy(p => p.GiftId),
            _ => query.OrderByDescending(p => p.Id)
        };
        return Ok(await query.ToListAsync());
    }

    [HttpGet("basket")]
    public async Task<IActionResult> GetBasket([FromQuery] int userId)
    {
        var basket = await _db.Purchases
            .Where(p => p.UserId == userId && p.IsDraft)
            .ToListAsync();
        return Ok(basket);
    }

    [HttpGet("by-gift")]
    public async Task<IActionResult> GetByGift([FromQuery] int giftId)
    {
        var purchases = await _db.Purchases
            .Where(p => p.GiftId == giftId && !p.IsDraft)
            .ToListAsync();
        return Ok(purchases);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var purchase = await _db.Purchases.FindAsync(id);
        return purchase == null ? NotFound() : Ok(purchase);
    }

    [HttpGet("total-revenue")]
    public async Task<IActionResult> GetTotalRevenue()
    {
        var count = await _db.Purchases.CountAsync(p => !p.IsDraft);
        return Ok(new { TotalPurchases = count });
    }

    [HttpPost("basket")]
    public async Task<IActionResult> AddToBasket(Purchase purchase)
    {
        purchase.IsDraft = true;
        purchase.Status = "Pending";
        purchase.PurchaseDate = DateTime.UtcNow;
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();
        return Ok(purchase);
    }

    // POST api/purchases/checkout — starts the saga by publishing OrderPlaced
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] List<int> purchaseIds)
    {
        var items = await _db.Purchases
            .Where(p => purchaseIds.Contains(p.Id) && p.IsDraft && p.Status == "Pending")
            .ToListAsync();

        if (!items.Any()) return BadRequest(new { message = "No pending draft purchases found" });

        foreach (var item in items)
        {
            item.IsDraft = false;
            // Status stays "Pending" until InventoryService responds
            _logger.LogInformation("[OrderService] Publishing OrderPlaced PurchaseId={Id} GiftId={GiftId}", item.Id, item.GiftId);
            _publisher.Publish(
                new OrderPlacedEvent(Guid.NewGuid(), item.Id, item.GiftId, item.UserId, DateTime.UtcNow),
                "order.placed");
        }

        await _db.SaveChangesAsync();
        return Accepted(new { message = "Order placed, awaiting inventory confirmation", purchaseIds });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var purchase = await _db.Purchases.FindAsync(id);
        if (purchase == null) return NotFound();
        _db.Purchases.Remove(purchase);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
