//-----------------------------------------------------------------------
// <copyright file="UserAgentAnalyzerTester.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2018 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//   
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Debug
{
    using log4net;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
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
            this.KeepTests();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzerTester"/> class.
        /// </summary>
        /// <param name="resourceString">The resourceString<see cref="string"/></param>
        /// <param name="pattern">The pattern<see cref="string"/></param>
        public UserAgentAnalyzerTester(string resourceString, string pattern = "*.yaml") : this()
        {
            this.LoadResources(resourceString, pattern);
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
        /// This function is used only for analyzing which patterns that could possibly be relevant
        /// were actually relevant for the matcher actions.
        /// </summary>
        /// <returns>The <see cref="List{MatchesList.Match}"/></returns>
        public IList<MatchesList.Match> GetMatches()
        {
            var allMatches = new List<MatchesList.Match>();
            foreach (var matcher in this.AllMatchers)
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
        public IList<MatchesList.Match> GetUsedMatches(UserAgent userAgent)
        {
            // Reset all Matchers
            foreach (var matcher in this.AllMatchers)
            {
                matcher.Reset();
                matcher.SetVerboseTemporarily(false);
            }

            this.Flattener.Parse(userAgent);

            var allMatches = new List<MatchesList.Match>();
            foreach (var matcher in this.AllMatchers)
            {
                allMatches.AddRange(matcher.GetUsedMatches());
            }

            return allMatches;
        }

        /// <summary>
        /// Run all the test_cases available.
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool RunTests()
        {
            return this.RunTests(false, true);
        }

        /// <summary>
        /// The RunTests
        /// </summary>
        /// <param name="showAll">The showAll<see cref="bool"/></param>
        /// <param name="failOnUnexpected">The failOnUnexpected<see cref="bool"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool RunTests(bool showAll, bool failOnUnexpected)
        {
            return this.RunTests(showAll, failOnUnexpected, null, false, false);
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
            var allPass = true;
            this.InitializeMatchers();
            if (this.TestCases == null)
            {
                return allPass;
            }

            var agent = new DebugUserAgent();

            var results = new List<TestResult>();

            var filenameHeader = "Test number and source";
            var filenameHeaderLength = filenameHeader.Length;
            var maxFilenameLength = filenameHeaderLength;
            foreach (var test in this.TestCases)
            {
                var metaData = test["metaData"];
                var filename = metaData["filename"];
                maxFilenameLength = Math.Max(maxFilenameLength, filename.Length);
            }

            maxFilenameLength += 11;

            var sb = new StringBuilder(1024);

            sb.Append("| ").Append(filenameHeader);
            for (var i = filenameHeaderLength; i < maxFilenameLength; i++)
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

            var testcount = 0;
            foreach (var test in this.TestCases)
            {
                testcount++;
                var input = test.ContainsKey("input") ? test["input"] : null;
                var expected = test.ContainsKey("expected") ? test["expected"] : null;

                IList<string> options = null;
                if (test.ContainsKey("options"))
                {
                    options = new List<string>(test["options"].Keys);
                }

                var metaData = test["metaData"];
                var filename = metaData["filename"];
                var linenumber = metaData["fileline"];

                var init = false;

                if (options == null)
                {
#if VERBOSE
                    SetVerbose(true);            
                    agent.IsDebug = true;
#else
                    this.SetVerbose(false);
                    agent.IsDebug = false;
#endif
                }
                else
                {
                    var newVerbose = options.Contains("verbose");
                    this.SetVerbose(newVerbose);
                    agent.IsDebug = newVerbose;
                    init = options.Contains("init");
                }

                if (expected == null || expected.Count == 0)
                {
                    init = true;
                }

                var testName = input.ContainsKey("name") ? input["name"] : null;
                var userAgentString = input["user_agent_string"];

                if (testName == null)
                {
                    testName = userAgentString;
                }

                sb.Length = 0;

                sb.Append("|").Append(string.Format("{0}", testcount))
                  .Append(".(").Append(filename).Append(':').Append(linenumber).Append(')');
                for (var i = filename.Length + linenumber.Length + 7; i < maxFilenameLength; i++)
                {
                    sb.Append(' ');
                }

                agent.UserAgentString = userAgentString;

                long measuredSpeed = -1;
                if (measureSpeed)
                {
                    this.DisableCaching();

                    // Preheat
                    for (var i = 0; i < 100; i++)
                    {
                        this.Parse(agent);
                    }

                    var stopwatch = Stopwatch.StartNew();
                    for (var i = 0; i < 1000; i++)
                    {
                        this.Parse(agent);
                    }

                    stopwatch.Stop();
                    measuredSpeed = stopwatch.ElapsedMilliseconds;
                }
                else
                {
                    this.Parse(agent);
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

                sb.Append(string.Format("|{0}", agent.NumberOfAppliedMatches));

                if (measureSpeed)
                {
                    sb.Append('|').Append(string.Format("{0}", measuredSpeed));
                    sb.Append('|').Append(string.Format("{0}", 1000.0 / measuredSpeed));
                }

                sb.Append("| ").Append(testName);

                // We create the log line but we keep it until we know it actually must be output to the screen
                var testLogLine = sb.ToString();

                sb.Length = 0;

                var pass = true;
                results.Clear();

                if (init)
                {
                    Log.Info(testLogLine);
                    sb.Append(agent.ToYamlTestCase());
                    Log.Info(sb.ToString());
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

                var maxNameLength = 6; // "Field".length()+1;
                var maxActualLength = 7; // "Actual".length()+1;
                var maxExpectedLength = 9; // "Expected".length()+1;

                if (expected != null)
                {
                    var fieldNames = agent.GetAvailableFieldNamesSorted();

                    if (onlyValidateFieldNames != null && onlyValidateFieldNames.Count == 0)
                    {
                        onlyValidateFieldNames = null;
                    }
                    else if (onlyValidateFieldNames != null)
                    {
                        fieldNames.Clear();
                        fieldNames.AddRange(onlyValidateFieldNames);
                    }

                    foreach (var newFieldName in expected.Keys)
                    {
                        if (!fieldNames.Contains(newFieldName))
                        {
                            fieldNames.Add(newFieldName);
                        }
                    }

                    foreach (var fieldName in fieldNames)
                    {
                        // Only check the desired fieldnames
                        if (onlyValidateFieldNames != null && !onlyValidateFieldNames.Contains(fieldName))
                        {
                            continue;
                        }

                        var result = new TestResult
                        {
                            Field = fieldName
                        };

                        bool expectedSomething;

                        // Actual value
                        result.Actual = agent.GetValue(result.Field);
                        result.Confidence = agent.GetConfidence(result.Field);
                        if (result.Actual == null)
                        {
                            result.Actual = UserAgent.NULL_VALUE;
                        }

                        // Expected value
                        var expectedValue = expected.ContainsKey(fieldName) ? expected[fieldName] : null;
                        if (expectedValue == null)
                        {
                            expectedSomething = false;
                            if (result.Confidence < 0)
                            {
                                continue; // A negative value really means 'absent'
                            }

                            result.Expected = "<<absent>>";
                        }
                        else
                        {
                            expectedSomething = true;
                            result.Expected = expectedValue;
                        }

                        result.Pass = result.Actual.Equals(result.Expected);
                        if (!result.Pass)
                        {
                            result.Warn = true;
                            if (expectedSomething)
                            {
                                result.Warn = false;
                                pass = false;
                                allPass = false;
                            }
                            else
                            {
                                if (failOnUnexpected)
                                {
                                    // We ignore this special field
                                    if (!UserAgent.SYNTAX_ERROR.Equals(result.Field))
                                    {
                                        result.Warn = false;
                                        pass = false;
                                        allPass = false;
                                    }
                                }
                            }
                        }

                        results.Add(result);

                        maxNameLength = Math.Max(maxNameLength, result.Field.Length);
                        maxActualLength = Math.Max(maxActualLength, result.Actual.Length);
                        maxExpectedLength = Math.Max(maxExpectedLength, result.Expected.Length);
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
                    foreach (var path in GetAllPaths(userAgentString))
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
                for (var i = 0; i < maxNameLength; i++)
                {
                    sb.Append('-');
                }

                sb.Append("-+-");
                for (var i = 0; i < maxActualLength; i++)
                {
                    sb.Append('-');
                }

                sb.Append("-+------------+-");
                for (var i = 0; i < maxExpectedLength; i++)
                {
                    sb.Append('-');
                }

                sb.Append("-+");

                var separator = sb.ToString();
                Log.Info(separator);

                sb.Length = 0;
                sb.Append("| Result | Field ");
                for (var i = 6; i < maxNameLength; i++)
                {
                    sb.Append(' ');
                }

                sb.Append(" | Actual ");

                for (var i = 7; i < maxActualLength; i++)
                {
                    sb.Append(' ');
                }

                sb.Append(" | Confidence | Expected ");
                for (var i = 9; i < maxExpectedLength; i++)
                {
                    sb.Append(' ');
                }

                sb.Append(" |");

                Log.Info(sb.ToString());

                Log.Info(separator);

                var failComments = new Dictionary<string, string>();

                var failedFieldNames = new List<string>();
                foreach (var result in results)
                {
                    sb.Length = 0;
                    if (result.Pass)
                    {
                        sb.Append("|        | ");
                    }
                    else
                    {
                        if (result.Warn)
                        {
                            sb.Append("| ~warn~ | ");
                            failComments[result.Field] = "~~ Unexpected result ~~";
                        }
                        else
                        {
                            sb.Append("| -FAIL- | ");
                            failComments[result.Field] = "FAILED; Should be '" + result.Expected + "'";
                            failedFieldNames.Add(result.Field);
                        }
                    }

                    sb.Append(result.Field);
                    for (var i = result.Field.Length; i < maxNameLength; i++)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(" | ");
                    sb.Append(result.Actual);

                    for (var i = result.Actual.Length; i < maxActualLength; i++)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(" | ");
                    sb.Append(string.Format("{0}", result.Confidence));
                    sb.Append(" | ");

                    if (result.Pass)
                    {
                        for (var i = 0; i < maxExpectedLength; i++)
                        {
                            sb.Append(' ');
                        }

                        sb.Append(" |");
                        Log.Info(sb.ToString());
                    }
                    else
                    {
                        sb.Append(result.Expected);
                        for (var i = result.Expected.Length; i < maxExpectedLength; i++)
                        {
                            sb.Append(' ');
                        }

                        sb.Append(" |");
                        if (result.Warn)
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
                Log.Info(string.Empty);

                Log.Info(agent.ToMatchTrace(failedFieldNames));

                Log.Info(string.Format("\n\nconfig:\n {0}", agent.ToYamlTestCase(!init, failComments)));
                Log.Info(string.Format("Location of failed test.({0}:{1})", filename, linenumber));
                if (!pass && !showAll)
                {
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
            /// Gets or sets the Actual
            /// </summary>
            internal string Actual { get; set; }

            /// <summary>
            /// Gets or sets the Confidence
            /// </summary>
            internal long Confidence { get; set; }

            /// <summary>
            /// Gets or sets the Expected
            /// </summary>
            internal string Expected { get; set; }

            /// <summary>
            /// Gets or sets the Field
            /// </summary>
            internal string Field { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether Pass
            /// </summary>
            internal bool Pass { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether Warn
            /// </summary>
            internal bool Warn { get; set; }
        }
    }
}
