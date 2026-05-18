using api_server.Models;
using server_API.DTO;

namespace server_API.BLL
{
    public interface IDonorBLL
    {
        Task<List<Donor>> GetAllDonors();
        Task<Donor> GetDonorById(int id);
        Task<List<Donor>> FilterDonors(string? name, string? email, string? gift);

        Task AddDonor(Donor donor);
        Task UpdateDonor(Donor donor);
        Task DeleteDonor(Donor donor);
    }
}
