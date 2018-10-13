using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace OrbintSoft.Yauaa.Analyzer.Test.Parse.UserAgentNS.Parse
{
    public class TestUserAgentFlattening: IClassFixture<LoggingFixture>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestUserAgentFlattening));

        [Fact]
        public void TestFlatteningProduct()
        {
            ValidateUserAgent(
                "Mozilla/5.0"
                , "agent=\"Mozilla/5.0\""
                , "agent.(1)product=\"Mozilla/5.0\""
                , "agent.(1)product.(1)name=\"Mozilla\""
                , "agent.(1)product.(1)version=\"5.0\""
            );

            // Special case with multiple spaces between words
            ValidateUserAgent(
                "one two  three   four/1"
                , "agent=\"one two  three   four/1\""
                , "agent.(1)product=\"one two  three   four/1\""
                , "agent.(1)product.(1)name=\"one two  three   four\""
                , "agent.(1)product.(1)version=\"1\""
            );

            // Edge case about where to break product and version info
            ValidateUserAgent(
                "one/two 3 four five/6 (one/two 3 four five/6)"
                , "agent=\"one/two 3 four five/6 (one/two 3 four five/6)\""
                , "agent.(1)product=\"one/two 3\""
                , "agent.(1)product.(1)name=\"one\""
                , "agent.(1)product.(1)version=\"two\""
                , "agent.(1)product.(2)version=\"3\""

                , "agent.(2)product=\"four five/6 (one/two 3 four five/6)\""
                , "agent.(2)product.(1)name=\"four five\""
                , "agent.(2)product.(1)version=\"6\""

                , "agent.(2)product.(1)comments.(1)entry=\"one/two 3 four five/6\""

                , "agent.(2)product.(1)comments.(1)entry.(1)product=\"one/two 3\""
                , "agent.(2)product.(1)comments.(1)entry.(1)product.(1)name=\"one\""
                , "agent.(2)product.(1)comments.(1)entry.(1)product.(1)version=\"two\""
                , "agent.(2)product.(1)comments.(1)entry.(1)product.(2)version=\"3\""

                , "agent.(2)product.(1)comments.(1)entry.(2)product=\"four five/6\""
                , "agent.(2)product.(1)comments.(1)entry.(2)product.(1)name=\"four five\""
                , "agent.(2)product.(1)comments.(1)entry.(2)product.(1)version=\"6\""
            );

            ValidateUserAgent(
                "Foo 1/A (Bar 2/B)"
                , "agent=\"Foo 1/A (Bar 2/B)\""
                , "agent.(1)product=\"Foo 1/A (Bar 2/B)\""
                , "agent.(1)product.(1)name=\"Foo\""
                , "agent.(1)product.(1)version=\"1\""
                , "agent.(1)product.(2)version=\"A\""
                , "agent.(1)product.(1)comments=\"(Bar 2/B)\""
                , "agent.(1)product.(1)comments.(1)entry=\"Bar 2/B\""
                , "agent.(1)product.(1)comments.(1)entry.(1)product=\"Bar 2/B\""
                , "agent.(1)product.(1)comments.(1)entry.(1)product.(1)name=\"Bar\""
                , "agent.(1)product.(1)comments.(1)entry.(1)product.(1)version=\"2\""
                , "agent.(1)product.(1)comments.(1)entry.(1)product.(2)version=\"B\""
            );

            ValidateUserAgent(
                "Mozilla/5.0 (foo)"
                , "agent=\"Mozilla/5.0 (foo)\""
                , "agent.(1)product=\"Mozilla/5.0 (foo)\""
                , "agent.(1)product.(1)name=\"Mozilla\""
                , "agent.(1)product.(1)version=\"5.0\""
                , "agent.(1)product.(1)comments=\"(foo)\""
                , "agent.(1)product.(1)comments.(1)entry=\"foo\""
                , "agent.(1)product.(1)comments.(1)entry.(1)text=\"foo\""
            );

            ValidateUserAgent(
                "Mozilla (foo)"
                , "agent=\"Mozilla (foo)\""
                , "agent.(1)product=\"Mozilla (foo)\""
                , "agent.(1)product.(1)name=\"Mozilla\""
                , "agent.(1)product.(1)comments=\"(foo)\""
                , "agent.(1)product.(1)comments.(1)entry=\"foo\""
                , "agent.(1)product.(1)comments.(1)entry.(1)text=\"foo\""
            );

            ValidateUserAgent(
                "The name 1 2 (foo bar baz) (one two three)"
                , "agent=\"The name 1 2 (foo bar baz) (one two three)\""
                , "agent.(1)product=\"The name 1 2 (foo bar baz) (one two three)\""
                , "agent.(1)product.(1)name=\"The name\""
                , "agent.(1)product.(1)version=\"1\""
                , "agent.(1)product.(2)version=\"2\""
                , "agent.(1)product.(1)comments=\"(foo bar baz)\""
                , "agent.(1)product.(1)comments.(1)entry.(1)text=\"foo bar baz\""
                , "agent.(1)product.(2)comments=\"(one two three)\""
                , "agent.(1)product.(2)comments.(1)entry.(1)text=\"one two three\""
            );

            ValidateUserAgent(
                "One 2 Three four 5 /6"
                , "agent=\"One 2 Three four 5 /6\""
                , "agent.(1)product=\"One 2\""
                , "agent.(1)product.(1)name=\"One\""
                , "agent.(1)product.(1)version=\"2\""
                , "agent.(2)product=\"Three four 5 /6\""
                , "agent.(2)product.(1)name=\"Three four\""
                , "agent.(2)product.(1)version=\"5\""
                , "agent.(2)product.(2)version=\"6\""
            );

            ValidateUserAgent(
                "One two 1 2 3/4/5.6"
                , "agent=\"One two 1 2 3/4/5.6\""
                , "agent.(1)product=\"One two 1 2 3/4/5.6\""
                , "agent.(1)product.(1)name=\"One two\""
                , "agent.(1)product.(1)version=\"1\""
                , "agent.(1)product.(2)version=\"2\""
                , "agent.(1)product.(3)version=\"3\""
                , "agent.(1)product.(4)version=\"4\""
                , "agent.(1)product.(5)version=\"5.6\""
            );

            // Product variations
            ValidateUserAgent("One=1 1 rv:2 (One; Numb3r)/3 (foo) Two Two 4 rv:5 (Two)/6 (bar)/7 (baz)/8"
                , "agent=\"One=1 1 rv:2 (One; Numb3r)/3 (foo) Two Two 4 rv:5 (Two)/6 (bar)/7 (baz)/8\""
                , "agent.(1)product=\"One=1 1 rv:2 (One; Numb3r)/3 (foo)\""
                , "agent.(1)product.(1)name=\"One=1\""
                , "agent.(1)product.(1)name.(1)keyvalue.(1)key=\"One\""
                , "agent.(1)product.(1)name.(1)keyvalue.(1)version=\"1\""
                , "agent.(1)product.(1)version=\"1\""
                , "agent.(1)product.(2)version=\"rv:2\""
                , "agent.(1)product.(2)version.(1)keyvalue=\"rv:2\""
                , "agent.(1)product.(2)version.(1)keyvalue.(1)key=\"rv\""
                , "agent.(1)product.(2)version.(1)keyvalue.(1)version=\"2\""
                , "agent.(1)product.(1)comments=\"(One; Numb3r)\""
                , "agent.(1)product.(1)comments.(1)entry=\"One\""
                , "agent.(1)product.(1)comments.(1)entry.(1)text=\"One\""
                , "agent.(1)product.(1)comments.(2)entry=\"Numb3r\""
                , "agent.(1)product.(1)comments.(2)entry.(1)text=\"Numb3r\""
                , "agent.(1)product.(3)version=\"3\""
                , "agent.(1)product.(2)comments=\"(foo)\""
                , "agent.(1)product.(2)comments.(1)entry=\"foo\""
                , "agent.(1)product.(2)comments.(1)entry.(1)text=\"foo\""
                , "agent.(2)product=\"Two Two 4 rv:5 (Two)/6 (bar)/7 (baz)/8\""
                , "agent.(2)product.(1)name=\"Two Two\""
                , "agent.(2)product.(1)version=\"4\""
                , "agent.(2)product.(2)version=\"rv:5\""
                , "agent.(2)product.(2)version.(1)keyvalue=\"rv:5\""
                , "agent.(2)product.(2)version.(1)keyvalue.(1)key=\"rv\""
                , "agent.(2)product.(2)version.(1)keyvalue.(1)version=\"5\""
                , "agent.(2)product.(1)comments=\"(Two)\""
                , "agent.(2)product.(1)comments.(1)entry=\"Two\""
                , "agent.(2)product.(1)comments.(1)entry.(1)text=\"Two\""
                , "agent.(2)product.(3)version=\"6\""
                , "agent.(2)product.(2)comments=\"(bar)\""
                , "agent.(2)product.(2)comments.(1)entry=\"bar\""
                , "agent.(2)product.(2)comments.(1)entry.(1)text=\"bar\""
                , "agent.(2)product.(4)version=\"7\""
                , "agent.(2)product.(3)comments=\"(baz)\""
                , "agent.(2)product.(3)comments.(1)entry=\"baz\""
                , "agent.(2)product.(3)comments.(1)entry.(1)text=\"baz\""
                , "agent.(2)product.(5)version=\"8\""
            );

            // Special product names
            ValidateUserAgent("TextName 1 (a) Versi0nName 2 (b) em@il.name 3 (c) key=value 4 (d)"
                , "agent=\"TextName 1 (a) Versi0nName 2 (b) em@il.name 3 (c) key=value 4 (d)\""
            );

        }


        private void ValidateUserAgent(string useragent, params string[] requiredValues)
        {
            bool developmentMode = requiredValues.Length == 0;

            if (developmentMode)
            {
                LOG.Info(string.Format("Developing {0}", useragent));
            }
            else
            {
                LOG.Info(string.Format("Validating {0}", useragent));
            }

            StringBuilder sb = new StringBuilder(2048);
            sb.Append('\n');
            sb.Append("|====================================== \n");
            sb.Append("| ").Append(useragent).Append('\n');
            sb.Append("|-------------------------------------- \n");

            UserAgentAnalyzerDirect.GetAllPathsAnalyzerClass analyzer = UserAgentAnalyzerDirect.GetAllPathsAnalyzer(useragent);
            UserAgent parsedUseragent = analyzer.GetResult();

            if (parsedUseragent.HasAmbiguity)
            {
                sb.Append("| Ambiguity \n");
            }
            if (parsedUseragent.HasSyntaxError)
            {
                sb.Append("| Syntax Error \n");
            }

            List<string> paths = analyzer.GetValues();

            bool ok = true;
            foreach (string value in requiredValues)
            {
                if (paths.Contains(value))
                {
                    sb.Append("|             : ").Append(value).Append('\n');
                }
                else
                {
                    sb.Append("| Missing --> : ").Append(value).Append('\n');
                    ok = false;
                }
            }

            if (requiredValues.Length == 0 || !ok)
            {
                sb.Append("|-------------------------------------- \n");
                foreach (string value in paths)
                {
                    if (value.Contains("="))
                    {
                        sb.Append("      ,\"").Append(Regex.Replace(value, "\\\"", "\\\\\"")).Append("\"\n");
                    }
                }
                sb.Append("|====================================== \n");
            }

            if (developmentMode)
            {
                LOG.Info(sb.ToString());
                return;
            }
            if (!ok)
            {
                LOG.Error(sb.ToString());
                throw new Exception("Not everything was found");
            }
        }
    }
}
