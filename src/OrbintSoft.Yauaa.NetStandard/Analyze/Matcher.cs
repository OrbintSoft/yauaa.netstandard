//-----------------------------------------------------------------------
// <copyright file="Matcher.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
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
    /// This class loads a matcher configuration from yaml and applies the rules to parse the user agent.
    /// </summary>
    [Serializable]
    public class Matcher
    {
        /// <summary>
        /// Defines the Logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(Matcher));

        /// <summary>
        /// The analyzer used for parsing.
        /// </summary>
        private readonly IAnalyzer analyzer;

        /// <summary>
        /// Defines the actions for fixed strings.
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
        /// <param name="analyzer">The analyzer used for parsing>.</param>
        /// <param name="lookups">The lookups.</param>
        /// <param name="lookupSets">The lookup Sets.</param>
        /// <param name="wantedFieldNames">The wanted field names.</param>
        /// <param name="matcherConfig">The matcher configuration.</param>
        /// <param name="filename">The filename.</param>
        public Matcher(IAnalyzer analyzer, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets, IList<string> wantedFieldNames, YamlMappingNode matcherConfig, string filename)
        {
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
                                throw new InvalidParserConfigurationException($"Invalid variable config line: {variableConfig}");
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
                                throw new InvalidParserConfigurationException($"Invalid extract config line: {extractConfig}");
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
                            if (wantedFieldNames is null || wantedFieldNames.Contains(attribute))
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
                    Log.Info($"{configLine.Type}: {configLine.Expression}");
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
        /// Initializes a new instance of the <see cref="Matcher"/> class for testing purpose.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        internal Matcher(IAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            this.fixedStringActions = new List<MatcherAction>();
            this.variableActions = new List<MatcherVariableAction>();
            this.dynamicActions = new List<MatcherAction>();
        }

        /// <summary>
        /// Gets the number of actions that requires an input.
        /// </summary>
        public long ActionsThatRequireInput { get; private set; } = 0;

        /// <summary>
        /// Gets the analyzer Lookups.
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> Lookups
        {
            get
            {
                return this.analyzer.GetLookups();
            }
        }

        /// <summary>
        /// Gets the LookupSets from the analyzer.
        /// </summary>
        public IDictionary<string, ISet<string>> LookupSets
        {
            get
            {
                return this.analyzer.GetLookupSets();
            }
        }

        /// <summary>
        /// Gets a value indicating whether Verbose is enabled.
        /// It is used for verbose logging.
        /// </summary>
        public bool Verbose { get; private set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether we have already notified that the analyzer has already received the input.
        /// </summary>
        internal bool AlreadyNotifiedAnalyzerWeReceivedInput { get; set; } = false;

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
                        Log.Error($"CANNOT BE VALID : {action.MatchExpression}");
                        good = false;
                    }
                }

                foreach (var action in this.dynamicActions)
                {
                    if (!action.ObtainResult())
                    {
                        Log.Error($"FAILED : {action.MatchExpression}");
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
                if (this.ActionsThatRequireInput != this.actionsThatRequireInputAndReceivedInput)
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
        /// Used to retrieve all available field names from the user agent.
        /// </summary>
        /// <returns>All possible field names.</returns>
        public ISet<string> GetAllPossibleFieldNames()
        {
            var results = new SortedSet<string>();
            results.UnionWith(this.GetAllPossibleFieldNames(this.dynamicActions));
            results.UnionWith(this.GetAllPossibleFieldNames(this.fixedStringActions));
            results.Remove(DefaultUserAgentFields.SET_ALL_FIELDS);
            return results;
        }

        /// <summary>
        /// Gets all matches.
        /// </summary>
        /// <returns>The list of matches/>.</returns>
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
        /// Retrieves all matches that have been used.
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
        /// Inform me about a matcher that matches a key pattern.
        /// </summary>
        /// <param name="matcherAction">The matcher action.</param>
        /// <param name="keyPattern">The key pattern.</param>
        public virtual void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            this.analyzer.InformMeAbout(matcherAction, keyPattern);
        }

        /// <summary>
        /// The Inform me about a matcher that matches a pattern with prefix.
        /// </summary>
        /// <param name="matcherAction">The matcher action.</param>
        /// <param name="keyPattern">The key pattern.</param>
        /// <param name="prefix">The prefix.</param>
        public virtual void InformMeAboutPrefix(MatcherAction matcherAction, string keyPattern, string prefix)
        {
            this.analyzer.InformMeAboutPrefix(matcherAction, keyPattern, prefix);
        }

        /// <summary>
        /// TUsed to initialize the matcher.
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
                throw new InvalidParserConfigurationException($"Syntax error.({this.matcherSourceLocation})", e);
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
                        throw new InvalidParserConfigurationException($"Syntax error.({this.matcherSourceLocation})" + e.Message, e);
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
                            throw new InvalidParserConfigurationException($"Syntax error: The line >>{interestedAction}<< is referencing variable @{variableAction.VariableName} which is not defined yet.");
                        }
                    }
                }
            }

            var allDynamicActions = new List<MatcherAction>();
            allDynamicActions.AddRange(this.variableActions);
            allDynamicActions.AddRange(this.dynamicActions);
            this.dynamicActions = allDynamicActions;

            this.ActionsThatRequireInput = this.CountActionsThatMustHaveMatches(this.dynamicActions);

            if (this.Verbose)
            {
                Log.Info("---------------------------");
            }
        }

        /// <summary>
        /// Look for a range in the tree.
        /// </summary>
        /// <param name="treeName">The tree name.</param>
        /// <param name="range">The range.</param>
        public virtual void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            this.analyzer.LookingForRange(treeName, range);
        }

        /// <summary>
        /// The received input.
        /// </summary>
        public void ReceivedInput()
        {
            if (!this.AlreadyNotifiedAnalyzerWeReceivedInput)
            {
                this.analyzer.ReceivedInput(this);
                this.AlreadyNotifiedAnalyzerWeReceivedInput = true;
            }
        }

        /// <summary>
        /// Resets the matcher.
        /// </summary>
        public void Reset()
        {
            // If there are no dynamic actions we have fixed strings only
            this.AlreadyNotifiedAnalyzerWeReceivedInput = false;
            this.actionsThatRequireInputAndReceivedInput = 0;
            this.Verbose = this.permanentVerbose;
            foreach (var action in this.dynamicActions)
            {
                action.Reset();
            }
        }

        /// <summary>
        /// Used to change verbosity log level temporarly.
        /// </summary>
        /// <param name="newVerbose">The new verbose level>.</param>
        public void SetVerboseTemporarily(bool newVerbose)
        {
            foreach (var action in this.dynamicActions)
            {
                action.SetVerbose(newVerbose, true);
            }
        }

        /// <inheritdoc/>
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
        /// Used to count the actions that require input and receive input.
        /// </summary>
        internal void GotMyFirstStartingPoint()
        {
            this.actionsThatRequireInputAndReceivedInput++;
        }

        /// <summary>
        /// TheInform me when matches a variable name.
        /// </summary>
        /// <param name="matcherAction">The matcher action.</param>
        /// <param name="variableName">The variable name.</param>
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
        /// Counts the actions that must have matches..
        /// </summary>
        /// <param name="actions">The actions.</param>
        /// <returns>The count number.</returns>
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
        /// Gets all possible field names.
        /// </summary>
        /// <param name="actions">The actions.</param>
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
        /// This class is used to load configuration from extracted conffig line in yaml.
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
            /// Defines the type of configuration.
            /// </summary>
            public enum ConfigType
            {
                /// <summary>
                /// The configured variable.
                /// </summary>
                VARIABLE = 2,

                /// <summary>
                /// Configure if required.
                /// </summary>
                REQUIRE = 1,

                /// <summary>
                /// Configure what to extract.
                /// </summary>
                EXTRACT = 0,
            }

            /// <summary>
            /// Gets the attribute to extract.
            /// </summary>
            public string Attribute { get; }

            /// <summary>
            /// Gets the extracted confidence.
            /// </summary>
            public long? Confidence { get; }

            /// <summary>
            /// Gets the expression to apply.
            /// </summary>
            public string Expression { get; }

            /// <summary>
            /// Gets the Tconfiguration type.
            /// </summary>
            public ConfigType Type { get; }
        }
    }
}
