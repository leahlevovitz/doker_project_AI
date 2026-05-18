using api_server.Models;
using server_API.DTO;
using System.Reflection;
using WebApplication1.BLL.interfaces;
using WebApplication1.DAL.interfaces;
using Microsoft.Extensions.Logging;

public class LotteryBLL : ILotteryBLL
{
    private readonly ILotteryDAL _repository;
    private readonly ILogger<LotteryBLL> _logger;

    public LotteryBLL(
        ILotteryDAL repository,
        ILogger<LotteryBLL> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Lotteries> PerformLotteryAsync(int giftId)
    {
        _logger.LogInformation("Performing lottery for giftId: {GiftId}", giftId);

        var alreadyExists = await _repository.HasWinnerForGiftAsync(giftId);

        if (alreadyExists)
        {
            _logger.LogWarning("Lottery already performed for giftId: {GiftId}", giftId);
            throw new Exception("כבר בוצעה הגרלה למתנה זו");
        }

        var gift = await _repository.GetGiftByIdAsync(giftId);

        if (gift == null)
        {
            _logger.LogError("Gift not found for giftId: {GiftId}", giftId);
            throw new Exception("המתנה לא קיימת");
        }

        if (gift.Purchases == null || gift.Purchases.Count == 0)
        {
            _logger.LogWarning("No purchases found for giftId: {GiftId}", giftId);
            throw new Exception("אין רכישות למתנה זו");
        }

        int winnersCount = gift.Quantity;

        if (winnersCount <= 0)
        {
            _logger.LogError("Invalid gift quantity for giftId: {GiftId}", giftId);
            throw new Exception("כמות מתנות לא חוקית");
        }

        var random = new Random();

        var selectedPurchases = gift.Purchases
            .OrderBy(x => random.Next())
            .Take(winnersCount)
            .ToList();

        Lotteries lastWinner = null;

        foreach (var purchase in selectedPurchases)
        {
            if (purchase.User == null)
            {
                _logger.LogError("User data missing for purchaseId: {PurchaseId}", purchase.Id);
                throw new Exception("נתוני משתמש חסרים");
            }

            var winner = new Lotteries
            {
                GiftId = gift.Id,
                UserId = purchase.UserId,
                LotteryDate = DateTime.Now
            };

            await _repository.AddWinnerAsync(winner);

            gift.IsLocked = true;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Winner added: UserId={UserId}, GiftId={GiftId}", purchase.UserId, gift.Id);

            lastWinner = winner;
        }

        _logger.LogInformation("Lottery completed for giftId: {GiftId}", giftId);

        return lastWinner;
    }

    public async Task<List<Lotteries>> GetWinnersForGiftAsync(int giftId)
    {
        _logger.LogInformation("Getting winners for giftId: {GiftId}", giftId);
        try { var w = await _repository.GetWinnersByGiftAsync(giftId);
            _logger.LogInformation("GetWinnersForGiftAsync socsses");
            return w;
        
            }      
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all donors.");
                throw;
            }
    }

    public async Task<List<GiftWinnersDTO>> GetAllGiftsWithWinnersAsync()
    {
        _logger.LogInformation("Getting all gifts with winners");
        var gifts = await _repository.GetAllGiftsWithWinnersAsync();

        return gifts.Select(g => new GiftWinnersDTO
        {
            GiftId = g.Id,
            GiftName = g.Name,
            Winners = g.Lotteries
                .Select(l => l.User.Name)
                .ToList()
        }).ToList();
    }
}


