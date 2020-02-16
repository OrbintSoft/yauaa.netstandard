//-----------------------------------------------------------------------
// <copyright file="UserAgentAnalyzerDirect.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2019 Niels Basjes
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
// <date>2018, 11, 24, 12:51</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Antlr4.Runtime.Tree;
    using DomainParser.Library;
    using log4net;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Parse;
    using OrbintSoft.Yauaa.Utils;
    using YamlDotNet.Core;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Defines the <see cref="UserAgentAnalyzerDirect" />.
    /// </summary>
    [Serializable]
    public class UserAgentAnalyzerDirect : IAnalyzer
    {
        /// <summary>
        /// Defines the DEFAULT_USER_AGENT_MAX_LENGTH.
        /// </summary>
        public const int DEFAULT_USER_AGENT_MAX_LENGTH = 2048;

        /// <summary>
        /// Defines the MAX_PREFIX_HASH_MATCH.
        /// </summary>
        public const int MAX_PREFIX_HASH_MATCH = 3;

        /// <summary>
        /// Defines the MAX_PRE_HEAT_ITERATIONS.
        /// </summary>
        private const long MAX_PRE_HEAT_ITERATIONS = 1_000_000L;

        /// <summary>
        /// Defines the DefaultResources.
        /// </summary>
        private static readonly ResourcesPath DefaultResources = new ResourcesPath($@"YamlResources{Path.DirectorySeparatorChar}UserAgents", "*.yaml");

        /// <summary>
        /// Defines the HardCodedGeneratedFields.
        /// </summary>
        private static readonly IList<string> HardCodedGeneratedFields = new List<string>();

        /// <summary>
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserAgentAnalyzerDirect));

        /// <summary>
        /// Defines the informMatcherActionPrefixesLengths.
        /// </summary>
        private readonly IDictionary<string, ISet<int?>> informMatcherActionPrefixesLengths = new Dictionary<string, ISet<int?>>();

        /// <summary>
        /// Defines the informMatcherActionRanges.
        /// </summary>
        private readonly IDictionary<string, ISet<WordRangeVisitor.Range>> informMatcherActionRanges = new Dictionary<string, ISet<WordRangeVisitor.Range>>();

        /// <summary>
        /// Defines the informMatcherActions.
        /// </summary>
        private readonly IDictionary<string, ISet<MatcherAction>> informMatcherActions = new Dictionary<string, ISet<MatcherAction>>();

        /// <summary>
        /// Defines the lookupSets.
        /// </summary>
        private readonly IDictionary<string, ISet<string>> lookupSets = new Dictionary<string, ISet<string>>();

        /// <summary>
        /// A list of methers that doesn't require an input.
        /// </summary>
        private readonly MatcherList zeroInputMatchers = new MatcherList(100);

        /// <summary>
        /// Defines the delayInitialization.
        /// </summary>
        private bool delayInitialization = true;

        /// <summary>
        /// Defines the doingOnlyASingleTest.
        /// </summary>
        private bool doingOnlyASingleTest = false;

        /// <summary>
        /// Defines the lookups.
        /// </summary>
        private IDictionary<string, IDictionary<string, string>> lookups = new Dictionary<string, IDictionary<string, string>>();

        /// <summary>
        /// Defines the matcherConfigs.
        /// </summary>
        [NonSerialized]
        private IDictionary<string, IList<YamlMappingNode>> matcherConfigs = new Dictionary<string, IList<YamlMappingNode>>();

        /// <summary>
        /// Defines the matchersHaveBeenInitialized.
        /// </summary>
        private bool matchersHaveBeenInitialized = false;

        /// <summary>
        /// Defines the showMatcherStats.
        /// </summary>
        private bool showMatcherStats = false;

        /// <summary>
        /// Defines the userAgentMaxLength.
        /// </summary>
        private int userAgentMaxLength = DEFAULT_USER_AGENT_MAX_LENGTH;

        /// <summary>
        /// Defines a verbose property to enable verbose logging.
        /// </summary>
        private bool verbose = false;

        /// <summary>
        /// A list of matchers that have been touched with parsing.
        /// </summary>
        private MatcherList touchedMatchers = null;

        /// <summary>
        /// Initializes static members of the <see cref="UserAgentAnalyzerDirect"/> class.
        /// </summary>
        static UserAgentAnalyzerDirect()
        {
            HardCodedGeneratedFields.Add(UserAgent.SYNTAX_ERROR);
            HardCodedGeneratedFields.Add(UserAgent.AGENT_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
            HardCodedGeneratedFields.Add("AgentNameVersion");
            HardCodedGeneratedFields.Add("AgentNameVersionMajor");
            HardCodedGeneratedFields.Add("LayoutEngineNameVersion");
            HardCodedGeneratedFields.Add("LayoutEngineNameVersionMajor");
            HardCodedGeneratedFields.Add("OperatingSystemNameVersion");
            HardCodedGeneratedFields.Add("WebviewAppVersionMajor");
            HardCodedGeneratedFields.Add("WebviewAppNameVersionMajor");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentAnalyzerDirect"/> class.
        /// </summary>
        protected UserAgentAnalyzerDirect()
        {
        }

        /// <summary>
        /// Gets the NumberOfTestCases.
        /// </summary>
        public long NumberOfTestCases => this.TestCases.Count;

        /// <summary>
        /// Gets the TestCases.
        /// </summary>
        public IList<IDictionary<string, IDictionary<string, string>>> TestCases { get; } = new List<IDictionary<string, IDictionary<string, string>>>();

        /// <summary>
        /// Gets a value indicating whether WillKeepTests.
        /// </summary>
        public bool WillKeepTests { get; private set; } = false;

        /// <summary>
        /// Gets or sets the WantedFieldNames
        /// Defines the wantedFieldNames.
        /// </summary>
        internal List<string> WantedFieldNames { get; set; } = null;

        /// <summary>
        /// Gets the AllMatchers.
        /// </summary>
        protected IList<Matcher> AllMatchers { get; } = new List<Matcher>();

        /// <summary>
        /// Gets or sets the Flattener.
        /// </summary>
        protected UserAgentTreeFlattener Flattener { get; set; } = null;

        /// <summary>
        /// The FirstCharactersForPrefixHash.
        /// </summary>
        /// <param name="input">The input<see cref="string"/>.</param>
        /// <param name="maxChars">The maxChars<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string FirstCharactersForPrefixHash(string input, int maxChars)
        {
            return input.Substring(0, FirstCharactersForPrefixHashLength(input, maxChars));
        }

        /// <summary>
        /// The FirstCharactersForPrefixHashLength.
        /// </summary>
        /// <param name="input">The input<see cref="string"/>.</param>
        /// <param name="maxChars">The maxChars<see cref="int"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public static int FirstCharactersForPrefixHashLength(string input, int maxChars)
        {
            return Math.Min(maxChars, Math.Min(MAX_PREFIX_HASH_MATCH, input.Length));
        }

        /// <summary>
        /// The GetAllPaths.
        /// </summary>
        /// <param name="agent">The agent<see cref="string"/>.</param>
        /// <returns>The paths.</returns>
        public static IList<string> GetAllPaths(string agent)
        {
            return new GetAllPathsAnalyzerClass(agent).Values;
        }

        /// <summary>
        /// The GetAllPathsAnalyzer.
        /// </summary>
        /// <param name="agent">The agent<see cref="string"/>.</param>
        /// <returns>The <see cref="GetAllPathsAnalyzerClass"/>.</returns>
        public static GetAllPathsAnalyzerClass GetAllPathsAnalyzer(string agent)
        {
            return new GetAllPathsAnalyzerClass(agent);
        }

        /// <summary>
        /// The NewBuilder.
        /// </summary>
        /// <typeparam name="TUAA">Type of UserAgent Analyzer.</typeparam>
        /// <typeparam name="TB">Type of builder.</typeparam>
        /// <returns>The <see cref="UserAgentAnalyzerDirectBuilder{UAA, B}"/>.</returns>
        public static UserAgentAnalyzerDirectBuilder<TUAA, TB> NewBuilder<TUAA, TB>()
            where TUAA : UserAgentAnalyzerDirect, new()
            where TB : UserAgentAnalyzerDirectBuilder<TUAA, TB>, new()
        {
            var a = new TUAA();
            var b = new TB();
            b.SetUAA(a);
            return b;
        }

        /// <summary>
        /// The DelayInitialization.
        /// </summary>
        public void DelayInitialization()
        {
            this.delayInitialization = true;
        }

        /// <summary>
        /// The DropTests.
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerDirect"/>.</returns>
        public UserAgentAnalyzerDirect DropTests()
        {
            this.WillKeepTests = false;
            this.TestCases.Clear();
            return this;
        }

        /// <summary>
        /// The GetAllPossibleFieldNames.
        /// </summary>
        /// <returns>The field names/>.</returns>
        public ISet<string> GetAllPossibleFieldNames()
        {
            var results = new SortedSet<string>(HardCodedGeneratedFields);
            foreach (var matcher in this.AllMatchers)
            {
                results.UnionWith(matcher.GetAllPossibleFieldNames());
            }

            return results;
        }

        /// <summary>
        /// The GetAllPossibleFieldNamesSorted.
        /// </summary>
        /// <returns>The field names.</returns>
        public IList<string> GetAllPossibleFieldNamesSorted()
        {
            var fieldNames = new List<string>(this.GetAllPossibleFieldNames());
            fieldNames.Sort();

            var result = new List<string>();
            foreach (var fieldName in UserAgent.PreSortedFieldList)
            {
                fieldNames.Remove(fieldName);
                result.Add(fieldName);
            }

            result.AddRange(fieldNames);

            return result;
        }

        /// <summary>
        /// The GetRequiredInformRanges.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <returns>The ranges.</returns>
        public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
        {
            if (!this.informMatcherActionRanges.Keys.Contains(treeName))
            {
                this.informMatcherActionRanges[treeName] = new HashSet<WordRangeVisitor.Range>();
            }

            return this.informMatcherActionRanges[treeName];
        }

        /// <summary>
        /// The GetRequiredPrefixLengths.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <returns>The required prefix lenght.</returns>
        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            return this.informMatcherActionPrefixesLengths.ContainsKey(treeName) ? this.informMatcherActionPrefixesLengths[treeName] : null;
        }

        /// <summary>
        /// The GetUserAgentMaxLength.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetUserAgentMaxLength()
        {
            return this.userAgentMaxLength;
        }

        /// <summary>
        /// The ImmediateInitialization.
        /// </summary>
        public void ImmediateInitialization()
        {
            this.delayInitialization = false;
        }

        /// <summary>
        /// The Inform.
        /// </summary>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <param name="ctx">The ctx<see cref="IParseTree"/>.</param>
        public void Inform(string key, string value, IParseTree ctx)
        {
            this.Inform(key, key, value, ctx);
            this.Inform(key + "=\"" + value + '"', key, value, ctx);

            var lengths = this.GetRequiredPrefixLengths(key);
            if (lengths != null)
            {
                var valueLength = value.Length;
                foreach (var prefixLength in lengths)
                {
                    if (valueLength >= prefixLength)
                    {
                        this.Inform(key + "{\"" + FirstCharactersForPrefixHash(value, prefixLength.Value) + '"', key, value, ctx);
                    }
                }
            }
        }

        /// <summary>
        /// The InformMeAbout.
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
        /// <param name="keyPattern">The keyPattern<see cref="string"/>.</param>
        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            var hashKey = keyPattern.ToLower();
            if (!this.informMatcherActions.Keys.Contains(hashKey))
            {
                this.informMatcherActions[hashKey] = new HashSet<MatcherAction>();
            }

            var analyzerSet = this.informMatcherActions[hashKey];
            analyzerSet.Add(matcherAction);
        }

        /// <summary>
        /// The InformMeAboutPrefix.
        /// </summary>
        /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="prefix">The prefix<see cref="string"/>.</param>
        public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
            this.InformMeAbout(matcherAction, treeName + "{\"" + FirstCharactersForPrefixHash(prefix, MAX_PREFIX_HASH_MATCH) + "\"");

            if (!this.informMatcherActionPrefixesLengths.Keys.Contains(treeName))
            {
                this.informMatcherActionPrefixesLengths[treeName] = new HashSet<int?>();
            }

            var lengths = this.informMatcherActionPrefixesLengths[treeName];
            lengths.Add(FirstCharactersForPrefixHashLength(prefix, MAX_PREFIX_HASH_MATCH));
        }

        /// <summary>
        /// The InitializeMatchers.
        /// </summary>
        public void InitializeMatchers()
        {
            if (this.matchersHaveBeenInitialized)
            {
                return;
            }

            Log.Info("Initializing Analyzer data structures");

            if (!this.AllMatchers.Any())
            {
                throw new InvalidParserConfigurationException("No matchers were loaded at all.");
            }

            var stopwatch = Stopwatch.StartNew();
            foreach (var item in this.AllMatchers)
            {
                item.Initialize();
            }

            stopwatch.Stop();
            this.matchersHaveBeenInitialized = true;
            Log.Info($"Built in {stopwatch.ElapsedMilliseconds} msec : Hashmap {this.informMatcherActions.Count}, Ranges map:{this.informMatcherActionRanges.Count}");

            foreach (var matcher in this.AllMatchers)
            {
                if (matcher.ActionsThatRequireInput == 0)
                {
                    this.zeroInputMatchers.Add(matcher);
                }
            }

            // Reset all Matchers
            foreach (var matcher in this.AllMatchers)
            {
                matcher.Reset();
            }

            this.touchedMatchers = new MatcherList(16);
        }

        /// <summary>
        /// The IsWantedField.
        /// </summary>
        /// <param name="fieldName">The fieldName<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool IsWantedField(string fieldName)
        {
            if (this.WantedFieldNames == null)
            {
                return true;
            }

            return this.WantedFieldNames.Contains(fieldName);
        }

        /// <summary>
        /// The KeepTests.
        /// </summary>
        /// <returns>The <see cref="UserAgentAnalyzerDirect"/>.</returns>
        public UserAgentAnalyzerDirect KeepTests()
        {
            this.WillKeepTests = true;
            return this;
        }

        /// <summary>
        /// The LoadResources.
        /// </summary>
        /// <param name="resourceString">The resourceString<see cref="string"/>.</param>
        /// <param name="pattern">The pattern<see cref="string"/>.</param>
        public void LoadResources(string resourceString, string pattern = "*.yaml")
        {
            if (this.matchersHaveBeenInitialized)
            {
                throw new Exception("Refusing to load additional resources after the datastructures have been initialized.");
            }

            var filesStopwatch = Stopwatch.StartNew();

            this.Flattener = new UserAgentTreeFlattener(this);
            YamlDocument yaml;

#if VERBOSE
            IDictionary<string, FileInfo> resources = new SortedDictionary<string, FileInfo>(StringComparer.Ordinal);
#else
            IDictionary<string, FileInfo> resources = new Dictionary<string, FileInfo>();
#endif
            try
            {
                var filePaths = Directory.GetFiles(resourceString, pattern, SearchOption.TopDirectoryOnly);

                foreach (var filePath in filePaths)
                {
                    resources[Path.GetFileName(filePath)] = new FileInfo(filePath);
                }
            }
            catch (Exception e)
            {
                throw new InvalidParserConfigurationException("Error reading resources: " + e.Message, e);
            }

            this.doingOnlyASingleTest = false;
            var maxFilenameLength = 0;

            if (!resources.Any())
            {
                Log.Warn($"NO config files were found matching this expression: {resourceString}");
                Log.Error("If you are using wildcards in your expression then try explicitly naming all yamls files explicitly.");
                return;
            }

            // We need to determine if we are trying to load the yaml files TWICE.
            // This can happen if the library is loaded twice (perhaps even two different versions).
            var alreadyLoadedResourceBasenames = this.matcherConfigs.Keys.Where(r => resources.Keys.Contains(r)).ToArray();

            if (alreadyLoadedResourceBasenames.Length > 0)
            {
                Log.Error(string.Format("Trying to load these {0} resources for the second time: [{1}]", alreadyLoadedResourceBasenames.Length, string.Join(",", alreadyLoadedResourceBasenames)));
                throw new InvalidParserConfigurationException("Trying to load " + alreadyLoadedResourceBasenames.Length + " resources for the second time");
            }

            foreach (var resourceEntry in resources)
            {
                var filename = resourceEntry.Value.Name;
                try
                {
                    using (var reader = new StreamReader(resourceEntry.Value.FullName))
                    {
                        // Load the stream
                        var yamlStream = new YamlStream();
                        yamlStream.Load(reader);
                        yaml = yamlStream.Documents.FirstOrDefault();
                    }

                    if (!string.IsNullOrEmpty(filename))
                    {
                        maxFilenameLength = Math.Max(maxFilenameLength, filename.Length);
                        this.LoadResource(yaml, filename);
                    }
                }
                catch (YamlException e)
                {
                    if (e.Message.Contains("Duplicate key"))
                    {
                        throw new InvalidParserConfigurationException(e.InnerException.Message, e);
                    }
                    else
                    {
                        throw new InvalidParserConfigurationException("Parse error in the file " + filename + ": " + e.Message, e);
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidParserConfigurationException("Error reading resources: " + e.Message, e);
                }
            }

            filesStopwatch.Stop();
            var msg = $"Loading {resources.Count} files in {filesStopwatch.ElapsedMilliseconds} msec from {resourceString}";
            Log.Info(msg);

            if (!resources.Any())
            {
                throw new InvalidParserConfigurationException("No matchers were loaded at all.");
            }

            if (this.lookups != null && this.lookups.Count != 0)
            {
                // All compares are done in a case insensitive way. So we lowercase ALL keys of the lookups beforehand.
                IDictionary<string, IDictionary<string, string>> cleanedLookups = new Dictionary<string, IDictionary<string, string>>();
                foreach (var lookupsEntry in this.lookups)
                {
                    var cleanedLookup = new Dictionary<string, string>();
                    foreach (var entry in lookupsEntry.Value)
                    {
                        cleanedLookup[entry.Key.ToLower()] = entry.Value;
                    }

                    cleanedLookups[lookupsEntry.Key] = cleanedLookup;
                }

                this.lookups = cleanedLookups;
            }

            var totalNumberOfMatchers = 0;
            var skippedMatchers = 0;

            if (this.matcherConfigs != null)
            {
                var fullStopwatch = Stopwatch.StartNew();
                foreach (var resourceEntry in resources)
                {
                    var resource = resourceEntry.Value;
                    var configFilename = resource.Name;
                    IList<YamlMappingNode> matcherConfig;
                    if (this.matcherConfigs.ContainsKey(configFilename))
                    {
                        matcherConfig = this.matcherConfigs[configFilename];
                    }
                    else
                    {
                        // No matchers in this file (probably only lookups and/or tests)
                        continue;
                    }

                    var stopwatch = Stopwatch.StartNew();
                    var startSkipped = skippedMatchers;
                    foreach (var map in matcherConfig)
                    {
                        try
                        {
                            this.AllMatchers.Add(new Matcher(this, this.lookups, this.lookupSets, this.WantedFieldNames, map, configFilename));
                            totalNumberOfMatchers++;
                        }
                        catch (UselessMatcherException)
                        {
                            skippedMatchers++;
                        }
                    }

                    stopwatch.Stop();
                    var stopSkipped = skippedMatchers;

                    if (this.showMatcherStats)
                    {
                        Log.Info(
                            string.Format(
                                "Loading {0} (dropped {1}) matchers from {2} took {3} msec",
                                matcherConfig.Count - (stopSkipped - startSkipped),
                                stopSkipped - startSkipped,
                                configFilename,
                                stopwatch.ElapsedMilliseconds));
                    }
                }

                fullStopwatch.Stop();
            }
        }

        /// <summary>
        /// The LookingForRange.
        /// </summary>
        /// <param name="treeName">The treeName<see cref="string"/>.</param>
        /// <param name="range">The range<see cref="WordRangeVisitor.Range"/>.</param>
        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            if (!this.informMatcherActionRanges.Keys.Contains(treeName))
            {
                this.informMatcherActionRanges[treeName] = new HashSet<WordRangeVisitor.Range>();
            }

            this.informMatcherActionRanges[treeName].Add(range);
        }

        /// <summary>
        /// The Parse.
        /// </summary>
        /// <param name="userAgentString">The userAgentString<see cref="string"/>.</param>
        /// <returns>The <see cref="UserAgent"/>.</returns>
        public virtual UserAgent Parse(string userAgentString)
        {
            var userAgent = new UserAgent(userAgentString);
            return this.Parse(userAgent);
        }

        /// <summary>
        /// The Parse.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="UserAgent"/>.</returns>
        public virtual UserAgent Parse(UserAgent userAgent)
        {
            lock (this)
            {
                this.InitializeMatchers();
                var useragentString = userAgent.UserAgentString;
                if (useragentString != null && useragentString.Length > this.userAgentMaxLength)
                {
                    this.SetAsHacker(userAgent, 100);
                    userAgent.SetForced("HackerAttackVector", "Buffer overflow", 100);
                    return this.HardCodedPostProcessing(userAgent);
                }

                // Reset all Matchers
                foreach (var matcher in this.touchedMatchers)
                {
                    matcher.Reset();
                }

                this.touchedMatchers.Clear();

                foreach (var matcher in this.zeroInputMatchers)
                {
                    matcher.Reset();
                }

                if (userAgent.IsDebug)
                {
                    foreach (var matcher in this.AllMatchers)
                    {
                        matcher.SetVerboseTemporarily(true);
                    }
                }

                try
                {
                    userAgent = this.Flattener.Parse(userAgent);

                    // Fire all Analyzers
                    foreach (var matcher in this.touchedMatchers)
                    {
                        matcher.Analyze(userAgent);
                    }

                    // Fire all Analyzers that should not get input
                    foreach (var matcher in this.zeroInputMatchers)
                    {
                        matcher.Analyze(userAgent);
                    }

                    userAgent.ProcessSetAll();
                    return this.HardCodedPostProcessing(userAgent);
                }
                catch (NullReferenceException)
                {
                    // If this occurs then someone has found a previously undetected problem.
                    // So this is a safety for something that 'can' but 'should not' occur.
                    // I guess this exploit can work only in Java, but better to keep the code as safety measure
                    userAgent.Reset();
                    userAgent = this.SetAsHacker(userAgent, 10000);
                    userAgent.SetForced("HackerAttackVector", "Yauaa NPE Exploit", 10000);
                    return this.HardCodedPostProcessing(userAgent);
                }
            }
        }

        /// <summary>
        /// Runs all testcases once to heat up the JVM.
        /// </summary>
        /// <returns>The <see cref="long"/>.</returns>
        public long PreHeat()
        {
            return this.PreHeat(this.TestCases.Count, true);
        }

        /// <summary>
        /// Runs the number of specified testcases to heat up the CLR.
        /// </summary>
        /// <param name="preheatIterations">The preheatIterations<see cref="long"/>.</param>
        /// <returns>The <see cref="long"/>.</returns>
        public long PreHeat(long preheatIterations)
        {
            return this.PreHeat(preheatIterations, true);
        }

        /// <summary>
        /// Runs the number of specified testcases to heat up the CLR.
        /// </summary>
        /// <param name="preheatIterations">The preheatIterations<see cref="long"/>.</param>
        /// <param name="log">The log<see cref="bool"/>.</param>
        /// <returns>The <see cref="long"/>.</returns>
        public long PreHeat(long preheatIterations, bool log)
        {
            if (this.TestCases.Count == 0)
            {
                Log.Warn("NO PREHEAT WAS DONE. Simply because there are no test cases available.");
                return 0;
            }

            if (preheatIterations <= 0)
            {
                Log.Warn(string.Format("NO PREHEAT WAS DONE. Simply because you asked for {0} to run.", preheatIterations));
                return 0;
            }

            if (preheatIterations > MAX_PRE_HEAT_ITERATIONS)
            {
                Log.Warn(string.Format("NO PREHEAT WAS DONE. Simply because you asked for too many ({0} > {1}) to run.", preheatIterations, MAX_PRE_HEAT_ITERATIONS));
                return 0;
            }

            if (log)
            {
                Log.Info(string.Format("Preheating CLR by running {0} testcases.", preheatIterations));
            }

            var remainingIterations = preheatIterations;
            var goodResults = 0;
            while (remainingIterations > 0)
            {
                foreach (var test in this.TestCases)
                {
                    var input = test["input"];
                    var userAgentString = input["user_agent_string"];
                    remainingIterations--;

                    // Calculate and use result to guarantee not optimized away.
                    if (!this.Parse(userAgentString).HasSyntaxError)
                    {
                        goodResults++;
                    }

                    if (remainingIterations <= 0)
                    {
                        break;
                    }
                }
            }

            if (log)
            {
                Log.Info(string.Format("Preheating CLR completed. ({0} of {1} were proper results)", goodResults, preheatIterations));
            }

            return preheatIterations;
        }

        /// <summary>
        /// The SetShowMatcherStats.
        /// </summary>
        /// <param name="newShowMatcherStats">The newShowMatcherStats<see cref="bool"/>.</param>
        /// <returns>The <see cref="UserAgentAnalyzerDirect"/>.</returns>
        public UserAgentAnalyzerDirect SetShowMatcherStats(bool newShowMatcherStats)
        {
            this.showMatcherStats = newShowMatcherStats;
            return this;
        }

        /// <summary>
        /// The SetUserAgentMaxLength.
        /// </summary>
        /// <param name="newUserAgentMaxLength">The newUserAgentMaxLength<see cref="int"/>.</param>
        public void SetUserAgentMaxLength(int newUserAgentMaxLength)
        {
            if (newUserAgentMaxLength <= 0)
            {
                this.userAgentMaxLength = DEFAULT_USER_AGENT_MAX_LENGTH;
            }
            else
            {
                this.userAgentMaxLength = newUserAgentMaxLength;
            }
        }

        /// <summary>
        /// The SetVerbose.
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/>.</param>
        public void SetVerbose(bool newVerbose)
        {
            this.verbose = newVerbose;
            this.Flattener.SetVerbose(newVerbose);
        }

        /// <inheritdoc/>
        public void ReceivedInput(Matcher matcher)
        {
            this.touchedMatchers.Add(matcher);
        }

        /// <inheritdoc/>
        public IDictionary<string, IDictionary<string, string>> GetLookups()
        {
            return this.lookups;
        }

        /// <inheritdoc/>
        public IDictionary<string, ISet<string>> GetLookupSets()
        {
            return this.lookupSets;
        }

        /// <summary>
        /// This is used to concatenate two parsed fields into one.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <param name="targetName">The name of the new field after concatenation.</param>
        /// <param name="firstName">The name of first field you want concatenate</param>
        /// <param name="secondName">The name of the second field.</param>
        internal void ConcatFieldValuesNONDuplicated(UserAgent userAgent, string targetName, string firstName, string secondName)
        {
            if (!this.IsWantedField(targetName))
            {
                return;
            }

            var firstField = userAgent.Get(firstName);
            var secondField = userAgent.Get(secondName);

            string first = null;
            long firstConfidence = -1;
            string second = null;
            long secondConfidence = -1;

            if (firstField != null)
            {
                first = firstField.GetValue();
                firstConfidence = firstField.GetConfidence();
            }

            if (secondField != null)
            {
                second = secondField.GetValue();
                secondConfidence = secondField.GetConfidence();
            }

            if (first == null && second == null)
            {
                return; // Nothing to do
            }

            if (second == null)
            {
                if (firstConfidence >= 0)
                {
                    userAgent.Set(targetName, first, firstConfidence);
                }

                return; // Nothing to do
            }
            else
            {
                if (first == null)
                {
                    if (secondConfidence >= 0)
                    {
                        userAgent.Set(targetName, second, secondConfidence);
                    }

                    return;
                }
            }

            if (first.Equals(second))
            {
                userAgent.Set(targetName, first, firstConfidence);
            }
            else
            {
                if (second.StartsWith(first))
                {
                    userAgent.Set(targetName, second, secondConfidence);
                }
                else
                {
                    userAgent.Set(targetName, first + " " + second, Math.Max(firstField.GetConfidence(), secondField.GetConfidence()));
                }
            }
        }

        /// <summary>
        /// The Initialize.
        /// </summary>
        protected internal void Initialize()
        {
            this.Initialize(new List<ResourcesPath>() { DefaultResources });
        }

        /// <summary>
        /// The Initialize.
        /// </summary>
        /// <param name="resources">The resources<see cref="List{ResourcesPath}"/>.</param>
        protected void Initialize(IList<ResourcesPath> resources)
        {
            YauaaVersion.LogVersion();
            var fullStart = Stopwatch.StartNew();
            if (this.WantedFieldNames != null)
            {
                int wantedSize = this.WantedFieldNames.Count;
                if (this.WantedFieldNames.Contains(UserAgent.SET_ALL_FIELDS))
                {
                    wantedSize--;
                }

                Log.Info($"Building all needed matchers for the requested {wantedSize} fields.");
            }
            else
            {
                Log.Info("Building all matchers for all possible fields.");
            }

            foreach (var r in resources)
            {
                this.LoadResources(r.Directory, r.Filter);
            }

            if (!this.matcherConfigs.Any())
            {
                throw new InvalidParserConfigurationException("No matchers were loaded at all.");
            }

            fullStart.Stop();
            var fullStop = fullStart.ElapsedMilliseconds;
            var lookupsCount = (this.lookups == null) ? 0 : this.lookups.Count;
            var msg = $"Loading {this.AllMatchers.Count} matchers, {lookupsCount} lookups, {this.lookupSets.Count} lookupsets, {this.TestCases.Count} testcases from {this.matcherConfigs.Count} files took {fullStop} msec";
            Log.Info(msg);

            this.VerifyWeAreNotAskingForImpossibleFields();
            if (!this.delayInitialization)
            {
                this.InitializeMatchers();
            }
        }

        /// <summary>
        /// The VerifyWeAreNotAskingForImpossibleFields.
        /// </summary>
        protected void VerifyWeAreNotAskingForImpossibleFields()
        {
            if (this.WantedFieldNames == null)
            {
                return; //// Nothing to check
            }

            var impossibleFields = new List<string>();
            var allPossibleFields = this.GetAllPossibleFieldNamesSorted();

            foreach (var wantedFieldName in this.WantedFieldNames)
            {
                if (UserAgent.IsSystemField(wantedFieldName))
                {
                    continue; // These are fine
                }

                if (!allPossibleFields.Contains(wantedFieldName))
                {
                    impossibleFields.Add(wantedFieldName);
                }
            }

            if (impossibleFields.Count == 0)
            {
                return;
            }

            var bd = new StringBuilder();
            foreach (var item in impossibleFields)
            {
                bd.AppendFormat(" [{0}]", item);
            }

            throw new InvalidParserConfigurationException("We cannot provide these fields:" + bd.ToString());
        }

        /// <summary>
        /// The AddMajorVersionField.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <param name="versionName">The versionName<see cref="string"/>.</param>
        /// <param name="majorVersionName">The majorVersionName<see cref="string"/>.</param>
        private void AddMajorVersionField(UserAgent userAgent, string versionName, string majorVersionName)
        {
            if (!this.IsWantedField(majorVersionName))
            {
                return;
            }

            var agentVersionMajor = userAgent.Get(majorVersionName);
            if (agentVersionMajor == null || agentVersionMajor.GetConfidence() == -1)
            {
                var agentVersion = userAgent.Get(versionName);
                if (agentVersion != null)
                {
                    var version = agentVersion.GetValue();
                    if (version != null)
                    {
                        version = VersionSplitter.GetInstance().GetSingleSplit(agentVersion.GetValue(), 1);
                    }

                    userAgent.Set(
                        majorVersionName,
                        version,
                        agentVersion.GetConfidence());
                }
            }
        }

        /// <summary>
        /// The DetermineDeviceBrand.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string DetermineDeviceBrand(UserAgent userAgent)
        {
            // If no brand is known but we do have a URL then we assume the hostname to be the brand.
            // We put this AFTER the creation of the DeviceName because we choose to not have
            // this brandname in the DeviceName.
            var informationUrl = userAgent.Get("AgentInformationUrl");
            if (informationUrl != null && informationUrl.GetConfidence() >= 0)
            {
                var hostname = informationUrl.GetValue();
                try
                {
                    var url = new Uri(hostname);
                    hostname = url.Host;
                }
                catch (Exception)
                {
                    // Ignore any exception and continue.
                }

                hostname = this.ExtractCompanyFromHostName(hostname);
                if (hostname != null)
                {
                    return hostname;
                }
            }

            var informationEmail = userAgent.Get("AgentInformationEmail");
            if (informationEmail != null && informationEmail.GetConfidence() >= 0)
            {
                var hostname = informationEmail.GetValue();
                var atOffset = hostname.IndexOf('@');
                if (atOffset >= 0)
                {
                    hostname = hostname.Substring(atOffset + 1);
                }

                hostname = this.ExtractCompanyFromHostName(hostname);
                if (hostname != null)
                {
                    return hostname;
                }
            }

            return null;
        }

        /// <summary>
        /// The ExtractCompanyFromHostName.
        /// </summary>
        /// <param name="hostname">The hostname<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string ExtractCompanyFromHostName(string hostname)
        {
            if (DomainName.TryParse(hostname, out var outDomain))
            {
                return Normalize.Brand(outDomain.Domain?.ToLower());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The HardCodedPostProcessing.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <returns>The <see cref="UserAgent"/>.</returns>
        private UserAgent HardCodedPostProcessing(UserAgent userAgent)
        {
            // If it is really really bad ... then it is a Hacker.
            if ("true".Equals(userAgent.GetValue(UserAgent.SYNTAX_ERROR)))
            {
                if (userAgent.Get(UserAgent.DEVICE_CLASS).GetConfidence() == -1)
                {
                    userAgent.Set(UserAgent.DEVICE_CLASS, "Hacker", 10);
                    userAgent.Set(UserAgent.DEVICE_BRAND, "Hacker", 10);
                    userAgent.Set(UserAgent.DEVICE_NAME, "Hacker", 10);
                    userAgent.Set(UserAgent.DEVICE_VERSION, "Hacker", 10);
                    userAgent.Set(UserAgent.OPERATING_SYSTEM_CLASS, "Hacker", 10);
                    userAgent.Set(UserAgent.OPERATING_SYSTEM_NAME, "Hacker", 10);
                    userAgent.Set(UserAgent.OPERATING_SYSTEM_VERSION, "Hacker", 10);
                    userAgent.Set(UserAgent.LAYOUT_ENGINE_CLASS, "Hacker", 10);
                    userAgent.Set(UserAgent.LAYOUT_ENGINE_NAME, "Hacker", 10);
                    userAgent.Set(UserAgent.LAYOUT_ENGINE_VERSION, "Hacker", 10);
                    userAgent.Set(UserAgent.LAYOUT_ENGINE_VERSION_MAJOR, "Hacker", 10);
                    userAgent.Set(UserAgent.AGENT_CLASS, "Hacker", 10);
                    userAgent.Set(UserAgent.AGENT_NAME, "Hacker", 10);
                    userAgent.Set(UserAgent.AGENT_VERSION, "Hacker", 10);
                    userAgent.Set(UserAgent.AGENT_VERSION_MAJOR, "Hacker", 10);
                    userAgent.Set("HackerToolkit", "Unknown", 10);
                    userAgent.Set("HackerAttackVector", "Unknown", 10);
                }
            }

            // !!!!!!!!!! NOTE !!!!!!!!!!!!
            // IF YOU ADD ANY EXTRA FIELDS YOU MUST ADD THEM TO THE BUILDER TOO !!!!
            this.AddMajorVersionField(userAgent, UserAgent.AGENT_VERSION, UserAgent.AGENT_VERSION_MAJOR);
            this.AddMajorVersionField(userAgent, UserAgent.LAYOUT_ENGINE_VERSION, UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
            this.AddMajorVersionField(userAgent, "WebviewAppVersion", "WebviewAppVersionMajor");

            this.ConcatFieldValuesNONDuplicated(userAgent, "AgentNameVersion", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION);
            this.ConcatFieldValuesNONDuplicated(userAgent, "AgentNameVersionMajor", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION_MAJOR);
            this.ConcatFieldValuesNONDuplicated(userAgent, "WebviewAppNameVersionMajor", "WebviewAppName", "WebviewAppVersionMajor");
            this.ConcatFieldValuesNONDuplicated(userAgent, "LayoutEngineNameVersion", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION);
            this.ConcatFieldValuesNONDuplicated(userAgent, "LayoutEngineNameVersionMajor", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
            this.ConcatFieldValuesNONDuplicated(userAgent, "OperatingSystemNameVersion", UserAgent.OPERATING_SYSTEM_NAME, UserAgent.OPERATING_SYSTEM_VERSION);

            // The device brand field is a mess.
            var deviceBrand = userAgent.Get(UserAgent.DEVICE_BRAND);
            if (deviceBrand.GetConfidence() >= 0)
            {
                userAgent.SetForced(
                    UserAgent.DEVICE_BRAND,
                    Normalize.Brand(deviceBrand.GetValue()),
                    deviceBrand.GetConfidence());
            }

            // The email address is a mess
            var email = userAgent.Get("AgentInformationEmail");
            if (email != null && email.GetConfidence() >= 0)
            {
                userAgent.SetForced(
                    "AgentInformationEmail",
                    Normalize.Email(email.GetValue()),
                    email.GetConfidence());
            }

            if (deviceBrand.GetConfidence() < 0)
            {
                // If no brand is known then try to extract something that looks like a Brand from things like URL and Email addresses.
                var newDeviceBrand = this.DetermineDeviceBrand(userAgent);
                if (newDeviceBrand != null)
                {
                    userAgent.SetForced(UserAgent.DEVICE_BRAND, newDeviceBrand, 1);
                }
            }

            // Make sure the DeviceName always starts with the DeviceBrand
            var deviceName = userAgent.Get(UserAgent.DEVICE_NAME);
            if (deviceName.GetConfidence() >= 0)
            {
                deviceBrand = userAgent.Get(UserAgent.DEVICE_BRAND);
                var deviceNameValue = deviceName.GetValue();
                var deviceBrandValue = deviceBrand.GetValue();
                if (deviceName.GetConfidence() >= 0 &&
                    deviceBrand.GetConfidence() >= 0 &&
                    !deviceBrandValue.Equals("Unknown"))
                {
                    // In some cases it does start with the brand but without a separator following the brand
                    deviceNameValue = Normalize.CleanupDeviceBrandName(deviceBrandValue, deviceNameValue);
                }
                else
                {
                    deviceNameValue = Normalize.Brand(deviceNameValue);
                }

                userAgent.SetForced(
                    UserAgent.DEVICE_NAME,
                    deviceNameValue,
                    deviceName.GetConfidence());
            }

            return userAgent;
        }

        /// <summary>
        /// The Inform.
        /// </summary>
        /// <param name="match">The match<see cref="string"/>.</param>
        /// <param name="key">The key<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <param name="ctx">The ctx<see cref="IParseTree"/>.</param>
        private void Inform(string match, string key, string value, IParseTree ctx)
        {
            var cmatch = match.ToLower(CultureInfo.InvariantCulture);
            var relevantActions = this.informMatcherActions.ContainsKey(cmatch) ? this.informMatcherActions[cmatch] : null;
            if (this.verbose)
            {
                if (relevantActions == null)
                {
                    Log.Info(string.Format("--- Have (0): {0}", match));
                }
                else
                {
                    Log.Info(string.Format("+++ Have ({0}): {1}", relevantActions.Count, match));

                    var count = 1;
                    foreach (var action in relevantActions)
                    {
                        Log.Info(string.Format("+++ -------> ({0}): {1}", count, action.ToString()));
                        count++;
                    }
                }
            }

            if (relevantActions != null)
            {
                foreach (var matcherAction in relevantActions)
                {
                    matcherAction.Inform(key, value, ctx);
                }
            }
        }

        /// <summary>
        /// The InitTransientFields.
        /// </summary>
        private void InitTransientFields()
        {
            this.matcherConfigs = new Dictionary<string, IList<YamlMappingNode>>();
        }

        /// <summary>
        /// The LoadResource.
        /// </summary>
        /// <param name="yaml">The yaml<see cref="YamlDocument"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        private void LoadResource(YamlDocument yaml, string filename)
        {
            YamlNode loadedYaml;
            try
            {
                loadedYaml = yaml?.RootNode;
            }
            catch (Exception e)
            {
                throw new InvalidParserConfigurationException("Parse error in the file " + filename + ": " + e.Message, e);
            }

            if (loadedYaml == null)
            {
                Log.Warn($"The file {filename} is empty");
                return;
            }

            // Get and check top level config
            YamlUtils.RequireNodeInstanceOf(typeof(YamlMappingNode), loadedYaml, filename, "File must be a Map");
            var rootNode = (YamlMappingNode)loadedYaml;

            var configNodeTuple = new KeyValuePair<YamlNode, YamlNode>(null, null);
            foreach (var tuple in rootNode)
            {
                var name = YamlUtils.GetKeyAsString(tuple, filename);
                if ("config".Equals(name))
                {
                    configNodeTuple = tuple;
                    break;
                }

                if ("version".Equals(name))
                {
                    // Check the version information from the Yaml files
                    YauaaVersion.AssertSameVersion(tuple, filename);
                    return;
                }
            }

            YamlUtils.Require(configNodeTuple.Key != null, loadedYaml, filename, "The top level entry MUST be 'config'.");

            var configNode = YamlUtils.GetValueAsSequenceNode(configNodeTuple, filename);
            var configList = configNode.Children;

            foreach (var configEntry in configList)
            {
                YamlUtils.RequireNodeInstanceOf(typeof(YamlMappingNode), configEntry, filename, "The entry MUST be a mapping");

                var entry = YamlUtils.GetExactlyOneNodeTuple((YamlMappingNode)configEntry, filename);
                var actualEntry = YamlUtils.GetValueAsMappingNode(entry, filename);
                var entryType = YamlUtils.GetKeyAsString(entry, filename);
                switch (entryType)
                {
                    case "lookup":
                        this.LoadYamlLookup(actualEntry, filename);
                        break;
                    case "set":
                        this.LoadYamlLookupSets(actualEntry, filename);
                        break;
                    case "matcher":
                        this.LoadYamlMatcher(actualEntry, filename);
                        break;
                    case "test":
                        if (this.WillKeepTests)
                        {
                            this.LoadYamlTestcase(actualEntry, filename);
                        }

                        break;
                    default:
                        throw new InvalidParserConfigurationException(
                            "Yaml config.(" + filename + ":" + actualEntry.Start.Line + "): " +
                                "Found unexpected config entry: " + entryType + ", allowed are 'lookup', 'set', 'matcher' and 'test'");
                }
            }
        }

        /// <summary>
        /// The LoadYamlLookup.
        /// </summary>
        /// <param name="entry">The entry<see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        private void LoadYamlLookup(YamlMappingNode entry, string filename)
        {
            string name = null;
            IDictionary<string, string> map = null;

            foreach (var tuple in entry)
            {
                switch (YamlUtils.GetKeyAsString(tuple, filename))
                {
                    case "name":
                        name = YamlUtils.GetValueAsString(tuple, filename);
                        break;
                    case "map":
                        if (map == null)
                        {
                            map = new Dictionary<string, string>();
                        }

                        var mappings = YamlUtils.GetValueAsMappingNode(tuple, filename).ToList();
                        foreach (var mapping in mappings)
                        {
                            var key = YamlUtils.GetKeyAsString(mapping, filename);
                            var value = YamlUtils.GetValueAsString(mapping, filename);
                            map[key] = value;
                        }

                        break;
                    default:
                        break;
                }
            }

            YamlUtils.Require(name != null && map != null, entry, filename, "Invalid lookup specified");

            this.lookups[name] = map;
        }

        /// <summary>
        /// The LoadYamlLookupSets.
        /// </summary>
        /// <param name="entry">The entry<see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        private void LoadYamlLookupSets(YamlMappingNode entry, string filename)
        {
            string name = null;
            var lookupSet = new HashSet<string>();

            foreach (var tuple in entry)
            {
                switch (YamlUtils.GetKeyAsString(tuple, filename))
                {
                    case "name":
                        name = YamlUtils.GetValueAsString(tuple, filename);
                        break;
                    case "values":
                        var node = YamlUtils.GetValueAsSequenceNode(tuple, filename);
                        foreach (var value in YamlUtils.GetStringValues(node, filename))
                        {
                            lookupSet.Add(value.ToLower(CultureInfo.InvariantCulture));
                        }

                        break;
                    default:
                        break;
                }
            }

            this.lookupSets[name] = lookupSet;
        }

        /// <summary>
        /// The LoadYamlMatcher.
        /// </summary>
        /// <param name="entry">The entry<see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        private void LoadYamlMatcher(YamlMappingNode entry, string filename)
        {
            var matcherConfigList = this.matcherConfigs.FirstOrDefault(e => e.Key == filename).Value;
            if (matcherConfigList == null)
            {
                matcherConfigList = this.matcherConfigs[filename] = new List<YamlMappingNode>();
            }

            matcherConfigList.Add(entry);
        }

        /// <summary>
        /// The LoadYamlTestcase.
        /// </summary>
        /// <param name="entry">The entry<see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        private void LoadYamlTestcase(YamlMappingNode entry, string filename)
        {
            if (!this.doingOnlyASingleTest)
            {
                var metaData = new Dictionary<string, string>
                {
                    ["filename"] = filename,
                    ["fileline"] = Convert.ToString(entry.Start.Line),
                };

                IDictionary<string, string> input = null;
                IList<string> options = null;
                var expected = new Dictionary<string, string>();
                foreach (var tuple in entry)
                {
                    var name = YamlUtils.GetKeyAsString(tuple, filename);
                    switch (name)
                    {
                        case "options":
                            options = YamlUtils.GetStringValues(tuple.Value, filename);
                            if (options.Contains("only"))
                            {
                                this.doingOnlyASingleTest = true;
                                this.TestCases.Clear();
                            }

                            break;
                        case "input":
                            foreach (var inputTuple in YamlUtils.GetValueAsMappingNode(tuple, filename))
                            {
                                var inputName = YamlUtils.GetKeyAsString(inputTuple, filename);
                                switch (inputName)
                                {
                                    case "user_agent_string":
                                        var inputString = YamlUtils.GetValueAsString(inputTuple, filename);
                                        input = new Dictionary<string, string>
                                        {
                                            [inputName] = inputString,
                                        };
                                        break;
                                    default:
                                        break;
                                }
                            }

                            break;
                        case "expected":
                            var mappings = YamlUtils.GetValueAsMappingNode(tuple, filename).ToList();
                            foreach (var mapping in mappings)
                            {
                                var key = YamlUtils.GetKeyAsString(mapping, filename);
                                var value = YamlUtils.GetValueAsString(mapping, filename);
                                expected[key] = value;
                            }

                            break;
                        default:

                            // ignore, skip
                            break;
                    }
                }

                YamlUtils.Require(input != null, entry, filename, "Test is missing input");

                if (expected.Count == 0)
                {
                    this.doingOnlyASingleTest = true;
                    this.TestCases.Clear();
                }

                IDictionary<string, IDictionary<string, string>> testCase = new Dictionary<string, IDictionary<string, string>>
                {
                    ["input"] = input,
                };
                if (expected.Count > 0)
                {
                    testCase["expected"] = expected;
                }

                if (options != null)
                {
                    var optionsMap = new Dictionary<string, string>();
                    foreach (var option in options)
                    {
                        optionsMap[option] = option;
                    }

                    testCase["options"] = optionsMap;
                }

                testCase["metaData"] = metaData;
                this.TestCases.Add(testCase);
            }
        }

        /// <summary>
        /// The ReadObject.
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/>.</param>
        private void ReadObject(Stream stream)
        {
            this.InitTransientFields();

            var lines = new List<string>
            {
                "This Analyzer instance was deserialized.",
                string.Empty,
                "Lookups      : " + ((this.lookups == null) ? 0 : this.lookups.Count),
                "LookupSets   : " + this.lookupSets.Count,
                "Matchers     : " + this.AllMatchers.Count,
                "Hashmap size : " + this.informMatcherActions.Count,
                "Testcases    : " + this.TestCases.Count,
            };

            string[] x = { };
            YauaaVersion.LogVersion(lines.ToArray());
        }

        /// <summary>
        /// The SetAsHacker.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        /// <param name="confidence">The confidence<see cref="int"/>.</param>
        /// <returns>The <see cref="UserAgent"/>.</returns>
        private UserAgent SetAsHacker(UserAgent userAgent, int confidence)
        {
            userAgent.Set(UserAgent.DEVICE_CLASS, "Hacker", confidence);
            userAgent.Set(UserAgent.DEVICE_BRAND, "Hacker", confidence);
            userAgent.Set(UserAgent.DEVICE_NAME, "Hacker", confidence);
            userAgent.Set(UserAgent.DEVICE_VERSION, "Hacker", confidence);
            userAgent.Set(UserAgent.OPERATING_SYSTEM_CLASS, "Hacker", confidence);
            userAgent.Set(UserAgent.OPERATING_SYSTEM_NAME, "Hacker", confidence);
            userAgent.Set(UserAgent.OPERATING_SYSTEM_VERSION, "Hacker", confidence);
            userAgent.Set(UserAgent.LAYOUT_ENGINE_CLASS, "Hacker", confidence);
            userAgent.Set(UserAgent.LAYOUT_ENGINE_NAME, "Hacker", confidence);
            userAgent.Set(UserAgent.LAYOUT_ENGINE_VERSION, "Hacker", confidence);
            userAgent.Set(UserAgent.LAYOUT_ENGINE_VERSION_MAJOR, "Hacker", confidence);
            userAgent.Set(UserAgent.AGENT_CLASS, "Hacker", confidence);
            userAgent.Set(UserAgent.AGENT_NAME, "Hacker", confidence);
            userAgent.Set(UserAgent.AGENT_VERSION, "Hacker", confidence);
            userAgent.Set(UserAgent.AGENT_VERSION_MAJOR, "Hacker", confidence);
            userAgent.Set("HackerToolkit", "Unknown", confidence);
            userAgent.Set("HackerAttackVector", "Buffer overflow", confidence);
            return userAgent;
        }

        /// <summary>
        /// Defines the <see cref="GetAllPathsAnalyzerClass" />.
        /// </summary>
        public class GetAllPathsAnalyzerClass : IAnalyzer
        {
            /// <summary>
            /// Defines the flattener.
            /// </summary>
            private readonly UserAgentTreeFlattener flattener;

            /// <summary>
            /// Defines the result.
            /// </summary>
            private readonly UserAgent result;

            /// <summary>
            /// Defines the values.
            /// </summary>
            private readonly IList<string> values = new List<string>();

            /// <summary>
            /// Initializes a new instance of the <see cref="GetAllPathsAnalyzerClass"/> class.
            /// </summary>
            /// <param name="useragent">The useragent<see cref="string"/>.</param>
            internal GetAllPathsAnalyzerClass(string useragent)
            {
                this.flattener = new UserAgentTreeFlattener(this);
                this.result = this.flattener.Parse(useragent);
            }

            /// <summary>
            /// Gets the Result.
            /// </summary>
            public UserAgent Result => this.result;

            /// <summary>
            /// Gets the Values.
            /// </summary>
            public IList<string> Values => this.values;

            public IDictionary<string, IDictionary<string, string>> GetLookups()
            {
                return new Dictionary<string, IDictionary<string, string>>();
            }

            public IDictionary<string, ISet<string>> GetLookupSets()
            {
                return new Dictionary<string, ISet<string>>();
            }

            /// <summary>
            /// The GetRequiredInformRanges.
            /// </summary>
            /// <param name="treeName">The treeName<see cref="string"/>.</param>
            /// <returns>The ranges.</returns>
            public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
            {
                // Not needed to only get all paths
                return new HashSet<WordRangeVisitor.Range>();
            }

            /// <summary>
            /// The GetRequiredPrefixLengths.
            /// </summary>
            /// <param name="treeName">The treeName<see cref="string"/>.</param>
            /// <returns>The prefix lengths.</returns>
            public ISet<int?> GetRequiredPrefixLengths(string treeName)
            {
                // Not needed to only get all paths
                return new HashSet<int?>();
            }

            /// <summary>
            /// The Inform.
            /// </summary>
            /// <param name="path">The path<see cref="string"/>.</param>
            /// <param name="value">The value<see cref="string"/>.</param>
            /// <param name="ctx">The ctx<see cref="IParseTree"/>.</param>
            public void Inform(string path, string value, IParseTree ctx)
            {
                this.values.Add(path);
                this.values.Add(path + "=\"" + value + "\"");
                this.values.Add(path + "{\"" + FirstCharactersForPrefixHash(value, MAX_PREFIX_HASH_MATCH) + "\"");
            }

            /// <summary>
            /// The InformMeAbout.
            /// </summary>
            /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
            /// <param name="keyPattern">The keyPattern<see cref="string"/>.</param>
            public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
            {
            }

            /// <summary>
            /// The InformMeAboutPrefix.
            /// </summary>
            /// <param name="matcherAction">The matcherAction<see cref="MatcherAction"/>.</param>
            /// <param name="treeName">The treeName<see cref="string"/>.</param>
            /// <param name="prefix">The prefix<see cref="string"/>.</param>
            public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
            {
            }

            /// <summary>
            /// The LookingForRange.
            /// </summary>
            /// <param name="treeName">The treeName<see cref="string"/>.</param>
            /// <param name="range">The range<see cref="WordRangeVisitor.Range"/>.</param>
            public void LookingForRange(string treeName, WordRangeVisitor.Range range)
            {
            }

            /// <inheritdoc/>
            public void ReceivedInput(Matcher matcher)
            {
            }
        }

        /// <summary>
        /// Defines the <see cref="UserAgentAnalyzerDirectBuilder{UAA, B}" />.
        /// </summary>
        /// <typeparam name="TUAA">the UserAgent Analyzer.</typeparam>
        /// <typeparam name="TB">the Builder.</typeparam>
        public class UserAgentAnalyzerDirectBuilder<TUAA, TB>
            where TUAA : UserAgentAnalyzerDirect
            where TB : UserAgentAnalyzerDirectBuilder<TUAA, TB>
        {
            /// <summary>
            /// Defines the resources.
            /// </summary>
            private readonly IList<ResourcesPath> resources = new List<ResourcesPath>();

            /// <summary>
            /// Defines the didBuildStep.
            /// </summary>
            private bool didBuildStep = false;

            /// <summary>
            /// Defines the preheatIterations.
            /// </summary>
            private int preheatIterations = 0;

            /// <summary>
            /// Defines the uaa.
            /// </summary>
            private TUAA uaa;

            /// <summary>
            /// Initializes a new instance of the <see cref="UserAgentAnalyzerDirectBuilder{UAA, B}"/> class.
            /// </summary>
            /// <param name="newUaa">The newUaa.</param>
            protected UserAgentAnalyzerDirectBuilder(TUAA newUaa)
            {
                this.uaa = newUaa;
                this.uaa.SetShowMatcherStats(false);
                this.resources.Add(DefaultResources);
            }

            /// <summary>
            /// Add a set of additional rules. Useful in handling specific cases.
            /// </summary>
            /// <param name="resourceString">resourceString The dirctory that contains the resources list that needs to be added.</param>
            /// <param name="filter">The filter<see cref="string"/>.</param>
            /// <returns>the current Builder instance.</returns>
            public TB AddResources(string resourceString, string filter = "*.yaml")
            {
                this.FailIfAlreadyBuilt();
                this.resources.Add(new ResourcesPath(resourceString, filter));
                return (TB)this;
            }

            /// <summary>
            /// Construct the analyzer and run the preheat (if requested).
            /// </summary>
            /// <returns>The User Agent Analyzer.</returns>
            public virtual TUAA Build()
            {
                this.FailIfAlreadyBuilt();
                if (this.uaa.WantedFieldNames != null)
                {
                    this.AddGeneratedFields("AgentNameVersion", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION);
                    this.AddGeneratedFields("AgentNameVersionMajor", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION_MAJOR);
                    this.AddGeneratedFields("WebviewAppNameVersionMajor", "WebviewAppName", "WebviewAppVersionMajor");
                    this.AddGeneratedFields("LayoutEngineNameVersion", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION);
                    this.AddGeneratedFields("LayoutEngineNameVersionMajor", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
                    this.AddGeneratedFields("OperatingSystemNameVersion", UserAgent.OPERATING_SYSTEM_NAME, UserAgent.OPERATING_SYSTEM_VERSION);
                    this.AddGeneratedFields(UserAgent.DEVICE_NAME, UserAgent.DEVICE_BRAND);
                    this.AddGeneratedFields(UserAgent.AGENT_VERSION_MAJOR, UserAgent.AGENT_VERSION);
                    this.AddGeneratedFields(UserAgent.LAYOUT_ENGINE_VERSION_MAJOR, UserAgent.LAYOUT_ENGINE_VERSION);
                    this.AddGeneratedFields("WebviewAppVersionMajor", "WebviewAppVersion");

                    // If we do not have a Brand we try to extract it from URL/Email iff present.
                    this.AddGeneratedFields(UserAgent.DEVICE_BRAND, "AgentInformationUrl", "AgentInformationEmail");

                    // Special field that affects ALL fields.
                    this.uaa.WantedFieldNames.Add(UserAgent.SET_ALL_FIELDS);

                    // This is always needed to determine the Hacker fallback
                    this.uaa.WantedFieldNames.Add(UserAgent.DEVICE_CLASS);
                }

                var mustDropTestsLater = !this.uaa.WillKeepTests;
                if (this.preheatIterations != 0)
                {
                    this.uaa.KeepTests();
                }

                this.uaa.Initialize(this.resources);
                if (this.preheatIterations < 0)
                {
                    this.uaa.PreHeat();
                }
                else
                {
                    if (this.preheatIterations > 0)
                    {
                        this.uaa.PreHeat(this.preheatIterations);
                    }
                }

                if (mustDropTestsLater)
                {
                    this.uaa.DropTests();
                }

                this.didBuildStep = true;
                return this.uaa;
            }

            /// <summary>
            /// Load all patterns and rules but do not yet build the lookup hashMaps yet.
            /// For the engine to run these lookup hashMaps are needed so they will be constructed once "just in time".
            /// </summary>
            /// <returns>The builder.</returns>
            public TB DelayInitialization()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.DelayInitialization();
                return (TB)this;
            }

            /// <summary>
            /// Drop the default set of rules. Useful in parsing ONLY company specific useragents.
            /// </summary>
            /// <returns>the current Builder instance.</returns>
            public TB DropDefaultResources()
            {
                this.FailIfAlreadyBuilt();
                this.resources.Remove(DefaultResources);
                return (TB)this;
            }

            /// <summary>
            /// Remove all testcases in memory after initialization.
            /// </summary>
            /// <returns>The builder.</returns>
            public TB DropTests()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.DropTests();
                return (TB)this;
            }

            /// <summary>
            /// Set the stats logging during the startup of the analyzer back to the default of "minimal".
            /// </summary>
            /// <returns>The builder.</returns>
            public TB HideMatcherLoadStats()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.SetShowMatcherStats(false);
                return (TB)this;
            }

            /// <summary>
            /// Load all patterns and rules and immediately build the lookup hashMaps.
            /// </summary>
            /// <returns>the current Builder instance.</returns>
            public TB ImmediateInitialization()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.ImmediateInitialization();
                return (TB)this;
            }

            /// <summary>
            /// Retain all testcases in memory after initialization.
            /// </summary>
            /// <returns>The builder.</returns>
            public TB KeepTests()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.KeepTests();
                return (TB)this;
            }

            /// <summary>
            /// Use the available testcases to preheat the jvm on this analyzer.
            /// All available testcases will be run exactly once.
            /// </summary>
            /// <returns>the current Builder instance.</returns>
            public TB Preheat()
            {
                this.FailIfAlreadyBuilt();
                this.preheatIterations = -1;
                return (TB)this;
            }

            /// <summary>
            /// Use the available testcases to preheat the jvm on this analyzer.
            /// </summary>
            /// <param name="iterations">iterations How many testcases must be run.</param>
            /// <returns>the current Builder instance.</returns>
            public TB Preheat(int iterations)
            {
                this.FailIfAlreadyBuilt();
                this.preheatIterations = iterations;
                return (TB)this;
            }

            /// <summary>
            /// Log additional information during the startup of the analyzer.
            /// </summary>
            /// <returns>The builder.</returns>
            public TB ShowMatcherLoadStats()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.SetShowMatcherStats(true);
                return (TB)this;
            }

            /// <summary>
            /// Specify that we simply want to retrieve all possible fields.
            /// </summary>
            /// <returns>The builder.</returns>
            public TB WithAllFields()
            {
                this.FailIfAlreadyBuilt();
                this.uaa.WantedFieldNames = null;
                return (TB)this;
            }

            /// <summary>
            /// Specify an additional field that we want to retrieve.
            /// </summary>
            /// <param name="fieldName">The name of the additional field.</param>
            /// <returns>the current Builder instance.</returns>
            public TB WithField(string fieldName)
            {
                this.FailIfAlreadyBuilt();
                if (this.uaa.WantedFieldNames == null)
                {
                    this.uaa.WantedFieldNames = new List<string>();
                }

                if (!this.uaa.WantedFieldNames.Contains(fieldName))
                {
                    this.uaa.WantedFieldNames.Add(fieldName);
                }

                return (TB)this;
            }

            /// <summary>
            /// Specify a set of additional fields that we want to retrieve.
            /// </summary>
            /// <param name="fieldNames">The collection of names of the additional fields.</param>
            /// <returns>the current Builder instance.</returns>
            public TB WithFields(ICollection<string> fieldNames)
            {
                foreach (var fieldName in fieldNames)
                {
                    this.WithField(fieldName);
                }

                return (TB)this;
            }

            /// <summary>
            /// Specify a set of additional fields that we want to retrieve.
            /// </summary>
            /// <param name="fieldNames">The fieldNames.</param>
            /// <returns>the current Builder instance.</returns>
            public TB WithFields(params string[] fieldNames)
            {
                foreach (var fieldName in fieldNames)
                {
                    this.WithField(fieldName);
                }

                return (TB)this;
            }

            /// <summary>
            /// Set maximum length of a useragent for it to be classified as Hacker without any analysis.
            /// </summary>
            /// <param name="newUserAgentMaxLength">The newUserAgentMaxLength<see cref="int"/>.</param>
            /// <returns>The builder.</returns>
            public TB WithUserAgentMaxLength(int newUserAgentMaxLength)
            {
                this.FailIfAlreadyBuilt();
                this.uaa.SetUserAgentMaxLength(newUserAgentMaxLength);
                return (TB)this;
            }

            /// <summary>
            /// The SetUAA.
            /// </summary>
            /// <param name="a">The User Agent Analyzer.</param>
            internal void SetUAA(TUAA a)
            {
                this.uaa = a;
            }

            /// <summary>
            /// The FailIfAlreadyBuilt.
            /// </summary>
            protected void FailIfAlreadyBuilt()
            {
                if (this.didBuildStep)
                {
                    throw new Exception("A builder can provide only a single instance. It is not allowed to set values after doing build()");
                }
            }

            /// <summary>
            /// The AddGeneratedFields.
            /// </summary>
            /// <param name="result">The result<see cref="string"/>.</param>
            /// <param name="dependencies">The dependencies.</param>
            private void AddGeneratedFields(string result, params string[] dependencies)
            {
                if (this.uaa.WantedFieldNames.Contains(result))
                {
                    this.uaa.WantedFieldNames.AddRange(dependencies);
                }
            }
        }
    }
}
