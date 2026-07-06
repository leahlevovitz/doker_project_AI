using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PurchasesController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/purchases
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

    // GET api/purchases/basket?userId=5
    [HttpGet("basket")]
    public async Task<IActionResult> GetBasket([FromQuery] int userId)
    {
        var basket = await _db.Purchases
            .Where(p => p.UserId == userId && p.IsDraft)
            .ToListAsync();
        return Ok(basket);
    }

    // GET api/purchases/by-gift?giftId=3
    [HttpGet("by-gift")]
    public async Task<IActionResult> GetByGift([FromQuery] int giftId)
    {
        var purchases = await _db.Purchases
            .Where(p => p.GiftId == giftId && !p.IsDraft)
            .ToListAsync();
        return Ok(purchases);
    }

    // GET api/purchases/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var purchase = await _db.Purchases.FindAsync(id);
        return purchase == null ? NotFound() : Ok(purchase);
    }

    // GET api/purchases/total-revenue
    [HttpGet("total-revenue")]
    public async Task<IActionResult> GetTotalRevenue()
    {
        var count = await _db.Purchases.CountAsync(p => !p.IsDraft);
        return Ok(new { TotalPurchases = count });
    }

    // POST api/purchases/basket — add to basket (draft)
    [HttpPost("basket")]
    public async Task<IActionResult> AddToBasket(Purchase purchase)
    {
        purchase.IsDraft = true;
        purchase.PurchaseDate = DateTime.UtcNow;
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();
        return Ok(purchase);
    }

    // POST api/purchases/checkout — confirm basket items
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] List<int> purchaseIds)
    {
        var items = await _db.Purchases
            .Where(p => purchaseIds.Contains(p.Id) && p.IsDraft)
            .ToListAsync();

        if (!items.Any()) return BadRequest(new { message = "No draft purchases found" });

        foreach (var item in items)
            item.IsDraft = false;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Checkout successful", count = items.Count });
    }

    // DELETE api/purchases/{id}
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
