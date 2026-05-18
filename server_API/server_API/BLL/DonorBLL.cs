using api_server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 
using server_API.DAL;
using server_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server_API.BLL
{
    public class DonorBLL : IDonorBLL
    {
        private readonly IDonorDAL _donorDAL;
        private readonly ILogger<DonorBLL> _logger;

        public DonorBLL(IDonorDAL donorDAL, ILogger<DonorBLL> logger)
        {
            _donorDAL = donorDAL;
            _logger = logger;
        }

        public async Task<List<Donor>> GetAllDonors()
        {
            _logger.LogInformation("Attempting to retrieve all donors.");
            try
            {
                var donors = await _donorDAL.GetAllDonors();
                _logger.LogInformation("Successfully retrieved {Count} donors.", donors.Count);
                return donors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all donors.");
                throw;
            }
        }

        public async Task<Donor> GetDonorById(int id)
        {
            _logger.LogInformation("Fetching donor with ID: {DonorId}", id);
            var donor = await _donorDAL.GetDonorById(id);

            if (donor == null)
            {
                _logger.LogWarning("Donor with ID {DonorId} was not found in the database.", id);
            }

            return donor;
        }

        public async Task<List<Donor>> FilterDonors(string? name, string? email, string? gift)
        {
            _logger.LogInformation("Filtering donors with criteria - Name: {Name}, Email: {Email}, Gift: {Gift}", name, email, gift);

            var donors = await _donorDAL.GetAllDonors();

            if (!string.IsNullOrWhiteSpace(name))//אם השם לא ריק או רווחים בלבד, מבצעים סינון על השמות של התורמים
                donors = donors.Where(d => d.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();//סינון התורמים לפי שם, תוך התעלמות מהבדלי רישיות

            if (!string.IsNullOrWhiteSpace(email))
                donors = donors.Where(d => d.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(gift))
                donors = donors.Where(d => d.GiftList != null && d.GiftList.Any(g => g.Name.Contains(gift, StringComparison.OrdinalIgnoreCase))).ToList();

            _logger.LogInformation("Filter returned {Count} results.", donors.Count);
            return donors;
        }

        public async Task AddDonor(Donor donor)
        {
            //להוסיץ וידציות של שם
            _logger.LogInformation("Starting process to add donor: {Email}", donor.Email);

            // וולידציה של אימייל
            if (string.IsNullOrWhiteSpace(donor.Email) || !donor.Email.Contains("@") || !donor.Email.Contains("."))
            {
                _logger.LogWarning("Validation failed: Email format is invalid for donor {DonorName}.", donor.Name);
                throw new Exception("Invalid email format");
            }

            // בדיקת כפילות
            if (await _donorDAL.EmailExists(donor.Email))
            {
                _logger.LogWarning("Validation failed: Email {Email} already exists.", donor.Email);
                throw new Exception("Email already exists");
            }

            try
            {
                await _donorDAL.AddDonor(donor);
                _logger.LogInformation("Donor {DonorName} was successfully added with ID: {DonorId}", donor.Name, donor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Critical failure while adding donor {Email} to the database.", donor.Email);
                throw;
            }
        }

        public async Task UpdateDonor(Donor _donor)
        {
            //להוסיף וידציות של שם ואימייל
            _logger.LogInformation("Attempting to update donor ID: {DonorId}", _donor.Id);

            var donor = await _donorDAL.GetDonorById(_donor.Id);
            if (donor == null)
            {
                _logger.LogError("Update failed: Donor ID {DonorId} not found.", _donor.Id);
                throw new Exception("Donor not found");
            }

            donor.Name = _donor.Name;
            donor.Email = _donor.Email;

            try
            {
                await _donorDAL.UpdateDonor();
                _logger.LogInformation("Donor ID {DonorId} updated successfully.", _donor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating donor ID {DonorId}.", _donor.Id);
                throw;
            }
        }

        public async Task DeleteDonor(Donor donor)
        {
            _logger.LogInformation("Request to delete donor ID: {DonorId}", donor.Id);

            if (donor.GiftList != null && donor.GiftList.Count != 0)
            {
                _logger.LogWarning("Delete aborted: Donor ID {DonorId} still has {Count} gifts linked.", donor.Id, donor.GiftList.Count);
                throw new Exception("לא ניתן למחוק תורם שיש לו מתנות רשומות במערכת.");
            }

            try
            {
                await _donorDAL.DeleteDonor(donor);
                _logger.LogInformation("Donor ID {DonorId} deleted successfully.", donor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting donor ID {DonorId}.", donor.Id);
                throw;
            }
        }
    }
}