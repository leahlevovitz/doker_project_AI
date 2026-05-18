using api_server.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server_API.BLL;
using server_API.DTO;
using Microsoft.Extensions.Logging;

namespace server_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftsController : ControllerBase
    {
        private readonly IGiftBLL _giftBLL;
        private readonly IDonorBLL _donorBLL;
        private readonly IMapper _mapper;
        private readonly ILogger<GiftsController> _logger;

        public GiftsController(IGiftBLL giftBLL, IDonorBLL donorBLL, IMapper mapper, ILogger<GiftsController> logger)
        {
            _giftBLL = giftBLL;
            _donorBLL = donorBLL;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all gifts");
            var gifts = await _giftBLL.GetAllGifts();
            var dto = _mapper.Map<List<GiftDTO>>(gifts);
            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching gift with ID: {Id}", id);
            var gift = await _giftBLL.GetGiftById(id);
            if (gift == null)
            {
                _logger.LogWarning("Gift with ID: {Id} not found", id);
                return NotFound();
            }

            var dto = _mapper.Map<GiftDTO>(gift);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> Create(GiftDTO giftDto)
        {
            _logger.LogInformation("Creating new gift: {GiftName}", giftDto.Name);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating gift");
                return BadRequest(ModelState);
            }

            var result = await _giftBLL.AddGift(giftDto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> Update(int id, GiftDTO dto)
        {
            _logger.LogInformation("Updating gift ID: {Id}", id);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedGift = await _giftBLL.UpdateGift(id, dto);
            if (updatedGift == null)
            {
                _logger.LogWarning("Update failed: Gift ID {Id} not found", id);
                return NotFound();
            }
            return Ok(updatedGift);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting gift ID: {Id}", id);
            try
            {
                await _giftBLL.DeleteGift(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Delete failed: Gift ID {Id} not found", id);
                return NotFound();
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string giftName = null, [FromQuery] string donorName = null, [FromQuery] int? minPurchasers = null)
        {
            _logger.LogInformation("Searching gifts. Name: {GiftName}, Donor: {DonorName}", giftName, donorName);
            var results = await _giftBLL.SearchGifts(giftName, donorName, minPurchasers);
            var dto = _mapper.Map<List<GiftDTO>>(results);
            return Ok(dto);
        }
    }
}