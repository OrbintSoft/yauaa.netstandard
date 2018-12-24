using Microsoft.AspNetCore.Mvc;
using OrbintSoft.Yauaa.Analyzer;
using OrbintSoft.Yauaa.WebSample.AspNetCore.Models;
using OrbintSoft.Yauaa.WebSample.AspNetCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            string userAgent = this.HttpContext?.Request?.Headers?.FirstOrDefault(s => s.Key.ToLower() == "user-agent").Value;
            return this.View(new IndexViewModel() { UserAgent = userAgent });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IndexViewModel model)
        {
            var watch = Stopwatch.StartNew();
            var ua =YauaaSingleton.Analyzer.Parse(model.UserAgent);
            var fieldNames = ua.GetAvailableFieldNames();
            model.Fields = new Dictionary<string, UserAgent.AgentField>();
            foreach (var name in fieldNames)
            {
                model.Fields[name] = ua.Get(name);
            }
            watch.Stop();
            model.ElapsedTime = watch.ElapsedMilliseconds.ToString();
            return this.View(model);
        }
    }
}
