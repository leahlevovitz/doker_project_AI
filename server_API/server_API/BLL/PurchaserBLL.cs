using api_server.Model;
using api_server.Models;
using Microsoft.Extensions.Logging; 
using server_API.DAL;
using server_API.DTO;
using server_API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server_API.BLL
{
    public class PurchaserBLL : IPurchaserBLL
    {
        private readonly IPurchaserDAL _dal;
        private readonly ILogger<PurchaserBLL> _logger; 

        public PurchaserBLL(IPurchaserDAL dal, ILogger<PurchaserBLL> logger)
        {
            _dal = dal;
            _logger = logger;
        }

        public async Task<List<PurchaserDTO>> GetAll(string sortBy = null)
        {
            _logger.LogInformation("Retrieving all finalized purchases. SortBy: {SortBy}", sortBy ?? "None");

            var data = (await _dal.GetAllPurchasers())
                .Where(p => !p.IsDraft );

            data = ApplySorting(data, sortBy);

            return data.Select(MapToDTO).ToList();
        }

        public async Task<List<PurchaserDTO>> GetByGift(int? giftId, string sortBy = null)
        {
            _logger.LogInformation("Fetching purchases for Gift ID: {GiftId}", giftId);

            var data = (await _dal.GetPurchasersByGift(giftId))
                   .ToList()
                .Where(p => !p.IsDraft);

            data = ApplySorting(data, sortBy);

            return data.Select(MapToDTO).ToList();
        }

        public async Task<PurchaserDTO?> GetById(int id)
        {
            _logger.LogInformation("Fetching purchase details for ID: {PurchaseId}", id);
            var p = await _dal.GetPurchaserById(id);

            if (p == null || p.IsDraft)
            {
                _logger.LogWarning("Purchase ID {PurchaseId} not found or is still a draft.", id);
                return null;
            }

            return MapToDTO(p);
        }

        public async Task Add(IEnumerable<PurchaserDTO> dtos)
        {
            var draftIds = dtos.Select(d => d.Id).ToList();
            _logger.LogInformation("Finalizing purchases for draft IDs: {Ids}", string.Join(",", draftIds));

            try
            {
                var drafts = await _dal.GetDraftsByIds(draftIds);

                foreach (var draft in drafts)
                {
                    if (!draft.IsDraft)
                    {
                        _logger.LogError("Attempted to finalize a non-draft purchase. ID: {Id}", draft.Id);
                        throw new Exception($"Purchase {draft.Id} is not a draft");
                    }
                    if (draft.Gift.IsLocked)
                    {
                        _logger.LogWarning("Attempt to purchase locked gift. Gift ID: {GiftId}", draft.GiftId);
                        throw new Exception("This gift is already locked and cannot be purchased.");
                    }
                    draft.IsDraft = false;
                    draft.PurchaseDate = DateTime.UtcNow;
                }

                await _dal.Save();
                _logger.LogInformation("Successfully finalized {Count} purchases.", drafts.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during purchase finalization.");
                throw;
            }
        }

        public async Task AddToBasket(PurchaserDTO dto)
        {
            _logger.LogInformation("Adding Gift ID {GiftId} to basket for User ID {UserId}.", dto.GiftId, dto.UserId);

            var purchaser = new Purchaser
            {
                GiftId = dto.GiftId,
                UserId = dto.UserId,
                PurchaseDate = DateTime.UtcNow,
                IsDraft = true,
                

            };

            await _dal.AddToBasket(purchaser);
        }

        public async Task<List<PurchaserDTO>> GetBasketByUser(int userId)
        {
            _logger.LogInformation("Retrieving basket items for user {UserId}.", userId);

            var data = (await _dal.GetAllPurchasers())
                .Where(p => p.IsDraft && p.UserId == userId && !p.Gift.IsLocked);

            return data.Select(MapToDTO).ToList();
        }

        public async Task Delete(int id)
        {
            _logger.LogInformation("Attempting to delete item from basket. ID: {Id}", id);

            var purchaser = await _dal.GetPurchaserById(id);
            if (purchaser == null)
            {
                _logger.LogWarning("Delete failed: Item ID {Id} not found.", id);
                throw new Exception("Purchase not found");
            }

            if (!purchaser.IsDraft)
            {
                _logger.LogError("Security Alert: User tried to delete a finalized purchase! ID: {Id}", id);
                throw new Exception("Cannot delete a finalized purchase");
            }

            await _dal.DeletePurchaser(purchaser);
            _logger.LogInformation("Item ID {Id} removed from basket successfully.", id);
        }

        public async Task<TotalRevenueDTO> GetTotalRevenue()
        {
            _logger.LogInformation("Calculating total revenue for the auction.");

            var purchases = await _dal.GetAllWithGift();
            var totalIncome = purchases.Sum(p => p.Gift.price);

            _logger.LogInformation("Revenue calculated: {TotalCount} purchases, Total Income: {TotalIncome}", purchases.Count, totalIncome);

            return new TotalRevenueDTO
            {
                TotalPurchases = purchases.Count,
                TotalIncome = totalIncome
            };
        }

        // ---------- helpers ----------

        private IEnumerable<Purchaser> ApplySorting(IEnumerable<Purchaser> data, string sortBy)
        {
            return sortBy switch
            {
                "price" => data.OrderByDescending(p => p.Gift.price),
                "count" => data
                    .GroupBy(p => p.GiftId)
                    .OrderByDescending(g => g.Count())
                    .SelectMany(g => g),
                _ => data
            };
        }

        private PurchaserDTO MapToDTO(Purchaser p)
        {
            return new PurchaserDTO
            {
                Id = p.Id,
                GiftId = p.GiftId,
                UserId = p.UserId,
                UserName = p.User.UserName,
                UserAddress = p.User.adress,
                UserEmail = p.User.Email,
                UserPhone = p.User.phone,
                Name = p.User.Name,
                GiftImage = p.Gift.Image,
                GiftName = p.Gift.Name,
                GiftPrice = p.Gift.price,
                PurchaseDate = p.PurchaseDate
            };
        }
    }
}