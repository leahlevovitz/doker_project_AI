using api_server.Models;

public interface IDonorDAL
{
    Task<bool> EmailExists(string email);
    Task AddDonor(Donor donor);
    Task DeleteDonor(Donor donor);
    Task<List<Donor>> GetAllDonors();
    Task<Donor> GetDonorById(int id);
    Task UpdateDonor();
}
