using api_server.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace server_API.DAL
{
    public class GiftDAL : IGiftDAL
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GiftDAL> _logger;

        public GiftDAL(AppDbContext context, ILogger<GiftDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Gift>> GetAllGifts()
        {
            _logger.LogInformation("DAL: Fetching all gifts");
            return await _context.gifts
                .Include(g => g.Donor)
                .Include(g => g.Purchases)
                .ToListAsync();
        }

        public async Task<Gift> GetGiftById(int id)
        {
            _logger.LogInformation("DAL: Fetching gift ID: {Id}", id);
            return await _context.gifts
                .Include(g => g.Donor)
                .Include(g => g.Purchases)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task AddGift(Gift gift)
        {
            _logger.LogInformation("DAL: Adding new gift to DB");
            _context.gifts.Add(gift);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGift(Gift gift)
        {
            _logger.LogInformation("DAL: Removing gift ID: {Id} from DB", gift.Id);
            _context.gifts.Remove(gift);
            await _context.SaveChangesAsync();
        }

        public async Task Save()
        {
            _logger.LogInformation("DAL: Executing SaveChanges for Gifts");
            await _context.SaveChangesAsync();
        }

        public async Task<List<Gift>> SearchGifts(string giftName = null, string donorName = null, int? minPurchasers = null)
        {
            _logger.LogInformation("DAL: Searching gifts in DB with filters");
            var query = _context.gifts.Include(g => g.Donor).AsQueryable();

            if (!string.IsNullOrWhiteSpace(giftName))
                query = query.Where(g => g.Name.Contains(giftName));

            if (!string.IsNullOrWhiteSpace(donorName))
                query = query.Where(g => g.Donor.Name.Contains(donorName));

            if (minPurchasers.HasValue)
            {
                query = query.Where(g => g.Purchases.Count >= minPurchasers.Value);
            }
            return await query.ToListAsync();
        }
    }
}