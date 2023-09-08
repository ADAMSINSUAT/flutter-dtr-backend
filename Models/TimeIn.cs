using System;
using System.Collections.Generic;

namespace Sample_DTR_API.Models;

public partial class TimeIn
{
    public int TimeInId { get; set; }

    public DateTime? TimeInDate { get; set; }

    public TimeSpan? TimeInAm { get; set; }

    public TimeSpan? TimeOutAm { get; set; }

    public TimeSpan? TimeInPm { get; set; }

    public TimeSpan? TimeOutPm { get; set; }

    public double? TotalLoggedHours { get; set; }

    public int UserId { get; set; }

    public virtual UserCredential User { get; set; } = null!;
}
