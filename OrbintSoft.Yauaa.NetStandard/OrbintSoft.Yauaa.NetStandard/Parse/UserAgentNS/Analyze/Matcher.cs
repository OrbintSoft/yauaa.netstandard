//<copyright file="Matcher.cs" company="OrbintSoft">
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
//<date>2018, 7, 26, 23:01</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    using log4net;
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Defines the <see cref="Matcher" />
    /// </summary>
    [Serializable]
    public class Matcher
    {
        /// <summary>
        /// Defines the Log
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(Matcher));

        /// <summary>
        /// Defines the analyzer
        /// </summary>
        private readonly IAnalyzer analyzer;

        /// <summary>
        /// Defines the variableActions
        /// </summary>
        private readonly List<MatcherVariableAction> variableActions;

        /// <summary>
        /// Defines the fixedStringActions
        /// </summary>
        private readonly List<MatcherAction> fixedStringActions;

        /// <summary>
        /// Defines the newValuesUserAgent
        /// </summary>
        private readonly UserAgent newValuesUserAgent = new UserAgent();

        /// <summary>
        /// Defines the lookups
        /// </summary>
        private readonly IDictionary<string, IDictionary<string, string>> lookups;

        /// <summary>
        /// Defines the lookupSets
        /// </summary>
        private readonly IDictionary<string, ISet<string>> lookupSets;

        // Used for error reporting: The filename and line number where the config was located.
        /// <summary>
        /// Defines the matcherSourceLocation
        /// </summary>
        private readonly string matcherSourceLocation;

        /// <summary>
        /// Defines the informMatcherActionsAboutVariables
        /// </summary>
        private readonly IDictionary<string, ISet<MatcherAction>> informMatcherActionsAboutVariables = new Dictionary<string, ISet<MatcherAction>>();

        /// <summary>
        /// Defines the actionsThatRequireInput
        /// </summary>
        private long actionsThatRequireInput = 0;

        /// <summary>
        /// Defines the dynamicActions
        /// </summary>
        private IList<MatcherAction> dynamicActions = null;

        /// <summary>
        /// Defines the actionsThatRequireInputAndReceivedInput
        /// </summary>
        private long actionsThatRequireInputAndReceivedInput = 0;

#if VERBOSE
        private bool verbose = true;
        private readonly bool permanentVerbose = true;
#else
        /// <summary>
        /// Defines the verbose
        /// </summary>
        private bool verbose = false;

        /// <summary>
        /// Defines the permanentVerbose
        /// </summary>
        private readonly bool permanentVerbose = false;

#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="Matcher"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/></param>
        /// <param name="lookups">The lookups<see cref="IDictionary{string, IDictionary{string, string}}"/></param>
        /// <param name="lookupSets">The lookupSets<see cref="IDictionary{string, ISet{string}}"/></param>
        /// <param name="wantedFieldNames">The wantedFieldNames<see cref="IList{string}"/></param>
        /// <param name="matcherConfig">The matcherConfig<see cref="YamlMappingNode"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        public Matcher(IAnalyzer analyzer, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets, IList<string> wantedFieldNames, YamlMappingNode matcherConfig, string filename)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.analyzer = analyzer;
            fixedStringActions = new List<MatcherAction>();
            variableActions = new List<MatcherVariableAction>();
            dynamicActions = new List<MatcherAction>();

            matcherSourceLocation = filename + ':' + matcherConfig.Start.Line;

#if VERBOSE
            verbose = true;
#else
            verbose = false;
