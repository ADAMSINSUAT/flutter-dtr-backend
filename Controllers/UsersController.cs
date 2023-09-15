using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using Sample_DTR_API.DTO;
using Sample_DTR_API.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Sample_DTR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SampleDtrDbContext _sampleDtrDbContext;

        public UsersController(SampleDtrDbContext sampleDtrDbContext)
        {
            _sampleDtrDbContext = sampleDtrDbContext;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult<List<GetUserDTO>>> GetAllUsers()
        {
            try
            {
                var users = await (from employees in _sampleDtrDbContext.Employees
                                   join departments in _sampleDtrDbContext.Departments on employees.DepartmentId equals departments.DepartmentId
                                   join roles in _sampleDtrDbContext.Roles on employees.RoleId equals roles.RoleId
                                   join statuses in _sampleDtrDbContext.Statuses on employees.StatusId equals statuses.StatusId
                                   join usercredentials in _sampleDtrDbContext.UserCredentials on employees.UserId equals usercredentials.UserId
                                   select new GetUserDTO
                                   {
                                       EmpId = employees.EmpId,
                                       FirstName = employees.FirstName,
                                       Mi = employees.Mi + ".",
                                       LastName = employees.LastName,
                                       DateOfBirth = employees.DateOfBirth,
                                       Email = employees.Email,
                                       Department = departments.DepartmentName,
                                       Role = roles.Role1,
                                       Status = statuses.Status1,
                                       Username = usercredentials.Username,
                                   }).ToListAsync();

                if (users == null)
                {
                    return NotFound();
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUserByUsernameOrName/{searchValue}")]
        public async Task<ActionResult<GetUserDTO>> GetUserByUserName(String searchValue)
        {
            try
            {
                var searchParts = searchValue.Split(' ');

                for (int i = 0; i < searchParts.Length; i++)
                {

                    var result = await (from employees in _sampleDtrDbContext.Employees
                                        join departments in _sampleDtrDbContext.Departments on employees.DepartmentId equals departments.DepartmentId
                                        join roles in _sampleDtrDbContext.Roles on employees.RoleId equals roles.RoleId
                                        join statuses in _sampleDtrDbContext.Statuses on employees.StatusId equals statuses.StatusId
                                        join usercredentials in _sampleDtrDbContext.UserCredentials on employees.UserId equals usercredentials.UserId
                                        select new GetUserDTO
                                        {
                                            EmpId = employees.EmpId,
                                            FirstName = employees.FirstName,
                                            Mi = employees.Mi + ".",
                                            LastName = employees.LastName,
                                            DateOfBirth = employees.DateOfBirth,
                                            Email = employees.Email,
                                            Department = departments.DepartmentName,
                                            Role = roles.Role1,
                                            Status = statuses.Status1,
                                            Username = usercredentials.Username,
                                        }).Where(x => x.Username == searchValue || x.Username == searchValue.Trim() || (x.FirstName + " " + x.LastName).Contains(searchParts[i])).ToListAsync();



                    if (result != null)
                    {
                        return Ok(result);
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> Register(RegisterUserDTO registerDTO)
        {
            try
            {
                //Mapping entity properties

                //Get the role from the Departments model
                registerDTO.DepartmentName = _sampleDtrDbContext.Departments.FirstOrDefaultAsync(x => x.DepartmentName == registerDTO.DepartmentName).Result?.DepartmentName;
                var DepartmentId = _sampleDtrDbContext.Departments.Where(x => x.DepartmentName == registerDTO.DepartmentName).First().DepartmentId;

                //Get the role from the Role model
                registerDTO.Role1 = _sampleDtrDbContext.Roles.FirstOrDefaultAsync(x => x.Role1 == registerDTO.Role1).Result?.Role1;
                var RoleId = _sampleDtrDbContext.Roles.Where(x => x.Role1 == registerDTO.Role1).First().RoleId;

                //Get the status from the Status model
                registerDTO.Status1 = _sampleDtrDbContext.Statuses.FirstOrDefaultAsync(x => x.Status1 == registerDTO.Status1).Result?.Status1;
                var StatusId = _sampleDtrDbContext.Statuses.Where(x => x.Status1 == (registerDTO.Status1 != null ? registerDTO.Status1 : "Active")).First().StatusId;

                //Mapping data to UserCredential model
                var userCredential = new UserCredential();
                userCredential.Username = registerDTO.Username;
                userCredential.Password = registerDTO.Password;

                //Check if username and password is already taken
                var checkUserCredentials = await _sampleDtrDbContext.UserCredentials.Where(x => x.Username == registerDTO.Username).ToListAsync();

                //Check if the employee's exact name is already taken
                var checkEmployeeName = await _sampleDtrDbContext.Employees.Where(x => x.FirstName == registerDTO.FirstName && x.Mi == registerDTO.Mi && x.LastName == registerDTO.LastName).ToListAsync();

                if (registerDTO.Mi.Length > 1)
                {
                    return BadRequest("Middle Initial must be only one letter");
                }

                if (checkUserCredentials.Any())
                {
                    return BadRequest("Username is already taken");
                }
                else
                {
                    //Adding user credentials to UserCredential DataTable
                    await _sampleDtrDbContext.UserCredentials.AddAsync(userCredential);
                    var userCredentialResult = await _sampleDtrDbContext.SaveChangesAsync();

                    if (registerDTO.DepartmentName == null) //Check if the department selected exists
                    {
                        return NotFound("Deparment not found");
                    }
                    else if (registerDTO.Role1 == null) //Check if the role selected exists
                    {
                        return NotFound("Role not found");
                    }
                    else if (registerDTO.Status1 == null) //Check if the role selected exists
                    {
                        return NotFound("Status not found");
                    }
                    else if (checkEmployeeName.Any())
                    {
                        return BadRequest("Employee name is already registered!");
                    }
                    else if (userCredentialResult == 0)
                    {
                        return BadRequest("Registration failed");
                    }
                    else
                    {
                        //Get user ID from the UserCredential model
                        var UserId = _sampleDtrDbContext.UserCredentials.FirstOrDefaultAsync(x => x.Username == registerDTO.Username && x.Password == registerDTO.Password).Result?.UserId;


                        //Mapping data to Employee model
                        var employee = new Employee()
                        {
                            FirstName = registerDTO.FirstName,
                            LastName = registerDTO.LastName,
                            Mi = registerDTO.Mi,
                            DateOfBirth = Convert.ToDateTime(registerDTO.DateOfBirth.ToShortDateString()),
                            Email = registerDTO.Email,
                            DepartmentId = Convert.ToInt32(DepartmentId),
                            RoleId = Convert.ToInt32(RoleId),
                            StatusId = Convert.ToInt32(StatusId),
                            UserId = Convert.ToInt32(UserId),
                        };

                        Console.WriteLine(employee);

                        //Adding the new user to the model then saving the changes
                        await _sampleDtrDbContext.Employees.AddAsync(employee);
                        //_sampleDtrDbContext.Employees.Entry(employee).State = EntityState.Added;
                        var result = await _sampleDtrDbContext.SaveChangesAsync();
                        //var result = 1;

                        if (result != 0)
                        {
                            //Show a message back to the user
                            return Ok("User successfully registered");
                        }
                        else
                        {
                            await _sampleDtrDbContext.UserCredentials.Where(x => x.UserId == userCredential.UserId).ExecuteDeleteAsync();
                        }
                    }

                    return BadRequest("User registration unsuccessful");
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("LoginUser")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                var users = await _sampleDtrDbContext.UserCredentials.Select(u => new LoginDTO
                {
                    Username = u.Username,
                    Password = u.Password
                }).Where(x => x.Username == loginDTO.Username && x.Password == loginDTO.Password).ToListAsync();

                Console.WriteLine(users);

                if (users == null || users.Count == 0)
                {
                    return NotFound("Invalid username or password. Please try again");
                }
                return Ok("Logged in successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
