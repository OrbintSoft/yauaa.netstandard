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
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using log4net;
using Newtonsoft.Json;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS
{
    [Serializable]
    public class UserAgent : UserAgentBaseListener, IParserErrorListener, IAntlrErrorListener<int>
    {

        public const string DEVICE_CLASS = "DeviceClass";
        public const string DEVICE_BRAND = "DeviceBrand";
        public const string DEVICE_NAME = "DeviceName";
        public const string DEVICE_VERSION = "DeviceVersion";
        public const string OPERATING_SYSTEM_CLASS = "OperatingSystemClass";
        public const string OPERATING_SYSTEM_NAME = "OperatingSystemName";
        public const string OPERATING_SYSTEM_VERSION = "OperatingSystemVersion";
        public const string LAYOUT_ENGINE_CLASS = "LayoutEngineClass";
        public const string LAYOUT_ENGINE_NAME = "LayoutEngineName";
        public const string LAYOUT_ENGINE_VERSION = "LayoutEngineVersion";
        public const string LAYOUT_ENGINE_VERSION_MAJOR = "LayoutEngineVersionMajor";
        public const string AGENT_CLASS = "AgentClass";
        public const string AGENT_NAME = "AgentName";
        public const string AGENT_VERSION = "AgentVersion";
        public const string AGENT_VERSION_MAJOR = "AgentVersionMajor";

        public const string SYNTAX_ERROR = "__SyntaxError__";
        public const string USERAGENT = "Useragent";

        public const string SET_ALL_FIELDS = "__Set_ALL_Fields__";
        public const string NULL_VALUE = "<<<null>>>";
        public const string UNKNOWN_VALUE = "Unknown";
        public const string UNKNOWN_VERSION = "??";

        public static readonly string[] StandardFields = {
            DEVICE_CLASS,
            DEVICE_BRAND,
            DEVICE_NAME,
            OPERATING_SYSTEM_CLASS,
            OPERATING_SYSTEM_NAME,
            OPERATING_SYSTEM_VERSION,
            LAYOUT_ENGINE_CLASS,
            LAYOUT_ENGINE_NAME,
            LAYOUT_ENGINE_VERSION,
            LAYOUT_ENGINE_VERSION_MAJOR,
            AGENT_CLASS,
            AGENT_NAME,
            AGENT_VERSION,
            AGENT_VERSION_MAJOR
        };

        // We manually sort the list of fields to ensure the output is consistent.
        // Any unspecified fieldnames will be appended to the end.
        public static readonly List<string> PreSortedFieldList = new List<string>(32);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // The original input value
        private string userAgentString = null;

#if VERBOSE
        private bool debug = true; 
        private readonly IDictionary<string, AgentField> allFields = new SortedDictionary<string, AgentField>();
#else
        private bool debug = false;
        private readonly IDictionary<string, AgentField> allFields = new Dictionary<string, AgentField>();
#endif
        static UserAgent()
        {
            PreSortedFieldList.Add("DeviceClass");
            PreSortedFieldList.Add("DeviceName");
            PreSortedFieldList.Add("DeviceBrand");
            PreSortedFieldList.Add("DeviceCpu");
            PreSortedFieldList.Add("DeviceCpuBits");
            PreSortedFieldList.Add("DeviceFirmwareVersion");
            PreSortedFieldList.Add("DeviceVersion");

            PreSortedFieldList.Add("OperatingSystemClass");
            PreSortedFieldList.Add("OperatingSystemName");
            PreSortedFieldList.Add("OperatingSystemVersion");
            PreSortedFieldList.Add("OperatingSystemNameVersion");
            PreSortedFieldList.Add("OperatingSystemVersionBuild");

            PreSortedFieldList.Add("LayoutEngineClass");
            PreSortedFieldList.Add("LayoutEngineName");
            PreSortedFieldList.Add("LayoutEngineVersion");
            PreSortedFieldList.Add("LayoutEngineVersionMajor");
            PreSortedFieldList.Add("LayoutEngineNameVersion");
            PreSortedFieldList.Add("LayoutEngineNameVersionMajor");
            PreSortedFieldList.Add("LayoutEngineBuild");

            PreSortedFieldList.Add("AgentClass");
            PreSortedFieldList.Add("AgentName");
            PreSortedFieldList.Add("AgentVersion");
            PreSortedFieldList.Add("AgentVersionMajor");
            PreSortedFieldList.Add("AgentNameVersion");
            PreSortedFieldList.Add("AgentNameVersionMajor");
            PreSortedFieldList.Add("AgentBuild");
            PreSortedFieldList.Add("AgentLanguage");
            PreSortedFieldList.Add("AgentLanguageCode");
            PreSortedFieldList.Add("AgentInformationEmail");
            PreSortedFieldList.Add("AgentInformationUrl");
            PreSortedFieldList.Add("AgentSecurity");
            PreSortedFieldList.Add("AgentUuid");

            PreSortedFieldList.Add("WebviewAppName");
            PreSortedFieldList.Add("WebviewAppVersion");
            PreSortedFieldList.Add("WebviewAppVersionMajor");
            PreSortedFieldList.Add("WebviewAppNameVersionMajor");

            PreSortedFieldList.Add("FacebookCarrier");
            PreSortedFieldList.Add("FacebookDeviceClass");
            PreSortedFieldList.Add("FacebookDeviceName");
            PreSortedFieldList.Add("FacebookDeviceVersion");
            PreSortedFieldList.Add("FacebookFBOP");
            PreSortedFieldList.Add("FacebookFBSS");
            PreSortedFieldList.Add("FacebookOperatingSystemName");
            PreSortedFieldList.Add("FacebookOperatingSystemVersion");

            PreSortedFieldList.Add("Anonymized");

            PreSortedFieldList.Add("HackerAttackVector");
            PreSortedFieldList.Add("HackerToolkit");

            PreSortedFieldList.Add("KoboAffiliate");
            PreSortedFieldList.Add("KoboPlatformId");

            PreSortedFieldList.Add("IECompatibilityVersion");
            PreSortedFieldList.Add("IECompatibilityVersionMajor");
            PreSortedFieldList.Add("IECompatibilityNameVersion");
            PreSortedFieldList.Add("IECompatibilityNameVersionMajor");

            PreSortedFieldList.Add(SYNTAX_ERROR);
        }

        public UserAgent()
        {
            Init();
        }

        public UserAgent(string userAgentString)
        {
            Init();
            SetUserAgentString(userAgentString);
        }

        public UserAgent(UserAgent userAgent)
        {
            Clone(userAgent);
        }

        public bool HasSyntaxError { get; private set; }
        public bool HasAmbiguity { get; private set; }
        public int AmbiguityCount { get; private set; }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (debug)
            {
                Log.Error("Syntax error");
                Log.Error(string.Format("Source : {0}", userAgentString));
                Log.Error(string.Format("Message: {0}", msg));
            }
            HasSyntaxError = true;
            AgentField syntaxError = new AgentField("false");
            syntaxError.SetValue("true", 1);
            allFields[SYNTAX_ERROR] = syntaxError;
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            SyntaxError(recognizer, null, line, charPositionInLine, msg, e);
        }

        public void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
        {
            HasAmbiguity = true;
            AmbiguityCount++;
        }

        public void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
        {

        }

        public void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
        {

        }

        public bool IsDebug()
        {
            return debug;
        }

        public void SetDebug(bool newDebug)
        {
            debug = newDebug;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is UserAgent))
            {
                return false;
            }          
            UserAgent agent = (UserAgent)obj;
            return Equals(userAgentString, agent.userAgentString) &&
                   (allFields == agent.allFields || (allFields != null && allFields.SequenceEqual(agent.allFields)));
        }

        public override int GetHashCode()
        {
            return ValueTuple.Create(userAgentString, allFields).GetHashCode();
        }

        public void Clone(UserAgent userAgent)
        {
            Init();
            debug = userAgent.debug;
            SetUserAgentString(userAgent.userAgentString);

            foreach (var entry in userAgent.allFields)
            {
                Set(entry.Key, entry.Value.GetValue(), entry.Value.confidence);
            }
            HasSyntaxError = userAgent.HasSyntaxError;
            HasAmbiguity = userAgent.HasAmbiguity;
            AmbiguityCount = userAgent.AmbiguityCount;
        }

        private void Init()
        {
            // Device : Family - Brand - Model
            allFields[DEVICE_CLASS] = new AgentField(UNKNOWN_VALUE); // Hacker / Cloud / Server / Desktop / Tablet / Phone / Watch
            allFields[DEVICE_BRAND] = new AgentField(UNKNOWN_VALUE); // (Google/AWS/Azure) / ????
            allFields[DEVICE_NAME] = new AgentField(UNKNOWN_VALUE); // (Google/AWS/Azure) / ????

            // Operating system
            allFields[OPERATING_SYSTEM_CLASS] = new AgentField(UNKNOWN_VALUE); // Cloud, Desktop, Mobile, Embedded
            allFields[OPERATING_SYSTEM_NAME] = new AgentField(UNKNOWN_VALUE); // ( Linux / Android / Windows ...)
            allFields[OPERATING_SYSTEM_VERSION] = new AgentField(UNKNOWN_VERSION); // 1.2 / 43 / ...

            // Engine : Class (=None/Hacker/Robot/Browser) - Name - Version
            allFields[LAYOUT_ENGINE_CLASS] = new AgentField(UNKNOWN_VALUE); // None / Hacker / Robot / Browser /
            allFields[LAYOUT_ENGINE_NAME] = new AgentField(UNKNOWN_VALUE); // ( GoogleBot / Bing / ...) / (Trident / Gecko / ...)
            allFields[LAYOUT_ENGINE_VERSION] = new AgentField(UNKNOWN_VERSION); // 1.2 / 43 / ...
            allFields[LAYOUT_ENGINE_VERSION_MAJOR] = new AgentField(UNKNOWN_VERSION); // 1 / 43 / ...

            // Agent: Class (=Hacker/Robot/Browser) - Name - Version
            allFields[AGENT_CLASS] = new AgentField(UNKNOWN_VALUE); // Hacker / Robot / Browser /
            allFields[AGENT_NAME] = new AgentField(UNKNOWN_VALUE); // ( GoogleBot / Bing / ...) / ( Firefox / Chrome / ... )
            allFields[AGENT_VERSION] = new AgentField(UNKNOWN_VERSION); // 1.2 / 43 / ...
            allFields[AGENT_VERSION_MAJOR] = new AgentField(UNKNOWN_VERSION); // 1 / 43 / ...
        }

        public void SetUserAgentString(string newUserAgentString)
        {
            userAgentString = newUserAgentString;
            Reset();
        }

        public string GetUserAgentString()
        {
            return userAgentString;
        }

        public virtual void Reset()
        {
            HasSyntaxError = false;
            HasAmbiguity = false;
            AmbiguityCount = 0;

            foreach (AgentField field in allFields.Values)
            {
                field.Reset();
            }
        }

        internal static bool IsSystemField(string fieldname)
        {
            return SET_ALL_FIELDS.Equals(fieldname) ||
                    SYNTAX_ERROR.Equals(fieldname) ||
                    USERAGENT.Equals(fieldname);
        }

        public void ProcessSetAll()
        {
            AgentField setAllField = allFields.ContainsKey(SET_ALL_FIELDS) ? allFields[SET_ALL_FIELDS] : null;
            if (setAllField == null)
            {
                return;
            }
            string value = setAllField.GetValue();
            long confidence = setAllField.confidence;
            foreach (var fieldEntry in allFields)
            {
                if (!IsSystemField(fieldEntry.Key))
                {
                    fieldEntry.Value.SetValue(value, confidence);
                }
            }
        }

        public virtual void Set(string attribute, string value, long confidence)
        {
            AgentField field = allFields.ContainsKey(attribute) ? allFields[attribute] : null;
            if (field == null)
            {
                field = new AgentField(null); // The fields we do not know get a 'null' default
            }

            bool wasEmpty = confidence == -1;
            bool updated = field.SetValue(value, confidence);
            if (debug && !wasEmpty)
            {
                if (updated)
                {
                    Log.Info(string.Format("USE  {0} ({1}) = {2}", attribute, confidence, value ?? "null"));
                }
                else
                {
                    Log.Info(string.Format(" SKIP {0} ({1}) = {2}", attribute, confidence, value ?? "null"));
                }
            }
            allFields[attribute] = field;
        }

        public void SetForced(string attribute, string value, long confidence)
        {
            AgentField field = allFields[attribute];
            if (field == null)
            {
                field = new AgentField(null); // The fields we do not know get a 'null' default
            }

            bool wasEmpty = confidence == -1;
            field.SetValueForced(value, confidence);
            if (debug && !wasEmpty)
            {
                Log.Info(string.Format("USE  {0} ({1}) = {2}", attribute, confidence, value));
            }
            allFields[attribute] = field;
        }

        // The appliedMatcher parameter is needed for development and debugging.
        public virtual void Set(UserAgent newValuesUserAgent, Matcher appliedMatcher)
        {
            foreach (string fieldName in newValuesUserAgent.allFields.Keys)
            {
                Set(fieldName, newValuesUserAgent.allFields[fieldName]);
            }
        }

        private void Set(string fieldName, AgentField agentField)
        {
            Set(fieldName, agentField.GetValue(), agentField.confidence);
        }

        public AgentField Get(string fieldName)
        {
            if (USERAGENT.Equals(fieldName))
            {
                AgentField agentField = new AgentField(userAgentString);
                agentField.SetValue(userAgentString, 0L);
                return agentField;
            }
            else
            {
                return allFields.ContainsKey(fieldName) ? allFields[fieldName] : null;
            }
        }

        public string GetValue(string fieldName)
        {
            if (USERAGENT.Equals(fieldName))
            {
                return userAgentString;
            }
            AgentField field = allFields.ContainsKey(fieldName) ? allFields[fieldName] : null;
            if (field == null)
            {
                return UNKNOWN_VALUE;
            }
            return field.GetValue();
        }

        public long GetConfidence(string fieldName)
        {
            if (USERAGENT.Equals(fieldName))
            {
                return 0L;
            }
            AgentField field = allFields.ContainsKey(fieldName) ? allFields[fieldName] : null;
            if (field == null)
            {
                return -1L;
            }
            return field.GetConfidence();
        }

        public string ToYamlTestCase()
        {
            return ToYamlTestCase(false, null);
        }
        public string ToYamlTestCase(bool showConfidence)
        {
            return ToYamlTestCase(showConfidence, null);
        }

        public string ToYamlTestCase(bool showConfidence, Dictionary<string, string> comments)
        {
            StringBuilder sb = new StringBuilder(10240);
            sb.Append("\n");
            sb.Append("- test:\n");
            //        sb.append("#    options:\n");
            //        sb.append("#    - 'verbose'\n");
            //        sb.append("#    - 'init'\n");
            //        sb.append("#    - 'only'\n");
            sb.Append("    input:\n");
            //        sb.append("#      name: 'You can give the test case a name'\n");
            sb.Append("      user_agent_string: '").Append(userAgentString).Append("'\n");
            sb.Append("    expected:\n");

            List<string> fieldNames = GetAvailableFieldNamesSorted();

            int maxNameLength = 30;
            int maxValueLength = 0;
            foreach (string fieldName in allFields.Keys)
            {
                maxNameLength = Math.Max(maxNameLength, fieldName.Length);
            }
            foreach (string fieldName in fieldNames)
            {
                maxValueLength = Math.Max(maxValueLength, Get(fieldName).GetValue().Length);
            }

            foreach (string fieldName in fieldNames)
            {
                sb.Append("      ").Append(fieldName);
                for (int l = fieldName.Length; l < maxNameLength + 7; l++)
                {
                    sb.Append(' ');
                }
                string value = Get(fieldName).GetValue();
                sb.Append(": '").Append(value).Append('\'');
                if (showConfidence)
                {
                    for (int l = value.Length; l < maxValueLength + 5; l++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append("# ").Append(string.Format("%5d", Get(fieldName).confidence));
                }
                if (comments != null)
                {
                    string comment = comments.ContainsKey(fieldName) ? comments[fieldName] : null;
                    if (comment != null)
                    {
                        sb.Append(" | ").Append(comment);
                    }
                }
                sb.Append('\n');
            }
            sb.Append("\n\n");

            return sb.ToString();
        }

        //    {
        //        "agent": {
        //            "user_agent_string": "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46
        //                                  (KHTML, like Gecko) Version/9.0 Mobile/13D15 Safari/601.1"
        //            "AgentClass": "Browser",
        //            "AgentName": "Safari",
        //            "AgentVersion": "9.0",
        //            "DeviceBrand": "Apple",
        //            "DeviceClass": "Phone",
        //            "DeviceFirmwareVersion": "13D15",
        //            "DeviceName": "iPhone",
        //            "LayoutEngineClass": "Browser",
        //            "LayoutEngineName": "AppleWebKit",
        //            "LayoutEngineVersion": "601.1.46",
        //            "OperatingSystemClass": "Mobile",
        //            "OperatingSystemName": "iOS",
        //            "OperatingSystemVersion": "9_2_1",
        //        }
        //    }

        public string ToJson()
        {
            List<string> fields = GetAvailableFieldNames();
            fields.Add("Useragent");
            return ToJson(fields);
        }

        public string ToJson(List<string> fieldNames)
        {
            StringBuilder sb = new StringBuilder(10240);
            sb.Append("{");            
            bool addSeparator = false;
            foreach (string fieldName in fieldNames)
            {
                if (addSeparator)
                {
                    sb.Append(',');
                }
                else
                {
                    addSeparator = true;
                }
                if ("Useragent".Equals(fieldName))
                {
                    sb
                        .Append("\"Useragent\"")
                        .Append(':')
                        .Append(JsonConvert.ToString(GetUserAgentString()));
                }
                else
                {
                    sb
                        .Append(JsonConvert.ToString(fieldName))
                        .Append(':')
                        .Append(JsonConvert.ToString(GetValue(fieldName)));
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(GetAvailableFieldNamesSorted());
        }

        public string ToString(List<string> fieldNames)
        {
            StringBuilder sb = new StringBuilder("  - user_agent_string: '\"" + userAgentString + "\"'\n");
            int maxLength = 0;
            foreach (string fieldName in fieldNames)
            {
                maxLength = Math.Max(maxLength, fieldName.Length);
            }
            foreach (string fieldName in fieldNames)
            {
                if (!"Useragent".Equals(fieldName))
                {
                    AgentField field = allFields[fieldName];
                    if (field.GetValue() != null)
                    {
                        sb.Append("    ").Append(fieldName);
                        for (int l = fieldName.Length; l < maxLength + 2; l++)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(": '").Append(field.GetValue()).Append('\'');
                        sb.Append('\n');
                    }
                }
            }
            return sb.ToString();
        }

        public List<string> GetAvailableFieldNames()
        {
            List<string> resultSet = new List<string>(allFields.Count + 10);
            resultSet.AddRange(StandardFields);
            foreach (string fieldName in allFields.Keys)
            {
                if (!resultSet.Contains(fieldName))
                {
                    AgentField field = allFields[fieldName];
                    if (field != null && field.confidence >= 0 && field.GetValue() != null)
                    {
                        resultSet.Add(fieldName);
                    }
                }
            }

            // This is not a field; this is a special operator.
            resultSet.Remove(SET_ALL_FIELDS);
            return resultSet;
        }

        public List<string> GetAvailableFieldNamesSorted()
        {
            List<string> fieldNames = new List<string>(GetAvailableFieldNames());

            List<string> result = new List<string>();
            foreach (string fieldName in PreSortedFieldList)
            {
                if (fieldNames.Remove(fieldName))
                {
                    result.Add(fieldName);
                }
            }

            fieldNames.Sort();
            result.AddRange(fieldNames);
            return result;

        }

        [Serializable]
        public class AgentField
        {
            private readonly string defaultValue;
            private string value;

            internal long confidence;

            internal AgentField(string defaultValue)
            {
                this.defaultValue = defaultValue;
                Reset();
            }

            public void Reset()
            {
                value = defaultValue;
                confidence = -1;
            }

            public string GetValue()
            {
                if (value == null)
                {
                    return defaultValue;
                }
                return value;
            }

            public long GetConfidence()
            {
                if (value == null)
                {
                    return -1; // Lie in case the value was wiped.
                }
                return confidence;
            }


            public bool SetValue(AgentField field)
            {
                return SetValue(field.value, field.confidence);
            }

            public bool SetValue(string newValue, long newConfidence)
            {
                if (newConfidence > confidence)
                {
                    confidence = newConfidence;

                    if (NULL_VALUE.Equals(newValue))
                    {
                        value = defaultValue;
                    }
                    else
                    {
                        value = newValue;
                    }
                    return true;
                }
                return false;
            }

            public void SetValueForced(string newValue, long newConfidence)
            {
                this.confidence = newConfidence;

                if (NULL_VALUE.Equals(newValue))
                {
                    value = defaultValue;
                }
                else
                {
                    value = newValue;
                }
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (!(obj is AgentField))
                {
                    return false;
                }
                AgentField that = (AgentField)obj;
                return confidence == that.confidence &&
                    Equals(defaultValue, that.defaultValue) &&
                    Equals(value, that.value);
            }

            public override int GetHashCode()
            {
                return ValueTuple.Create(defaultValue, value, confidence).GetHashCode();
            }

            public override string ToString()
            {
                return ">" + value + "#" + confidence + "<";
            }
        }
    }
}
