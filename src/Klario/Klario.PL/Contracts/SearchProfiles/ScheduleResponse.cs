using Klario.Common.Enums;

namespace Klario.PL.Contracts.SearchProfiles;

public sealed record ScheduleResponse(
    PostingSource Source,
    int IntervalMinutes,
    bool IsEnabled
);
