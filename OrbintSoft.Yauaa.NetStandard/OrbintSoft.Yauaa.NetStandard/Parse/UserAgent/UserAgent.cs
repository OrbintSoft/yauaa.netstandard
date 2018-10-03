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

using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using log4net;
using Newtonsoft.Json;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Antlr4Source;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent
{
    public class UserAgent : UserAgentBaseListener, IParserErrorListener, IAntlrErrorListener<int>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(UserAgent));
        public static readonly string DEVICE_CLASS = "DeviceClass";
        public static readonly string DEVICE_BRAND = "DeviceBrand";
        public static readonly string DEVICE_NAME = "DeviceName";
        public static readonly string DEVICE_VERSION = "DeviceVersion";
        public static readonly string OPERATING_SYSTEM_CLASS = "OperatingSystemClass";
        public static readonly string OPERATING_SYSTEM_NAME = "OperatingSystemName";
        public static readonly string OPERATING_SYSTEM_VERSION = "OperatingSystemVersion";
        public static readonly string LAYOUT_ENGINE_CLASS = "LayoutEngineClass";
        public static readonly string LAYOUT_ENGINE_NAME = "LayoutEngineName";
        public static readonly string LAYOUT_ENGINE_VERSION = "LayoutEngineVersion";
        public static readonly string LAYOUT_ENGINE_VERSION_MAJOR = "LayoutEngineVersionMajor";
        public static readonly string AGENT_CLASS = "AgentClass";
        public static readonly string AGENT_NAME = "AgentName";
        public static readonly string AGENT_VERSION = "AgentVersion";
        public static readonly string AGENT_VERSION_MAJOR = "AgentVersionMajor";

        public static readonly string SYNTAX_ERROR = "__SyntaxError__";
        public static readonly string USERAGENT = "Useragent";

        public static readonly string SET_ALL_FIELDS = "__Set_ALL_Fields__";
        public static readonly string NULL_VALUE = "<<<null>>>";
        public static readonly string UNKNOWN_VALUE = "Unknown";
        public static readonly string UNKNOWN_VERSION = "??";

        public static readonly string[] STANDARD_FIELDS = {
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

        public bool HasSyntaxError { get; private set; }
        public bool HasAmbiguity { get; private set; }
        public int AmbiguityCount { get; private set; }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (debug)
            {
                LOG.Error("Syntax error");
                LOG.Error(string.Format("Source : {0}", userAgentString));
                LOG.Error(string.Format("Message: {0}", msg));
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

        // The original input value
        private string userAgentString = null;

        private bool debug = false;


        public bool IsDebug()
        {
            return debug;
        }

        public void setDebug(bool newDebug)
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
                   Equals(allFields, agent.allFields);
        }

        public override int GetHashCode()
        {
            return ValueTuple.Create(userAgentString, allFields).GetHashCode();
        }

        public class AgentField
        {
            private readonly string defaultValue;
            private string value;

            private long confidence;

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

        private readonly Dictionary<string, AgentField> allFields = new Dictionary<String, AgentField>();

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

        public void Clone(UserAgent userAgent)
        {
            Init();
            SetUserAgentString(userAgent.userAgentString);
            foreach (var entry in userAgent.allFields)
            {
                Set(entry.Key, entry.Value.GetValue(), entry.Value.GetConfidence());
            }
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
            AgentField setAllField = allFields[SET_ALL_FIELDS];
            if (setAllField == null)
            {
                return;
            }
            string value = setAllField.GetValue();
            long confidence = setAllField.GetConfidence();
            foreach (var fieldEntry in allFields)
            {
                if (!IsSystemField(fieldEntry.Key))
                {
                    fieldEntry.Value.SetValue(value, confidence);
                }
            }
        }

        public virtual void Set(String attribute, String value, long confidence)
        {
            AgentField field = allFields[attribute];
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
                    LOG.Info(string.Format("USE  {0} ({1}) = {2}", attribute, confidence, value));
                }
                else
                {
                    LOG.Info(string.Format("SKIP {0} ({1}) = {2}", attribute, confidence, value));
                }
            }
            allFields[attribute] = field;
        }

        public void SetForced(String attribute, String value, long confidence)
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
                LOG.Info(string.Format("USE  {0} ({1}) = {2}", attribute, confidence, value));
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
            Set(fieldName, agentField.GetValue(), agentField.GetConfidence());
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
                return allFields[fieldName];
            }
        }

        public string GetValue(string fieldName)
        {
            if (USERAGENT.Equals(fieldName))
            {
                return userAgentString;
            }
            AgentField field = allFields[fieldName];
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
            AgentField field = allFields[fieldName];
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
                    sb.Append("# ").Append(string.Format("%5d", Get(fieldName).GetConfidence()));
                }
                if (comments != null)
                {
                    string comment = comments[fieldName];
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
                        .Append('"').AppendFormat(JsonConvert.ToString(GetUserAgentString())).Append('"');
                }
                else
                {
                    sb
                        .Append('"').Append(JsonConvert.ToString(fieldName)).Append('"')
                        .Append(':')
                        .Append('"').Append(JsonConvert.ToString(GetValue(fieldName))).Append('"');
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return base.ToString();
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
            resultSet.AddRange(STANDARD_FIELDS);
            foreach (string fieldName in allFields.Keys)
            {
                if (!resultSet.Contains(fieldName))
                {
                    AgentField field = allFields[fieldName];
                    if (field != null && field.GetConfidence() >= 0 && field.GetValue() != null)
                    {
                        resultSet.Add(fieldName);
                    }
                }
            }

            // This is not a field; this is a special operator.
            resultSet.Remove(SET_ALL_FIELDS);
            return resultSet;
        }

        // We manually sort the list of fields to ensure the output is consistent.
        // Any unspecified fieldnames will be appended to the end.
        public static readonly List<string> PRE_SORTED_FIELDS_LIST = new List<string>(32);

        static UserAgent()
        {
            PRE_SORTED_FIELDS_LIST.Add("DeviceClass");
            PRE_SORTED_FIELDS_LIST.Add("DeviceName");
            PRE_SORTED_FIELDS_LIST.Add("DeviceBrand");
            PRE_SORTED_FIELDS_LIST.Add("DeviceCpu");
            PRE_SORTED_FIELDS_LIST.Add("DeviceCpuBits");
            PRE_SORTED_FIELDS_LIST.Add("DeviceFirmwareVersion");
            PRE_SORTED_FIELDS_LIST.Add("DeviceVersion");

            PRE_SORTED_FIELDS_LIST.Add("OperatingSystemClass");
            PRE_SORTED_FIELDS_LIST.Add("OperatingSystemName");
            PRE_SORTED_FIELDS_LIST.Add("OperatingSystemVersion");
            PRE_SORTED_FIELDS_LIST.Add("OperatingSystemNameVersion");
            PRE_SORTED_FIELDS_LIST.Add("OperatingSystemVersionBuild");

            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineClass");
            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineName");
            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineVersion");
            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineVersionMajor");
            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineNameVersion");
            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineNameVersionMajor");
            PRE_SORTED_FIELDS_LIST.Add("LayoutEngineBuild");

            PRE_SORTED_FIELDS_LIST.Add("AgentClass");
            PRE_SORTED_FIELDS_LIST.Add("AgentName");
            PRE_SORTED_FIELDS_LIST.Add("AgentVersion");
            PRE_SORTED_FIELDS_LIST.Add("AgentVersionMajor");
            PRE_SORTED_FIELDS_LIST.Add("AgentNameVersion");
            PRE_SORTED_FIELDS_LIST.Add("AgentNameVersionMajor");
            PRE_SORTED_FIELDS_LIST.Add("AgentBuild");
            PRE_SORTED_FIELDS_LIST.Add("AgentLanguage");
            PRE_SORTED_FIELDS_LIST.Add("AgentLanguageCode");
            PRE_SORTED_FIELDS_LIST.Add("AgentInformationEmail");
            PRE_SORTED_FIELDS_LIST.Add("AgentInformationUrl");
            PRE_SORTED_FIELDS_LIST.Add("AgentSecurity");
            PRE_SORTED_FIELDS_LIST.Add("AgentUuid");

            PRE_SORTED_FIELDS_LIST.Add("WebviewAppName");
            PRE_SORTED_FIELDS_LIST.Add("WebviewAppVersion");
            PRE_SORTED_FIELDS_LIST.Add("WebviewAppVersionMajor");
            PRE_SORTED_FIELDS_LIST.Add("WebviewAppNameVersionMajor");

            PRE_SORTED_FIELDS_LIST.Add("FacebookCarrier");
            PRE_SORTED_FIELDS_LIST.Add("FacebookDeviceClass");
            PRE_SORTED_FIELDS_LIST.Add("FacebookDeviceName");
            PRE_SORTED_FIELDS_LIST.Add("FacebookDeviceVersion");
            PRE_SORTED_FIELDS_LIST.Add("FacebookFBOP");
            PRE_SORTED_FIELDS_LIST.Add("FacebookFBSS");
            PRE_SORTED_FIELDS_LIST.Add("FacebookOperatingSystemName");
            PRE_SORTED_FIELDS_LIST.Add("FacebookOperatingSystemVersion");

            PRE_SORTED_FIELDS_LIST.Add("Anonymized");

            PRE_SORTED_FIELDS_LIST.Add("HackerAttackVector");
            PRE_SORTED_FIELDS_LIST.Add("HackerToolkit");

            PRE_SORTED_FIELDS_LIST.Add("KoboAffiliate");
            PRE_SORTED_FIELDS_LIST.Add("KoboPlatformId");

            PRE_SORTED_FIELDS_LIST.Add("IECompatibilityVersion");
            PRE_SORTED_FIELDS_LIST.Add("IECompatibilityVersionMajor");
            PRE_SORTED_FIELDS_LIST.Add("IECompatibilityNameVersion");
            PRE_SORTED_FIELDS_LIST.Add("IECompatibilityNameVersionMajor");

            PRE_SORTED_FIELDS_LIST.Add(SYNTAX_ERROR);
        }
    

        public List<string> GetAvailableFieldNamesSorted()
        {
            List<string> fieldNames = new List<string>(GetAvailableFieldNames());

            List<string> result = new List<string>();
            foreach (string fieldName in PRE_SORTED_FIELDS_LIST)
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

    }
}
