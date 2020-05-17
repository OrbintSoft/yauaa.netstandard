//-----------------------------------------------------------------------
// <copyright file="DebugUserAgent.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Debug
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using log4net;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;

    /// <summary>
    /// Defines the <see cref="DebugUserAgent" />.
    /// </summary>
    public class DebugUserAgent : UserAgent
    {
        /// <summary>
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(DebugUserAgent));

        /// <summary>
        /// Defines the appliedMatcherResults.
        /// </summary>
        [NonSerialized]
        private readonly IList<Tuple<UserAgent, Matcher>> appliedMatcherResults = new List<Tuple<UserAgent, Matcher>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugUserAgent"/> class.
        /// </summary>
        /// <param name="wantedFieldNames">The field names you want to retrieve.</param>
        public DebugUserAgent(ICollection<string> wantedFieldNames)
            : base(wantedFieldNames)
        {
        }

        /// <summary>
        /// Gets the NumberOfAppliedMatches.
        /// </summary>
        public int NumberOfAppliedMatches => this.appliedMatcherResults.Count;

        /// <summary>
        /// The AnalyzeMatchersResult.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool AnalyzeMatchersResult()
        {
            var passed = true;
            foreach (var fieldName in this.GetAvailableFieldNamesSorted())
            {
                var receivedValues = new Dictionary<long?, string>(32);
                foreach (var pair in this.appliedMatcherResults)
                {
                    var result = pair.Item1;
                    var partialField = result.Get(fieldName);
                    if (partialField != null && partialField.GetConfidence() >= 0)
                    {
                        var conf = partialField.GetConfidence();
                        var previousValue = receivedValues.ContainsKey(conf) ? receivedValues[conf] : null;
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
                                Log.Error(
                                    string.Format(
                                        "Found different value for \"{0}\" with SAME confidence {1}: \"{2}\" and \"{3}\"",
                                        fieldName,
                                        partialField.GetConfidence(),
                                        previousValue,
                                        partialField.GetValue()));
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

        /// <summary>
        /// The Reset.
        /// </summary>
        public override void Reset()
        {
            this.appliedMatcherResults.Clear();
            base.Reset();
        }

        /// <summary>
        /// The Set.
        /// </summary>
        /// <param name="newValuesUserAgent">The newValuesUserAgent<see cref="UserAgent"/>.</param>
        /// <param name="appliedMatcher">The appliedMatcher<see cref="Matcher"/>.</param>
        public override void Set(UserAgent newValuesUserAgent, Matcher appliedMatcher)
        {
            this.appliedMatcherResults.Add(new Tuple<UserAgent, Matcher>(new UserAgent(newValuesUserAgent), appliedMatcher));
            base.Set(newValuesUserAgent, appliedMatcher);
        }

        /// <summary>
        /// The ToMatchTrace.
        /// </summary>
        /// <param name="highlightNames">The highlightNames.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToMatchTrace(IList<string> highlightNames)
        {
            var sb = new StringBuilder(4096);
            sb.Append('\n');
            sb.Append("+=========================================+\n");
            sb.Append("| Matcher results that have been combined |\n");
            sb.Append("+=========================================+\n");
            sb.Append('\n');

            foreach (var pair in this.appliedMatcherResults)
            {
                sb.Append('\n');
                sb.Append("+================\n");
                sb.Append("+ Applied matcher\n");
                sb.Append("+----------------\n");
                var result = pair.Item1;
                var matcher = pair.Item2;
                sb.Append(matcher.ToString());
                sb.Append("+----------------\n");
                sb.Append("+ Results\n");
                sb.Append("+----------------\n");
                foreach (var fieldName in result.GetAvailableFieldNamesSorted())
                {
                    var field = result.Get(fieldName);

                    if (field == null)
                    {
                        Log.Error("Should not happen");
                    }

                    if (field.GetConfidence() >= 0)
                    {
                        var marker = string.Empty;
                        if (highlightNames.Contains(fieldName))
                        {
                            marker = " <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<";
                        }

                        sb.Append("| ").Append(fieldName).Append('(').Append(field.GetConfidence());
                        if (field.IsDefaultValue)
                        {
                            sb.Append(" => isDefaultValue");
                        }

                        sb.Append(") = ").Append(field.GetValue()).Append(marker).Append('\n');
                    }
                }

                sb.Append("+================\n");
            }

            return sb.ToString();
        }
    }
}
