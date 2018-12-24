using OrbintSoft.Yauaa.Analyzer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Models
{
    public class IndexViewModel
    {
        [Required(ErrorMessage = "The User Agent is required")]
        public string UserAgent { get; set; }

        public Dictionary<string, UserAgent.AgentField> Fields;

        public string ElapsedTime { get; set; }
    }
}
