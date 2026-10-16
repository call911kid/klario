using System;
using Klario.Common.Enums;

namespace Klario.DAL.Models;

public class Schedule
{
    public Guid SearchProfileId { get; set; }
    public SearchProfile SearchProfile { get; set; } = null!;
    public PostingSource Source { get; init; }
    public int IntervalMinutes { get; set; }
    public bool IsEnabled { get; set; } = true;
}
