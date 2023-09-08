using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample_DTR_API.DTO;
using Sample_DTR_API.Models;

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
                var users = await _sampleDtrDbContext.UserCredentials.Select(u => new GetUserDTO
                {
                    Username = u.Username,
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
        [Route("GetUserByName/{username}")]
        public async Task<ActionResult<GetUserDTO>> GetUserByUserName(string username)
        {
            try
            {
                var user = await _sampleDtrDbContext.UserCredentials.Select(u => new GetUserDTO
                {
                    Username = u.Username
                }).Where(x=>x.Username == username).ToListAsync();

                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                //Mapping entity properties

                //Get the role from the Departments model
                registerDTO.DepartmentName = _sampleDtrDbContext.Departments.FirstOrDefaultAsync(x => x.DepartmentName == registerDTO.DepartmentName).Result?.DepartmentName;

                //Get the role from the Role model
                registerDTO.Role1 = _sampleDtrDbContext.Roles.FirstOrDefaultAsync(x => x.Role1 == registerDTO.Role1).Result?.Role1;

                //Mapping data to UserCredential model
                var userCredential = new UserCredential();
                userCredential.Username = registerDTO.Username;
                userCredential.Password = registerDTO.Password;

                //Check if username and password is already taken
                var checkUserCredentials = await _sampleDtrDbContext.UserCredentials.Where(x => x.Username == registerDTO.Username).ToListAsync();

                //Check if the employee's exact name is already taken
                var checkEmployeeName = await _sampleDtrDbContext.Employees.Where(x => x.FirstName == registerDTO.FirstName && x.Mi == registerDTO.Mi && x.LastName == registerDTO.LastName).ToListAsync();

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
                    else if (checkEmployeeName.Any())
                    {
                        return BadRequest("Employee name is already registered!");
                    }
                    else if (userCredentialResult == 0)
                    {
                        return BadRequest("Registration failed");
                    }

                    //Mapping data to Employee model
                    var employee = new Employee()
                    {
                        FirstName = registerDTO.FirstName,
                        LastName = registerDTO.LastName,
                        Mi = registerDTO.Mi,
                        DateOfBirth = Convert.ToDateTime(registerDTO.DateOfBirth.ToShortDateString()),
                        Email = registerDTO.Email,
                        DepartmentId = Convert.ToInt32(_sampleDtrDbContext.Departments.FirstOrDefault(x => x.DepartmentName == registerDTO.DepartmentName)?.DepartmentId),
                        RoleId = Convert.ToInt32(_sampleDtrDbContext.Roles.FirstOrDefault(x => x.Role1 == registerDTO.Role1)?.RoleId),
                        StatusId = Convert.ToInt32(_sampleDtrDbContext.Statuses.FirstOrDefault(x => x.Status1 == "Active")?.StatusId),
                        UserId = Convert.ToInt32(_sampleDtrDbContext.UserCredentials.FirstOrDefaultAsync(x => x.Username == registerDTO.Username && x.Password == registerDTO.Password).Result?.UserId),
                    };

                    Console.WriteLine(employee);

                    //Adding the new user to the model then saving the changes
                    await _sampleDtrDbContext.Employees.AddAsync(employee);
                    _sampleDtrDbContext.Employees.Entry(employee).State = EntityState.Added;
                    var result = await _sampleDtrDbContext.SaveChangesAsync();

                    if (result != 0)
                    {
                        //Show a message back to the user
                        return Ok("User successfully registered");
                    }
                    else
                    {
                        await _sampleDtrDbContext.UserCredentials.Where(x => x.UserId == userCredential.UserId).ExecuteDeleteAsync();
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
