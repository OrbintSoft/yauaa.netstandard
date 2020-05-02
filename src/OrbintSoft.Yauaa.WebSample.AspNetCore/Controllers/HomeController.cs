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
        public IActionResult Index()
        {
            return this.View();
        }

        [HttpGet]
        public IActionResult Demo()
        {
            string userAgent = this.HttpContext?.Request?.Headers?.FirstOrDefault(s => s.Key.ToLower() == "user-agent").Value;
            return this.View(new DemoViewModel()
            {
                UserAgent = userAgent,
                Version = Yauaa.Utils.YauaaVersion.GetVersion()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Demo(DemoViewModel model)
        {
            var watch = Stopwatch.StartNew();
            var ua = YauaaSingleton.Analyzer.Parse(model.UserAgent);
            model.Version = Utils.YauaaVersion.GetVersion();
            var fieldNames = ua.GetAvailableFieldNames();
            model.Fields = new Dictionary<string, AgentField>();
            foreach (var name in fieldNames)
            {
                model.Fields[name] = ua.Get(name);
            }
            watch.Stop();
            var seconds = watch.ElapsedTicks / (double)Stopwatch.Frequency;
            if (seconds < 1)
            {
                var ms = seconds * 1000;
                if (ms < 1)
                {
                    var us = ms * 1000;
                    if (us < 1)
                    {
                        var ns = us * 1000;
                        model.ElapsedTime = $"{ns} ns";
                    }
                    else
                    {
                        model.ElapsedTime = $"{us} µs";
                    }
                }
                else
                {
                    model.ElapsedTime = $"{ms} ms";
                }
            }
            else
            {
                model.ElapsedTime = $"{seconds} s";
            }
            return this.View(model);
        }

        public IActionResult Guide()
        {
            return this.View();
        }
    }
}
