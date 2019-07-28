//-----------------------------------------------------------------------
// <copyright file="Matcher.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using log4net;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Utils;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Defines the <see cref="Matcher" />.
    /// </summary>
    [Serializable]
    public class Matcher
    {
        /// <summary>
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(Matcher));

        /// <summary>
        /// Defines the analyzer.
        /// </summary>
        private readonly IAnalyzer analyzer;

        /// <summary>
        /// Defines the fixedStringActions.
        /// </summary>
        private readonly IList<MatcherAction> fixedStringActions;

        /// <summary>
        /// Defines the informMatcherActionsAboutVariables.
        /// </summary>
        private readonly IDictionary<string, ISet<MatcherAction>> informMatcherActionsAboutVariables = new Dictionary<string, ISet<MatcherAction>>();

        /// <summary>
        /// Defines the matcherSourceLocation.
        /// </summary>
        private readonly string matcherSourceLocation;

        /// <summary>
        /// Defines the newValuesUserAgent.
        /// </summary>
        private readonly UserAgent newValuesUserAgent = new UserAgent();

        /// <summary>
        /// Defines the permanentVerbose.
        /// </summary>
        private readonly bool permanentVerbose = false;

        /// <summary>
        /// Defines the variableActions.
        /// </summary>
        private readonly IList<MatcherVariableAction> variableActions;

        /// <summary>
        /// Defines the actionsThatRequireInput.
        /// </summary>
        private long actionsThatRequireInput = 0;

        /// <summary>
        /// Defines the actionsThatRequireInputAndReceivedInput.
        /// </summary>
        private long actionsThatRequireInputAndReceivedInput = 0;

        /// <summary>
        /// Defines the dynamicActions.
        /// </summary>
        private IList<MatcherAction> dynamicActions = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Matcher"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/>.</param>
        /// <param name="lookups">The lookups.</param>
        /// <param name="lookupSets">The lookupSets.</param>
        /// <param name="wantedFieldNames">The wantedFieldNames.</param>
        /// <param name="matcherConfig">The matcherConfig<see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        public Matcher(IAnalyzer analyzer, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets, IList<string> wantedFieldNames, YamlMappingNode matcherConfig, string filename)
        {
            this.Lookups = lookups;
            this.LookupSets = lookupSets;
            this.analyzer = analyzer;
            this.fixedStringActions = new List<MatcherAction>();
            this.variableActions = new List<MatcherVariableAction>();
            this.dynamicActions = new List<MatcherAction>();

            this.matcherSourceLocation = filename + ':' + matcherConfig.Start.Line;

#if VERBOSE
            this.Verbose = true;
#else
            this.Verbose = false;
#endif
            var hasActiveExtractConfigs = false;
            var hasDefinedExtractConfigs = false;

            // List of 'attribute', 'confidence', 'expression'
            var configLines = new List<ConfigLine>(16);
            foreach (var nodeTuple in matcherConfig)
            {
                var name = YamlUtils.GetKeyAsString(nodeTuple, this.matcherSourceLocation);
                switch (name)
                {
                    case "options":
                        var options = YamlUtils.GetStringValues(nodeTuple.Value, this.matcherSourceLocation);
                        this.Verbose = options.Contains("verbose");
                        break;
                    case "variable":
                        foreach (var variableConfig in YamlUtils.GetStringValues(nodeTuple.Value, this.matcherSourceLocation))
                        {
                            var configParts = variableConfig.Split(new char[] { ':' }, 2);

                            if (configParts.Length != 2)
                            {
                                throw new InvalidParserConfigurationException("Invalid variable config line: " + variableConfig);
                            }

                            var variableName = configParts[0].Trim();
                            var config = configParts[1].Trim();

                            configLines.Add(new ConfigLine(ConfigLine.ConfigType.VARIABLE, variableName, null, config));
                        }

                        break;
                    case "require":
                        foreach (var requireConfig in YamlUtils.GetStringValues(nodeTuple.Value, this.matcherSourceLocation))
                        {
                            configLines.Add(new ConfigLine(ConfigLine.ConfigType.REQUIRE, null, null, requireConfig));
                        }

                        break;
                    case "extract":
                        foreach (var extractConfig in YamlUtils.GetStringValues(nodeTuple.Value, this.matcherSourceLocation))
                        {
                            var configParts = extractConfig.Split(new char[] { ':' }, 3);

                            if (configParts.Length != 3)
                            {
                                throw new InvalidParserConfigurationException("Invalid extract config line: " + extractConfig);
                            }

                            var attribute = configParts[0].Trim();
                            long? confidence = null;
                            if (long.TryParse(configParts[1].Trim(), out var tmp))
                            {
                                confidence = tmp;
                            }

                            var config = configParts[2].Trim();
                            hasDefinedExtractConfigs = true;

                            // If we have a restriction on the wanted fields we check if this one is needed at all
                            if (wantedFieldNames == null || wantedFieldNames.Contains(attribute))
                            {
                                configLines.Add(new ConfigLine(ConfigLine.ConfigType.EXTRACT, attribute, confidence, config));
                                hasActiveExtractConfigs = true;
                            }
                            else
                            {
                                configLines.Add(new ConfigLine(ConfigLine.ConfigType.REQUIRE, null, null, config));
                            }
                        }

                        break;
                    default:
                        break; //// Ignore
                }
            }

            this.permanentVerbose = this.Verbose;

            if (this.Verbose)
            {
                Log.Info("---------------------------");
                Log.Info("- MATCHER -");
            }

            if (!hasDefinedExtractConfigs)
            {
                throw new InvalidParserConfigurationException($"Matcher does not extract anything: {this.matcherSourceLocation}");
            }

            if (!hasActiveExtractConfigs)
            {
                throw new UselessMatcherException($"Does not extract any wanted fields: {this.matcherSourceLocation}");
            }

            foreach (var configLine in configLines)
            {
                if (this.Verbose)
                {
                    Log.Info(string.Format("{0}: {1}", configLine.Type, configLine.Expression));
                }

                switch (configLine.Type)
                {
                    case ConfigLine.ConfigType.VARIABLE:
                        this.variableActions.Add(new MatcherVariableAction(configLine.Attribute, configLine.Expression, this));
                        break;
                    case ConfigLine.ConfigType.REQUIRE:
                        this.dynamicActions.Add(new MatcherRequireAction(configLine.Expression, this));
                        break;
                    case ConfigLine.ConfigType.EXTRACT:
                        var action = new MatcherExtractAction(configLine.Attribute, configLine.Confidence ?? 0, configLine.Expression, this);
                        this.dynamicActions.Add(action);

                        // Make sure the field actually exists
                        this.newValuesUserAgent.Set(configLine.Attribute, "Dummy", -9999);
                        action.SetResultAgentField(this.newValuesUserAgent.Get(configLine.Attribute));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matcher"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/>.</param>
        /// <param name="lookups">The lookups.</param>
        /// <param name="lookupSets">The lookupSets.</param>
        internal Matcher(IAnalyzer analyzer, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets)
        {
            this.Lookups = lookups;
            this.LookupSets = lookupSets;
            this.analyzer = analyzer;
            this.fixedStringActions = new List<MatcherAction>();
            this.variableActions = new List<MatcherVariableAction>();
            this.dynamicActions = new List<MatcherAction>();
        }

        /// <summary>
        /// Gets the Lookups.
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> Lookups { get; }

        /// <summary>
        /// Gets the LookupSets.
        /// </summary>
        public IDictionary<string, ISet<string>> LookupSets { get; }

        /// <summary>
        /// Gets a value indicating whether Verbose.
        /// </summary>
        public bool Verbose { get; private set; } = false;

        /// <summary>
        /// Fires all matcher actions.
        /// IFF all success then we tell the userAgent.
        /// </summary>
        /// <param name="userAgent">userAgent The UserAgent that needs to analyzed.</param>
        public virtual void Analyze(UserAgent userAgent)
        {
            if (this.Verbose)
            {
                Log.Info(string.Empty);
                Log.Info("--- Matcher ------------------------");
                Log.Info("ANALYSE ----------------------------");
                var good = true;
                foreach (var action in this.dynamicActions)
                {
                    if (action.CannotBeValid())
                    {
                        Log.Error(string.Format("CANNOT BE VALID : {0}", action.MatchExpression));
                        good = false;
                    }
                }

                foreach (var action in this.dynamicActions)
                {
                    if (!action.ObtainResult())
                    {
                        Log.Error(string.Format("FAILED : {0}", action.MatchExpression));
                        good = false;
                    }
                }

                if (good)
                {
                    Log.Info("COMPLETE ----------------------------");
                }
                else
                {
                    Log.Info("INCOMPLETE ----------------------------");
                    return;
                }
            }
            else
            {
                if (this.actionsThatRequireInput != this.actionsThatRequireInputAndReceivedInput)
                {
                    return;
                }

                foreach (var action in this.dynamicActions)
                {
                    if (action.ObtainResult())
                    {
                        continue;
                    }

                    return; // If one of them is bad we skip the rest
                }
            }

            userAgent.Set(this.newValuesUserAgent, this);
        }

        /// <summary>
        /// The GetAllPossibleFieldNames.
        /// </summary>
        /// <returns>All possible field names.</returns>
        public ISet<string> GetAllPossibleFieldNames()
        {
            var results = new SortedSet<string>();
            results.UnionWith(this.GetAllPossibleFieldNames(this.dynamicActions));
            results.UnionWith(this.GetAllPossibleFieldNames(this.fixedStringActions));
            results.Remove(UserAgent.SET_ALL_FIELDS);
            return results;
        }

        /// <summary>
        /// The GetMatches.
        /// </summary>
        /// <returns>The list of matchers/>.</returns>
        public IList<MatchesList.Match> GetMatches()
        {
            var allMatches = new List<MatchesList.Match>();
            foreach (var action in this.dynamicActions)
            {
                allMatches.AddRange(action.Matches);
            }

            return allMatches;
        }

        /// <summary>
        /// The GetUsedMatches.
        /// </summary>
        /// <returns>The list of matchers.</returns>
        public IList<MatchesList.Match> GetUsedMatches()
        {
            var allMatches = new List<MatchesList.Match>();
            foreach (var action in this.dynamicActions)
            {
                if (action.CannotBeValid())
                {
                    return new List<MatchesList.Match>(); // There is NO way one of them is valid
                }
            }

            foreach (var action in this.dynamicActions)
            {
                if (!action.ObtainResult())
                {
                    return new List<MatchesList.Match>(); // There is NO way one of them is valid
                }
                else
                {
                    allMatches.AddRange(action.Matches);
                }
            }

            return allMatches;
        }

        /// <summary>
        /// The InformMeAbout.
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
        /// <param name="keyPattern">The keyPattern<see cref="string"/>.</param>
        public virtual void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            this.analyzer.InformMeAbout(matcherAction, keyPattern);
        }

        /// <summary>
        /// The InformMeAboutPrefix.
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
        /// <param name="keyPattern">The keyPattern<see cref="string"/>.</param>
        /// <param name="prefix">The prefix<see cref="string"/>.</param>
        public virtual void InformMeAboutPrefix(MatcherAction matcherAction, string keyPattern, string prefix)
        {
            this.analyzer.InformMeAboutPrefix(matcherAction, keyPattern, prefix);
        }

        /// <summary>
        /// The Initialize.
        /// </summary>
        public void Initialize()
        {
            try
            {
                foreach (var item in this.variableActions)
                {
                    item.Initialize();
                }
            }
            catch (InvalidParserConfigurationException e)
            {
                throw new InvalidParserConfigurationException("Syntax error.(" + this.matcherSourceLocation + ")", e);
            }

            var uselessRequireActions = new HashSet<MatcherAction>();
            foreach (var dynamicAction in this.dynamicActions)
            {
                try
                {
                    dynamicAction.Initialize();
                }
                catch (InvalidParserConfigurationException e)
                {
                    if (!e.Message.StartsWith("It is useless to put a fixed value"))
                    {
                        // Ignore fixed values in require
                        throw new InvalidParserConfigurationException("Syntax error.(" + this.matcherSourceLocation + ")" + e.Message, e);
                    }

                    uselessRequireActions.Add(dynamicAction);
                }
            }

            foreach (var action in this.dynamicActions)
            {
                if (action is MatcherExtractAction)
                {
                    if (((MatcherExtractAction)action).IsFixedValue())
                    {
                        this.fixedStringActions.Add(action);
                        action.ObtainResult();
                    }
                }
            }

            foreach (var item in this.fixedStringActions)
            {
                this.dynamicActions.Remove(item);
            }

            uselessRequireActions.ToList().ForEach(u => this.dynamicActions.Remove(u));

            // Verify that a variable only contains the variables that have been defined BEFORE it (also not referencing itself).
            // If all is ok we link them
            var seenVariables = new HashSet<MatcherAction>();
            foreach (var variableAction in this.variableActions)
            {
                seenVariables.Add(variableAction); // Add myself
                var variableName = variableAction.VariableName;
                if (this.informMatcherActionsAboutVariables.ContainsKey(variableName) && this.informMatcherActionsAboutVariables[variableName].Count > 0)
                {
                    var interestedActions = this.informMatcherActionsAboutVariables[variableName];
                    variableAction.SetInterestedActions(interestedActions);
                    foreach (var interestedAction in interestedActions)
                    {
                        if (seenVariables.Contains(interestedAction))
                        {
                            throw new InvalidParserConfigurationException("Syntax error: The line >>" + interestedAction + "<< " +
                                "is referencing variable @" + variableAction.VariableName + " which is not defined yet.");
                        }
                    }
                }
            }

            var allDynamicActions = new List<MatcherAction>();
            allDynamicActions.AddRange(this.variableActions);
            allDynamicActions.AddRange(this.dynamicActions);
            this.dynamicActions = allDynamicActions;

            this.actionsThatRequireInput = this.CountActionsThatMustHaveMatches(this.dynamicActions);

            if (this.Verbose)
            {
                Log.Info("---------------------------");
            }
        }

        /// <summary>
        /// The LookingForRange.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="range">The range<see cref="WordRangeVisitor.Range"/>.</param>
        public virtual void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            this.analyzer.LookingForRange(treeName, range);
        }

        /// <summary>
        /// The Reset.
        /// </summary>
        public void Reset()
        {
            // If there are no dynamic actions we have fixed strings only
            this.actionsThatRequireInputAndReceivedInput = 0;
            this.Verbose = this.permanentVerbose;
            foreach (var action in this.dynamicActions)
            {
                action.Reset();
            }
        }

        /// <summary>
        /// The SetVerboseTemporarily.
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/>.</param>
        public void SetVerboseTemporarily(bool newVerbose)
        {
            foreach (var action in this.dynamicActions)
            {
                action.SetVerbose(newVerbose, true);
            }
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(512);
            sb.Append("MATCHER.(").Append(this.matcherSourceLocation).Append("):\n").Append("    VARIABLE:\n");
            foreach (var action in this.dynamicActions)
            {
                if (action is MatcherVariableAction)
                {
                    sb.Append("        @").Append(((MatcherVariableAction)action).VariableName)
                        .Append(":    ").Append(action.MatchExpression).Append('\n');
                    sb.Append("        -->[").Append(string.Join(",", action.Matches.ToStrings().ToArray())).Append("]\n");
                }
            }

            sb.Append("    REQUIRE:\n");
            foreach (var action in this.dynamicActions)
            {
                if (action is MatcherRequireAction)
                {
                    sb.Append("        ").Append(action.MatchExpression).Append('\n');
                    sb.Append("        -->[").Append(string.Join(",", action.Matches.ToStrings().ToArray())).Append("]\n");
                }
            }

            sb.Append("    EXTRACT:\n");
            foreach (var action in this.dynamicActions)
            {
                if (action is MatcherExtractAction)
                {
                    sb.Append("        ").Append(action.ToString()).Append('\n');
                    sb.Append("        -->[").Append(string.Join(",", action.Matches.ToStrings().ToArray())).Append("]\n");
                }
            }

            foreach (var action in this.fixedStringActions)
            {
                sb.Append("        ").Append(action.ToString()).Append('\n');
            }

            return sb.ToString();
        }

        /// <summary>
        /// The GotMyFirstStartingPoint.
        /// </summary>
        internal void GotMyFirstStartingPoint()
        {
            this.actionsThatRequireInputAndReceivedInput++;
        }

        /// <summary>
        /// The InformMeAboutVariable.
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
        /// <param name="variableName">The variableName<see cref="string"/>.</param>
        internal void InformMeAboutVariable(MatcherAction matcherAction, string variableName)
        {
            if (!this.informMatcherActionsAboutVariables.ContainsKey(variableName))
            {
                var analyzerSet = new HashSet<MatcherAction>();
                this.informMatcherActionsAboutVariables[variableName] = analyzerSet;
            }

            this.informMatcherActionsAboutVariables[variableName].Add(matcherAction);
        }

        /// <summary>
        /// The CountActionsThatMustHaveMatches.
        /// </summary>
        /// <param name="actions">The actions<see cref="IList{MatcherAction}"/>.</param>
        /// <returns>The <see cref="long"/>.</returns>
        private long CountActionsThatMustHaveMatches(IList<MatcherAction> actions)
        {
            long actionsThatMustHaveMatches = 0;
            foreach (var action in actions)
            {
                // If an action exists which without any data can be valid, then we must force the evaluation
                action.Reset();
                if (action.MustHaveMatches)
                {
                    actionsThatMustHaveMatches++;
                }
            }

            return actionsThatMustHaveMatches;
        }

        /// <summary>
        /// The GetAllPossibleFieldNames.
        /// </summary>
        /// <param name="actions">The actions<see cref="IList{MatcherAction}"/>.</param>
        /// <returns>The possible field names.</returns>
        private ISet<string> GetAllPossibleFieldNames(IList<MatcherAction> actions)
        {
            var results = new HashSet<string>();
            foreach (var action in actions)
            {
                if (action is MatcherExtractAction extractAction)
                {
                    results.Add(extractAction.Attribute);
                }
            }

            return results;
        }

        /// <summary>
        /// Defines the <see cref="ConfigLine" />.
        /// </summary>
        internal class ConfigLine
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigLine"/> class.
            /// </summary>
            /// <param name="type">The type<see cref="ConfigType"/>.</param>
            /// <param name="attribute">The attribute<see cref="string"/>.</param>
            /// <param name="confidence">The confidence/>.</param>
            /// <param name="expression">The expression/>.</param>
            public ConfigLine(ConfigType type, string attribute, long? confidence, string expression)
            {
                this.Type = type;
                this.Attribute = attribute;
                this.Confidence = confidence;
                this.Expression = expression;
            }

            /// <summary>
            /// Defines the Type.
            /// </summary>
            public enum ConfigType
            {
                /// <summary>
                /// Defines the VARIABLE
                /// </summary>
                VARIABLE = 2,

                /// <summary>
                /// Defines the REQUIRE
                /// </summary>
                REQUIRE = 1,

                /// <summary>
                /// Defines the EXTRACT
                /// </summary>
                EXTRACT = 0,
            }

            /// <summary>
            /// Gets the Attribute.
            /// </summary>
            public string Attribute { get; }

            /// <summary>
            /// Gets the Confidence.
            /// </summary>
            public long? Confidence { get; }

            /// <summary>
            /// Gets the Expression.
            /// </summary>
            public string Expression { get; }

            /// <summary>
            /// Gets the Type.
            /// </summary>
            public ConfigType Type { get; }
        }
    }
}
