using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace OrbintSoft.Yauaa.Analyzer.Test.Fixtures
{
    public class LogFixture
    {
        public LogFixture()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));            
        }
    }
}
