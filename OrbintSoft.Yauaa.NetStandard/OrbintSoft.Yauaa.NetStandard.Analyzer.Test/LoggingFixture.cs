using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using log4net.Util;

namespace OrbintSoft.Yauaa.Analyzer.Test
{
    public class LoggingFixture
    {
        public LoggingFixture()
        {
            var asm = Assembly.GetExecutingAssembly();
            var logRepository = LogManager.GetRepository(asm);
            var logfile = new FileInfo("log4net.config");
            var c = XmlConfigurator.Configure(logRepository, logfile);
            if (!LogManager.GetRepository(asm).Configured)
            {
                // log4net not configured
                foreach (log4net.Util.LogLog message in LogManager.GetRepository(asm).ConfigurationMessages.Cast<LogLog>())
                {
                    // evaluate configuration message
                }
            }
        }
    }
}
