using Sample_DTR_API.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sample_DTR_API.DTO
{
    public partial class RegisterDTO
    {
        //Employee entity
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Mi { get; set; } = null!;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateOnly DateOfBirth { get; set; }

        public string Email { get; set; } = null!;


        //Department entity
        public string? DepartmentName { get; set; }


        //Role entity
        public string? Role1 { get; set; }


        //Status entity
        [DefaultValue("Active")]
        public string? Status1 { get; set; }


        //UserCredential entity
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; } = null!;

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; } = null!;
    }
}
