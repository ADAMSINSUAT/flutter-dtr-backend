using System.ComponentModel;

namespace Sample_DTR_API.DTO
{
    public class GetUserDTO
    {
        public int EmpId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Mi { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; } = null!;

        public string? Department { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }

        public string? Username { get; set; } = null!;
    }
}
