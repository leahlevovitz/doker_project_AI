using api_server.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication1.BLL.interfaces;
using WebApplication1.DTO;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryBLL _lotteryService;
        private readonly ILogger<LotteryController> _logger;

        public LotteryController(ILotteryBLL lotteryService, ILogger<LotteryController> logger)
        {
            _lotteryService = lotteryService;
            _logger = logger;
        }

        [HttpPost("draw/{giftId}")]
        public async Task<IActionResult> DrawLottery(int giftId)
        {
            _logger.LogInformation("Starting lottery draw for giftId: {GiftId}", giftId);
            try
            {
                var winner = await _lotteryService.PerformLotteryAsync(giftId);
                _logger.LogInformation("Lottery draw successful for giftId: {GiftId}. Winner UserId: {UserId}", giftId, winner.UserId);
                return Ok("הוגרל בהצלחה");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during lottery draw for giftId: {GiftId}", giftId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{giftId}/winners")]
        public async Task<IActionResult> GetWinnersForGift(int giftId)
        {
            _logger.LogInformation("Fetching winners for giftId: {GiftId}", giftId);
            var winners = await _lotteryService.GetWinnersForGiftAsync(giftId);

            if (winners == null || winners.Count == 0)
            {
                _logger.LogWarning("No winners found for giftId: {GiftId}", giftId);
                return NotFound("אין זוכים עבור מתנה זו");
            }

            return Ok(winners);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport()
        {
            _logger.LogInformation("Generating full lottery report");
            try
            {
                var result = await _lotteryService.GetAllGiftsWithWinnersAsync();
                _logger.LogInformation("Generating full lottery report");

                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Errer by Generating full lottery report" );
                return BadRequest(ex.Message);
            }
        }
    }
}