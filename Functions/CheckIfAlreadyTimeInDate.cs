using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.DTO;
using Sample_DTR_API.Global_variables;
using Sample_DTR_API.Models;

namespace Sample_DTR_API.Functions
{
    public class CheckIfAlreadyTimeInDate
    {
        public static async Task<Boolean> CheckDate(int userID, SampleDtrDbContext _sampleDtrDbContext)
        {
            CurrentDate date = new CurrentDate();
            var check = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == date.currentDate).FirstOrDefaultAsync() != null ? true : false;
            return check;
        }
    }
}
