using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BffService.Controllers;

[ApiController]
[Route("bff/[controller]")]
public class OrderDetailsController : ControllerBase
{
    private readonly IHttpClientFactory _factory;

    public OrderDetailsController(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    // GET /bff/orderdetails/{purchaseId}
    // מאחד: נתוני הזמנה מ-OrderService + נתוני מוצר מ-ProductCatalogService
    [HttpGet("{purchaseId}")]
    public async Task<IActionResult> GetOrderDetails(int purchaseId)
    {
        var orderClient = _factory.CreateClient("order");
        var catalogClient = _factory.CreateClient("catalog");

        var orderResponse = await orderClient.GetAsync($"/api/purchases/{purchaseId}");
        if (!orderResponse.IsSuccessStatusCode)
            return NotFound(new { message = $"Purchase {purchaseId} not found" });

        var orderJson = await orderResponse.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<JsonElement>(orderJson);

        var giftId = order.GetProperty("giftId").GetInt32();

        var catalogResponse = await catalogClient.GetAsync($"/api/gifts/{giftId}");
        JsonElement? gift = null;
        if (catalogResponse.IsSuccessStatusCode)
        {
            var giftJson = await catalogResponse.Content.ReadAsStringAsync();
            gift = JsonSerializer.Deserialize<JsonElement>(giftJson);
        }

        return Ok(new { Purchase = order, Gift = gift });
    }

    // GET /bff/orderdetails/user/{userId}
    // מאחד: כל ההזמנות של משתמש + פרטי המוצרים שלהן
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrderDetails(int userId)
    {
        var orderClient = _factory.CreateClient("order");
        var catalogClient = _factory.CreateClient("catalog");

        var basketResponse = await orderClient.GetAsync($"/api/purchases/basket?userId={userId}");
        if (!basketResponse.IsSuccessStatusCode)
            return StatusCode((int)basketResponse.StatusCode);

        var basketJson = await basketResponse.Content.ReadAsStringAsync();
        var purchases = JsonSerializer.Deserialize<JsonElement[]>(basketJson) ?? [];

        var tasks = purchases.Select(async p =>
        {
            var giftId = p.GetProperty("giftId").GetInt32();
            var giftResponse = await catalogClient.GetAsync($"/api/gifts/{giftId}");
            JsonElement? gift = null;
            if (giftResponse.IsSuccessStatusCode)
                gift = JsonSerializer.Deserialize<JsonElement>(await giftResponse.Content.ReadAsStringAsync());

            return new { Purchase = p, Gift = gift };
        });

        var results = await Task.WhenAll(tasks);
        return Ok(results);
    }
}
