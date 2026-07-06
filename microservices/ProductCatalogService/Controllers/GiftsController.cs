using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProductCatalogService.Cache;
using ProductCatalogService.Models;

namespace ProductCatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GiftsController : ControllerBase
{
    private readonly IMongoCollection<Gift> _gifts;
    private readonly GiftCacheService _cache;

    public GiftsController(IMongoDatabase db, GiftCacheService cache)
    {
        _gifts = db.GetCollection<Gift>("gifts");
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Category? category = null)
    {
        var cacheKey = category.HasValue ? $"all:{category}" : "all";

        var cached = await _cache.GetAsync<List<Gift>>(cacheKey);
        if (cached != null) return Ok(cached);

        var filter = category.HasValue
            ? Builders<Gift>.Filter.Eq(g => g.Category, category.Value)
            : Builders<Gift>.Filter.Empty;

        var gifts = await _gifts.Find(filter).ToListAsync();
        await _cache.SetAsync(cacheKey, gifts);
        return Ok(gifts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var cached = await _cache.GetAsync<Gift>(id);
        if (cached != null) return Ok(cached);

        var gift = await _gifts.Find(g => g.Id == id).FirstOrDefaultAsync();
        if (gift == null) return NotFound();

        await _cache.SetAsync(id, gift);
        return Ok(gift);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? name = null, [FromQuery] string? donorName = null)
    {
        var builder = Builders<Gift>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(name))
            filter &= builder.Regex(g => g.Name, new MongoDB.Bson.BsonRegularExpression(name, "i"));

        if (!string.IsNullOrEmpty(donorName))
            filter &= builder.Regex(g => g.DonorName, new MongoDB.Bson.BsonRegularExpression(donorName, "i"));

        var gifts = await _gifts.Find(filter).ToListAsync();
        return Ok(gifts);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Gift gift)
    {
        await _gifts.InsertOneAsync(gift);
        await _cache.InvalidateAllAsync();
        return CreatedAtAction(nameof(GetById), new { id = gift.Id }, gift);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Gift gift)
    {
        gift.Id = id;
        var result = await _gifts.ReplaceOneAsync(g => g.Id == id, gift);
        if (result.MatchedCount == 0) return NotFound();
        // Invalidate both the specific item and the list cache
        await _cache.InvalidateAsync(id);
        await _cache.InvalidateAllAsync();
        return Ok(gift);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _gifts.DeleteOneAsync(g => g.Id == id);
        if (result.DeletedCount == 0) return NotFound();
        await _cache.InvalidateAsync(id);
        await _cache.InvalidateAllAsync();
        return NoContent();
    }
}
