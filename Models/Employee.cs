using System;
using System.Collections.Generic;

namespace Sample_DTR_API.Models;

public partial class Employee
{
    public int EmpId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Mi { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int RoleId { get; set; }

    public int StatusId { get; set; }

    public int UserId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual UserCredential User { get; set; } = null!;
}
