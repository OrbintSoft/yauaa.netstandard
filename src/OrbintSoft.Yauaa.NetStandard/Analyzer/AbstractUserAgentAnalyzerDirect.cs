//-----------------------------------------------------------------------
// <copyright file="AbstractUserAgentAnalyzerDirect.cs" company="OrbintSoft">
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
// <date>2020, 06, 07, 18:05</date>
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
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Calculate;
    using OrbintSoft.Yauaa.Logger;
    using OrbintSoft.Yauaa.Parse;
    using OrbintSoft.Yauaa.Utils;
    using YamlDotNet.Core;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Abstract implementation of direct user agent analyzer.
    /// </summary>
    [Serializable]
    public abstract class AbstractUserAgentAnalyzerDirect : IAnalyzer
    {
        /// <summary>
        /// Defines the user agent max lenght.
        /// </summary>
        public const int DEFAULT_USER_AGENT_MAX_LENGTH = 2048;

        /// <summary>
        /// Defines the max prefix hash match.
        /// </summary>
        public const int MAX_PREFIX_HASH_MATCH = 3;

        /// <summary>
        /// Defines the default resources path to be loaded.
        /// </summary>
        protected internal static readonly ResourcesPath DefaultResources = new ResourcesPath($@"YamlResources{Path.DirectorySeparatorChar}UserAgents", "*.yaml");

        /// <summary>
        /// Defines the max iterations that can be used for preheat.
        /// </summary>
        private const long MAX_PRE_HEAT_ITERATIONS = 1_000_000L;

        // We set this to 1000000 always.
        // Why?
        // At the time of writing this the actual HashMap size needed about 410K entries.
        // To keep the bins small the load factor of 0.75 already puts us at the capacity of 1048576
        private const int INFORM_ACTIONS_HASHMAP_CAPACITY = 1000000;

        /// <summary>
        /// Defines the Logger.
        /// </summary>
        private static readonly ILogger Logger = new Logger<AbstractUserAgentAnalyzerDirect>();

        /// <summary>
        /// A list of hardcoded generated fields.
        /// </summary>
        private static readonly IList<string> HardCodedGeneratedFields = new List<string>();

        /// <summary>
        /// A list of methers that doesn't require an input.
        /// </summary>
        private readonly MatcherList zeroInputMatchers = new MatcherList(100);

        /// <summary>
        /// Defines the inform matchers.
        /// </summary>
        private readonly IDictionary<string, ISet<MatcherAction>> informMatcherActions = new Dictionary<string, ISet<MatcherAction>>(INFORM_ACTIONS_HASHMAP_CAPACITY);

        /// <summary>
        /// The configuraion dictionary of the matchers that have been loaded.
        /// </summary>
        [NonSerialized]
        private readonly IDictionary<string, IList<YamlMappingNode>> matcherConfigs = new Dictionary<string, IList<YamlMappingNode>>();

        /// <summary>
        /// Defines the lookup sets.
        /// </summary>
        private readonly IDictionary<string, ISet<string>> lookupSets = new Dictionary<string, ISet<string>>(128);

        /// <summary>
        /// Defines the inform matcher action ranges.
        /// </summary>
        private readonly IDictionary<string, ISet<WordRangeVisitor.Range>> informMatcherActionRanges = new Dictionary<string, ISet<WordRangeVisitor.Range>>();

        /// <summary>
        /// Defines the inform matcher action prefix lenghts.
        /// </summary>
        private readonly IDictionary<string, ISet<int?>> informMatcherActionPrefixesLengths = new Dictionary<string, ISet<int?>>();

        /// <summary>
        /// True if you want log the statistics.
        /// </summary>
        private bool showMatcherStats = false;

        /// <summary>
        /// True if tyou want do only a single test.
        /// </summary>
        private bool doingOnlyASingleTest = false;

        /// <summary>
        /// True if you want delay the initialization.
        /// </summary>
        private bool delayInitialization = true;

        /// <summary>
        /// True if matchers have already been initialized.
        /// </summary>
        private bool matchersHaveBeenInitialized = false;

        /// <summary>
        /// Defines a verbose property to enable verbose logging.
        /// </summary>
        private bool verbose = false;

        /// <summary>
        /// A list of matchers that have been used during the parsing.
        /// </summary>
        private MatcherList touchedMatchers = null;

        /// <summary>
        /// The lookups dictionary.
        /// </summary>
        private IDictionary<string, IDictionary<string, string>> lookups = new Dictionary<string, IDictionary<string, string>>(128);

        /// <summary>
        /// Initializes static members of the <see cref="AbstractUserAgentAnalyzerDirect"/> class.
        /// </summary>
        static AbstractUserAgentAnalyzerDirect()
        {
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.SYNTAX_ERROR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.AGENT_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.LAYOUT_ENGINE_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.AGENT_NAME_VERSION);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.AGENT_NAME_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.LAYOUT_ENGINE_NAME_VERSION);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.LAYOUT_ENGINE_NAME_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.OPERATING_SYSTEM_NAME_VERSION);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.OPERATING_SYSTEM_NAME_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.WEBVIEW_APP_VERSION_MAJOR);
            HardCodedGeneratedFields.Add(DefaultUserAgentFields.WEBVIEW_APP_NAME_VERSION_MAJOR);
        }

        /// <summary>
        /// Gets the test cases that have been loaded.
        /// </summary>
        public IList<IDictionary<string, IDictionary<string, string>>> TestCases { get; } = new List<IDictionary<string, IDictionary<string, string>>>(2048);

        /// <summary>
        /// Gets the user agent max lenght that will be considered during the parsing.
        /// </summary>
        public int UserAgentMaxLength { get; private set; } = DEFAULT_USER_AGENT_MAX_LENGTH;

        /// <summary>
        /// Gets a value indicating whether tests will be keeed or will be discarded.
        /// </summary>
        public bool WillKeepTests { get; private set; } = false;

        /// <summary>
        /// Gets the number of test cases that have been loaded.
        /// </summary>
        public long NumberOfTestCases => this.TestCases.Count;

        /// <summary>
        /// Gets or sets the WantedFieldNames
        /// If we want ALL fields this is null. If we only want specific fields this is a list of names.
        /// </summary>
        protected internal ICollection<string> WantedFieldNames { get; set; } = null;

        /// <summary>
        /// The list of field caclculators that can be used.
        /// </summary>
        protected internal IList<IFieldCalculator> FieldCalculators { get; set; } = new List<IFieldCalculator>();

        /// <summary>
        /// Gets all matchers.
        /// </summary>
        protected IList<Matcher> AllMatchers { get; } = new List<Matcher>(5000);

        /// <summary>
        /// Gets or sets the user agent tree flattern.
        /// </summary>
        protected UserAgentTreeFlattener Flattener { get; set; } = null;

        /// <summary>
        /// Finds the length of first characters for prefix hash.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="maxChars">Max chars to retrieve.</param>
        /// <returns>The length.</returns>
        public static int FirstCharactersForPrefixHashLength(string input, int maxChars)
        {
            return Math.Min(maxChars, Math.Min(MAX_PREFIX_HASH_MATCH, input.Length));
        }

        /// <summary>
        /// Finds the first characters for prefix hash.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="maxChars">Max chars to retrieve.</param>
        /// <returns>The first characters.</returns>
        public static string FirstCharactersForPrefixHash(string input, int maxChars)
        {
            return input.Substring(0, FirstCharactersForPrefixHashLength(input, maxChars));
        }

        /// <summary>
        /// Gets all paths from the user agent string.
        /// </summary>
        /// <param name="agent">The user agent.</param>
        /// <returns>The paths.</returns>
        public static IList<string> GetAllPaths(string agent)
        {
            return new GetAllPathsAnalyzer(agent).Values;
        }

        /// <summary>
        /// Get the Path Analyzer.
        /// </summary>
        /// <param name="agent">The user agent string.</param>
        /// <returns>The Path analyzer.</returns>
        public static GetAllPathsAnalyzer GetAllPathsAnalyzer(string agent)
        {
            return new GetAllPathsAnalyzer(agent);
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
        /// Used to delay the initialization.
        /// </summary>
        public void DelayInitialization()
        {
            this.delayInitialization = true;
        }

        /// <summary>
        /// The analyzer will be initialized and ready before start parsing.
        /// </summary>
        public void ImmediateInitialization()
        {
            this.delayInitialization = false;
        }

        /// <summary>
        /// Removes all tests.
        /// </summary>
        /// <returns>The <see cref="AbstractUserAgentAnalyzerDirect"/> for chaining.</returns>
        public AbstractUserAgentAnalyzerDirect DropTests()
        {
            this.WillKeepTests = false;
            this.TestCases.Clear();
            return this;
        }

        /// <summary>
        /// The analyzer will keep all tests.
        /// </summary>
        /// <returns>The <see cref="AbstractUserAgentAnalyzerDirect"/> for chaining.</returns>
        public AbstractUserAgentAnalyzerDirect KeepTests()
        {
            this.WillKeepTests = true;
            return this;
        }

        /// <summary>
        /// Enable statistics logging.
        /// </summary>
        /// <param name="newShowMatcherStats">Trye to enable statistics.</param>
        /// <returns>The <see cref="AbstractUserAgentAnalyzerDirect"/> for chaining.</returns>
        public AbstractUserAgentAnalyzerDirect SetShowMatcherStats(bool newShowMatcherStats)
        {
            this.showMatcherStats = newShowMatcherStats;
            return this;
        }

        /// <summary>
        /// Loads all yaml resources with user agent definitions, matcher configurations and tests.
        /// </summary>
        /// <param name="resourceString">The folder path where to load all resources.</param>
        /// <param name="pattern">The pattern, for default all yaml files in the folder will be loaded.</param>
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
                throw new InvalidParserConfigurationException($"Error reading resources: {e.Message}", e);
            }

            this.doingOnlyASingleTest = false;
            var maxFilenameLength = 0;

            if (!resources.Any())
            {
                Logger.Warn($"NO config files were found matching this expression: {resourceString}");
                Logger.Error($"If you are using wildcards in your expression then try explicitly naming all yamls files explicitly.");
                return;
            }

            // We need to determine if we are trying to load the yaml files TWICE.
            // This can happen if the library is loaded twice (perhaps even two different versions).
            var alreadyLoadedResourceBasenames = this.matcherConfigs.Keys.Where(r => resources.Keys.Contains(r)).ToArray();

            if (alreadyLoadedResourceBasenames.Length > 0)
            {
                Logger.Error($"Trying to load these {alreadyLoadedResourceBasenames.Length} resources for the second time: [{string.Join(",", alreadyLoadedResourceBasenames)}]");
                throw new InvalidParserConfigurationException($"Trying to load {alreadyLoadedResourceBasenames.Length} resources for the second time");
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
                        throw new InvalidParserConfigurationException($"Parse error in the file {filename}: {e.Message}", e);
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidParserConfigurationException($"Error reading resources: {e.Message}", e);
                }
            }

            filesStopwatch.Stop();

            Logger.Info($"Loading {resources.Count} files in {filesStopwatch.ElapsedMilliseconds} msec from {resourceString}");

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

            var skippedMatchers = 0;

            if (this.matcherConfigs != null)
            {
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
                            this.AllMatchers.Add(new Matcher(this, this.WantedFieldNames, map, configFilename));
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
                        Logger.Info($"Loading {matcherConfig.Count - (stopSkipped - startSkipped)} (dropped {stopSkipped - startSkipped}) matchers from {configFilename} took {stopwatch.ElapsedMilliseconds} msec");
                    }
                }
            }
        }

        /// <summary>
        /// Initialize all matchers.
        /// </summary>
        public void InitializeMatchers()
        {
            if (this.matchersHaveBeenInitialized)
            {
                return;
            }

            Logger.Info($"Initializing Analyzer data structures");

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
            Logger.Info($"Built in {stopwatch.ElapsedMilliseconds} msec : Hashmap {this.informMatcherActions.Count}, Ranges map:{this.informMatcherActionRanges.Count}");

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
        /// Retrieves all possible field names.
        /// </summary>
        /// <returns>The field names.</returns>
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
        /// Set true to enable verbose logging.
        /// </summary>
        /// <param name="newVerbose">True to enable verbose.</param>
        public void SetVerbose(bool newVerbose)
        {
            this.verbose = newVerbose;
            this.Flattener.SetVerbose(newVerbose);
        }

        /// <summary>
        /// Sets the use agent max length.
        /// </summary>
        /// <param name="newUserAgentMaxLength">The length.</param>
        public void SetUserAgentMaxLength(int newUserAgentMaxLength)
        {
            if (newUserAgentMaxLength <= 0)
            {
                this.UserAgentMaxLength = DEFAULT_USER_AGENT_MAX_LENGTH;
            }
            else
            {
                this.UserAgentMaxLength = newUserAgentMaxLength;
            }
        }

        /// <summary>
        /// Retrieves all possible field names sorted.
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
        /// Informs me about a prefix.
        /// </summary>
        /// <param name="matcherAction">The <see cref="MatcherAction"/>.</param>
        /// <param name="treeName">The name of the tree.</param>
        /// <param name="prefix">The prefix.</param>
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
        /// Informs me ambout a key pattern.
        /// </summary>
        /// <param name="matcherAction">The <see cref="MatcherAction"/>.</param>
        /// <param name="keyPattern">The key pattern.</param>
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
        /// Looks for range.
        /// </summary>
        /// <param name="treeName">The name of the tree.</param>
        /// <param name="range">The range.</param>
        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            if (!this.informMatcherActionRanges.Keys.Contains(treeName))
            {
                this.informMatcherActionRanges[treeName] = new HashSet<WordRangeVisitor.Range>();
            }

            this.informMatcherActionRanges[treeName].Add(range);
        }

        /// <inheritdoc/>
        public void ReceivedInput(Matcher matcher)
        {
            this.touchedMatchers.Add(matcher);
        }

        /// <summary>
        /// Resets the state of the Analyzer to the default state.
        /// </summary>
        public void Reset()
        {
            // Reset all Matchers
            foreach (Matcher matcher in this.touchedMatchers)
            {
                matcher.Reset();
            }

            this.touchedMatchers.Clear();

            foreach (Matcher matcher in this.zeroInputMatchers)
            {
                matcher.Reset();
            }
        }

        /// <summary>
        /// Checks if the field is one of the wanted fields.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>True if wanted.</returns>
        public bool IsWantedField(string fieldName)
        {
            if (this.WantedFieldNames is null)
            {
                return true;
            }

            return this.WantedFieldNames.Contains(fieldName);
        }

        /// <summary>
        /// Parse the user agent string.
        /// </summary>
        /// <param name="userAgentString">The user agent string.</param>
        /// <returns>The <see cref="UserAgent"/>.</returns>
        public virtual UserAgent Parse(string userAgentString)
        {
            var userAgent = new UserAgent(userAgentString, this.WantedFieldNames);
            return this.Parse(userAgent);
        }

        /// <summary>
        /// Retrieves the required inform ranges.
        /// </summary>
        /// <param name="treeName">The tree name.</param>
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
        /// Retrieves the required prefixed lengths.
        /// </summary>
        /// <param name="treeName">The name of the tree.</param>
        /// <returns>The required prefix lenght.</returns>
        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            return this.informMatcherActionPrefixesLengths.ContainsKey(treeName) ? this.informMatcherActionPrefixesLengths[treeName] : null;
        }

        /// <summary>
        /// Informs when matches a key value in context tree node.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="ctx">The context node.</param>
        public void Inform(string key, string value, IParseTree ctx)
        {
            this.Inform(key, key, value, ctx);
            this.Inform($"{key}=\"{value}\"", key, value, ctx);

            var lengths = this.GetRequiredPrefixLengths(key);
            if (lengths != null)
            {
                var valueLength = value.Length;
                foreach (var prefixLength in lengths)
                {
                    if (valueLength >= prefixLength)
                    {
                        this.Inform(key + $"{{\"{FirstCharactersForPrefixHash(value, prefixLength.Value)}\"", key, value, ctx);
                    }
                }
            }
        }

        /// <summary>
        /// Runs all testcases once to heat up the CLR.
        /// </summary>
        /// <returns>The number of ierations.</returns>
        public long PreHeat()
        {
            return this.PreHeat(this.TestCases.Count, true);
        }

        /// <summary>
        /// Runs the number of specified testcases to heat up the CLR.
        /// </summary>
        /// <param name="preheatIterations">The number of iterations.</param>
        /// <returns>The <see cref="long"/>.</returns>
        public long PreHeat(long preheatIterations)
        {
            return this.PreHeat(preheatIterations, true);
        }

        /// <summary>
        /// Runs the number of specified testcases to heat up the CLR.
        /// </summary>
        /// <param name="preheatIterations">The number of ierations .</param>
        /// <param name="log">True to enable loggng.</param>
        /// <returns>The number of iterations.</returns>
        public long PreHeat(long preheatIterations, bool log)
        {
            if (this.TestCases.Count == 0)
            {
                Logger.Warn($"NO PREHEAT WAS DONE. Simply because there are no test cases available.");
                return 0;
            }

            if (preheatIterations <= 0)
            {
                Logger.Warn($"NO PREHEAT WAS DONE. Simply because you asked for {preheatIterations} to run.");
                return 0;
            }

            if (preheatIterations > MAX_PRE_HEAT_ITERATIONS)
            {
                Logger.Warn($"NO PREHEAT WAS DONE. Simply because you asked for too many ({preheatIterations} > {MAX_PRE_HEAT_ITERATIONS}) to run.");
                return 0;
            }

            if (log)
            {
                Logger.Info($"Preheating CLR by running {preheatIterations} testcases.");
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
                Logger.Info($"Preheating CLR completed. ({goodResults} of {preheatIterations} were proper results)");
            }

            return preheatIterations;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "UserAgentAnalyzerDirect{" +
            "\nallMatchers=[" + string.Join(",", this.AllMatchers) + "]" +
            "\n, zeroInputMatchers=[" + string.Join(",", this.zeroInputMatchers) + "]" +
            "\n, informMatcherActions=[" + string.Join(",", this.informMatcherActions) + "]" +
            "\n, showMatcherStats=[" + string.Join(",", this.showMatcherStats) + "]" +
            "\n, doingOnlyASingleTest=[" + string.Join(",", this.doingOnlyASingleTest) + "]" +
            "\n, wantedFieldNames=[" + string.Join(",", this.WantedFieldNames) + "]" +
            "\n, testCases=[" + string.Join(",", this.TestCases) + "]" +
            "\n, lookups=[" + string.Join(",", this.lookups) + "]" +
            "\n, lookupSets=[" + string.Join(",", this.lookupSets) + "]" +
            "\n, flattener=[" + string.Join(",", this.Flattener) + "]" +
            "\n, userAgentMaxLength=[" + string.Join(",", this.UserAgentMaxLength) + "]" +
            "\n, loadTests=[" + string.Join(",", this.WillKeepTests) + "]" +
            "\n, delayInitialization=[" + string.Join(",", this.delayInitialization) + "]" +
            "\n, matchersHaveBeenInitialized=[" + string.Join(",", this.matchersHaveBeenInitialized) + "]" +
            "\n, verbose=[" + this.verbose +
            "\n, fieldCalculators=[" + string.Join(",", this.FieldCalculators) + "]" +
            "\n}";
        }

        /// <summary>
        /// Parse a <see cref="UserAgent"/> object filling all fields.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/> to parse.</param>
        /// <returns>The parsed <see cref="UserAgent"/>.</returns>
        public virtual UserAgent Parse(UserAgent userAgent)
        {
            lock (this)
            {
                this.InitializeMatchers();
                var useragentString = userAgent.UserAgentString;
                if (useragentString != null && useragentString.Length > this.UserAgentMaxLength)
                {
                    this.SetAsHacker(userAgent, 100);
                    userAgent.SetForced(DefaultUserAgentFields.HACKER_ATTACK_VECTOR, "Buffer overflow", 100);
                    return this.HardCodedPostProcessing(userAgent);
                }

                // Reset all Matchers
                this.Reset();

                if (userAgent.IsDebug)
                {
                    foreach (Matcher matcher in this.AllMatchers)
                    {
                        matcher.SetVerboseTemporarily(true);
                    }
                }

                try
                {
                    userAgent = this.Flattener.Parse(userAgent);

                    if (this.verbose)
                    {
                        Logger.Info($" =========== Checking all Touched Matchers: {this.touchedMatchers.Count}");
                    }

                    // Fire all Analyzers
                    foreach (var matcher in this.touchedMatchers)
                    {
                        matcher.Analyze(userAgent);
                    }

                    if (this.verbose)
                    {
                        Logger.Info($" =========== Checking all Zero Input Matchers:: {this.zeroInputMatchers.Count}");
                    }

                    // Fire all Analyzers that should not get input
                    foreach (var matcher in this.zeroInputMatchers)
                    {
                        matcher.Analyze(userAgent);
                    }

                    userAgent.ProcessSetAll();
                    return this.HardCodedPostProcessing(userAgent);
                }
                catch (Exception)
                {
                    // If this occurs then someone has found a previously undetected problem.
                    // So this is a safety for something that 'can' but 'should not' occur.
                    userAgent.Reset();
                    userAgent = this.SetAsHacker(userAgent, 10000);
                    userAgent.SetForced(DefaultUserAgentFields.HACKER_ATTACK_VECTOR, "Yauaa Exploit", 10000);
                    return this.HardCodedPostProcessing(userAgent);
                }
            }
        }

        /// <summary>
        /// Initialize the analyzer.
        /// </summary>
        protected internal void Initialize()
        {
            this.Initialize(new List<ResourcesPath>() { DefaultResources });
        }

        /// <summary>
        /// Initialize the analyser with a set of resources.
        /// </summary>
        /// <param name="resources">The resources to be loaded.</param>
        protected internal void Initialize(IList<ResourcesPath> resources)
        {
            YauaaVersion.LogVersion();
            var fullStart = Stopwatch.StartNew();
            if (this.WantedFieldNames != null)
            {
                int wantedSize = this.WantedFieldNames.Count;
                if (this.WantedFieldNames.Contains(DefaultUserAgentFields.SET_ALL_FIELDS))
                {
                    wantedSize--;
                }

                Logger.Info($"Building all needed matchers for the requested {wantedSize} fields.");
            }
            else
            {
                Logger.Info($"Building all matchers for all possible fields.");
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

            Logger.Info($"Loading {this.AllMatchers.Count} matchers, {lookupsCount} lookups, {this.lookupSets.Count} lookupsets, {this.TestCases.Count} testcases from {this.matcherConfigs.Count} files took {fullStop} msec");

            this.VerifyWeAreNotAskingForImpossibleFields();
            if (!this.delayInitialization)
            {
                this.InitializeMatchers();
            }
        }

        /// <summary>
        /// Verify that we are not asking impossible fields.
        /// </summary>
        protected void VerifyWeAreNotAskingForImpossibleFields()
        {
            if (this.WantedFieldNames is null)
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

            throw new InvalidParserConfigurationException($"We cannot provide these fields:{bd}");
        }

        /// <summary>
        /// Informs about a match.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="ctx">The context.</param>
        private void Inform(string match, string key, string value, IParseTree ctx)
        {
            var cmatch = match.ToLower(CultureInfo.InvariantCulture);
            var relevantActions = this.informMatcherActions.ContainsKey(cmatch) ? this.informMatcherActions[cmatch] : null;
            if (this.verbose)
            {
                if (relevantActions is null)
                {
                    Logger.Info($"--- Have (0): {match}");
                }
                else
                {
                    Logger.Info($"+++ Have ({relevantActions.Count}): {match}");

                    var count = 1;
                    foreach (var action in relevantActions)
                    {
                        Logger.Info($"+++ -------> ({count}): {action}");
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
        /// Load a resource form yaml document.
        /// </summary>
        /// <param name="yaml">The <see cref="YamlDocument"/>.</param>
        /// <param name="filename">The filename.</param>
        private void LoadResource(YamlDocument yaml, string filename)
        {
            YamlNode loadedYaml;
            try
            {
                loadedYaml = yaml?.RootNode;
            }
            catch (Exception e)
            {
                throw new InvalidParserConfigurationException($"Parse error in the file {filename}: {e.Message}", e);
            }

            if (loadedYaml is null)
            {
                Logger.Warn($"The file {filename} is empty");
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
                            $"Yaml config.({filename}:{actualEntry.Start.Line}): Found unexpected config entry: {entryType}, allowed are 'lookup', 'set', 'matcher' and 'test'");
                }
            }
        }

        /// <summary>
        /// Load lookups from yaml node.
        /// </summary>
        /// <param name="entry">The <see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The filename.</param>
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
                        if (map is null)
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
        /// Load all yaml lookup sets from yaml node.
        /// </summary>
        /// <param name="entry">The <see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The name of the node.</param>
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
        /// Loads the matcher from yaml node.
        /// </summary>
        /// <param name="entry">The entry <see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The name of the file.</param>
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
        /// Loads a test case from yaml node.
        /// </summary>
        /// <param name="entry">The <see cref="YamlMappingNode"/>.</param>
        /// <param name="filename">The name of the file.</param>
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
                                if ("user_agent_string".Equals(inputName))
                                {
                                    var inputString = YamlUtils.GetValueAsString(inputTuple, filename);
                                    input = new Dictionary<string, string>
                                    {
                                        [inputName] = inputString,
                                    };
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
        /// Set as user agent as hacker with given confidence.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/>.</param>
        /// <param name="confidence">The confidence.</param>
        /// <returns>The resulting <see cref="UserAgent"/>.</returns>
        private UserAgent SetAsHacker(UserAgent userAgent, int confidence)
        {
            userAgent.Set(DefaultUserAgentFields.DEVICE_CLASS, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.DEVICE_BRAND, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.DEVICE_NAME, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.DEVICE_VERSION, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.OPERATING_SYSTEM_CLASS, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.OPERATING_SYSTEM_NAME, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.OPERATING_SYSTEM_VERSION, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.LAYOUT_ENGINE_CLASS, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.LAYOUT_ENGINE_NAME, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.LAYOUT_ENGINE_VERSION, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.LAYOUT_ENGINE_VERSION_MAJOR, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.AGENT_CLASS, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.AGENT_NAME, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.AGENT_VERSION, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.AGENT_VERSION_MAJOR, "Hacker", confidence);
            userAgent.Set(DefaultUserAgentFields.HACKER_TOOLKIT, "Unknown", confidence);
            userAgent.Set(DefaultUserAgentFields.HACKER_ATTACK_VECTOR, "Buffer overflow", confidence);
            return userAgent;
        }

        /// <summary>
        /// Used to hardcode processing fields.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/> to be processed.</param>
        /// <returns>The parsed <see cref="UserAgent"/>.</returns>
        private UserAgent HardCodedPostProcessing(UserAgent userAgent)
        {
            // If it is really really bad ... then it is a Hacker.
            if ("true".Equals(userAgent.GetValue(DefaultUserAgentFields.SYNTAX_ERROR)))
            {
                if (userAgent.Get(DefaultUserAgentFields.DEVICE_CLASS).GetConfidence() == -1)
                {
                    this.SetAsHacker(userAgent, 10);
                }
            }

            // Calculate all fields that are constructed from the found ones.
            foreach (var fieldCalculator in this.FieldCalculators)
            {
                if (this.verbose)
                {
                    Logger.Info($"Running FieldCalculator: {fieldCalculator}");
                }

                fieldCalculator.Calculate(userAgent);
            }

            return userAgent;
        }
    }
}
