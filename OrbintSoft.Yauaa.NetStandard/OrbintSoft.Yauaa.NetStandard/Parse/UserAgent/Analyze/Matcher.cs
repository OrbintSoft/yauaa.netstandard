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
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze
{
    public class Matcher 
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Matcher));

        private readonly IAnalyzer analyzer;
        private readonly List<MatcherVariableAction> variableActions;
        private List<MatcherAction> dynamicActions;
        private readonly List<MatcherAction> fixedStringActions;

        private readonly UserAgent newValuesUserAgent = new UserAgent();

        private long actionsThatRequireInput;
        private readonly Dictionary<string, Dictionary<string, string>> lookups;
        private readonly Dictionary<string, HashSet<string>> lookupSets;
        private bool verbose;
        private readonly bool permanentVerbose;

        // Used for error reporting: The filename and line number where the config was located.
        private readonly string matcherSourceLocation;

        // Package private constructor for testing purposes only
        internal Matcher(IAnalyzer analyzer, Dictionary<string, Dictionary<string, string>> lookups, Dictionary<string, HashSet<string>> lookupSets)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.analyzer = analyzer;
            fixedStringActions = new List<MatcherAction>();
            variableActions = new List<MatcherVariableAction>();
            dynamicActions = new List<MatcherAction>();
        }

        public Dictionary<string, Dictionary<string, string>> GetLookups()
        {
            return lookups;
        }

        public Dictionary<string, HashSet<string>> GetLookupSets()
        {
            return lookupSets;
        }

        internal class ConfigLine
        {
            public enum Type
            {
                VARIABLE,
                REQUIRE,
                EXTRACT
            }
            public readonly Type type;
            public readonly string attribute;
            public readonly long? confidence;
            public readonly string expression;

            public ConfigLine(Type type, string attribute, long? confidence, string expression)
            {
                this.type = type;
                this.attribute = attribute;
                this.confidence = confidence;
                this.expression = expression;
            }
        }

        public Matcher(IAnalyzer analyzer, Dictionary<string, Dictionary<string, string>> lookups, Dictionary<string, HashSet<string>> lookupSets, List<string> wantedFieldNames, YamlMappingNode matcherConfig, string filename)
        {
            this.lookups = lookups;
            this.lookupSets = lookupSets;
            this.analyzer = analyzer;
            fixedStringActions = new List<MatcherAction>();
            variableActions = new List<MatcherVariableAction>();
            dynamicActions = new List<MatcherAction>();

            matcherSourceLocation = filename + ':' + matcherConfig.Start.Line;

            verbose = false;

            bool hasActiveExtractConfigs = false;
            bool hasDefinedExtractConfigs = false;

            // List of 'attribute', 'confidence', 'expression'
            List<ConfigLine> configLines = new List<ConfigLine>(16);
            foreach (KeyValuePair<YamlNode,YamlNode> nodeTuple in matcherConfig.ToList()) {
                string name = YamlUtils.GetKeyAsString(nodeTuple, matcherSourceLocation);
                switch (name) {
                    case "options":
                        List<string> options = YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation);
                        if (options != null) {
                            verbose = options.Contains("verbose");
                        }
                        break;
                    case "variable":
                        foreach (string variableConfig in YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation)) {
                            string[] configParts = variableConfig.Split(new Char[] { ':' }, 2);

                            if (configParts.Length != 2) {
                                throw new InvalidParserConfigurationException("Invalid variable config line: " + variableConfig);
                        }
                        string variableName = configParts[0].Trim();
                        string config = configParts[1].Trim();

                        configLines.Add(new ConfigLine(ConfigLine.Type.VARIABLE, variableName, null, config));
                        }
                        break;
                    case "require":
                        foreach (string requireConfig in YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation)) {
                            configLines.Add(new ConfigLine(ConfigLine.Type.REQUIRE, null, null, requireConfig));
                        }
                        break;
                    case "extract":
                        foreach (string extractConfig in YamlUtils.GetStringValues(nodeTuple.Value, matcherSourceLocation)) {
                            string[] configParts = extractConfig.Split(new Char[] { ':' }, 3);

                            if (configParts.Length != 3) {
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
                            if (wantedFieldNames == null || wantedFieldNames.Contains(attribute)) {
                                configLines.Add(new ConfigLine(ConfigLine.Type.EXTRACT, attribute, confidence, config));
                                hasActiveExtractConfigs = true;
                            } else {
                                configLines.Add(new ConfigLine(ConfigLine.Type.REQUIRE, null, null, config));
                            }
                        }
                        break;
                    default:
                        break;
                        // Ignore
                        //fail(nodeTuple.getKeyNode(), matcherSourceLocation, "Unexpected " + name);
                }
            }

            permanentVerbose = verbose;

            if (verbose) {
                LOG.Info("---------------------------");
                LOG.Info("- MATCHER -");
            }

            if (!hasDefinedExtractConfigs) {
                throw new InvalidParserConfigurationException("Matcher does not extract anything");
            }

            if (!hasActiveExtractConfigs) {
                throw new UselessMatcherException("Does not extract any wanted fields");
            }

            foreach (ConfigLine configLine in configLines) {
                if (verbose) {
                    LOG.Info(string.Format("{0}: {1}", configLine.type, configLine.expression));
                }
                switch (configLine.type) {
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

            fixedStringActions.Clear();
            uselessRequireActions.Clear();

            // Verify that a variable only contains the variables that have been defined BEFORE it (also not referencing itself).
            // If all is ok we link them
            HashSet<MatcherAction> seenVariables = new HashSet<MatcherAction>();
            foreach (MatcherVariableAction variableAction in variableActions)
            {
                seenVariables.Add(variableAction); // Add myself
                HashSet<MatcherAction> interestedActions = informMatcherActionsAboutVariables[variableAction.GetVariableName()];
                if (interestedActions != null && interestedActions.Count > 0)
                {
                    variableAction.SetInterestedActions(interestedActions);
                    foreach (MatcherAction interestedAction in interestedActions)
                    {
                        if (seenVariables.Contains(interestedAction))
                        {
                            throw new InvalidParserConfigurationException("Syntax error: The line >>" + interestedAction + "<< " +
                                "is referencing variable @" + variableAction.GetVariableName() + " which is not defined yet.");
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
                LOG.Info("---------------------------");
            }
        }

        private long CountActionsThatMustHaveMatches(List<MatcherAction> actions)
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

        public ISet<string> GetAllPossibleFieldNames()
        {
            ISet<string> results = new SortedSet<string>();
            results.UnionWith(GetAllPossibleFieldNames(dynamicActions));
            results.UnionWith(GetAllPossibleFieldNames(fixedStringActions));
            results.Remove(UserAgent.SET_ALL_FIELDS);
            return results;
        }

        private HashSet<string> GetAllPossibleFieldNames(List<MatcherAction> actions)
        {
            HashSet<string> results = new HashSet<string>();
            foreach (MatcherAction action in actions)
            {
                if (action is MatcherExtractAction extractAction)
                {
                    results.Add(extractAction.GetAttribute());
                }
            }
            return results;
        }

        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            analyzer.LookingForRange(treeName, range);
        }

        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            analyzer.InformMeAbout(matcherAction, keyPattern);
        }

        public void InformMeAboutPrefix(MatcherAction matcherAction, string keyPattern, string prefix)
        {
            analyzer.InformMeAboutPrefix(matcherAction, keyPattern, prefix);
        }

        private static Dictionary<string, HashSet<MatcherAction>> informMatcherActionsAboutVariables = new Dictionary<string, HashSet<MatcherAction>>();

        internal void InformMeAboutVariable(MatcherAction matcherAction, string variableName)
        {
            if (!informMatcherActionsAboutVariables.ContainsKey(variableName))
            {
                ISet<MatcherAction> analyzerSet = new HashSet<MatcherAction>
                {
                    matcherAction
                };
            }
        }

        /// <summary>
        /// Fires all matcher actions.
        /// IFF all success then we tell the userAgent
        /// </summary>
        /// <param name="userAgent">userAgent The useragent that needs to analyzed</param>
        public void Analyze(UserAgent userAgent)
        {

            if (verbose)
            {
                LOG.Info("");
                LOG.Info("--- Matcher ------------------------");
                LOG.Info("ANALYSE ----------------------------");
                bool good = true;
                foreach (MatcherAction action in dynamicActions)
                {
                    if (action.CannotBeValid())
                    {
                        LOG.Error(string.Format("CANNOT BE VALID : {0}", action.GetMatchExpression()));
                        good = false;
                    }
                }
                foreach (MatcherAction action in dynamicActions)
                {
                    if (!action.ObtainResult())
                    {
                        LOG.Error(string.Format("FAILED : {0}", action.GetMatchExpression()));
                        good = false;
                    }
                }
                if (good)
                {
                    LOG.Info("COMPLETE ----------------------------");
                }
                else
                {
                    LOG.Info("INCOMPLETE ----------------------------");
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

        public bool GetVerbose()
        {
            return verbose;
        }

        private long actionsThatRequireInputAndReceivedInput = 0;


        internal void GotMyFirstStartingPoint()
        {
            actionsThatRequireInputAndReceivedInput++;
        }

        public void SetVerboseTemporarily(bool newVerbose)
        {
            foreach (MatcherAction action in dynamicActions)
            {
                action.SetVerbose(newVerbose, true);
            }
        }

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

        public List<MatchesList.Match> GetMatches()
        {
            List<MatchesList.Match> allMatches = new List<MatchesList.Match>();
            foreach (MatcherAction action in dynamicActions)
            {
                allMatches.AddRange(action.GetMatches());
            }
            return allMatches;
        }

        public List<MatchesList.Match> GetUsedMatches()
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(512);
            sb.Append("MATCHER.(").Append(matcherSourceLocation).Append("):\n").Append("    VARIABLE:\n");
            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherVariableAction)
                {
                    sb.Append("        @").Append(((MatcherVariableAction)action).GetVariableName())
                        .Append(":    ").Append(action.GetMatchExpression()).Append('\n');
                    sb.Append("        -->").Append(action.GetMatches().ToStrings()).Append('\n');
                }
            }
            sb.Append("    REQUIRE:\n");
            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherRequireAction)
                {
                    sb.Append("        ").Append(action.GetMatchExpression()).Append('\n');
                    sb.Append("        -->").Append(action.GetMatches().ToStrings()).Append('\n');
                }
            }
            sb.Append("    EXTRACT:\n");
            foreach (MatcherAction action in dynamicActions)
            {
                if (action is MatcherExtractAction)
                {
                    sb.Append("        ").Append(action.ToString()).Append('\n');
                    sb.Append("        -->").Append(action.GetMatches().ToStrings()).Append('\n');
                }
            }
            foreach (MatcherAction action in fixedStringActions)
            {
                sb.Append("        ").Append(action.ToString()).Append('\n');
            }
            return sb.ToString();
        }
    }
}
