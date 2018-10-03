using log4net;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Parse;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using YamlDotNet.RepresentationModel;
using System.Linq;
using System.Globalization;
using Antlr4.Runtime.Tree;
using Nager.PublicSuffix;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent
{
    public class UserAgentAnalyzerDirect: IAnalyzer
    {

        // We set this to 1000000 always.
        // Why?
        // At the time of writing this the actual HashMap size needed about 410K entries.
        // To keep the bins small the load factor of 0.75 already puts us at the capacity of 1048576
        private static readonly int INFORM_ACTIONS_HASHMAP_CAPACITY = 1000000;

        private static readonly ILog LOG = LogManager.GetLogger(typeof(UserAgentAnalyzerDirect));
        protected readonly List<Matcher> allMatchers = new List<Matcher>();
        private readonly Dictionary<string, HashSet<MatcherAction>> informMatcherActions = new Dictionary<string, HashSet<MatcherAction>>();
        private Dictionary<string, List<YamlMappingNode>> matcherConfigs = new Dictionary<string, List<YamlMappingNode>>();

        private bool showMatcherStats = false;
        private bool doingOnlyASingleTest = false;

        // If we want ALL fields this is null. If we only want specific fields this is a list of names.
        protected List<string> wantedFieldNames = null;

        protected readonly List<Dictionary<string, Dictionary<string, string>>> testCases = new List<Dictionary<string, Dictionary<string, string>>>();
        private Dictionary<string, Dictionary<string, string>> lookups = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, HashSet<string>> lookupSets = new Dictionary<string, HashSet<string>>();

        protected UserAgentTreeFlattener flattener;

        public static readonly int DEFAULT_USER_AGENT_MAX_LENGTH = 2048;
        private int userAgentMaxLength = DEFAULT_USER_AGENT_MAX_LENGTH;
        private bool loadTests = false;

        private static readonly string DEFAULT_RESOURCES = "UserAgents";

        private void InitTransientFields()
        {
            matcherConfigs = new Dictionary<string, List<YamlMappingNode>>();
        }

        private void ReadObject(Stream stream)
        {
            InitTransientFields();
        
            List<string> lines = new List<string>();
            lines.Add("This Analyzer instance was deserialized.");
            lines.Add("");
            lines.Add("Lookups      : " + ((lookups == null) ? 0 : lookups.Count));
            lines.Add("LookupSets   : " + lookupSets.Count);
            lines.Add("Matchers     : " + allMatchers.Count);
            lines.Add("Hashmap size : " + informMatcherActions.Count);
            lines.Add("Testcases    : " + testCases.Count);

            string[] x = { };
            YauaaVersion.LogVersion(lines.ToArray());
        }

        protected UserAgentAnalyzerDirect()
        {
        }

        private bool delayInitialization = true;
        public void DelayInitialization()
        {
            delayInitialization = true;
        }

        public void ImmediateInitialization()
        {
            delayInitialization = false;
        }

        public UserAgentAnalyzerDirect SetShowMatcherStats(bool newShowMatcherStats)
        {
            this.showMatcherStats = newShowMatcherStats;
            return this;
        }

        public UserAgentAnalyzerDirect DropTests()
        {
            loadTests = false;
            testCases.Clear();
            return this;
        }

        public UserAgentAnalyzerDirect KeepTests()
        {
            loadTests = true;
            return this;
        }

        public bool WillKeepTests()
        {
            return loadTests;
        }

        public long GetNumberOfTestCases()
        {
            return testCases.Count;
        }

        protected void Initialize()
        {
            Initialize(new List<string>() { DEFAULT_RESOURCES });
        }

        protected void Initialize(List<string> resources)
        {
            YauaaVersion.LogVersion();
            resources.ForEach(r => this.LoadResources(r));
            VerifyWeAreNotAskingForImpossibleFields();
            if (!delayInitialization)
            {
                InitializeMatchers();
            }
        }

        protected void VerifyWeAreNotAskingForImpossibleFields()
        {
            if (wantedFieldNames == null)
            {
                return; // Nothing to check
            }
            List<string> impossibleFields = new List<string>();
            List<string> allPossibleFields = getAllPossibleFieldNamesSorted();

            foreach (string wantedFieldName in wantedFieldNames)
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
            throw new InvalidParserConfigurationException("We cannot provide these fields:" + impossibleFields.ToString());
        }

        // --------------------------------------------

        public static string GetVersion()
        {
            return "Yauaa " + ThisVersion.GetProjectVersion() + " (" + ThisVersion.GetGitCommitIdDescribeShort() + " @ " + ThisVersion.GetBuildTimestamp() + ")";
        }

        public void LoadResources(string resourceString)
        {
            if (matchersHaveBeenInitialized)
            {
                throw new Exception("Refusing to load additional resources after the datastructures have been initialized.");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            LOG.Info(string.Format("Loading from: \"{0}\"", resourceString));
            long startFiles = DateTime.Now.Ticks;

            flattener = new UserAgentTreeFlattener(this);
            YamlDocument yaml;

            Dictionary<string, FileInfo> resources = new Dictionary<string, FileInfo>();
            try
            {
                string[] filePaths = Directory.GetFiles(resourceString, "*.yaml");
                
                foreach (string filePath in filePaths)
                {
                    resources[Path.GetFileName(filePath)] = new FileInfo(filePath);
                }
            }
            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                return;
            }
            doingOnlyASingleTest = false;
            int maxFilenameLength = 0;

            if (resources.Count == 0)
            {
                throw new InvalidParserConfigurationException("Unable to find ANY config files");
            }

            // We need to determine if we are trying to load the yaml files TWICE.
            // This can happen if the library is loaded twice (perhaps even two different versions).

            HashSet<string>  alreadyLoadedResourceBasenames = new HashSet<string>();
            foreach (var item in matcherConfigs.Keys)
            {
                if (resources.Keys.Contains(item))
                {
                    alreadyLoadedResourceBasenames.Add(item);
                }
            }

            if (alreadyLoadedResourceBasenames.Count != 0)
            {
                LOG.Error(string.Format("Trying to load these {0} resources for the second time: {1}", alreadyLoadedResourceBasenames.Count,  alreadyLoadedResourceBasenames.ToString()));
                throw new InvalidParserConfigurationException("Trying to load " + alreadyLoadedResourceBasenames.Count +" resources for the second time");
            }

            
            foreach (KeyValuePair<string, FileInfo> resourceEntry in resources)
            {
                try
                {
                    using (var reader = new StreamReader(resourceEntry.Value.FullName))
                    {
                        // Load the stream
                        var yamlStream = new YamlStream();
                        yamlStream.Load(reader);
                        yaml = yamlStream.Documents.FirstOrDefault();
                    }

                   
                    string filename = resourceEntry.Value.FullName;
                    maxFilenameLength = Math.Max(maxFilenameLength, filename.Length);
                    LoadResource(yaml, filename);
                }
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }

            stopwatch.Stop();
            LOG.Info(string.Format("Loaded {0} files in {1} msec", resources.Count, stopwatch.ElapsedMilliseconds));

            if (lookups != null && lookups.Count != 0)
            {
                // All compares are done in a case insensitive way. So we lowercase ALL keys of the lookups beforehand.
                Dictionary<string, Dictionary<string, string>> cleanedLookups = new Dictionary<string, Dictionary<string, string>>();
                foreach ( var lookupsEntry in lookups)
                {
                    Dictionary<string, string> cleanedLookup = new Dictionary<string, string>();
                    foreach (var entry in lookupsEntry.Value)
                    {
                        cleanedLookup[entry.Key.ToLower()] = entry.Value;
                    }
                    cleanedLookups[lookupsEntry.Key] = cleanedLookup;
                }
                lookups = cleanedLookups;
            }

            if (wantedFieldNames != null)
            {
                int wantedSize = wantedFieldNames.Count;
                if (wantedFieldNames.Contains(UserAgent.SET_ALL_FIELDS))
                {
                    wantedSize--;
                }
                LOG.Info(string.Format("Building all needed matchers for the requested {0} fields.", wantedSize));
            }
            else
            {
                LOG.Info("Building all matchers for all possible fields.");
            }
            int totalNumberOfMatchers = 0;
            int skippedMatchers = 0;
            if (matcherConfigs != null)
            {
                Stopwatch fullStart = Stopwatch.StartNew();
                foreach (var resourceEntry in resources)
                {
                    FileInfo resource = resourceEntry.Value;
                    string configFilename = resource.Name;
                    List<YamlMappingNode> matcherConfig = matcherConfigs[configFilename];
                    if (matcherConfig == null)
                    {
                        continue; // No matchers in this file (probably only lookups and/or tests)
                    }

                    Stopwatch start = Stopwatch.StartNew();
                    int startSkipped = skippedMatchers;
                    foreach (YamlMappingNode map in matcherConfig)
                    {
                        try
                        {
                            allMatchers.Add(new Matcher(this, lookups, lookupSets, wantedFieldNames, map, configFilename));
                            totalNumberOfMatchers++;
                        }
                        catch (UselessMatcherException ume)
                        {
                            skippedMatchers++;
                        }
                    }
                    start.Stop();
                    int stopSkipped = skippedMatchers;

                    if (showMatcherStats)
                    {
                        LOG.Info(string.Format("Loading {0} (dropped {1}) matchers from {2} took {3} msec",
                            matcherConfig.Count - (stopSkipped - startSkipped),
                            stopSkipped - startSkipped,
                            configFilename,
                            start.ElapsedMilliseconds));
                    }
                    fullStart.Stop();

                    LOG.Info(string.Format("Loading {0} (dropped {1}) matchers, {2} lookups, {3} lookupsets, {4} testcases from {5} files took {6} msec",
                        totalNumberOfMatchers,
                        skippedMatchers,
                        (lookups == null) ? 0 : lookups.Count(),
                        lookupSets.Count(),
                        testCases.Count,
                        matcherConfigs.Count,
                        fullStart.ElapsedMilliseconds
                    ));
                }

            }
        }

        private bool matchersHaveBeenInitialized = false;
        public void InitializeMatchers()
        {
            if (matchersHaveBeenInitialized)
            {
                return;
            }
            LOG.Info("Initializing Analyzer data structures");
            Stopwatch start = Stopwatch.StartNew();
            allMatchers.ForEach(m => m.Initialize());
            start.Stop();
            matchersHaveBeenInitialized = true;
            LOG.Info(string.Format("Built in {0} msec : Hashmap {1}, Ranges map:{2}",
                start.ElapsedMilliseconds,
                informMatcherActions.Count,
                informMatcherActionRanges.Count));
        }

        public SortedSet<string> getAllPossibleFieldNames()
        {
            SortedSet<string> results = new SortedSet<string>();
            foreach (Matcher matcher in allMatchers)
            {
                results.UnionWith(matcher.GetAllPossibleFieldNames());
            }
            return results;
        }

        public List<string> getAllPossibleFieldNamesSorted()
        {
            List<string> fieldNames = new List<string>(getAllPossibleFieldNames());
            fieldNames.Sort();

            List<string> result = new List<string>();
            foreach (string fieldName in UserAgent.PRE_SORTED_FIELDS_LIST)
            {
                fieldNames.Remove(fieldName);
                result.Add(fieldName);
            }
            result.AddRange(fieldNames);

            return result;
        }

        /*
            Example of the structure of the yaml file:
            ----------------------------
            config:
                - lookup:
                name: 'lookupname'
                map:
                    "From1" : "To1"
                    "From2" : "To2"
                    "From3" : "To3"
                - matcher:
                    options:
                    - 'verbose'
                    - 'init'
                    require:
                    - 'Require pattern'
                    - 'Require pattern'
                    extract:
                    - 'Extract pattern'
                    - 'Extract pattern'
                - test:
                    input:
                    user_agent_string: 'Useragent'
                    expected:
                    FieldName     : 'ExpectedValue'
                    FieldName     : 'ExpectedValue'
                    FieldName     : 'ExpectedValue'
            ----------------------------
        */

        private void LoadResource(YamlDocument yaml, string filename)
        {
            YamlNode loadedYaml;
            try
            {  
                loadedYaml = yaml.RootNode;
            }
            catch (Exception e)
            {
                throw new InvalidParserConfigurationException("Parse error in the file " + filename + ": " + e.Message, e);
            }

            if (loadedYaml == null)
            {
                throw new InvalidParserConfigurationException("The file " + filename + " is empty");
            }

            // Get and check top level config
            if (!(loadedYaml is YamlMappingNode)) {
                YamlUtils.Fail(loadedYaml, filename, "File must be a Map");
            }

            YamlMappingNode rootNode = (YamlMappingNode)loadedYaml;

            KeyValuePair<YamlNode, YamlNode> configNodeTuple = new KeyValuePair<YamlNode, YamlNode>(null, null);
            foreach (KeyValuePair<YamlNode, YamlNode> tuple in rootNode)
            {
                string name = YamlUtils.GetKeyAsString(tuple, filename);
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

            if (configNodeTuple.Key == null && configNodeTuple.Value == null)
            {
                YamlUtils.Fail(loadedYaml, filename, "The top level entry MUST be 'config'.");
            }

            YamlSequenceNode configNode = YamlUtils.GetValueAsSequenceNode(configNodeTuple, filename);
            List<YamlNode> configList = configNode.AllNodes.ToList();

            foreach (YamlNode configEntry in configList)
            {
                if (!(configEntry is YamlMappingNode)) {
                    YamlUtils.Fail(loadedYaml, filename, "The entry MUST be a mapping");
                }

                KeyValuePair<YamlNode, YamlNode> entry = YamlUtils.GetExactlyOneNodeTuple((YamlMappingNode)configEntry, filename);
                YamlMappingNode actualEntry = YamlUtils.GetValueAsMappingNode(entry, filename);
                string entryType = YamlUtils.GetKeyAsString(entry, filename);
                switch (entryType)
                {
                    case "lookup":
                        LoadYamlLookup(actualEntry, filename);
                        break;
                    case "set":
                        LoadYamlLookupSets(actualEntry, filename);
                        break;
                    case "matcher":
                        LoadYamlMatcher(actualEntry, filename);
                        break;
                    case "test":
                        if (loadTests)
                        {
                            LoadYamlTestcase(actualEntry, filename);
                        }
                        break;
                    default:
                        throw new InvalidParserConfigurationException(
                            "Yaml config.(" + filename + ":" + actualEntry.Start.Line + "): " +
                                "Found unexpected config entry: " + entryType + ", allowed are 'lookup', 'set', 'matcher' and 'test'");
                }
            }
        }

        private void LoadYamlLookup(YamlMappingNode entry, string filename)
        {
            //        LOG.info("Loading lookup.({}:{})", filename, entry.getStartMark().getLine());
            string name = null;
            Dictionary<string, string> map = null;

            foreach (KeyValuePair<YamlNode,YamlNode> tuple in entry)
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
                        List<KeyValuePair<YamlNode, YamlNode>> mappings = YamlUtils.GetValueAsMappingNode(tuple, filename).ToList();
                        foreach (KeyValuePair<YamlNode, YamlNode> mapping in mappings)
                        {
                            string key = YamlUtils.GetKeyAsString(mapping, filename);
                            string value = YamlUtils.GetValueAsString(mapping, filename);
                            map[key] = value;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (name == null && map == null)
            {
                YamlUtils.Fail(entry, filename, "Invalid lookup specified");
            }

            lookups[name] = map;
        }

        private void LoadYamlLookupSets(YamlMappingNode entry, string filename)
        {
            //        LOG.info("Loading lookupSet.({}:{})", filename, entry.getStartMark().getLine());
            string name = null;
            HashSet<string> lookupSet = new HashSet<string>();

            foreach (KeyValuePair<YamlNode, YamlNode> tuple in entry)
            {
                switch (YamlUtils.GetKeyAsString(tuple, filename))
                {
                    case "name":
                        name = YamlUtils.GetValueAsString(tuple, filename);
                        break;
                    case "values":
                        YamlSequenceNode node = YamlUtils.GetValueAsSequenceNode(tuple, filename);
                        List<string> values = YamlUtils.GetStringValues(node, filename);
                        foreach (string value in values)
                        {
                            lookupSet.Add(value.ToLower(CultureInfo.InvariantCulture));
                        }
                        break;
                    default:
                        break;
                }
            }

            lookupSets[name] = lookupSet;
        }

        private void LoadYamlMatcher(YamlMappingNode entry, string filename)
        {
            //        LOG.info("Loading matcher.({}:{})", filename, entry.getStartMark().getLine());
            List<YamlMappingNode> matcherConfigList = matcherConfigs.FirstOrDefault(e => e.Key == filename).Value;
            if (matcherConfigList == null)
            {
                matcherConfigList = matcherConfigs[filename] = new List<YamlMappingNode>();
            }
            matcherConfigList.Add(entry);
        }

        private void LoadYamlTestcase(YamlMappingNode entry, string filename)
        {
            if (!doingOnlyASingleTest)
            {
                //            LOG.info("Skipping testcase.({}:{})", filename, entry.getStartMark().getLine());
                //        } else {
                //            LOG.info("Loading testcase.({}:{})", filename, entry.getStartMark().getLine());

                Dictionary<string, string> metaData = new Dictionary<string, string>
                {
                    ["filename"] = filename,
                    ["fileline"] = Convert.ToString(entry.Start.Line)
                };

                Dictionary<string, string> input = null;
                List<string> options = null;
                Dictionary<string, string> expected = null;
                foreach (KeyValuePair<YamlNode, YamlNode> tuple in entry)
                {
                    string name = YamlUtils.GetKeyAsString(tuple, filename);
                    switch (name)
                    {
                        case "options":
                            options = YamlUtils.GetStringValues(tuple.Value, filename);
                            if (options != null)
                            {
                                if (options.Contains("only"))
                                {
                                    doingOnlyASingleTest = true;
                                    testCases.Clear();
                                }
                            }
                            break;
                        case "input":
                            foreach (KeyValuePair<YamlNode, YamlNode> inputTuple in YamlUtils.GetValueAsMappingNode(tuple, filename))
                            {
                                String inputName = YamlUtils.GetKeyAsString(inputTuple, filename);
                                switch (inputName)
                                {
                                    case "user_agent_string":
                                        string inputString = YamlUtils.GetValueAsString(inputTuple, filename);
                                        input = new Dictionary<string, string>();
                                        input[inputName] = inputString;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "expected":
                            List<KeyValuePair<YamlNode, YamlNode>> mappings = YamlUtils.GetValueAsMappingNode(tuple, filename).ToList();
                            if (mappings != null)
                            {
                                if (expected == null)
                                {
                                    expected = new Dictionary<string, string>();
                                }
                                foreach (KeyValuePair<YamlNode, YamlNode> mapping in mappings)
                                {
                                    string key = YamlUtils.GetKeyAsString(mapping, filename);
                                    string value = YamlUtils.GetValueAsString(mapping, filename);
                                    expected[key] = value;
                                }
                            }
                            break;
                        default:
                            //                        fail(tuple.getKeyNode(), filename, "Unexpected: " + name);
                            break; // Skip
                    }
                }

                if (input == null)
                {
                    YamlUtils.Fail(entry, filename, "Test is missing input");
                }

                if (expected == null || expected.Count == 0)
                {
                    doingOnlyASingleTest = true;
                    testCases.Clear();
                }

                Dictionary<string, Dictionary<string, string>> testCase = new Dictionary<string, Dictionary<string, string>>
                {
                    ["input"] = input
                };
                if (expected != null)
                {
                    testCase["expected"] = expected;
                }
                if (options != null)
                {
                    Dictionary<string, string> optionsMap = new Dictionary<string, string>();
                    foreach (string option in options)
                    {
                        optionsMap[option] = option;
                    }
                    testCase["options"] = optionsMap;
                }
                testCase["metaData"] = metaData;
                testCases.Add(testCase);
            }
        }

        // These are the actual subrange we need for the paths.
        private readonly Dictionary<string, HashSet<WordRangeVisitor.Range>> informMatcherActionRanges = new Dictionary<string, HashSet<WordRangeVisitor.Range>>();

        public void LookingForRange(string treeName, WordRangeVisitor.Range range)
        {
            if (!informMatcherActionRanges.Keys.Contains(treeName))
            {
                informMatcherActionRanges[treeName] = new HashSet<WordRangeVisitor.Range>();
            }

            informMatcherActionRanges[treeName].Add(range);
        }

        // We do not want to put ALL lengths in the hashmap for performance reasons
        public static readonly int MAX_PREFIX_HASH_MATCH = 3;


        // Calculate the max length we will put in the hashmap.
        public static int FirstCharactersForPrefixHashLength(string input, int maxChars)
        {
            return Math.Min(maxChars, Math.Min(MAX_PREFIX_HASH_MATCH, input.Length));
        }

        public static string FirstCharactersForPrefixHash(string input, int maxChars)
        {
            return input.Substring(0, FirstCharactersForPrefixHashLength(input, maxChars));
        }

        // These are the paths for which we have prefix requests.
        private readonly Dictionary<string, HashSet<int?>> informMatcherActionPrefixesLengths = new Dictionary<string, HashSet<int?>>();

        public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
        {
            this.InformMeAbout(matcherAction, treeName + "{\"" + FirstCharactersForPrefixHash(prefix, MAX_PREFIX_HASH_MATCH) + "\"");

            if (!informMatcherActionPrefixesLengths.Keys.Contains(treeName))
            {
                informMatcherActionPrefixesLengths[treeName] = new HashSet<int?>();
            }

            HashSet<int?> lengths = informMatcherActionPrefixesLengths[treeName];
            lengths.Add(FirstCharactersForPrefixHashLength(prefix, MAX_PREFIX_HASH_MATCH));
        }

        public ISet<int?> GetRequiredPrefixLengths(string treeName)
        {
            return informMatcherActionPrefixesLengths[treeName];
        }

        public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
        {
            string hashKey = keyPattern.ToLower();
            if (!informMatcherActions.Keys.Contains(hashKey))
            {
                informMatcherActions[hashKey] = new HashSet<MatcherAction>();
            }

            HashSet<MatcherAction> analyzerSet = informMatcherActions[hashKey];                
            analyzerSet.Add(matcherAction);
        }

        private bool verbose = false;

        public void SetVerbose(bool newVerbose)
        {
            verbose = newVerbose;
            flattener.SetVerbose(newVerbose);
        }

        public void SetUserAgentMaxLength(int newUserAgentMaxLength)
        {
            if (newUserAgentMaxLength <= 0)
            {
                userAgentMaxLength = DEFAULT_USER_AGENT_MAX_LENGTH;
            }
            else
            {
                userAgentMaxLength = newUserAgentMaxLength;
            }
        }

        public int GetUserAgentMaxLength()
        {
            return userAgentMaxLength;
        }

        public virtual UserAgent Parse(string userAgentString)
        {
            UserAgent userAgent = new UserAgent(userAgentString);
            return Parse(userAgent);
        }

        private UserAgent setAsHacker(UserAgent userAgent, int confidence)
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

        public virtual UserAgent Parse(UserAgent userAgent)
        {
            InitializeMatchers();
            string useragentString = userAgent.GetUserAgentString();
            if (useragentString != null && useragentString.Length > userAgentMaxLength)
            {
                userAgent = setAsHacker(userAgent, 100);
                userAgent.SetForced("HackerAttackVector", "Buffer overflow", 100);
                return hardCodedPostProcessing(userAgent);
            }

            // Reset all Matchers
            foreach (Matcher matcher in allMatchers)
            {
                matcher.Reset();
            }

            if (userAgent.IsDebug())
            {
                foreach (Matcher matcher in allMatchers)
                {
                    matcher.SetVerboseTemporarily(true);
                }
            }

            try
            {
                userAgent = flattener.Parse(userAgent);

                // Fire all Analyzers
                foreach (Matcher matcher in allMatchers)
                {
                    matcher.Analyze(userAgent);
                }

                userAgent.ProcessSetAll();
                return hardCodedPostProcessing(userAgent);
            }
            catch (NullReferenceException npe)
            {
                userAgent.Reset();
                userAgent = setAsHacker(userAgent, 10000);
                userAgent.SetForced("HackerAttackVector", "Yauaa NPE Exploit", 10000);
                return hardCodedPostProcessing(userAgent);
            }
        }

        private static readonly List<string> HARD_CODED_GENERATED_FIELDS = new List<string>();

        static void UserAgentAnalyzer()
        {
            HARD_CODED_GENERATED_FIELDS.Add(UserAgent.SYNTAX_ERROR);
            HARD_CODED_GENERATED_FIELDS.Add(UserAgent.AGENT_VERSION_MAJOR);
            HARD_CODED_GENERATED_FIELDS.Add(UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
            HARD_CODED_GENERATED_FIELDS.Add("AgentNameVersion");
            HARD_CODED_GENERATED_FIELDS.Add("AgentNameVersionMajor");
            HARD_CODED_GENERATED_FIELDS.Add("LayoutEngineNameVersion");
            HARD_CODED_GENERATED_FIELDS.Add("LayoutEngineNameVersionMajor");
            HARD_CODED_GENERATED_FIELDS.Add("OperatingSystemNameVersion");
            HARD_CODED_GENERATED_FIELDS.Add("WebviewAppVersionMajor");
            HARD_CODED_GENERATED_FIELDS.Add("WebviewAppNameVersionMajor");
        }

        public bool IsWantedField(string fieldName)
        {
            if (wantedFieldNames == null)
            {
                return true;
            }
            return wantedFieldNames.Contains(fieldName);
        }

        private UserAgent hardCodedPostProcessing(UserAgent userAgent)
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
            // TODO: Perhaps this should be more generic. Like a "Post processor"  (Generic: Create fields from fields)?
            AddMajorVersionField(userAgent, UserAgent.AGENT_VERSION, UserAgent.AGENT_VERSION_MAJOR);
            AddMajorVersionField(userAgent, UserAgent.LAYOUT_ENGINE_VERSION, UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
            AddMajorVersionField(userAgent, "WebviewAppVersion", "WebviewAppVersionMajor");

            ConcatFieldValuesNONDuplicated(userAgent, "AgentNameVersion", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION);
            ConcatFieldValuesNONDuplicated(userAgent, "AgentNameVersionMajor", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION_MAJOR);
            ConcatFieldValuesNONDuplicated(userAgent, "WebviewAppNameVersionMajor", "WebviewAppName", "WebviewAppVersionMajor");
            ConcatFieldValuesNONDuplicated(userAgent, "LayoutEngineNameVersion", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION);
            ConcatFieldValuesNONDuplicated(userAgent, "LayoutEngineNameVersionMajor", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
            ConcatFieldValuesNONDuplicated(userAgent, "OperatingSystemNameVersion", UserAgent.OPERATING_SYSTEM_NAME, UserAgent.OPERATING_SYSTEM_VERSION);

            // The device brand field is a mess.
            UserAgent.AgentField deviceBrand = userAgent.Get(UserAgent.DEVICE_BRAND);
            if (deviceBrand.GetConfidence() >= 0)
            {
                userAgent.SetForced(
                    UserAgent.DEVICE_BRAND,
                    Normalize.Brand(deviceBrand.GetValue()),
                    deviceBrand.GetConfidence());
            }

            // The email address is a mess
            UserAgent.AgentField email = userAgent.Get("AgentInformationEmail");
            if (email != null && email.GetConfidence() >= 0)
            {
                userAgent.SetForced(
                    "AgentInformationEmail",
                    Normalize.Email(email.GetValue()),
                    email.GetConfidence());
            }

            // Make sure the DeviceName always starts with the DeviceBrand
            UserAgent.AgentField deviceName = userAgent.Get(UserAgent.DEVICE_NAME);
            if (deviceName.GetConfidence() >= 0)
            {
                deviceBrand = userAgent.Get(UserAgent.DEVICE_BRAND);
                string deviceNameValue = deviceName.GetValue();
                string deviceBrandValue = deviceBrand.GetValue();
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

            if (deviceBrand.GetConfidence() < 0)
            {
                // If no brand is known then try to extract something that looks like a Brand from things like URL and Email addresses.
                string newDeviceBrand = DetermineDeviceBrand(userAgent);
                if (newDeviceBrand != null)
                {
                    userAgent.SetForced(
                        UserAgent.DEVICE_BRAND,
                        newDeviceBrand,
                        1);
                }
            }

            return userAgent;
        }

        private string ExtractCompanyFromHostName(string hostname)
        {
            try
            {
                var domainParser = new DomainParser(new WebTldRuleProvider());
                DomainName domainName = domainParser.Get(hostname);
                return Normalize.Brand(domainName.Hostname);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string DetermineDeviceBrand(UserAgent userAgent)
        {
            // If no brand is known but we do have a URL then we assume the hostname to be the brand.
            // We put this AFTER the creation of the DeviceName because we choose to not have
            // this brandname in the DeviceName.

            UserAgent.AgentField informationUrl = userAgent.Get("AgentInformationUrl");
            if (informationUrl != null && informationUrl.GetConfidence() >= 0)
            {
                String hostname = informationUrl.GetValue();
                try
                {
                    Uri url = new Uri(hostname);
                    hostname = url.Host;
                }
                catch (Exception e)
                {
                    // Ignore any exception and continue.
                }
                hostname = ExtractCompanyFromHostName(hostname);
                if (hostname != null)
                {
                    return hostname;
                }
            }

            UserAgent.AgentField informationEmail = userAgent.Get("AgentInformationEmail");
            if (informationEmail != null && informationEmail.GetConfidence() >= 0)
            {
                string hostname = informationEmail.GetValue();
                int atOffset = hostname.IndexOf('@');
                if (atOffset >= 0)
                {
                    hostname = hostname.Substring(atOffset + 1);
                }
                hostname = ExtractCompanyFromHostName(hostname);
                if (hostname != null)
                {
                    return hostname;
                }
            }

            return null;
        }

        private void ConcatFieldValuesNONDuplicated(UserAgent userAgent, string targetName, string firstName, string secondName)
        {
            if (!IsWantedField(targetName))
            {
                return;
            }
            UserAgent.AgentField firstField = userAgent.Get(firstName);
            UserAgent.AgentField secondField = userAgent.Get(secondName);

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
                    return;
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

        private void AddMajorVersionField(UserAgent userAgent, string versionName, string majorVersionName)
        {
            if (!IsWantedField(majorVersionName))
            {
                return;
            }
            UserAgent.AgentField agentVersionMajor = userAgent.Get(majorVersionName);
            if (agentVersionMajor == null || agentVersionMajor.GetConfidence() == -1)
            {
                UserAgent.AgentField agentVersion = userAgent.Get(versionName);
                if (agentVersion != null)
                {
                    string version = agentVersion.GetValue();
                    if (version != null)
                    {
                        version = VersionSplitter.getInstance().getSingleSplit(agentVersion.GetValue(), 1);
                    }
                    userAgent.Set(
                        majorVersionName,
                        version,
                        agentVersion.GetConfidence());
                }
            }
        }

        public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
        {
            if (!informMatcherActionRanges.Keys.Contains(treeName))
            {
                informMatcherActionRanges[treeName] = new HashSet<WordRangeVisitor.Range>();
            }
            
            return informMatcherActionRanges[treeName];
        }

        public void Inform(string key, string value, IParseTree ctx)
        {
            Inform(key, key, value, ctx);
            Inform(key + "=\"" + value + '"', key, value, ctx);

            ISet<int?> lengths = GetRequiredPrefixLengths(key);
            if (lengths != null)
            {
                int valueLength = value.Length;
                foreach (int? prefixLength in lengths)
                {
                    if (valueLength >= prefixLength)
                    {
                        Inform(key + "{\"" + FirstCharactersForPrefixHash(value, prefixLength.Value) + '"', key, value, ctx);
                    }
                }
            }
        }

        private void Inform(string match, string key, string value, IParseTree ctx)
        {
            HashSet<MatcherAction> relevantActions = informMatcherActions[match.ToLower(CultureInfo.InvariantCulture)];
            if (verbose)
            {
                if (relevantActions == null)
                {
                    LOG.Info(string.Format("--- Have (0): {0}", match));
                }
                else
                {
                    LOG.Info(string.Format("+++ Have ({0}): {1}", relevantActions.Count, match));

                    int count = 1;
                    foreach (MatcherAction action in relevantActions)
                    {
                        LOG.Info(string.Format("+++ -------> ({0}): {1}", count, action.ToString()));
                        count++;
                    }
                }
            }

            if (relevantActions != null)
            {
                foreach (MatcherAction matcherAction in  relevantActions)
                {
                    matcherAction.Inform(key, value, ctx);
                }
            }
        }

        public void PreHeat()
        {
            PreHeat(testCases.Count, true);
        }

        public void PreHeat(int preheatIterations)
        {
            PreHeat(preheatIterations, true);
        }

        public void PreHeat(int preheatIterations, bool log)
        {
            if (testCases.Count == 0 || preheatIterations == 0)
            {
                LOG.Warn("NO PREHEAT WAS DONE. Simply because there are no test cases available.");
            }
            else
            {
                if (log)
                {
                    LOG.Info(string.Format("Preheating JVM by running {0} testcases.", preheatIterations));
                }
                int remainingIterations = preheatIterations;
                int goodResults = 0;
                while (remainingIterations > 0)
                {
                    foreach (Dictionary<string, Dictionary<string, string>> test in testCases)
                    {
                        Dictionary<string, string> input = test["input"];
                        if (input == null)
                        {
                            continue;
                        }

                        string userAgentString = input["user_agent_string"];
                        if (userAgentString == null)
                        {
                            continue;
                        }
                        remainingIterations--;
                        // Calculate and use result to guarantee not optimized away.
                        if (!Parse(userAgentString).HasSyntaxError) {
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
                    LOG.Info(string.Format("Preheating JVM completed. ({0} of {1} were proper results)", goodResults, preheatIterations));
                }
            }
        }

        // ===============================================================================================================

        public class GetAllPathsAnalyzer: IAnalyzer
        {
            internal readonly List<string> values = new List<string>();
            internal readonly UserAgentTreeFlattener flattener;

            private readonly UserAgent result;

            internal GetAllPathsAnalyzer(string useragent)
            {
                flattener = new UserAgentTreeFlattener(this);
                result = flattener.Parse(useragent);
            }

            public List<string> GetValues()
            {
                return values;
            }

            public UserAgent GetResult()
            {
                return result;
            }

            public void Inform(string path, string value, IParseTree ctx)
            {
                values.Add(path);
                values.Add(path + "=\"" + value + "\"");
                values.Add(path + "{\"" + FirstCharactersForPrefixHash(value, MAX_PREFIX_HASH_MATCH) + "\"");
            }

            public void InformMeAbout(MatcherAction matcherAction, string keyPattern)
            {
            }

            public void LookingForRange(string treeName, WordRangeVisitor.Range range)
            {
               
            }

            public ISet<WordRangeVisitor.Range> GetRequiredInformRanges(string treeName)
            {
                return new HashSet<WordRangeVisitor.Range>();
            }

            public void InformMeAboutPrefix(MatcherAction matcherAction, string treeName, string prefix)
            {
               
            }

            public ISet<int?> GetRequiredPrefixLengths(string treeName)
            {
                return new HashSet<int?>();
            }
        }

        public static List<string> GetAllPaths(string agent)
        {
            return new GetAllPathsAnalyzer(agent).GetValues();
        }

        public static GetAllPathsAnalyzer getAllPathsAnalyzer(string agent)
        {
            return new GetAllPathsAnalyzer(agent);
        }

        // ===============================================================================================================

        public static UserAgentAnalyzerDirectBuilder<UAA, B> NewBuilder<UAA, B>() 
            where UAA : UserAgentAnalyzerDirect, new()
            where B : UserAgentAnalyzerDirectBuilder<UAA, B>, new()
        {
            var a = new UAA();
            var b = new B();
            b.SetUAA(a);
            return b;
        }

        public class UserAgentAnalyzerDirectBuilder<UAA, B> where UAA: UserAgentAnalyzerDirect where B: UserAgentAnalyzerDirectBuilder<UAA, B>
        {
            private UAA uaa;
            private bool didBuildStep = false;
            private int preheatIterations = 0;

            private List<string> resources = new List<string>();

            protected void FailIfAlreadyBuilt()
            {
                if (didBuildStep)
                {
                    throw new Exception("A builder can provide only a single instance. It is not allowed to set values after doing build()");
                }
            }

            protected UserAgentAnalyzerDirectBuilder(UAA newUaa)
            {
                uaa = newUaa;
                uaa.SetShowMatcherStats(false);
                resources.Add(DEFAULT_RESOURCES);
            }

            /// <summary>
            /// Drop the default set of rules. Useful in parsing ONLY company specific useragents.
            /// </summary>
            /// <returns>the current Builder instance.</returns>
            public B DropDefaultResources()
            {
                FailIfAlreadyBuilt();
                resources.Remove(DEFAULT_RESOURCES);
                return (B)this;
            }

            /// <summary>
            /// Add a set of additional rules. Useful in handling specific cases.
            /// </summary>
            /// <param name="resourceString">resourceString The resource list that needs to be added.</param>
            /// <returns>the current Builder instance.</returns>
            public B AddResources(string resourceString)
            {
                FailIfAlreadyBuilt();
                resources.Add(resourceString);
                return (B)this;
            }

            /// <summary>
            /// Use the available testcases to preheat the jvm on this analyzer.
            /// </summary>
            /// <param name="iterations">iterations How many testcases must be run</param>
            /// <returns>the current Builder instance.</returns>
            public B Preheat(int iterations)
            {
                FailIfAlreadyBuilt();
                preheatIterations = iterations;
                return (B)this;
            }

            /// <summary>
            /// Use the available testcases to preheat the jvm on this analyzer.
            /// All available testcases will be run exactly once.
            /// </summary>
            /// <returns>the current Builder instance.</returns>
            public B Preheat()
            {
                FailIfAlreadyBuilt();
                this.preheatIterations = -1;
                return (B)this;
            }

           
            /// <summary>
            /// Specify an additional field that we want to retrieve.
            /// </summary>
            /// <param name="fieldName">The name of the additional field</param>
            /// <returns>the current Builder instance.</returns>
            public B WithField(string fieldName)
            {
                FailIfAlreadyBuilt();
                if (uaa.wantedFieldNames == null)
                {
                    uaa.wantedFieldNames = new List<string>();
                }
                uaa.wantedFieldNames.Add(fieldName);
                return (B)this;
            }

           
            /// <summary>
            /// Specify a set of additional fields that we want to retrieve.
            /// </summary>
            /// <param name="fieldNames">The collection of names of the additional fields</param>
            /// <returns>the current Builder instance.</returns>
            public B WithFields(List<string> fieldNames)
            {
                foreach (string fieldName in fieldNames)
                {
                    WithField(fieldName);
                }
                return (B)this;
            }

           
            /// <summary>
            /// Specify a set of additional fields that we want to retrieve.
            /// </summary>
            /// <param name="fieldNames">
            /// The array of names of the additional fields
            /// </param>
            /// <returns>the current Builder instance.</returns>
            public B WithFields(params string[] fieldNames)
            {
                foreach (string fieldName in fieldNames)
                {
                    WithField(fieldName);
                }
                return (B)this;
            }

          
            /// <summary>
            ///  Specify that we simply want to retrieve all possible fields.
            /// </summary>
            /// <returns>
            /// the current Builder instance.
            /// </returns>
            public B WithAllFields()
            {
                FailIfAlreadyBuilt();
                uaa.wantedFieldNames = null;
                return (B)this;
            }


            /// <summary>
            /// Log additional information during the startup of the analyzer.
            /// </summary>
            /// <returns>
            /// the current Builder instance.
            /// </returns>
            public B ShowMatcherLoadStats()
            {
                FailIfAlreadyBuilt();
                uaa.SetShowMatcherStats(true);
                return (B)this;
            }

            /// <summary>
            /// Set the stats logging during the startup of the analyzer back to the default of "minimal".
            /// </summary>
            /// <returns>
            /// the current Builder instance.
            /// </returns>
            public B HideMatcherLoadStats()
            {
                FailIfAlreadyBuilt();
                uaa.SetShowMatcherStats(false);
                return (B)this;
            }


            /// <summary>
            /// Set maximum length of a useragent for it to be classified as Hacker without any analysis.
            /// </summary>
            /// <param name="newUserAgentMaxLength">
            /// newUserAgentMaxLength The new maximum length of a useragent for it to be classified as Hacker without any analysis.
            /// </param>
            /// <returns>
            /// the current Builder instance.
            /// </returns>
            public B WithUserAgentMaxLength(int newUserAgentMaxLength)
            {
                FailIfAlreadyBuilt();
                uaa.SetUserAgentMaxLength(newUserAgentMaxLength);
                return (B)this;
            }


            /// <summary>
            /// Retain all testcases in memory after initialization.
            /// </summary>
            /// <returns>
            /// the current Builder instance.
            /// </returns>
            public B KeepTests()
            {
                FailIfAlreadyBuilt();
                uaa.KeepTests();
                return (B)this;
            }

            /// <summary>
            ///  Remove all testcases in memory after initialization.
            /// </summary>
            /// <returns>
            /// the current Builder instance.
            /// </returns>
            public B DropTests()
            {
                FailIfAlreadyBuilt();
                uaa.DropTests();
                return (B)this;
            }


            /// <summary>
            /// Load all patterns and rules but do not yet build the lookup hashMaps yet.
            /// For the engine to run these lookup hashMaps are needed so they will be constructed once "just in time".
            /// </summary>
            /// <returns>
            ///  the current Builder instance.
            /// </returns>
            public B DelayInitialization()
            {
                FailIfAlreadyBuilt();
                uaa.DelayInitialization();
                return (B)this;
            }


            /// <summary>
            /// Load all patterns and rules and immediately build the lookup hashMaps.
            /// </summary>
            /// <returns>the current Builder instance.</returns>
            public B ImmediateInitialization()
            {
                FailIfAlreadyBuilt();
                uaa.ImmediateInitialization();
                return (B)this;
            }

            private void AddGeneratedFields(string result, params string[] dependencies)
            {
                if (uaa.wantedFieldNames.Contains(result))
                {
                    uaa.wantedFieldNames.AddRange(dependencies);
                }
            }

            /// <summary>
            /// Construct the analyzer and run the preheat (if requested).
            /// </summary>
            /// <returns>
            /// the new analyzer instance.
            /// </returns>
            public virtual UAA Build()
            {
                FailIfAlreadyBuilt();
                if (uaa.wantedFieldNames != null)
                {
                    AddGeneratedFields("AgentNameVersion",  UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION);
                    AddGeneratedFields("AgentNameVersionMajor", UserAgent.AGENT_NAME, UserAgent.AGENT_VERSION_MAJOR);
                    AddGeneratedFields("WebviewAppNameVersionMajor", "WebviewAppName", "WebviewAppVersionMajor");
                    AddGeneratedFields("LayoutEngineNameVersion", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION);
                    AddGeneratedFields("LayoutEngineNameVersionMajor", UserAgent.LAYOUT_ENGINE_NAME, UserAgent.LAYOUT_ENGINE_VERSION_MAJOR);
                    AddGeneratedFields("OperatingSystemNameVersion", UserAgent.OPERATING_SYSTEM_NAME, UserAgent.OPERATING_SYSTEM_VERSION);
                    AddGeneratedFields(UserAgent.DEVICE_NAME, UserAgent.DEVICE_BRAND);
                    AddGeneratedFields(UserAgent.AGENT_VERSION_MAJOR, UserAgent.AGENT_VERSION);
                    AddGeneratedFields(UserAgent.LAYOUT_ENGINE_VERSION_MAJOR, UserAgent.LAYOUT_ENGINE_VERSION);
                    AddGeneratedFields("WebviewAppVersionMajor", "WebviewAppVersion");
                    
                    // If we do not have a Brand we try to extract it from URL/Email iff present.
                    AddGeneratedFields(UserAgent.DEVICE_BRAND, "AgentInformationUrl", "AgentInformationEmail");

                    // Special field that affects ALL fields.
                    uaa.wantedFieldNames.Add(UserAgent.SET_ALL_FIELDS);

                    // This is always needed to determine the Hacker fallback
                    uaa.wantedFieldNames.Add(UserAgent.DEVICE_CLASS);
                }

                bool mustDropTestsLater = !uaa.WillKeepTests();
                if (preheatIterations != 0)
                {
                    uaa.KeepTests();
                }
                uaa.Initialize(resources);
                if (preheatIterations < 0)
                {
                    uaa.PreHeat();
                }
                else
                {
                    if (preheatIterations > 0)
                    {
                        uaa.PreHeat(preheatIterations);
                    }
                }
                if (mustDropTestsLater)
                {
                    uaa.DropTests();
                }
                didBuildStep = true;
                return uaa;
            }

            internal void SetUAA(UAA a)
            {
                uaa = a;
            }
        }

    }
    
}
