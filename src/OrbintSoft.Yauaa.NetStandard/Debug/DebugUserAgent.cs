//-----------------------------------------------------------------------
// <copyright file="DebugUserAgent.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2020 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2020 Niels Basjes
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Debug
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Logger;

    /// <summary>
    /// Defines an extension of <see cref="UserAgent"/> for debugging and testing purpose.
    /// </summary>
    public class DebugUserAgent : UserAgent
    {
        /// <summary>
        /// Defines the logger.
        /// </summary>
        private static readonly ILogger Logger = new Logger<DebugUserAgent>();

        /// <summary>
        /// Defines the applied matchers results..
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
        /// Gets the number of applied matches.
        /// </summary>
        public int NumberOfAppliedMatches => this.appliedMatcherResults.Count;

        /// <summary>
        /// Analyze the matcher results and checks if they are accepptable or if there are conflicts (ex: same confidence level but different values).
        /// </summary>
        /// <returns>The analysis is ok.</returns>
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
                                    Logger.Error($"***********************************************************");
                                    Logger.Error($"***        REALLY IMPORTANT ERRORS IN THE RULESET       ***");
                                    Logger.Error($"*** YOU MUST CHANGE THE CONFIDENCE LEVELS OF YOUR RULES ***");
                                    Logger.Error($"***********************************************************");
                                }

                                passed = false;
                                Logger.Error($"Found different value for \"{fieldName}\" with SAME confidence {partialField.GetConfidence()}: \"{previousValue}\" and \"{partialField.GetValue()}\"");
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

        /// <inheritdoc/>
        public override void Reset()
        {
            this.appliedMatcherResults.Clear();
            base.Reset();
        }

        /// <inheritdoc/>
        public override void Set(UserAgent newValuesUserAgent, Matcher appliedMatcher)
        {
            this.appliedMatcherResults.Add(new Tuple<UserAgent, Matcher>(new UserAgent(newValuesUserAgent), appliedMatcher));
            base.Set(newValuesUserAgent, appliedMatcher);
        }

        /// <summary>
        /// Generate a trace dump of applied matcher results.
        /// </summary>
        /// <param name="highlightNames">The filed names you want highlight.</param>
        /// <returns>The trace.</returns>
        public string ToMatchTrace(IList<string> highlightNames)
        {
            var sb = new StringBuilder(4096);
            sb.AppendLine();
            sb.AppendLine("+=========================================+");
            sb.AppendLine("| Matcher results that have been combined |");
            sb.AppendLine("+=========================================+");
            sb.AppendLine();

            foreach (var pair in this.appliedMatcherResults)
            {
                sb.AppendLine();
                sb.AppendLine("+================");
                sb.AppendLine("+ Applied matcher");
                sb.AppendLine("+----------------");
                var result = pair.Item1;
                var matcher = pair.Item2;
                sb.Append(matcher.ToString());
                sb.AppendLine("+----------------");
                sb.AppendLine("+ Results");
                sb.AppendLine("+----------------");
                foreach (var fieldName in result.GetAvailableFieldNamesSorted())
                {
                    var field = result.Get(fieldName);

                    if (field == null)
                    {
                        Logger.Error($"Should not happen");
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

                        sb.Append(") = ").Append(field.GetValue()).Append(marker).AppendLine();
                    }
                }

                sb.AppendLine("+================");
            }

            return sb.ToString();
        }
    }
}
