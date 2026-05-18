using api_server.Model;
using api_server.Models;


namespace WebApplication1.DAL.interfaces
{
    public interface ILotteryDAL
    {
        Task<Gift> GetGiftByIdAsync(int giftId);
        Task<List<Purchaser>> GetPurchasesForGiftAsync(int giftId);
        Task AddWinnerAsync(Lotteries lottery);
        Task<bool> HasWinnerForGiftAsync(int giftId);
        Task SaveChangesAsync();
        Task<List<Lotteries>> GetWinnersByGiftAsync(int giftId);
        Task<List<Gift>> GetAllGiftsWithWinnersAsync();
    }
}
