using System;
using System.Collections.Generic;

namespace Sample_DTR_API.Models;

public partial class UserCredential
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<TimeIn> TimeIns { get; set; } = new List<TimeIn>();
}