#endif
            bool hasActiveExtractConfigs = false;
            bool hasDefinedExtractConfigs = false;

            // List of 'attribute', 'confidence', 'expression'
            IList<ConfigLine> configLines = new List<ConfigLine>(16);
            foreach (KeyValuePair<YamlNode, YamlNode> nodeTuple in matcherConfig)
            {
                string name = YamlUtils.GetKeyAsString(nodeTuple, matcherSourceLocation);
                switch (name)
                {
                    case "options":
                        List<string> options = YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation);
                        verbose = options.Contains("verbose");
                        break;
                    case "variable":
                        foreach (string variableConfig in YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation))
                        {
                            string[] configParts = variableConfig.Split(new Char[] { ':' }, 2);

                            if (configParts.Length != 2)
                            {
                                throw new InvalidParserConfigurationException("Invalid variable config line: " + variableConfig);
                            }
                            string variableName = configParts[0].Trim();
                            string config = configParts[1].Trim();

                            configLines.Add(new ConfigLine(ConfigLine.Type.VARIABLE, variableName, null, config));
                        }
                        break;
                    case "require":
                        foreach (string requireConfig in YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation))
                        {
                            configLines.Add(new ConfigLine(ConfigLine.Type.REQUIRE, null, null, requireConfig));
                        }
                        break;
                    case "extract":
                        foreach (string extractConfig in YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation))
                        {
                            string[] configParts = extractConfig.Split(new char[] { ':' }, 3);

                            if (configParts.Length != 3)
                            {
                                throw new InvalidParserConfigurationException("Invalid extract config line: " + extractConfig);
                            }
                            string attribute = configParts[0].Trim();
                            long? confidence = null;
                            if (long.TryParse(configParts[1].Trim(), out long tmp))
                            {
                                confidence = tmp;
                            }
                            string config = configParts[2].Trim();
                            hasDefinedExtractConfigs = true;
                            // If we have a restriction on the wanted fields we check if this one is needed at all
                            if (wantedFieldNames == null || wantedFieldNames.Contains(attribute))
                            {
                                configLines.Add(new ConfigLine(ConfigLine.Type.EXTRACT, attribute, confidence, config));
                                hasActiveExtractConfigs = true;
                            }
                            else
                            {
                                configLines.Add(new ConfigLine(ConfigLine.Type.REQUIRE, null, null, config));
                            }
                        }
                        break;
                    default:
                        break;
                        // Ignore
                }
            }

            permanentVerbose = verbose;

            if (verbose)
            {
                Log.Info("---------------------------");
                Log.Info("- MATCHER -");
            }

            if (!hasDefinedExtractConfigs)
            {
                throw new InvalidParserConfigurationException("Matcher does not extract anything");
            }

            if (!hasActiveExtractConfigs)
            {
                throw new UselessMatcherException("Does not extract any wanted fields");
            }
#if VERBOSE
            //configLines = configLines.OrderBy(c => c.type).ThenBy(n => n.attribute, StringComparer.Ordinal).ThenBy(n => n?.expression, StringComparer.Ordinal).ToList();
