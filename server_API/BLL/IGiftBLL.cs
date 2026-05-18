using api_server.Model;
using server_API.DTO;

namespace server_API.BLL
{
    public interface IGiftBLL
    {
        Task<List<Gift>> GetAllGifts();
        Task<Gift> GetGiftById(int id);
        Task<GiftDTO> AddGift(GiftDTO gift);
        Task<GiftDTO> UpdateGift(int id ,GiftDTO gift);
        Task DeleteGift(int Id);
        Task<List<Gift>> SearchGifts(string giftName = null, string donorName = null, int? minPurchasers = null);

    }
}
