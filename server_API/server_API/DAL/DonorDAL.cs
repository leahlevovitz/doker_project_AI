using api_server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace server_API.DAL
{
    public class DonorDAL : IDonorDAL
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DonorDAL> _logger;

        public DonorDAL(AppDbContext context, ILogger<DonorDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _context.Donors.AnyAsync(d => d.Email == email);
        }

        public async Task AddDonor(Donor donor)
        {
            _logger.LogInformation("DAL: Attempting to add donor with email: {Email}", donor.Email);
            bool exists = await EmailExists(donor.Email);
            if (exists)
            {
                _logger.LogWarning("DAL: Donor with email {Email} already exists", donor.Email);
                throw new Exception("Donor already exists");
            }

            await _context.Donors.AddAsync(donor);
            await _context.SaveChangesAsync();
            _logger.LogInformation("DAL: Donor added successfully");
        }

        public async Task DeleteDonor(Donor donor)
        {
            _logger.LogInformation("DAL: Deleting donor ID: {Id}", donor.Id);
            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Donor>> GetAllDonors()
        {
            _logger.LogInformation("DAL: Fetching all donors");
            return await _context.Donors
                .Include(d => d.GiftList)
                .ToListAsync();
        }

        public async Task<Donor> GetDonorById(int id)
        {
            _logger.LogInformation("DAL: Fetching donor ID: {Id}", id);
            return await _context.Donors
                .Include(d => d.GiftList)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task UpdateDonor()
        {
            _logger.LogInformation("DAL: Saving donor updates");
            await _context.SaveChangesAsync();
        }
    }
}