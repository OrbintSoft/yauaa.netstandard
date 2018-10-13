/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug
{
    public class UserAgentAnalyzerTester: UserAgentAnalyzer
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(UserAgentAnalyzerTester));

        public UserAgentAnalyzerTester():base()
        {
            KeepTests();
        }

        public UserAgentAnalyzerTester(string resourceString, string pattern = "*.yaml"):this()
        {
            LoadResources(resourceString, pattern);
        }

        internal class TestResult
        {
            internal string field;
            internal string expected;
            internal string actual;
            internal bool pass;
            internal bool warn;
            internal long confidence;
        }

        /// <summary>
        /// Run all the test_cases available.
        /// </summary>
        /// <returns>
        /// true if all tests were successful.
        /// </returns>
        public bool RunTests()
        {
            return RunTests(false, true);
        }

        public bool RunTests(bool showAll, bool failOnUnexpected)
        {
            return RunTests(showAll, failOnUnexpected, null, false, false);
        }

        public bool RunTests(bool showAll, bool failOnUnexpected, List<string> onlyValidateFieldNames, bool measureSpeed, bool showPassedTests)
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

            sb.Append(" |S|AA|MF|");
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
                LOG.Info("+===========================================================================================");
                LOG.Info(sb.ToString());
                LOG.Info("+-------------------------------------------------------------------------------------------");
            }

            int testcount = 0;
            foreach (Dictionary<string, Dictionary<string, string>> test in testCases)
            {
                testcount++;
                Dictionary<string, string> input = test["input"];
                Dictionary<string, string> expected = test["expected"];
    
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
#if DEBUG
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

                sb.Append("|").Append(String.Format("%5d", testcount))
                  .Append(".(").Append(filename).Append(':').Append(linenumber).Append(')');
                for (int i = filename.Length + linenumber.Length + 7; i < maxFilenameLength; i++)
                {
                    sb.Append(' ');
                }

                agent.SetUserAgentString(userAgentString);


                long measuredSpeed = -1;
                if (measureSpeed)
                {
                    DisableCaching();
                    // Preheat
                    for (int i = 0; i < 100; i++)
                    {
                        Parse(agent);
                    }
                    Stopwatch startTime = Stopwatch.StartNew();
                    for (int i = 0; i < 1000; i++)
                    {
                        Parse(agent);
                    }
                    startTime.Stop();
                    measuredSpeed = startTime.ElapsedMilliseconds;
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
                    sb.Append(string.Format("|%2d", agent.AmbiguityCount));
                }
                else
                {
                    sb.Append("|  ");
                }

                sb.Append(string.Format("|%2d", agent.GetNumberOfAppliedMatches()));

                if (measureSpeed)
                {
                    sb.Append('|').Append(string.Format("%5d", measuredSpeed));
                    sb.Append('|').Append(string.Format("%5.2f", 1000.0 / measuredSpeed));
                }

                sb.Append("| ").Append(testName);

                // We create the log line but we keep it until we know it actually must be output to the screen
                string testLogLine = sb.ToString();

                sb.Length = 0;

                bool pass = true;
                results.Clear();

                if (init)
                {
                    LOG.Info(testLogLine);
                    sb.Append(agent.ToYamlTestCase());
                    LOG.Info(sb.ToString());
                    //                return allPass;
                }
                else
                {
                    if (expected == null)
                    {
                        LOG.Info(testLogLine);
                        LOG.Warn("| - No expectations ... ");
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
                        LOG.Info(testLogLine);
                    }
                    continue;
                }

                if (!pass)
                {
                    LOG.Info(testLogLine);
                    LOG.Error("| TEST FAILED !");
                }

                if (agent.HasAmbiguity)
                {
                    LOG.Info(string.Format("| Parsing problem: Ambiguity {0} times. ", agent.AmbiguityCount));
                }
                if (agent.HasSyntaxError)
                {
                    LOG.Info("| Parsing problem: Syntax Error");
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
                    LOG.Info(sb.ToString());
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
                LOG.Info(separator);

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

                LOG.Info(sb.ToString());

                LOG.Info(separator);

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
                    sb.Append(String.Format("%10d", result.confidence));
                    sb.Append(" | ");

                    if (result.pass)
                    {
                        for (int i = 0; i < maxExpectedLength; i++)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(" |");
                        LOG.Info(sb.ToString());
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
                            LOG.Warn(sb.ToString());
                        }
                        else
                        {
                            LOG.Error(sb.ToString());
                        }
                    }
                }

                LOG.Info(separator);
                LOG.Info("");

                LOG.Info(agent.ToMatchTrace(failedFieldNames));

                LOG.Info("\n\nconfig:\n" + agent.ToYamlTestCase(!init, failComments));
                LOG.Info(string.Format("Location of failed test.({0}:{1})", filename, linenumber));
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
                LOG.Info("+===========================================================================================");
            }
            else
            {
                LOG.Info(string.Format("All {0} tests passed", testcount));
            }
            return allPass;
        }

        /// <summary>
        /// This function is used only for analyzing which patterns that could possibly be relevant
        /// were actually relevant for the matcher actions.
        /// </summary>
        /// <returns>
        /// The list of Matches that were possibly relevant.
        /// </returns>
        public List<MatchesList.Match> GetMatches()
        {
            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (Matcher matcher in allMatchers)
            {
                allMatches.AddRange(matcher.GetMatches());
            }
            return allMatches;
        }

        public List<MatchesList.Match> GetUsedMatches(UserAgent userAgent)
        {
            // Reset all Matchers
            foreach (Matcher matcher in allMatchers)
            {
                matcher.Reset();
#if !DEBUG
                matcher.SetVerboseTemporarily(false);
#endif
            }

            flattener.Parse(userAgent);

            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (Matcher matcher in allMatchers)
            {
                allMatches.AddRange(matcher.GetUsedMatches());
            }
            return allMatches;
        }

        public static new UserAgentAnalyzerTesterBuilder NewBuilder()
        {
            var a = new UserAgentAnalyzerTester();
            var b = new UserAgentAnalyzerTesterBuilder(a);
            b.SetUAA(a);
            return b;
        }

        public class UserAgentAnalyzerTesterBuilder : UserAgentAnalyzerBuilder
        { 
            public UserAgentAnalyzerTesterBuilder(UserAgentAnalyzerTester newUaa): base(newUaa)
            {
                
            }
            
            public override UserAgentAnalyzer Build()
            {
                return base.Build();
            }
        }

    }

}
