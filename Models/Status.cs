using System;
using System.Collections.Generic;

namespace Sample_DTR_API.Models;

public partial class Status
{
    public int StatusId { get; set; }

    public string? Status1 { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
