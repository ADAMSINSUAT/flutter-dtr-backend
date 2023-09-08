using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.DTO;
using Sample_DTR_API.Models;

namespace Sample_DTR_API.Functions
{
    public static class CheckUserIdFunction
    {
        public static async Task<bool> CheckID(int userID, SampleDtrDbContext _sampleDtrDbContext)
        {
            var checkID = await _sampleDtrDbContext.UserCredentials.Select(u => new GetUserIdDTO
            {
                UserId = u.UserId,
            }).Where(x => x.UserId == userID).ToListAsync();
            return checkID.Any();
        }
    }
}
