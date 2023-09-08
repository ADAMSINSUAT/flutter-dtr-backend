using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.DTO;
using Sample_DTR_API.Functions;
using Sample_DTR_API.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Sample_DTR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeInController : ControllerBase
    {
        private readonly SampleDtrDbContext _sampleDtrDbContext;
        DateTime currentDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
        public TimeInController(SampleDtrDbContext sampleDtrDbContext)
        {
            _sampleDtrDbContext = sampleDtrDbContext;
        }
        private static string CheckAMPM()
        {
            var ampm = DateTime.Now;
            Console.WriteLine(ampm.Hour);
            Console.WriteLine(ampm.Minute);

            if (ampm.Hour >= 13)
            {
                return "PM";
            }
            else
            {
                return "AM";
            }
        }

        private static TimeSpan? deductBreakTime(TimeSpan? time)
        {
            var startBreakTime = TimeSpan.FromHours(12);
            TimeSpan? resultTime = time - startBreakTime;
            return resultTime;
        }

        private static TimeSpan? deductEndWorkTime(TimeSpan? time)
        {
            var endWorkTime = TimeSpan.FromHours(17);
            TimeSpan? resultTime = time - endWorkTime;
            return resultTime;
        }

        private async Task<bool> CheckIfAlreadyTimeInDate(int userID)
        {
            var checkTimeInDate = await _sampleDtrDbContext.TimeIns.Where(x => x.TimeInDate == currentDate && x.UserId == userID).FirstOrDefaultAsync();
            Console.WriteLine(checkTimeInDate);

            if (checkTimeInDate != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task TimeInAM(int userID)
        {
            var timeInData = new TimeIn()
            {
                UserId = userID,
                TimeInDate = Convert.ToDateTime(DateTime.Now.ToShortDateString()),
                TimeInAm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0),
                TotalLoggedHours = 0,
            };

            await _sampleDtrDbContext.TimeIns.AddAsync(timeInData);
            await _sampleDtrDbContext.SaveChangesAsync();
        }

        private async Task<String> TimeOutAM(int userID)
        {
            //Gets the DTR of the user where it matches with the current date, user ID, and that the time out for the morning is currently null
            //If the result is null, it means that the user has already logged in the morning
            var userDTR = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == currentDate && x.TimeOutAm == null).FirstOrDefaultAsync();
            if (userDTR != null)
            {
                _sampleDtrDbContext.TimeIns.Entry(userDTR).State = EntityState.Modified;
                userDTR.TimeOutAm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                userDTR.TotalLoggedHours += (Convert.ToDouble(userDTR?.TimeOutAm.GetValueOrDefault().TotalMinutes - userDTR?.TimeInAm.GetValueOrDefault().TotalMinutes) - Convert.ToDouble(deductBreakTime(userDTR?.TimeOutAm).GetValueOrDefault().TotalMinutes)).Minutes().TotalMinutes / (double)60;

                await _sampleDtrDbContext.SaveChangesAsync();
                return "Success";
            }
            else
            {
                return "Error";
            }
        }

        private async Task TimeInPM(int userID, bool isTimedInAm, bool isTimedOutAm)
        {
            var userDTR = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == currentDate).FirstOrDefaultAsync();
            if (isTimedInAm)
            {
                if (isTimedOutAm)
                {
                    if (userDTR != null)
                    {
                        userDTR.TimeInPm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                        _sampleDtrDbContext.TimeIns.Entry(userDTR).State = EntityState.Modified;
                        await _sampleDtrDbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    await TimeOutAM(userID);
                }
            }
            else
            {
                var timeInData = new TimeIn()
                {
                    UserId = userID,
                    TimeInDate = Convert.ToDateTime(DateTime.Now.ToShortDateString()),
                    TimeInPm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0),
                    TotalLoggedHours = 0,
                };

                await _sampleDtrDbContext.TimeIns.AddAsync(timeInData);
                await _sampleDtrDbContext.SaveChangesAsync();
            }
        }

        private async Task<String> TimeOutPM(int userID)
        {
            var userDTR = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == currentDate && x.TimeOutPm == null).FirstOrDefaultAsync();

            if (userDTR != null)
            {
                _sampleDtrDbContext.TimeIns.Entry(userDTR).State = EntityState.Modified;

                userDTR.UserId = userID;
                userDTR.TimeInDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                userDTR.TimeOutPm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                userDTR.TotalLoggedHours += Convert.ToDouble(String.Format("{0:0.00}", (Convert.ToDouble(userDTR.TimeOutPm.GetValueOrDefault().TotalMinutes - userDTR.TimeInPm.GetValueOrDefault().TotalMinutes) - Convert.ToDouble(userDTR.TimeOutPm > TimeSpan.FromHours(17) ? Convert.ToDouble(deductEndWorkTime(userDTR?.TimeOutPm).GetValueOrDefault().TotalMinutes) : 0)) / (double)60));
                
                await _sampleDtrDbContext.SaveChangesAsync();

                return "Success";
            }
            return "Error";
        }

        private void FormatTime(TimeSpan time)
        {
            var amPm = time.Hours >= 12 ? "PM" : "AM";
            var formattedTime = $"{time} {amPm}";
            Console.WriteLine(formattedTime);
        }

        [HttpGet]
        [Route("GetUserDTR/{userID}")]
        public async Task<ActionResult<TimeInDTO>> GetDTR(int userID)
        {
            try
            {
                if (await CheckUserIdFunction.CheckID(userID, _sampleDtrDbContext))
                {
                    var DTR = await _sampleDtrDbContext.TimeIns.Select(u => new GetDtrDTO
                    {
                        UserId = u.UserId,
                        TimeInDate = u.TimeInDate.HasValue ? DateOnly.FromDateTime(u.TimeInDate.Value) : null,
                        TimeInAm = u.TimeInAm.HasValue ? new TimeOnly(u.TimeInAm.Value.Hours, u.TimeInAm.Value.Minutes).ToShortTimeString() : "Absent",
                        TimeOutAm = u.TimeOutAm.HasValue ? new TimeOnly(u.TimeOutAm.Value.Hours, u.TimeOutAm.Value.Minutes).ToShortTimeString() : "Absent",
                        TimeInPm = u.TimeInPm.HasValue ? new TimeOnly(u.TimeInPm.Value.Hours, u.TimeInPm.Value.Minutes).ToShortTimeString() : "Absent",
                        TimeOutPm = u.TimeOutPm.HasValue ? new TimeOnly(u.TimeOutPm.Value.Hours, u.TimeOutPm.Value.Minutes).ToShortTimeString() : "Absent",
                        TotalLoggedHours = u.TotalLoggedHours

                    }).Where(x => x.UserId == userID).ToListAsync();

                    if (DTR.Count != 0 && DTR != null)
                    {
                        return Ok(DTR);
                    }
                    return NotFound("No DTR logs to display as of yet");
                }
                else
                {
                    return NotFound("Incorrect user id!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("TimeInUser")]
        public async Task<IActionResult> TimeIn(TimeInDTO timeInDTO)
        {
            try
            {
                var userID = timeInDTO.UserId;

                //Checks if the user ID exists in the UserCredentials DB
                if (await CheckUserIdFunction.CheckID(userID, _sampleDtrDbContext))
                {
                    //Checks whether the user has timed in the morning, if true, return true, else return false
                    var isUserTimedInAm = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == currentDate && x.TimeInAm != null).FirstOrDefaultAsync() != null ? true : false;
                    //Check whether the user has timed out for morning, if true, return true, else return false
                    var isUserTimedOutAm = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == currentDate && x.TimeOutAm != null).FirstOrDefaultAsync() != null ? true : false;

                    //Check whether the user has timed in the afternoon, if true, return true, else return false
                    var isUserTimedInPM = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInPm == null && x.TimeInDate == currentDate).FirstOrDefaultAsync() != null ? true : false;

                    //Checks whether the user has already timed for today
                    if (await CheckIfAlreadyTimeInDate(userID))
                    {
                        //Checks whether the time is AM or PM
                        //If the time is more than or equals to 1:00 PM, it will return PM, else it will return AM
                        if (CheckAMPM() == "AM")
                        {
                            //Times out the user for the AM
                            //If the has not timed out for the morning, it will succeed. Else, it will return an error message
                            if (await TimeOutAM(userID) == "Success")
                            {
                                return Ok("Successfully timed out!");
                            }
                            return BadRequest("User has already timed out for this morning");
                        }

                        else
                        {
                            //Checks whether the user has timed in for the afternoon
                            if (isUserTimedInPM)
                            {
                                //Times in the user for the afternoon
                                //Takes a parameter of the user ID, and two boolean valuse of the user if they've timed in for the morning yet, and if they've timed out for the morning
                                await TimeInPM(userID, isUserTimedInAm, isUserTimedOutAm);

                                //Checks whether the user has timed out for the morning
                                if (isUserTimedOutAm)
                                {
                                    return Ok("Successfully timed in!");
                                }
                                else
                                {
                                    return Ok("Successfully timed in!");
                                }
                            }
                            else
                            {
                                //Times out the user for the afternoon
                                //If the user has not timed out, it will return return an error message
                                if (await TimeOutPM(userID) == "Success")
                                {
                                    return Ok("Successfully timed out!");
                                }
                                else
                                {
                                    return BadRequest("User has already timed out for this afternoon");
                                }
                            }
                        }
                    }
                    else
                    {
                        //Checks whether the time is AM or PM
                        //If the time is more than or equals to 1:00 PM, it will return PM, else it will return AM
                        if (CheckAMPM() == "AM")
                        {
                            //Times in the user for the morning
                            await TimeInAM(userID);
                            return Ok("Successfully timed in!");
                        }
                        else
                        {
                            //Times in the user for the afternoon
                            //Takes a parameter of the user ID, and two boolean valuse of the user if they've timed in for the morning yet, and if they've timed out for the morning
                            await TimeInPM(userID, isUserTimedInAm, isUserTimedOutAm);
                            return Ok("Successfully timed in!");
                        }
                    }
                }
                return NotFound("Incorrect user id!");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //public async void TimeInAM(TimeInDTO timeInDTO)
        //{

        //}
    }
}
