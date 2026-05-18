using api_server.Model;
using api_server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using server_API.DAL;
using WebApplication1.DAL.interfaces;

namespace WebApplication1.DAL
{
    public class LotteryDAL : ILotteryDAL
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LotteryDAL> _logger;

        public LotteryDAL(AppDbContext context, ILogger<LotteryDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Gift> GetGiftByIdAsync(int giftId)
        {
            _logger.LogInformation("DAL: Fetching gift for lottery: {GiftId}", giftId);
            return await _context.gifts
                .Include(g => g.Purchases)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(g => g.Id == giftId);
        }

        public async Task<List<Purchaser>> GetPurchasesForGiftAsync(int giftId)
        {
            _logger.LogInformation("DAL: Getting purchases list for gift: {GiftId}", giftId);
            var gift = await _context.gifts.Include(g => g.Purchases).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(g => g.Id == giftId);
            return gift?.Purchases.ToList() ?? new List<Purchaser>();
        }

        public async Task AddWinnerAsync(Lotteries lottery)
        {
            _logger.LogInformation("DAL: Recording new winner for gift {GiftId}", lottery.GiftId);
            _context.Lotteries.Add(lottery);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasWinnerForGiftAsync(int giftId)
        {
            return await _context.Lotteries.AnyAsync(l => l.GiftId == giftId);
        }

        public async Task<List<Lotteries>> GetWinnersByGiftAsync(int giftId)
        {
            _logger.LogInformation("DAL: Fetching winners for gift: {GiftId}", giftId);
            return await _context.Lotteries
                .Include(l => l.User)
                .Where(l => l.GiftId == giftId)
                .ToListAsync();
        }

        public async Task<List<Gift>> GetAllGiftsWithWinnersAsync()
        {
            _logger.LogInformation("DAL: Fetching all gifts and their winners for report");
            return await _context.gifts
                .Include(g => g.Lotteries)
                    .ThenInclude(l => l.User)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}