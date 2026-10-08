using System;
using System.Collections.Generic;
using System.Text;
using Klario.Common.Enums;

namespace Klario.DAL.Models
{
    public class SearchProfile: BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public List<string> TargetJobTitles { get; set; }
        public List<string> TargetLocations { get; set; }

        public TimeSpan MaxPostingAge { get; set; }
        public WorkplacePreference Workplace { get; set; }
        public ExperienceLevel Experience { get; set; }
        public JobType JobType { get; set; }

        public string TelegramChatId { get; set; }
        public string BotToken { get; set; }

        public SearchProfile()
        {
            TargetJobTitles = new();
            TargetLocations = new();
        }

    }
}
