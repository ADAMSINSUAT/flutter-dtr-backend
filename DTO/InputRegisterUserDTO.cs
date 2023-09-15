using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sample_DTR_API.DTO
{
    public class InputRegisterUserDTO
    {
        public int EmpId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Mi { get; set; } = null!;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; } = null!;

        public int DepartmentId { get; set; }

        public int RoleId { get; set; }

        public int StatusId { get; set; }

        public int UserId { get; set; }
    }
}
