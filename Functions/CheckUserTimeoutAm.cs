﻿using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.Global_variables;
using Sample_DTR_API.Models;

namespace Sample_DTR_API.Functions
{
    public static class CheckUserTimeoutAm
    {
        //Checks whether the user has timed out for morning, if true, return true, else return false
        public static async Task<Boolean> CheckTimeoutAm(int userID, SampleDtrDbContext _sampleDtrDbContext)
        {
            CurrentDate _date = new CurrentDate();
            var check = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == _date.currentDate && x.TimeOutAm != null).FirstOrDefaultAsync() != null ? true : false; ;
            return check;
        }
    }
}
