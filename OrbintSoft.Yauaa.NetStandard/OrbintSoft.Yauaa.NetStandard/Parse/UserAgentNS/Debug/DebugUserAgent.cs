//<copyright file="DebugUserAgent.cs" company="OrbintSoft">
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
//<date>2018, 10, 1, 19:34</date>
//<summary></summary>

using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Debug
{
    public class DebugUserAgent: UserAgent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DebugUserAgent));

        internal readonly List<Tuple<UserAgent, Matcher>> appliedMatcherResults = new List<Tuple<UserAgent, Matcher>>();

        public override void Set(UserAgent newValuesUserAgent, Matcher appliedMatcher)
        {
            appliedMatcherResults.Add(new Tuple<UserAgent, Matcher>(new UserAgent(newValuesUserAgent), appliedMatcher));
            base.Set(newValuesUserAgent, appliedMatcher);
        }

        public override void Reset()
        {
            appliedMatcherResults.Clear();
            base.Reset();
        }

        public int GetNumberOfAppliedMatches()
        {
            return appliedMatcherResults.Count;
        }

        public string ToMatchTrace(List<string> highlightNames)
        {
            StringBuilder sb = new StringBuilder(4096);
            sb.Append('\n');
            sb.Append("+=========================================+\n");
            sb.Append("| Matcher results that have been combined |\n");
            sb.Append("+=========================================+\n");
            sb.Append('\n');

            foreach (Tuple<UserAgent, Matcher> pair in appliedMatcherResults)
            {
                sb.Append('\n');
                sb.Append("+================\n");
                sb.Append("+ Applied matcher\n");
                sb.Append("+----------------\n");
                UserAgent result = pair.Item1;
                Matcher matcher = pair.Item2;
                sb.Append(matcher.ToString());
                sb.Append("+----------------\n");
                sb.Append("+ Results\n");
                sb.Append("+----------------\n");
                foreach (string fieldName in result.GetAvailableFieldNamesSorted())
                {
                    AgentField field = result.Get(fieldName);
                    if (field.GetConfidence() >= 0)
                    {
                        String marker = "";
                        if (highlightNames.Contains(fieldName))
                        {
                            marker = " <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<";
                        }
                        sb.Append("| ").Append(fieldName).Append('(').Append(field.GetConfidence()).Append(") = ")
                            .Append(field.GetValue()).Append(marker).Append('\n');
                    }
                }
                sb.Append("+================\n");
            }
            return sb.ToString();
        }

        public bool AnalyzeMatchersResult()
        {
            bool passed = true;
            foreach (string fieldName in GetAvailableFieldNamesSorted())
            {
                Dictionary<long?, string> receivedValues = new Dictionary<long?, string>(32);
                foreach (Tuple<UserAgent, Matcher> pair in appliedMatcherResults)
                {
                    UserAgent result = pair.Item1;
                    AgentField partialField = result.Get(fieldName);
                    if (partialField != null && partialField.GetConfidence() >= 0)
                    {
                        var conf = partialField.GetConfidence();
                        string previousValue =  receivedValues.ContainsKey(conf) ? receivedValues[conf] : null;
                        if (previousValue != null)
                        {
                            if (!previousValue.Equals(partialField.GetValue()))
                            {
                                if (passed)
                                {
                                    Log.Error("***********************************************************");
                                    Log.Error("***        REALLY IMPORTANT ERRORS IN THE RULESET       ***");
                                    Log.Error("*** YOU MUST CHANGE THE CONFIDENCE LEVELS OF YOUR RULES ***");
                                    Log.Error("***********************************************************");
                                }
                                passed = false;
                                Log.Error(string.Format("Found different value for \"{0}\" with SAME confidence {1}: \"{2}\" and \"{3}\"",
                                    fieldName, partialField.GetConfidence(), previousValue, partialField.GetValue()));
                            }
                        }
                        else
                        {
                            receivedValues[partialField.GetConfidence()] = partialField.GetValue();
                        }
                    }
                }
            }
            return passed;
        }

    } 
}
