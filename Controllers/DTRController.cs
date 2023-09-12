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
using Sample_DTR_API.Global_variables;
using Sample_DTR_API.Models;

namespace Sample_DTR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DTRController : ControllerBase
    {
        private readonly SampleDtrDbContext _sampleDtrDbContext;
        private readonly CurrentDate _date = new CurrentDate();
        public DTRController(SampleDtrDbContext sampleDtrDbContext)
        {
            _sampleDtrDbContext = sampleDtrDbContext;
        }

        //Method for deducting the breaktime from the time out so the real total logged hours is reflected
        private static TimeSpan? deductBreakTime(TimeSpan? time)
        {
            var startBreakTime = TimeSpan.FromHours(12);

            if (time > startBreakTime)
            {
                TimeSpan? resultTime = time - startBreakTime;
                return resultTime;
            }
            return new TimeSpan(0, 0, 0);
        }

        //Method for deducting the end of work hours from the time out so the real total logged hours is reflected
        private static TimeSpan? deductBeforeWorkTime(TimeSpan? time)
        {
            var beforeWorkTime = TimeSpan.FromHours(08);

            if (time < beforeWorkTime)
            {
                TimeSpan? resultTime = time - beforeWorkTime;
                return resultTime;
            }
            return new TimeSpan(0, 0, 0);
        }

        //Method for deducting the end of work hours from the time out so the real total logged hours is reflected
        private static TimeSpan? deductEndWorkTime(TimeSpan? time)
        {
            var endWorkTime = TimeSpan.FromHours(17);

            if (time > endWorkTime)
            {
                TimeSpan? resultTime = time - endWorkTime;
                return resultTime;
            }
            return new TimeSpan(0, 0, 0);
        }

        //Method for saving the user's time in in the morning
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

        //Method for saving the user's time out in the morning
        private async Task<String> TimeOutAM(int userID)
        {
            //Gets the DTR of the user where it matches with the current date, user ID, and that the time out for the morning is currently null
            //If the result is null, it means that the user has already logged in the morning
            var userDTR = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == _date.currentDate && x.TimeOutAm == null).FirstOrDefaultAsync();
            if (userDTR != null)
            {
                _sampleDtrDbContext.TimeIns.Entry(userDTR).State = EntityState.Modified;
                userDTR.TimeOutAm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                userDTR.TotalLoggedHours += Math.Round((Convert.ToDouble(userDTR?.TimeOutAm.GetValueOrDefault().TotalMinutes - userDTR?.TimeInAm.GetValueOrDefault().TotalMinutes) - Convert.ToDouble(deductBreakTime(userDTR?.TimeOutAm).GetValueOrDefault().TotalMinutes) - Convert.ToDouble(deductBeforeWorkTime(userDTR?.TimeInAm).GetValueOrDefault().TotalMinutes)).Minutes().TotalMinutes / (double)60, 2);
                await _sampleDtrDbContext.SaveChangesAsync();

                return "Success";
            }
            else
            {
                return "Error";
            }
        }

        //Method for saving the user's time in in the afternoon
        //The method has two conditions: the user has logged in the morning, and if the user hasn't
        //If the user has indeed logged in the morning, the method will instead save the user's time in for the afternoon by user the update method, and if the user hasn't timed out in the morning yet, it will then use the
        //current time to save their time out in the morning and then the time in for the afternoon will be done with an extra minute.
        //If the user hasn't logged in the morning yet, the method will instead create a new record and saves the time in for the afternoon
        private async Task TimeInPM(int userID, Task<Boolean> isTimedInAm, Task<Boolean> isTimedOutAm)
        {
            var userDTR = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == _date.currentDate).FirstOrDefaultAsync();

            if (isTimedInAm.Result)
            {
                if (userDTR != null)
                {
                    if (isTimedOutAm.Result == true)
                    {
                        await TimeOutAM(userID);
                    }

                    userDTR.TimeInPm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                    _sampleDtrDbContext.TimeIns.Entry(userDTR).State = EntityState.Modified;
                    await _sampleDtrDbContext.SaveChangesAsync();
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

        //Method for saving the time out the user for the afternoon. If the user has already logged out, the system will then return an error message for trying to log out again
        private async Task<String> TimeOutPM(int userID)
        {
            var userDTR = await _sampleDtrDbContext.TimeIns.Where(x => x.UserId == userID && x.TimeInDate == _date.currentDate && x.TimeOutPm == null).FirstOrDefaultAsync();

            if (userDTR != null)
            {
                _sampleDtrDbContext.TimeIns.Entry(userDTR).State = EntityState.Modified;

                userDTR.UserId = userID;
                userDTR.TimeInDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                userDTR.TimeOutPm = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                userDTR.TotalLoggedHours = Convert.ToDouble(String.Format("{0:0.00}", (Convert.ToDouble(userDTR?.TimeOutAm.GetValueOrDefault().TotalMinutes - userDTR?.TimeInAm.GetValueOrDefault().TotalMinutes) + Convert.ToDouble(deductBreakTime(userDTR?.TimeOutAm).GetValueOrDefault().TotalMinutes) - Convert.ToDouble(deductBeforeWorkTime(userDTR?.TimeInAm).GetValueOrDefault().TotalMinutes) + (Convert.ToDouble(userDTR?.TimeOutPm.GetValueOrDefault().TotalMinutes - userDTR?.TimeInPm.GetValueOrDefault().TotalMinutes) - Convert.ToDouble(deductEndWorkTime(userDTR?.TimeOutPm).GetValueOrDefault().TotalMinutes))) / 60));

                await _sampleDtrDbContext.SaveChangesAsync();

                return "Success";
            }
            return "Error";
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
        public async Task<IActionResult> TimeIn(TestTimeInDTO timeInDTO)
        {
            try
            {

                Console.WriteLine(_date.currentDate);
                var userID = timeInDTO.UserId;

                //Checks if the user ID exists in the UserCredentials DB
                if (await CheckUserIdFunction.CheckID(userID, _sampleDtrDbContext))
                {
                    //Checks whether the user has already timed for today
                    if (await CheckIfAlreadyTimeInDate.CheckDate(userID, _sampleDtrDbContext) == false)
                    {
                        //Checks whether the time is AM or PM
                        //If the time is more than or equals to 1:00 PM, it will return PM, else it will return AM
                        if (CheckAMPM.check() == "AM")
                        {
                            //Times in the user for the morning
                            await TimeInAM(userID);
                            return Ok("Successfully timed in!");
                        }
                        else
                        {
                            //Times in the user for the afternoon
                            //Takes a parameter of the user ID, and two boolean valuse of the user if they've timed in for the morning yet, and if they've timed out for the morning.
                            await TimeInPM(userID, CheckUserTimeInAm.CheckTimeInAm(userID, _sampleDtrDbContext), CheckUserTimeoutAm.CheckTimeoutAm(userID, _sampleDtrDbContext));
                            return Ok("Successfully timed in!");
                        }
                    }
                    else
                    {
                        //Checks if the user has timed in for the afternoon. If true, save the current time for the pm time in, if false, return an error.
                        if (CheckUserTimeInPm.CheckTimeinPm(userID, _sampleDtrDbContext).Result == false)
                        {
                            //Times in the user for the afternoon
                            //Takes a parameter of the user ID, and two boolean valuse of the user if they've timed in for the morning yet, and if they've timed out for the morning
                            await TimeInPM(userID, CheckUserTimeInAm.CheckTimeInAm(userID, _sampleDtrDbContext), CheckUserTimeoutAm.CheckTimeoutAm(userID, _sampleDtrDbContext));
                            return Ok("Successfully timed in!");
                        }
                        else
                        {
                            return BadRequest($"You have already timed in this {(CheckAMPM.check() == "AM" ? "morning" : "afternoon")}");
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

        [HttpPut]
        [Route("TimeOutUser")]
        public async Task<IActionResult> TimeOut(TestTimeInDTO timeInDTO)
        {
            try
            {
                var userID = timeInDTO.UserId;

                //Checks if the user ID exists in the UserCredentials DB
                if (await CheckUserIdFunction.CheckID(userID, _sampleDtrDbContext))
                {
                    if (await CheckIfAlreadyTimeInDate.CheckDate(userID, _sampleDtrDbContext))
                    {
                        //Checks whether the time is AM or PM
                        //If the time is more than or equals to 1:00 PM, it will return PM, else it will return AM
                        if (CheckAMPM.check() == "AM")
                        {
                            //Times out the user for the AM
                            //If the has not timed out for the morning, it will succeed. Else, it will return an error message
                            if (await TimeOutAM(userID) == "Success")
                            {
                                return Ok("Successfully timed out!");
                            }
                            return BadRequest("You have already timed out for this morning");
                        }

                        else
                        {
                            ////Checks whether the user has timed in for the afternoon
                            //if (isUserTimedInPM)
                            //{
                            //    //Times in the user for the afternoon
                            //    //Takes a parameter of the user ID, and two boolean valuse of the user if they've timed in for the morning yet, and if they've timed out for the morning
                            //    await TimeInPM(userID, CheckUserTimeInAm.CheckTimeInAm(userID, _sampleDtrDbContext), CheckUserTimeoutAm.CheckTimeoutAm(userID, _sampleDtrDbContext));

                            //    //Checks whether the user has timed out for the morning
                            //    if (await CheckUserTimeoutAm.CheckTimeoutAm(userID, _sampleDtrDbContext))
                            //    {
                            //        return Ok("Successfully timed in!");
                            //    }
                            //    else
                            //    {
                            //        return Ok("Successfully timed in!");
                            //    }
                            //}
                            //else
                            //{
                            //Times out the user for the afternoon

                            //}

                            if (CheckUserTimeInPm.CheckTimeinPm(userID, _sampleDtrDbContext).Result == true)
                            {
                                //If the user has not timed out, it will return return an error message
                                if (await TimeOutPM(userID) == "Success")
                                {
                                    return Ok("Successfully timed out!");
                                }
                                else
                                {
                                    return BadRequest("You have already timed out for this afternoon");
                                }
                            }
                            else
                            {
                                return BadRequest("You haven't timed out for this afternoon. You must time in first");
                            }
                        }
                    }
                    else
                    {
                        return BadRequest($"You have not timed in for this {(CheckAMPM.check() == "AM" ? "Morning" : "Afternoon")}. You need to time in first");
                    }
                }
                return NotFound("Incorrect user id!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
