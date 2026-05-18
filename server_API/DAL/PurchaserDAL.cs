using api_server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace server_API.DAL
{
    public class PurchaserDAL : IPurchaserDAL
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PurchaserDAL> _logger;

        public PurchaserDAL(AppDbContext context, ILogger<PurchaserDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Purchaser>> GetAllPurchasers()
        {
            _logger.LogInformation("DAL: Fetching all purchasers from DB");
            return await _context.Purchases
                .Include(p => p.User)
                .Include(p => p.Gift)
                .ToListAsync();
        }

        public async Task<List<Purchaser>> GetDraftsByIds(List<int> ids)
        {
            _logger.LogInformation("DAL: Fetching drafts for specific IDs");
            return await _context.Purchases
                .Where(p => ids.Contains(p.Id) && p.IsDraft)
                 .Include(p => p.Gift)
                .ToListAsync();
        }

        public async Task<List<Purchaser>> GetPurchasersByGift(int? giftId)
        {
            _logger.LogInformation("DAL: Fetching purchasers for giftId: {GiftId}", giftId);
            return await _context.Purchases
                .Where(p => giftId == null ? true : p.GiftId == giftId)
                .Include(p => p.User)
                .Include(p => p.Gift)
                .ToListAsync();
        }

        public async Task<Purchaser> GetPurchaserById(int id)
        {
            _logger.LogInformation("DAL: Fetching purchaser by ID: {Id}", id);
            return await _context.Purchases
                .Include(p => p.User)
                .Include(p => p.Gift)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddToBasket(Purchaser purchaser)
        {
            _logger.LogInformation("DAL: Adding new item to basket for user {UserId}", purchaser.UserId);
            await _context.Purchases.AddAsync(purchaser);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePurchaser(Purchaser purchaser)
        {
            _logger.LogInformation("DAL: Deleting purchaser record ID: {Id}", purchaser.Id);
            _context.Purchases.Remove(purchaser);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Purchaser>> GetAllWithGift()
        {
            _logger.LogInformation("DAL: Fetching all purchases with gift details");
            return await _context.Purchases
                .Include(p => p.Gift)
                .ToListAsync();
        }

        public async Task<List<Purchaser>> GetBasketByUser(int userId)
        {
            _logger.LogInformation("DAL: Fetching basket for userId: {UserId}", userId);
            return await _context.Purchases
                .Where(p => p.IsDraft && p.UserId == userId)
                .Include(p => p.Gift)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task Save()
        {
            _logger.LogInformation("DAL: Saving changes to DB");
            await _context.SaveChangesAsync();
        }
    }
}