#endif
            foreach (ConfigLine configLine in configLines)
            {
                if (verbose)
                {
                    Log.Info(string.Format("{0}: {1}", configLine.type, configLine.expression));
                }
                switch (configLine.type)
                {
                    case ConfigLine.Type.VARIABLE:
                        variableActions.Add(new MatcherVariableAction(configLine.attribute, configLine.expression, this));
                        break;
                    case ConfigLine.Type.REQUIRE:
                        dynamicActions.Add(new MatcherRequireAction(configLine.expression, this));
                        break;
                    case ConfigLine.Type.EXTRACT:
                        MatcherExtractAction action = new MatcherExtractAction(configLine.attribute, configLine.confidence ?? 0, configLine.expression, this);
                        dynamicActions.Add(action);

                        // Make sure the field actually exists
                        newValuesUserAgent.Set(configLine.attribute, "Dummy", -9999);
                        action.SetResultAgentField(newValuesUserAgent.Get(configLine.attribute));
                        break;
                    default:
                        break;
                }
            }
        }

        // Internal private constructor for testing purposes only
        /// <summary>
        /// Initializes a new instance of the <see cref="Matcher"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/></param>
        /// <param name="lookups">The lookups<see cref="IDictionary{string, IDictionary{string, string}}"/></param>
        /// <param name="lookupSets">The lookupSets<see cref="IDictionary{string, ISet{string}}"/></param>
        internal Matcher(IAnalyzer analyzer, IDictionary<string, IDictionary<string, string>> lookups, IDictionary<string, ISet<string>> lookupSets)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.analyzer = analyzer;
            fixedStringActions = new List<MatcherAction>();
            variableActions = new List<MatcherVariableAction>();
            dynamicActions = new List<MatcherAction>();
        }

        /// <summary>
        /// The GetLookups
        /// </summary>
        /// <returns>The <see cref="IDictionary{string, IDictionary{string, string}}"/></returns>
        public IDictionary<string, IDictionary<string, string>> GetLookups()
        {
            return lookups;
        }

        /// <summary>
        /// The GetLookupSets
        /// </summary>
        /// <returns>The <see cref="IDictionary{string, ISet{string}}"/></returns>
        public IDictionary<string, ISet<string>> GetLookupSets()
        {
            return lookupSets;
        }

        /// <summary>
        /// The Initialize
        /// </summary>
        public void Initialize()
        {
            try
            {
                variableActions.ForEach(v => v.Initialize());
            }
            catch (InvalidParserConfigurationException e)
            {
                throw new InvalidParserConfigurationException("Syntax error.(" + matcherSourceLocation + ")", e);
            }

            ISet<MatcherAction> uselessRequireActions = new HashSet<MatcherAction>();
            foreach (MatcherAction dynamicAction in dynamicActions)
            {
                try
                {
                    dynamicAction.Initialize();
                }
                catch (InvalidParserConfigurationException e)
                {
                    if (!e.Message.StartsWith("It is useless to put a fixed value"))
                    {// Ignore fixed values in require
                        throw new InvalidParserConfigurationException("Syntax error.(" + matcherSourceLocation + ")" + e.Message, e);
                    }
                    uselessRequireActions.Add(dynamicAction);
                }
            }

            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherExtractAction)
                {
                    if (((MatcherExtractAction)action).IsFixedValue())
                    {
                        fixedStringActions.Add(action);
                        action.ObtainResult();
                    }
                }
            }

            fixedStringActions.ForEach(f => dynamicActions.Remove(f));
            uselessRequireActions.ToList().ForEach(u => dynamicActions.Remove(u));

            // Verify that a variable only contains the variables that have been defined BEFORE it (also not referencing itself).
            // If all is ok we link them
            ISet<MatcherAction> seenVariables = new HashSet<MatcherAction>();
            foreach (MatcherVariableAction variableAction in variableActions)
            {
                seenVariables.Add(variableAction); // Add myself
                var variableName = variableAction.VariableName;
                if (informMatcherActionsAboutVariables.ContainsKey(variableName) && informMatcherActionsAboutVariables[variableName].Count > 0)
                {
                    ISet<MatcherAction> interestedActions = informMatcherActionsAboutVariables[variableName];
                    variableAction.SetInterestedActions(interestedActions);
                    foreach (MatcherAction interestedAction in interestedActions)
                    {
                        if (seenVariables.Contains(interestedAction))
                        {
                            throw new InvalidParserConfigurationException("Syntax error: The line >>" + interestedAction + "<< " +
                                "is referencing variable @" + variableAction.VariableName + " which is not defined yet.");
                        }
                    }
                }
            }

            List<MatcherAction> allDynamicActions = new List<MatcherAction>();
            allDynamicActions.AddRange(variableActions);
            allDynamicActions.AddRange(dynamicActions);
            dynamicActions = allDynamicActions;

            actionsThatRequireInput = CountActionsThatMustHaveMatches(dynamicActions);

            if (verbose)
            {
                Log.Info("---------------------------");
            }
        }

        /// <summary>
        /// The GetAllPossibleFieldNames
        /// </summary>
        /// <returns>The <see cref="ISet{string}"/></returns>
        public ISet<string> GetAllPossibleFieldNames()
        {
            ISet<string> results = new SortedSet<string>();
            results.UnionWith(GetAllPossibleFieldNames(dynamicActions));
            results.UnionWith(GetAllPossibleFieldNames(fixedStringActions));
            results.Remove(UserAgent.SET_ALL_FIELDS);
            return results;
        }

        /// <summary>
        /// The LookingForRange
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/></param>
        /// <param name="range">The range<see cref="WordRangeVisitor.Range"/></param>
        public virtual void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            analyzer.LookingForRange(treeName, range);
        }

        /// <summary>
        /// The InformMeAbout
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
        /// <param name="keyPattern">The keyPattern<see cref="string"/></param>
        public virtual void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            analyzer.InformMeAbout(matcherAction, keyPattern);
        }

        /// <summary>
        /// The InformMeAboutPrefix
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
        /// <param name="keyPattern">The keyPattern<see cref="string"/></param>
        /// <param name="prefix">The prefix<see cref="string"/></param>
        public virtual void InformMeAboutPrefix(MatcherAction matcherAction, string keyPattern, string prefix)
        {
            analyzer.InformMeAboutPrefix(matcherAction, keyPattern, prefix);
        }

        /// <summary>
        /// Fires all matcher actions.
        /// IFF all success then we tell the userAgent
        /// </summary>
        /// <param name="userAgent">userAgent The useragent that needs to analyzed</param>
        public virtual void Analyze(UserAgent userAgent)
        {

            if (verbose)
            {
                Log.Info("");
                Log.Info("--- Matcher ------------------------");
                Log.Info(" ANALYSE ----------------------------");
                bool good = true;
                foreach (MatcherAction action in dynamicActions)
                {
                    if (action.CannotBeValid())
                    {
                        Log.Error(string.Format("CANNOT BE VALID : {0}", action.GetMatchExpression()));
                        good = false;
                    }
                }
                foreach (MatcherAction action in dynamicActions)
                {
                    if (!action.ObtainResult())
                    {
                        Log.Error(string.Format("FAILED : {0}", action.GetMatchExpression()));
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
                if (actionsThatRequireInput != actionsThatRequireInputAndReceivedInput)
                {
                    return;
                }
                foreach (MatcherAction action in dynamicActions)
                {
                    if (action.ObtainResult())
                    {
                        continue;
                    }
                    return; // If one of them is bad we skip the rest
                }
            }
            userAgent.Set(newValuesUserAgent, this);
        }

        /// <summary>
        /// The GetVerbose
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public bool GetVerbose()
        {
            return verbose;
        }

        /// <summary>
        /// The SetVerboseTemporarily
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        public void SetVerboseTemporarily(bool newVerbose)
        {
            foreach (MatcherAction action in dynamicActions)
            {
                action.SetVerbose(newVerbose, true);
            }
        }

        /// <summary>
        /// The Reset
        /// </summary>
        public void Reset()
        {
            // If there are no dynamic actions we have fixed strings only
            actionsThatRequireInputAndReceivedInput = 0;
            verbose = permanentVerbose;
            foreach (MatcherAction action in dynamicActions)
            {
                action.Reset();
            }
        }

        /// <summary>
        /// The GetMatches
        /// </summary>
        /// <returns>The <see cref="IList{MatchesList.Match}"/></returns>
        public IList<MatchesList.Match> GetMatches()
        {
            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (MatcherAction action in dynamicActions)
            {
                allMatches.AddRange(action.GetMatches());
            }
            return allMatches;
        }

        /// <summary>
        /// The GetUsedMatches
        /// </summary>
        /// <returns>The <see cref="IList{MatchesList.Match}"/></returns>
        public IList<MatchesList.Match> GetUsedMatches()
        {
            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (MatcherAction action in dynamicActions)
            {
                if (action.CannotBeValid())
                {
                    return new List<MatchesList.Match>(); // There is NO way one of them is valid
                }
            }
            foreach (MatcherAction action in dynamicActions)
            {
                if (!action.ObtainResult())
                {
                    return new List<MatchesList.Match>(); // There is NO way one of them is valid
                }
                else
                {
                    allMatches.AddRange(action.GetMatches());
                }
            }
            return allMatches;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(512);
            sb.Append("MATCHER.(").Append(matcherSourceLocation).Append("):\n").Append("    VARIABLE:\n");
            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherVariableAction)
                {
                    sb.Append("        @").Append(((MatcherVariableAction)action).VariableName)
                        .Append(":    ").Append(action.GetMatchExpression()).Append('\n');
                    sb.Append("        -->[").Append(string.Join(",", action.GetMatches().ToStrings().ToArray())).Append("]\n");
                }
            }
            sb.Append("    REQUIRE:\n");
            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherRequireAction)
                {
                    sb.Append("        ").Append(action.GetMatchExpression()).Append('\n');
                    sb.Append("        -->[").Append(string.Join(",", action.GetMatches().ToStrings().ToArray())).Append("]\n");
                }
            }
            sb.Append("    EXTRACT:\n");
            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherExtractAction)
                {
                    sb.Append("        ").Append(action.ToString()).Append('\n');
                    sb.Append("        -->[").Append(string.Join(",", action.GetMatches().ToStrings().ToArray())).Append("]\n");
                }
            }
            foreach (MatcherAction action in fixedStringActions)
            {
                sb.Append("        ").Append(action.ToString()).Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>
        /// The InformMeAboutVariable
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/></param>
        /// <param name="variableName">The variableName<see cref="string"/></param>
        internal void InformMeAboutVariable(MatcherAction matcherAction, string variableName)
        {
            if (!informMatcherActionsAboutVariables.ContainsKey(variableName))
            {
                ISet<MatcherAction> analyzerSet = new HashSet<MatcherAction>();
                informMatcherActionsAboutVariables[variableName] = analyzerSet;
            }
            informMatcherActionsAboutVariables[variableName].Add(matcherAction);
        }

        /// <summary>
        /// The GotMyFirstStartingPoint
        /// </summary>
        internal void GotMyFirstStartingPoint()
        {
            actionsThatRequireInputAndReceivedInput++;
        }

        /// <summary>
        /// The CountActionsThatMustHaveMatches
        /// </summary>
        /// <param name="actions">The actions<see cref="IList{MatcherAction}"/></param>
        /// <returns>The <see cref="long"/></returns>
        private long CountActionsThatMustHaveMatches(IList<MatcherAction> actions)
        {
            long actionsThatMustHaveMatches = 0;
            foreach (MatcherAction action in actions)
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
        /// The GetAllPossibleFieldNames
        /// </summary>
        /// <param name="actions">The actions<see cref="IList{MatcherAction}"/></param>
        /// <returns>The <see cref="HashSet{string}"/></returns>
        private HashSet<string> GetAllPossibleFieldNames(IList<MatcherAction> actions)
        {
            HashSet<string> results = new HashSet<string>();
            foreach (MatcherAction action in actions)
            {
                if (action is MatcherExtractAction extractAction)
                {
                    results.Add(extractAction.Attribute);
                }
            }
            return results;
        }

        /// <summary>
        /// Defines the <see cref="ConfigLine" />
        /// </summary>
        internal class ConfigLine
        {
            /// <summary>
            /// Defines the Type
            /// </summary>
            public enum Type
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
                EXTRACT = 0
            }

            /// <summary>
            /// Defines the type
            /// </summary>
            public readonly Type type;

            /// <summary>
            /// Defines the attribute
            /// </summary>
            public readonly string attribute;

            /// <summary>
            /// Defines the confidence
            /// </summary>
            public readonly long? confidence;

            /// <summary>
            /// Defines the expression
            /// </summary>
            public readonly string expression;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfigLine"/> class.
            /// </summary>
            /// <param name="type">The type<see cref="Type"/></param>
            /// <param name="attribute">The attribute<see cref="string"/></param>
            /// <param name="confidence">The confidence<see cref="long?"/></param>
            /// <param name="expression">The expression<see cref="string"/></param>
            public ConfigLine(Type type, string attribute, long? confidence, string expression)
            {
                this.type = type;
                this.attribute = attribute;
                this.confidence = confidence;
                this.expression = expression;
            }
        }
    }
}
