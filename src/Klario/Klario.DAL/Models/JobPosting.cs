using System;
using System.Collections.Generic;
using System.Text;
using Klario.Common.Enums;

namespace Klario.DAL.Models
{
    public class JobPosting : BaseEntity
    {
        public string ExternalJobId { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
        public DateTime? PostedAt { get; set; }
        public PostingSource Source { get; set; }
        public string? DescriptionSnippet { get; set; }
    }
}
