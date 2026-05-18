using api_server.Model;

namespace server_API.DAL
{
    public interface IGiftDAL
    {

        Task AddGift(Gift gift);
        Task DeleteGift(Gift gift);
        Task<List<Gift>> GetAllGifts();
        Task<Gift> GetGiftById(int id);
        Task Save();
        Task<List<Gift>> SearchGifts(string giftName, string donorName, int? minPurchasers);
    }
}
