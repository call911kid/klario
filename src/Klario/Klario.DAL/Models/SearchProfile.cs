using System;
using System.Collections.Generic;
using Klario.Common.Enums;

namespace Klario.DAL.Models
{
    public class SearchProfile : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public List<string> TargetJobTitles { get; set; } = new();
        public List<string> TargetLocations { get; set; } = new();

        public int MaxPostingAgeMinutes { get; set; } = 1440;
        public List<Schedule> Schedules { get; set; } = new();
        public WorkplacePreference Workplace { get; set; }
        public ExperienceLevel Experience { get; set; }
        public JobType JobType { get; set; }

        public string TelegramChatId { get; set; } = string.Empty;
        public string BotToken { get; set; } = string.Empty;
    }
}
