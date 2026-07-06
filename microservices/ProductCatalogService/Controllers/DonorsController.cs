using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProductCatalogService.Models;

namespace ProductCatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonorsController : ControllerBase
{
    private readonly IMongoCollection<Donor> _donors;

    public DonorsController(IMongoDatabase db)
    {
        _donors = db.GetCollection<Donor>("donors");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var donors = await _donors.Find(_ => true).ToListAsync();
        return Ok(donors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var donor = await _donors.Find(d => d.Id == id).FirstOrDefaultAsync();
        return donor == null ? NotFound() : Ok(donor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Donor donor)
    {
        await _donors.InsertOneAsync(donor);
        return CreatedAtAction(nameof(GetById), new { id = donor.Id }, donor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Donor donor)
    {
        donor.Id = id;
        var result = await _donors.ReplaceOneAsync(d => d.Id == id, donor);
        return result.MatchedCount == 0 ? NotFound() : Ok(donor);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _donors.DeleteOneAsync(d => d.Id == id);
        return result.DeletedCount == 0 ? NotFound() : NoContent();
    }
}
