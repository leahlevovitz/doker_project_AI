using api_server.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using server_API.BLL;
using server_API.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonorsController : ControllerBase
    {
        private readonly IDonorBLL _BLL;
        private readonly IMapper _mapper;
        private readonly ILogger<DonorsController> _logger;

        public DonorsController(IDonorBLL Bll, IMapper mapper, ILogger<DonorsController> logger)
        {
            _BLL = Bll ?? throw new ArgumentNullException(nameof(Bll));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Request to retrieve all donors.");

                var donors = await _BLL.GetAllDonors();
                var dto = _mapper.Map<List<DonorDTO>>(donors);

                _logger.LogInformation("Successfully retrieved {Count} donors.", dto.Count);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving donors.");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid donor ID received: {Id}", id);
                return BadRequest("Invalid donor ID.");
            }

            try
            {
                _logger.LogInformation("Fetching donor with ID: {Id}", id);

                var donor = await _BLL.GetDonorById(id);
                if (donor == null)
                {
                    _logger.LogWarning("Donor with ID {Id} not found.", id);
                    return NotFound();
                }

                var dto = _mapper.Map<DonorDTO>(donor);

                _logger.LogInformation("Donor with ID {Id} retrieved successfully.", id);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving donor ID {Id}.", id);
                throw;
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(string? name, string? email, string? gift)
        {
            try
            {
                _logger.LogInformation("Filtering donors. Name: {Name}, Email: {Email}, Gift: {Gift}",
                    name ?? "None", email ?? "None", gift ?? "None");

                var donors = await _BLL.FilterDonors(name, email, gift);
                var dto = _mapper.Map<List<DonorDTO>>(donors);

                _logger.LogInformation("Filter completed. Found {Count} donors.", dto.Count);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while filtering donors.");
                throw;
            }
        }

        // POST: api/donors
        [HttpPost]
        public async Task<IActionResult> Create(DonorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ModelState while creating donor.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Creating new donor with Email: {Email}", dto.Email);

                var donor = _mapper.Map<Donor>(dto);
                await _BLL.AddDonor(donor);

                var resultDto = _mapper.Map<DonorDTO>(donor);

                _logger.LogInformation("Donor created successfully with ID: {Id}", donor.Id);

                return CreatedAtAction(nameof(GetById), new { id = donor.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating donor.");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DonorDTO dto)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid donor ID for update: {Id}", id);
                return BadRequest("Invalid donor ID.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ModelState while updating donor ID {Id}.", id);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Updating donor with ID: {Id}", id);

                var donor = await _BLL.GetDonorById(id);
                if (donor == null)
                {
                    _logger.LogWarning("Donor with ID {Id} not found for update.", id);
                    return NotFound();
                }

                _mapper.Map(dto, donor);
                await _BLL.UpdateDonor(donor);

                _logger.LogInformation("Donor with ID {Id} updated successfully.", id);

                return Ok(donor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating donor ID {Id}.", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid donor ID for deletion: {Id}", id);
                return BadRequest("Invalid donor ID.");
            }

            try
            {
                _logger.LogInformation("Attempting to delete donor with ID: {Id}", id);

                var donor = await _BLL.GetDonorById(id);
                if (donor == null)
                {
                    _logger.LogWarning("Donor with ID {Id} not found for deletion.", id);
                    return NotFound();
                }

                await _BLL.DeleteDonor(donor);

                _logger.LogInformation("Donor with ID {Id} deleted successfully.", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting donor ID {Id}.", id);
                throw;
            }
        }
    }
}
