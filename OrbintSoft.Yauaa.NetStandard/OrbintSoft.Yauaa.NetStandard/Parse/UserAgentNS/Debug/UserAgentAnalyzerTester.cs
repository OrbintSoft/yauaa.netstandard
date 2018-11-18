//<copyright file="UserAgentAnalyzerTester.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 10, 2, 06:13</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug
{
    using log4net;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="UserAgentAnalyzerTester" />
    /// </summary>
    [Serializable]
    public class UserAgentAnalyzerTester : UserAgentAnalyzer
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserAgentAnalyzerTester));

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzerTester"/> class.
        /// </summary>
        public UserAgentAnalyzerTester() : base()
        {
            KeepTests();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzerTester"/> class.
        /// </summary>
        /// <param name="resourceString">The resourceString<see cref="string"/></param>
        /// <param name="pattern">The pattern<see cref="string"/></param>
        public UserAgentAnalyzerTester(string resourceString, string pattern = "*.yaml") : this()
        {
            LoadResources(resourceString, pattern);
        }

        /// <summary>
        /// The GetAllTestCases
        /// </summary>
        /// <returns>The <see cref="IList{Dictionary{string, Dictionary{string, string}}}"/></returns>
        public IList<Dictionary<string, Dictionary<string, string>>> GetAllTestCases()
        {
            return testCases;
        }

        /// <summary>
        /// Run all the test_cases available.
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool RunTests()
        {
            return RunTests(false, true);
        }

        /// <summary>
        /// The RunTests
        /// </summary>
        /// <param name="showAll">The showAll<see cref="bool"/></param>
        /// <param name="failOnUnexpected">The failOnUnexpected<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool RunTests(bool showAll, bool failOnUnexpected)
        {
            return RunTests(showAll, failOnUnexpected, null, false, false);
        }

        /// <summary>
        /// The RunTests
        /// </summary>
        /// <param name="showAll">The showAll<see cref="bool"/></param>
        /// <param name="failOnUnexpected">The failOnUnexpected<see cref="bool"/></param>
        /// <param name="onlyValidateFieldNames">The onlyValidateFieldNames<see cref="ICollection{string}"/></param>
        /// <param name="measureSpeed">The measureSpeed<see cref="bool"/></param>
        /// <param name="showPassedTests">The showPassedTests<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool RunTests(bool showAll, bool failOnUnexpected, ICollection<string> onlyValidateFieldNames, bool measureSpeed, bool showPassedTests)
        {
            bool allPass = true;
            InitializeMatchers();
            if (testCases == null)
            {
                return allPass;
            }
            DebugUserAgent agent = new DebugUserAgent();

            List<TestResult> results = new List<TestResult>();

            string filenameHeader = "Test number and source";
            int filenameHeaderLength = filenameHeader.Length;
            int maxFilenameLength = filenameHeaderLength;
            foreach (Dictionary<string, Dictionary<string, string>> test in testCases)
            {
                Dictionary<string, string> metaData = test["metaData"];
                string filename = metaData["filename"];
                maxFilenameLength = Math.Max(maxFilenameLength, filename.Length);
            }

            maxFilenameLength += 11;

            StringBuilder sb = new StringBuilder(1024);

            sb.Append("| ").Append(filenameHeader);
            for (int i = filenameHeaderLength; i < maxFilenameLength; i++)
            {
                sb.Append(' ');
            }

            sb.Append("|S|AA|MF|");
            if (measureSpeed)
            {
                sb.Append("  PPS| msPP|");
            }
            sb.Append("--> S=Syntax Error, AA=Number of ambiguities during parse, MF=Matches Found");
            if (measureSpeed)
            {
                sb.Append(", PPS=parses/sec, msPP=milliseconds per parse");
            }

            if (showPassedTests)
            {
                Log.Info("+===========================================================================================");
                Log.Info(sb.ToString());
                Log.Info("+-------------------------------------------------------------------------------------------");
            }

            int testcount = 0;
            foreach (Dictionary<string, Dictionary<string, string>> test in testCases)
            {
                testcount++;
                Dictionary<string, string> input = test.ContainsKey("input") ? test["input"] : null;
                Dictionary<string, string> expected = test.ContainsKey("expected") ? test["expected"] : null;

                List<string> options = null;
                if (test.ContainsKey("options"))
                {
                    options = new List<string>(test["options"].Keys);
                }
                Dictionary<string, string> metaData = test["metaData"];
                string filename = metaData["filename"];
                string linenumber = metaData["fileline"];

                bool init = false;

                if (options == null)
                {
#if VERBOSE
                    SetVerbose(true);            
                    agent.SetDebug(true); 
#else
                    SetVerbose(false);
                    agent.SetDebug(false);
#endif
                }
                else
                {
                    bool newVerbose = options.Contains("verbose");
                    SetVerbose(newVerbose);
                    agent.SetDebug(newVerbose);
                    init = options.Contains("init");
                }
                if (expected == null || expected.Count == 0)
                {
                    init = true;
                }

                string testName = input.ContainsKey("name") ? input["name"] : null;
                string userAgentString = input["user_agent_string"];

                if (testName == null)
                {
                    testName = userAgentString;
                }

                sb.Length = 0;

                sb.Append("|").Append(string.Format("{0}", testcount))
                  .Append(".(").Append(filename).Append(':').Append(linenumber).Append(')');
                for (int i = filename.Length + linenumber.Length + 7; i < maxFilenameLength; i++)
                {
                    sb.Append(' ');
                }

                agent.UserAgentString = userAgentString;

                long measuredSpeed = -1;
                if (measureSpeed)
                {
                    DisableCaching();
                    // Preheat
                    for (int i = 0; i < 100; i++)
                    {
                        Parse(agent);
                    }
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    for (int i = 0; i < 1000; i++)
                    {
                        Parse(agent);
                    }
                    stopwatch.Stop();
                    measuredSpeed = stopwatch.ElapsedMilliseconds;
                }
                else
                {
                    Parse(agent);
                }

                sb.Append('|');
                if (agent.HasSyntaxError)
                {
                    sb.Append('S');
                }
                else
                {
                    sb.Append(' ');
                }
                if (agent.HasAmbiguity)
                {
                    sb.Append(string.Format("|{0}", agent.AmbiguityCount));
                }
                else
                {
                    sb.Append("|  ");
                }

                sb.Append(string.Format("|{0}", agent.GetNumberOfAppliedMatches()));

                if (measureSpeed)
                {
                    sb.Append('|').Append(string.Format("{0}", measuredSpeed));
                    sb.Append('|').Append(string.Format("{0}", 1000.0 / measuredSpeed));
                }

                sb.Append("| ").Append(testName);

                // We create the log line but we keep it until we know it actually must be output to the screen
                string testLogLine = sb.ToString();

                sb.Length = 0;

                bool pass = true;
                results.Clear();

                if (init)
                {
                    Log.Info(testLogLine);
                    sb.Append(agent.ToYamlTestCase());
                    Log.Info(sb.ToString());
                    //                return allPass;
                }
                else
                {
                    if (expected == null)
                    {
                        Log.Info(testLogLine);
                        Log.Warn("| - No expectations ... ");
                        continue;
                    }
                }

                int maxNameLength = 6; // "Field".length()+1;
                int maxActualLength = 7; // "Actual".length()+1;
                int maxExpectedLength = 9; // "Expected".length()+1;

                if (expected != null)
                {
                    List<string> fieldNames = agent.GetAvailableFieldNamesSorted();

                    if (onlyValidateFieldNames != null && onlyValidateFieldNames.Count == 0)
                    {
                        onlyValidateFieldNames = null;
                    }
                    else if (onlyValidateFieldNames != null)
                    {
                        fieldNames.Clear();
                        fieldNames.AddRange(onlyValidateFieldNames);
                    }

                    foreach (string newFieldName in expected.Keys)
                    {
                        if (!fieldNames.Contains(newFieldName))
                        {
                            fieldNames.Add(newFieldName);
                        }
                    }

                    foreach (string fieldName in fieldNames)
                    {
                        // Only check the desired fieldnames
                        if (onlyValidateFieldNames != null &&
                            !onlyValidateFieldNames.Contains(fieldName))
                        {
                            continue;
                        }

                        TestResult result = new TestResult();
                        result.field = fieldName;
                        bool expectedSomething;

                        // Actual value
                        result.actual = agent.GetValue(result.field);
                        result.confidence = agent.GetConfidence(result.field);
                        if (result.actual == null)
                        {
                            result.actual = UserAgent.NULL_VALUE;
                        }

                        // Expected value
                        string expectedValue = expected.ContainsKey(fieldName) ? expected[fieldName] : null;
                        if (expectedValue == null)
                        {
                            expectedSomething = false;
                            if (result.confidence < 0)
                            {
                                continue; // A negative value really means 'absent'
                            }
                            result.expected = "<<absent>>";
                        }
                        else
                        {
                            expectedSomething = true;
                            result.expected = expectedValue;
                        }

                        result.pass = result.actual.Equals(result.expected);
                        if (!result.pass)
                        {
                            result.warn = true;
                            if (expectedSomething)
                            {
                                result.warn = false;
                                pass = false;
                                allPass = false;
                            }
                            else
                            {
                                if (failOnUnexpected)
                                {
                                    // We ignore this special field
                                    if (!UserAgent.SYNTAX_ERROR.Equals(result.field))
                                    {
                                        result.warn = false;
                                        pass = false;
                                        allPass = false;
                                    }
                                }
                            }
                        }

                        results.Add(result);

                        maxNameLength = Math.Max(maxNameLength, result.field.Length);
                        maxActualLength = Math.Max(maxActualLength, result.actual.Length);
                        maxExpectedLength = Math.Max(maxExpectedLength, result.expected.Length);
                    }

                    if (!agent.AnalyzeMatchersResult())
                    {
                        pass = false;
                        allPass = false;
                    }
                }

                if (!init && pass && !showAll)
                {
                    if (showPassedTests)
                    {
                        Log.Info(testLogLine);
                    }
                    continue;
                }

                if (!pass)
                {
                    Log.Info(testLogLine);
                    Log.Error("| TEST FAILED !");
                }

                if (agent.HasAmbiguity)
                {
                    Log.Info(string.Format("| Parsing problem: Ambiguity {0} times. ", agent.AmbiguityCount));
                }
                if (agent.HasSyntaxError)
                {
                    Log.Info("| Parsing problem: Syntax Error");
                }

                if (init || !pass)
                {
                    sb.Length = 0;
                    sb.Append('\n');
                    sb.Append('\n');
                    sb.Append("- matcher:\n");
                    sb.Append("#    options:\n");
                    sb.Append("#    - 'verbose'\n");
                    sb.Append("    require:\n");
                    foreach (string path in GetAllPaths(userAgentString))
                    {
                        if (path.Contains("=\""))
                        {
                            sb.Append("#    - '").Append(path).Append("'\n");
                        }
                    }
                    sb.Append("    extract:\n");
                    sb.Append("#    - 'DeviceClass                         :      1 :' \n");
                    sb.Append("#    - 'DeviceBrand                         :      1 :' \n");
                    sb.Append("#    - 'DeviceName                          :      1 :' \n");
                    sb.Append("#    - 'OperatingSystemClass                :      1 :' \n");
                    sb.Append("#    - 'OperatingSystemName                 :      1 :' \n");
                    sb.Append("#    - 'OperatingSystemVersion              :      1 :' \n");
                    sb.Append("#    - 'LayoutEngineClass                   :      1 :' \n");
                    sb.Append("#    - 'LayoutEngineName                    :      1 :' \n");
                    sb.Append("#    - 'LayoutEngineVersion                 :      1 :' \n");
                    sb.Append("#    - 'AgentClass                          :      1 :' \n");
                    sb.Append("#    - 'AgentName                           :      1 :' \n");
                    sb.Append("#    - 'AgentVersion                        :      1 :' \n");
                    sb.Append('\n');
                    sb.Append('\n');
                    Log.Info(sb.ToString());
                }

                sb.Length = 0;
                sb.Append("+--------+-");
                for (int i = 0; i < maxNameLength; i++)
                {
                    sb.Append('-');
                }
                sb.Append("-+-");
                for (int i = 0; i < maxActualLength; i++)
                {
                    sb.Append('-');
                }
                sb.Append("-+------------+-");
                for (int i = 0; i < maxExpectedLength; i++)
                {
                    sb.Append('-');
                }
                sb.Append("-+");

                string separator = sb.ToString();
                Log.Info(separator);

                sb.Length = 0;
                sb.Append("| Result | Field ");
                for (int i = 6; i < maxNameLength; i++)
                {
                    sb.Append(' ');
                }
                sb.Append(" | Actual ");
                for (int i = 7; i < maxActualLength; i++)
                {
                    sb.Append(' ');
                }
                sb.Append(" | Confidence | Expected ");
                for (int i = 9; i < maxExpectedLength; i++)
                {
                    sb.Append(' ');
                }
                sb.Append(" |");

                Log.Info(sb.ToString());

                Log.Info(separator);

                Dictionary<string, string> failComments = new Dictionary<string, string>();

                List<string> failedFieldNames = new List<string>();
                foreach (TestResult result in results)
                {
                    sb.Length = 0;
                    if (result.pass)
                    {
                        sb.Append("|        | ");
                    }
                    else
                    {
                        if (result.warn)
                        {
                            sb.Append("| ~warn~ | ");
                            failComments[result.field] = "~~ Unexpected result ~~";
                        }
                        else
                        {
                            sb.Append("| -FAIL- | ");
                            failComments[result.field] = "FAILED; Should be '" + result.expected + "'";
                            failedFieldNames.Add(result.field);
                        }
                    }
                    sb.Append(result.field);
                    for (int i = result.field.Length; i < maxNameLength; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(" | ");
                    sb.Append(result.actual);

                    for (int i = result.actual.Length; i < maxActualLength; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(" | ");
                    sb.Append(string.Format("{0}", result.confidence));
                    sb.Append(" | ");

                    if (result.pass)
                    {
                        for (int i = 0; i < maxExpectedLength; i++)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(" |");
                        Log.Info(sb.ToString());
                    }
                    else
                    {
                        sb.Append(result.expected);
                        for (int i = result.expected.Length; i < maxExpectedLength; i++)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(" |");
                        if (result.warn)
                        {
                            Log.Warn(sb.ToString());
                        }
                        else
                        {
                            Log.Error(sb.ToString());
                        }
                    }
                }

                Log.Info(separator);
                Log.Info("");

                Log.Info(agent.ToMatchTrace(failedFieldNames));

                Log.Info("\n\nconfig:\n" + agent.ToYamlTestCase(!init, failComments));
                Log.Info(string.Format("Location of failed test.({0}:{1})", filename, linenumber));
                if (!pass && !showAll)
                {
                    //                LOG.info("+===========================================================================================");
                    return false;
                }
                if (init)
                {
                    return allPass;
                }
            }

            if (showPassedTests)
            {
                Log.Info("+===========================================================================================");
            }
            else
            {
                Log.Info(string.Format("All {0} tests passed", testcount));
            }
            return allPass;
        }

        /// <summary>
        /// This function is used only for analyzing which patterns that could possibly be relevant
        /// were actually relevant for the matcher actions.
        /// </summary>
        /// <returns>The <see cref="List{MatchesList.Match}"/></returns>
        public List<MatchesList.Match> GetMatches()
        {
            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (Matcher matcher in allMatchers)
            {
                allMatches.AddRange(matcher.GetMatches());
            }
            return allMatches;
        }

        /// <summary>
        /// The GetUsedMatches
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="List{MatchesList.Match}"/></returns>
        public List<MatchesList.Match> GetUsedMatches(UserAgent userAgent)
        {
            // Reset all Matchers
            foreach (Matcher matcher in allMatchers)
            {
                matcher.Reset();
                matcher.SetVerboseTemporarily(false);
            }

            flattener.Parse(userAgent);

            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (Matcher matcher in allMatchers)
            {
                allMatches.AddRange(matcher.GetUsedMatches());
            }
            return allMatches;
        }

        /// <summary>
        /// The NewBuilder
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerTesterBuilder"/></returns>
        public static new UserAgentAnalyzerTesterBuilder NewBuilder()
        {
            var a = new UserAgentAnalyzerTester();
            var b = new UserAgentAnalyzerTesterBuilder(a);
            b.SetUAA(a);
            return b;
        }

        /// <summary>
        /// Defines the <see cref="UserAgentAnalyzerTesterBuilder" />
        /// </summary>
        public class UserAgentAnalyzerTesterBuilder : UserAgentAnalyzerBuilder
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserAgentAnalyzerTesterBuilder"/> class.
            /// </summary>
            /// <param name="newUaa">The newUaa<see cref="UserAgentAnalyzerTester"/></param>
            public UserAgentAnalyzerTesterBuilder(UserAgentAnalyzerTester newUaa) : base(newUaa)
            {
            }

            /// <summary>
            /// The Build
            /// </summary>
            /// <returns>The <see cref="UserAgentAnalyzer"/></returns>
            public override UserAgentAnalyzer Build()
            {
                return base.Build();
            }
        }

        /// <summary>
        /// Defines the <see cref="TestResult" />
        /// </summary>
        internal class TestResult
        {
            /// <summary>
            /// Defines the field
            /// </summary>
            internal string field;

            /// <summary>
            /// Defines the expected
            /// </summary>
            internal string expected;

            /// <summary>
            /// Defines the actual
            /// </summary>
            internal string actual;

            /// <summary>
            /// Defines the pass
            /// </summary>
            internal bool pass;

            /// <summary>
            /// Defines the warn
            /// </summary>
            internal bool warn;

            /// <summary>
            /// Defines the confidence
            /// </summary>
            internal long confidence;
        }
    }
}
