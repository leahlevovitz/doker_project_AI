using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NotificationService.Models;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LotteryController : ControllerBase
{
    private readonly IMongoCollection<LotteryResult> _results;

    public LotteryController(IMongoDatabase db)
    {
        _results = db.GetCollection<LotteryResult>("lottery_results");
    }

    // POST api/lottery/draw — מבצע הגרלה לפי רשימת משתתפים
    [HttpPost("draw")]
    public async Task<IActionResult> Draw([FromBody] DrawRequest request)
    {
        if (request.ParticipantUserIds == null || !request.ParticipantUserIds.Any())
            return BadRequest(new { message = "No participants provided" });

        var random = new Random();
        var winnerIndex = random.Next(request.ParticipantUserIds.Count);
        var winnerId = request.ParticipantUserIds[winnerIndex];

        var result = new LotteryResult
        {
            GiftId = request.GiftId,
            GiftName = request.GiftName,
            WinnerUserId = winnerId,
            WinnerName = request.ParticipantNames?.ElementAtOrDefault(winnerIndex) ?? string.Empty,
            LotteryDate = DateTime.UtcNow
        };

        await _results.InsertOneAsync(result);
        return Ok(result);
    }

    // GET api/lottery/{giftId}/winners
    [HttpGet("{giftId}/winners")]
    public async Task<IActionResult> GetWinners(int giftId)
    {
        var winners = await _results.Find(r => r.GiftId == giftId).ToListAsync();
        if (!winners.Any()) return NotFound(new { message = "No winners found for this gift" });
        return Ok(winners);
    }

    // GET api/lottery/report
    [HttpGet("report")]
    public async Task<IActionResult> GetReport()
    {
        var all = await _results.Find(_ => true).SortByDescending(r => r.LotteryDate).ToListAsync();
        return Ok(all);
    }
}

public class DrawRequest
{
    public int GiftId { get; set; }
    public string GiftName { get; set; } = string.Empty;
    public List<int> ParticipantUserIds { get; set; } = new();
    public List<string>? ParticipantNames { get; set; }
}
