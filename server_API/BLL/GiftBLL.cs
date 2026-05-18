using api_server.Model;
using api_server.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using server_API.DAL;
using server_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server_API.BLL
{
    public class GiftBLL : IGiftBLL
    {
        private readonly IGiftDAL _giftDAL;
        private readonly IDonorDAL _donorDAL;
        private readonly IMapper _mapper;
        private readonly ILogger<GiftBLL> _logger;

        public GiftBLL(
            IGiftDAL giftDAL,
            IDonorDAL donorDAL,
            IMapper mapper,
            ILogger<GiftBLL> logger)
        {
            _giftDAL = giftDAL ?? throw new ArgumentNullException(nameof(giftDAL));
            _donorDAL = donorDAL ?? throw new ArgumentNullException(nameof(donorDAL));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ========================= GET ALL =========================
        public async Task<List<Gift>> GetAllGifts()
        {
            try
            {
                _logger.LogInformation("Request to retrieve all gifts started.");

                var gifts = await _giftDAL.GetAllGifts();

                _logger.LogInformation("Successfully retrieved {Count} gifts.", gifts?.Count ?? 0);

                return gifts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all gifts.");
                throw;
            }
        }

        // ========================= GET BY ID =========================
        public async Task<Gift> GetGiftById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid gift id received: {Id}", id);
                throw new ArgumentException("Invalid gift id.");
            }

            try
            {
                _logger.LogInformation("Fetching gift by id: {Id}", id);

                var gift = await _giftDAL.GetGiftById(id);

                if (gift == null)
                {
                    _logger.LogWarning("Gift with ID {Id} was not found.", id);
                    throw new KeyNotFoundException($"Gift with id {id} not found.");
                }

                _logger.LogInformation("Gift with ID {Id} retrieved successfully.", id);

                return gift;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching gift with id: {Id}", id);
                throw;
            }
        }

        // ========================= ADD =========================
        public async Task<GiftDTO> AddGift(GiftDTO giftDto)
        {
            if (giftDto == null)
            {
                _logger.LogWarning("AddGift called with null DTO.");
                throw new ArgumentNullException(nameof(giftDto));
            }

            if (string.IsNullOrWhiteSpace(giftDto.Name))
                throw new ArgumentException("Gift name is required.");

            if (giftDto.DonorId <= 0)
                throw new ArgumentException("Invalid DonorId.");

            if (string.IsNullOrWhiteSpace(giftDto.Image))
                throw new ArgumentException("Image is required.");

            if (giftDto.price < 20 || giftDto.price > 150)
                throw new ArgumentException("Price must be between 20 and 150.");

            if (!Enum.TryParse<Category>(giftDto.Category, true, out var parsedCategory))
                throw new ArgumentException("Invalid category value.");

            try
            {
                _logger.LogInformation("Adding new gift. DonorId: {DonorId}, Name: {Name}", giftDto.DonorId, giftDto.Name);

                var donor = await _donorDAL.GetDonorById(giftDto.DonorId);
                if (donor == null)
                {
                    _logger.LogWarning("Donor not found for id: {DonorId}", giftDto.DonorId);
                    throw new KeyNotFoundException("Donor not found.");
                }

                var gift = _mapper.Map<Gift>(giftDto);

                gift.DonorId = donor.Id;
                gift.Donor = donor;
                gift.Image = giftDto.Image;
                gift.category = parsedCategory;
                gift.price = giftDto.price;
                gift.IsLocked = false;

                await _giftDAL.AddGift(gift);

                var returnDto = _mapper.Map<GiftDTO>(gift);
                returnDto.donorName = donor.Name;

                _logger.LogInformation("Gift added successfully with id: {GiftId}", gift.Id);

                return returnDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding gift for donorId: {DonorId}", giftDto.DonorId);
                throw;
            }
        }

        // ========================= UPDATE =========================
        public async Task<GiftDTO> UpdateGift(int id, GiftDTO dto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid gift id.");

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Gift name is required.");

            if (dto.DonorId <= 0)
                throw new ArgumentException("Invalid DonorId.");

            if (dto.price < 20 || dto.price > 150)
                throw new ArgumentException("Price must be between 20 and 150.");

            if (!Enum.TryParse<Category>(dto.Category, true, out var parsedCategory))
                throw new ArgumentException("Invalid category value.");

            try
            {
                _logger.LogInformation("Updating gift with id: {Id}", id);

                var gift = await _giftDAL.GetGiftById(id);
                if (gift == null)
                {
                    _logger.LogWarning("Gift not found for id: {Id}", id);
                    throw new KeyNotFoundException("Gift not found.");
                }

                var donor = await _donorDAL.GetDonorById(dto.DonorId);
                if (donor == null)
                {
                    _logger.LogWarning("Invalid DonorId: {DonorId}", dto.DonorId);
                    throw new KeyNotFoundException("Invalid DonorId.");
                }

                gift.Name = dto.Name;
                gift.price = dto.price;
                gift.category = parsedCategory;
                gift.DonorId = donor.Id;
                gift.Image = dto.Image;

                await _giftDAL.Save();

                _logger.LogInformation("Gift updated successfully with id: {GiftId}", gift.Id);

                return _mapper.Map<GiftDTO>(gift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating gift with id: {Id}", id);
                throw;
            }
        }

        // ========================= DELETE =========================
        public async Task DeleteGift(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid gift id.");

            try
            {
                _logger.LogInformation("Deleting gift with id: {Id}", id);

                var gift = await _giftDAL.GetGiftById(id);

                if (gift == null)
                {
                    _logger.LogWarning("Gift not found for id: {Id}", id);
                    throw new KeyNotFoundException($"Gift {id} not found");
                }

                // בדיקה האם קיימת רכישה שהיא כבר לא טיוטה
                if (gift.Purchases != null && gift.Purchases.Any(p => !p.IsDraft))
                {
                    _logger.LogWarning("Cannot delete gift {Id} - has confirmed purchases.", id);
                    throw new InvalidOperationException("Cannot delete a gift that has confirmed purchases.");
                }

                if (gift.IsLocked)
                {
                    _logger.LogWarning("Gift {Id} is locked and cannot be deleted.", id);
                    throw new InvalidOperationException("This gift is locked and cannot be deleted.");
                }

                await _giftDAL.DeleteGift(gift);

                _logger.LogInformation("Gift deleted successfully with id: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting gift with id: {Id}", id);
                throw;
            }
        }

        // ========================= SEARCH =========================
        public async Task<List<Gift>> SearchGifts(
            string giftName = null,
            string donorName = null,
            int? minPurchasers = null)
        {
            try
            {
                _logger.LogInformation(
                    "Searching gifts. giftName: {GiftName}, donorName: {DonorName}, minPurchasers: {MinPurchasers}",
                    giftName, donorName, minPurchasers);

                if (minPurchasers.HasValue && minPurchasers < 0)
                    throw new ArgumentException("minPurchasers cannot be negative.");

                var result = await _giftDAL.SearchGifts(giftName, donorName, minPurchasers);

                _logger.LogInformation("Search completed. Found {Count} results.", result?.Count ?? 0);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching gifts.");
                throw;
            }
        }
    }
}
