using api_server.Models;
using server_API.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server_API.DAL
{
    public interface IPurchaserDAL
    {
        Task<List<Purchaser>> GetAllPurchasers();                    // כל הרוכשים ללא סינון/מיון
        Task<List<Purchaser>> GetPurchasersByGift(int? giftId);       // רוכשים לפי מתנה ללא סינון/מיון
        Task<Purchaser> GetPurchaserById(int id);                    // רוכש לפי Id
      
        Task AddToBasket(Purchaser purchaser);                      // הוספה לסל
        //Task AddPurchaser(IEnumerable<Purchaser> purchasers);       // הוספה
        Task DeletePurchaser(Purchaser purchaser);                  // מחיקה
        Task<List<Purchaser>> GetBasketByUser(int giftId);          // סל לפי משתמש
        Task Save();
        Task<List<Purchaser>> GetDraftsByIds(List<int> ids);
        Task<List<Purchaser>> GetAllWithGift();

    }
}