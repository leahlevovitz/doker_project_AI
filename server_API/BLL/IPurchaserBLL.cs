using server_API.DTO;

namespace server_API.BLL
{
    public interface IPurchaserBLL
    {
        Task<List<PurchaserDTO>> GetAll(string sortBy = null);
        Task<List<PurchaserDTO>> GetByGift(int? giftId, string sortBy = null);
        Task<PurchaserDTO?> GetById(int id);
        Task AddToBasket(PurchaserDTO dto);

        Task Add(IEnumerable<PurchaserDTO> dtos );   // הוספה בלבד
        Task Delete(int id);               // מחיקה (רק טיוטא)
        Task<List<PurchaserDTO>> GetBasketByUser(int userId);
        Task<TotalRevenueDTO> GetTotalRevenue();
    }
}
