using api_server.Models;
using server_API.DTO;
using System.Reflection;

namespace WebApplication1.BLL.interfaces
{
    public interface ILotteryBLL
    {
        Task<Lotteries> PerformLotteryAsync(int giftId);
        Task<List<Lotteries>> GetWinnersForGiftAsync(int giftId);
        Task<List<GiftWinnersDTO>> GetAllGiftsWithWinnersAsync();


    }

}
