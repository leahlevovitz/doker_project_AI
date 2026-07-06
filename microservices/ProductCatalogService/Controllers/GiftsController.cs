using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProductCatalogService.Models;

namespace ProductCatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GiftsController : ControllerBase
{
    private readonly IMongoCollection<Gift> _gifts;

    public GiftsController(IMongoDatabase db)
    {
        _gifts = db.GetCollection<Gift>("gifts");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Category? category = null)
    {
        var filter = category.HasValue
            ? Builders<Gift>.Filter.Eq(g => g.Category, category.Value)
            : Builders<Gift>.Filter.Empty;

        var gifts = await _gifts.Find(filter).ToListAsync();
        return Ok(gifts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var gift = await _gifts.Find(g => g.Id == id).FirstOrDefaultAsync();
        return gift == null ? NotFound() : Ok(gift);
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
        return CreatedAtAction(nameof(GetById), new { id = gift.Id }, gift);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Gift gift)
    {
        gift.Id = id;
        var result = await _gifts.ReplaceOneAsync(g => g.Id == id, gift);
        return result.MatchedCount == 0 ? NotFound() : Ok(gift);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _gifts.DeleteOneAsync(g => g.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
