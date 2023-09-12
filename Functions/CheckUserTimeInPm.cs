using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.Global_variables;
using Sample_DTR_API.Models;

namespace Sample_DTR_API.Functions
{
    public static class CheckUserTimeInPm
    {
        //Checks whether the user has timed in the afternoon, if true, return true, else return false
        public static async Task<Boolean> CheckTimeinPm(int userID, SampleDtrDbContext _sampleDtrDbContext)
        {
            CurrentDate _date = new CurrentDate();
            var check = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInPm != null && x.TimeInDate == _date.currentDate).FirstOrDefaultAsync() != null ? true : false;
            var testCheck = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInPm != null && x.TimeInDate == _date.currentDate).FirstOrDefaultAsync();

            if (testCheck != null)
            {
                Console.WriteLine("User has already timed in for PM");
            }
            else
            {
                Console.WriteLine("User hasn't already timed in for PM");
            }
            return check;
        }
    }
}
