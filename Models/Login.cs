using System;
using System.Collections.Generic;

namespace Sample_DTR_API.Models;

public partial class Login
{
    public int UserId { get; set; }

    public DateTime? TimeInAm { get; set; }

    public DateTime? TimeOutAm { get; set; }

    public DateTime? TimeInPm { get; set; }

    public DateTime? TimeOutPm { get; set; }

    public double? TotalLoggedHours { get; set; }

    public virtual UserCredential User { get; set; } = null!;
}
