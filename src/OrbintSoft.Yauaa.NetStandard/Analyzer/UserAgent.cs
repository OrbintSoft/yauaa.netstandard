//-----------------------------------------------------------------------
// <copyright file="UserAgent.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:51</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyzer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Atn;
    using Antlr4.Runtime.Dfa;
    using Antlr4.Runtime.Sharpen;
    using log4net;
    using Newtonsoft.Json;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// Defines the <see cref="UserAgent" />.
    /// </summary>
    [Serializable]
    public class UserAgent : UserAgentBaseListener, IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        /// <summary>
        /// Defines the AGENT_CLASS.
        /// </summary>
        public const string AGENT_CLASS = "AgentClass";

        /// <summary>
        /// Defines the AGENT_NAME.
        /// </summary>
        public const string AGENT_NAME = "AgentName";

        /// <summary>
        /// Defines the AGENT_VERSION.
        /// </summary>
        public const string AGENT_VERSION = "AgentVersion";

        /// <summary>
        /// Defines the AGENT_VERSION_MAJOR.
        /// </summary>
        public const string AGENT_VERSION_MAJOR = "AgentVersionMajor";

        /// <summary>
        /// Defines the DEVICE_BRAND.
        /// </summary>
        public const string DEVICE_BRAND = "DeviceBrand";

        /// <summary>
        /// Defines the DEVICE_CLASS.
        /// </summary>
        public const string DEVICE_CLASS = "DeviceClass";

        /// <summary>
        /// Defines the DEVICE_NAME.
        /// </summary>
        public const string DEVICE_NAME = "DeviceName";

        /// <summary>
        /// Defines the DEVICE_VERSION.
        /// </summary>
        public const string DEVICE_VERSION = "DeviceVersion";

        /// <summary>
        /// Defines the LAYOUT_ENGINE_CLASS.
        /// </summary>
        public const string LAYOUT_ENGINE_CLASS = "LayoutEngineClass";

        /// <summary>
        /// Defines the LAYOUT_ENGINE_NAME.
        /// </summary>
        public const string LAYOUT_ENGINE_NAME = "LayoutEngineName";

        /// <summary>
        /// Defines the LAYOUT_ENGINE_VERSION.
        /// </summary>
        public const string LAYOUT_ENGINE_VERSION = "LayoutEngineVersion";

        /// <summary>
        /// Defines the LAYOUT_ENGINE_VERSION_MAJOR.
        /// </summary>
        public const string LAYOUT_ENGINE_VERSION_MAJOR = "LayoutEngineVersionMajor";

        /// <summary>
        /// Defines the NULL_VALUE.
        /// </summary>
        public const string NULL_VALUE = "<<<null>>>";

        /// <summary>
        /// Defines the OPERATING_SYSTEM_CLASS.
        /// </summary>
        public const string OPERATING_SYSTEM_CLASS = "OperatingSystemClass";

        /// <summary>
        /// Defines the OPERATING_SYSTEM_NAME.
        /// </summary>
        public const string OPERATING_SYSTEM_NAME = "OperatingSystemName";

        /// <summary>
        /// Defines the OPERATING_SYSTEM_VERSION.
        /// </summary>
        public const string OPERATING_SYSTEM_VERSION = "OperatingSystemVersion";

        /// <summary>
        /// Defines the SET_ALL_FIELDS.
        /// </summary>
        public const string SET_ALL_FIELDS = "__Set_ALL_Fields__";

        /// <summary>
        /// Defines the SYNTAX_ERROR.
        /// </summary>
        public const string SYNTAX_ERROR = "__SyntaxError__";

        /// <summary>
        /// Defines the UNKNOWN_VALUE.
        /// </summary>
        public const string UNKNOWN_VALUE = "Unknown";

        /// <summary>
        /// Defines the UNKNOWN_VERSION.
        /// </summary>
        public const string UNKNOWN_VERSION = "??";

        /// <summary>
        /// Defines the USERAGENT_FIELDNAME.
        /// </summary>
        public const string USERAGENT_FIELDNAME = "Useragent";

        /// <summary>
        /// Standard fields used during parsing.
        /// </summary>
        public static readonly string[] StandardFields =
        {
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
            AGENT_VERSION_MAJOR,
        };

        /// <summary>
        /// We manually sort the list of fields to ensure the output is consistent.
        /// Any unspecified fieldnames will be appended to the end.
        /// </summary>
        protected internal static readonly IList<string> PreSortedFieldList = new List<string>(32);

        /// <summary>
        /// Defines the Log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserAgent));

        /// <summary>
        /// Defines the allFields.
        /// </summary>
        private readonly IDictionary<string, AgentField> allFields = new Dictionary<string, AgentField>();

        /// <summary>
        /// Defines the userAgentString.
        /// </summary>
        private string userAgentString = null;

        /// <summary>
        /// Initializes static members of the <see cref="UserAgent"/> class.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgent"/> class.
        /// </summary>
        public UserAgent()
        {
            this.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgent"/> class.
        /// </summary>
        /// <param name="userAgentString">The userAgentString<see cref="string"/>.</param>
        public UserAgent(string userAgentString)
        {
            this.Init();
            this.UserAgentString = userAgentString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgent"/> class.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        public UserAgent(UserAgent userAgent)
        {
            this.Clone(userAgent);
        }

        /// <summary>
        /// Gets the numer of ambiguities found.
        /// </summary>
        public int AmbiguityCount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether some fields are ambiguos.
        /// </summary>
        public bool HasAmbiguity { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user agent contains syntax errors.
        /// </summary>
        public bool HasSyntaxError { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsDebug.
        /// </summary>
        public bool IsDebug { get; set; } = false;

        /// <summary>
        /// Gets or sets the user agent strings.
        /// </summary>
        public string UserAgentString
        {
            get
            {
                return this.userAgentString;
            }

            set
            {
                this.userAgentString = value;
                this.Reset();
            }
        }

        /// <summary>
        /// The Clone.
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/>.</param>
        public void Clone(UserAgent userAgent)
        {
            this.Init();
            this.IsDebug = userAgent.IsDebug;
            this.UserAgentString = userAgent.userAgentString;

            foreach (var entry in userAgent.allFields)
            {
                this.Set(entry.Key, entry.Value.GetValue(), entry.Value.Confidence);
            }

            this.HasSyntaxError = userAgent.HasSyntaxError;
            this.HasAmbiguity = userAgent.HasAmbiguity;
            this.AmbiguityCount = userAgent.AmbiguityCount;
        }

        /// <summary>
        /// The Equals.
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
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

            var agent = (UserAgent)obj;
            return Equals(this.userAgentString, agent.userAgentString) &&
                   (this.allFields == agent.allFields || (this.allFields != null && this.allFields.SequenceEqual(agent.allFields)));
        }

        /// <summary>
        /// The Get.
        /// </summary>
        /// <param name="fieldName">The fieldName<see cref="string"/>.</param>
        /// <returns>The <see cref="AgentField"/>.</returns>
        public AgentField Get(string fieldName)
        {
            if (USERAGENT_FIELDNAME.Equals(fieldName))
            {
                var agentField = new AgentField(this.userAgentString);
                agentField.SetValue(this.userAgentString, 0L);
                return agentField;
            }
            else if (fieldName != null)
            {
                return this.allFields.ContainsKey(fieldName) ? this.allFields[fieldName] : null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The GetAvailableFieldNames.
        /// </summary>
        /// <returns>The list of available field names.</returns>
        public IList<string> GetAvailableFieldNames()
        {
            var resultSet = new List<string>(this.allFields.Count + 10);
            resultSet.AddRange(StandardFields);
            foreach (var fieldName in this.allFields.Keys)
            {
                if (!resultSet.Contains(fieldName))
                {
                    var field = this.allFields[fieldName];
                    if (field != null && field.Confidence >= 0 && field.GetValue() != null)
                    {
                        resultSet.Add(fieldName);
                    }
                }
            }

            // This is not a field; this is a special operator.
            resultSet.Remove(SET_ALL_FIELDS);
            return resultSet;
        }

        /// <summary>
        /// The GetAvailableFieldNamesSorted.
        /// </summary>
        /// <returns>The List of available field names sorted. </returns>
        public List<string> GetAvailableFieldNamesSorted()
        {
            var fieldNames = new List<string>(this.GetAvailableFieldNames());

            var result = new List<string>();
            foreach (var fieldName in PreSortedFieldList)
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

        /// <summary>
        /// The GetConfidence.
        /// </summary>
        /// <param name="fieldName">The fieldName<see cref="string"/>.</param>
        /// <returns>The <see cref="long"/>.</returns>
        public long GetConfidence(string fieldName)
        {
            if (USERAGENT_FIELDNAME.Equals(fieldName))
            {
                return 0L;
            }

            if (this.allFields.ContainsKey(fieldName))
            {
                return this.allFields[fieldName].GetConfidence();
            }
            else
            {
                return -1L;
            }
        }

        /// <summary>
        /// The GetHashCode.
        /// </summary>
        /// <returns>The <see cref="int"/>.</returns>
        public override int GetHashCode()
        {
            var hash = 3060293; // A random number
            foreach (var item in this.allFields.Keys)
            {
                hash = new Tuple<int, AgentField, string>(hash, this.allFields[item], item).GetHashCode();
            }

            return ValueTuple.Create(this.userAgentString, hash).GetHashCode();
        }

        /// <summary>
        /// The GetValue.
        /// </summary>
        /// <param name="fieldName">The fieldName<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetValue(string fieldName)
        {
            if (USERAGENT_FIELDNAME.Equals(fieldName))
            {
                return this.userAgentString;
            }

            var field = this.allFields.ContainsKey(fieldName) ? this.allFields[fieldName] : null;
            if (field == null)
            {
                return UNKNOWN_VALUE;
            }

            return field.GetValue();
        }

        /// <summary>
        /// The ProcessSetAll.
        /// </summary>
        public void ProcessSetAll()
        {
            if (this.allFields.ContainsKey(SET_ALL_FIELDS))
            {
                var setAllField = this.allFields[SET_ALL_FIELDS];
                var value = setAllField.GetValue();
                var confidence = setAllField.Confidence;
                foreach (var fieldEntry in this.allFields)
                {
                    if (!IsSystemField(fieldEntry.Key))
                    {
                        fieldEntry.Value.SetValue(value, confidence);
                    }
                }
            }
        }

        /// <summary>
        /// The ReportAmbiguity.
        /// </summary>
        /// <param name="recognizer">The recognizer<see cref="Parser"/>.</param>
        /// <param name="dfa">The dfa<see cref="DFA"/>.</param>
        /// <param name="startIndex">The startIndex<see cref="int"/>.</param>
        /// <param name="stopIndex">The stopIndex<see cref="int"/>.</param>
        /// <param name="exact">The exact<see cref="bool"/>.</param>
        /// <param name="ambigAlts">The ambigAlts<see cref="BitSet"/>.</param>
        /// <param name="configs">The configs<see cref="ATNConfigSet"/>.</param>
        public void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
        {
            this.HasAmbiguity = true;
            this.AmbiguityCount++;
        }

        /// <summary>
        /// The ReportAttemptingFullContext.
        /// </summary>
        /// <param name="recognizer">The recognizer<see cref="Parser"/>.</param>
        /// <param name="dfa">The dfa<see cref="DFA"/>.</param>
        /// <param name="startIndex">The startIndex<see cref="int"/>.</param>
        /// <param name="stopIndex">The stopIndex<see cref="int"/>.</param>
        /// <param name="conflictingAlts">The conflictingAlts<see cref="BitSet"/>.</param>
        /// <param name="conflictState">The conflictState<see cref="SimulatorState"/>.</param>
        public void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
        {
        }

        /// <summary>
        /// The ReportContextSensitivity.
        /// </summary>
        /// <param name="recognizer">The recognizer<see cref="Parser"/>.</param>
        /// <param name="dfa">The dfa<see cref="DFA"/>.</param>
        /// <param name="startIndex">The startIndex<see cref="int"/>.</param>
        /// <param name="stopIndex">The stopIndex<see cref="int"/>.</param>
        /// <param name="prediction">The prediction<see cref="int"/>.</param>
        /// <param name="acceptState">The acceptState<see cref="SimulatorState"/>.</param>
        public void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
        {
        }

        /// <summary>
        /// The Reset.
        /// </summary>
        public virtual void Reset()
        {
            this.HasSyntaxError = false;
            this.HasAmbiguity = false;
            this.AmbiguityCount = 0;

            foreach (var field in this.allFields.Values)
            {
                field.Reset();
            }
        }

        /// <summary>
        /// The Set.
        /// </summary>
        /// <param name="attribute">The attribute<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <param name="confidence">The confidence<see cref="long"/>.</param>
        public virtual void Set(string attribute, string value, long confidence)
        {
            var field = this.allFields.ContainsKey(attribute) ? this.allFields[attribute] : null;
            if (field == null)
            {
                field = new AgentField(null); // The fields we do not know get a 'null' default
            }

            var wasEmpty = confidence == -1;
            var updated = field.SetValue(value, confidence);
            if (this.IsDebug && !wasEmpty)
            {
                if (updated)
                {
                    Log.Info(string.Format("USE  {0} ({1}) = {2}", attribute, confidence, value ?? "null"));
                }
                else
                {
                    Log.Info(string.Format("SKIP {0} ({1}) = {2}", attribute, confidence, value ?? "null"));
                }
            }

            this.allFields[attribute] = field;
        }

        /// <summary>
        /// The Set.
        /// </summary>
        /// <param name="newValuesUserAgent">The newValuesUserAgent<see cref="UserAgent"/>.</param>
        /// <param name="appliedMatcher">The appliedMatcher<see cref="Matcher"/>.</param>
        public virtual void Set(UserAgent newValuesUserAgent, Matcher appliedMatcher)
        {
            foreach (var fieldName in newValuesUserAgent.allFields.Keys)
            {
                var field = newValuesUserAgent.allFields[fieldName];
                this.Set(fieldName, field.Value, field.Confidence);
            }
        }

        /// <summary>
        /// The SetForced.
        /// </summary>
        /// <param name="attribute">The attribute<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <param name="confidence">The confidence<see cref="long"/>.</param>
        public void SetForced(string attribute, string value, long confidence)
        {
            AgentField field;
            if (this.allFields.ContainsKey(attribute))
            {
                field = this.allFields[attribute];
            }
            else
            {
                field = new AgentField(null); // The fields we do not know get a 'null' default
            }

            var wasEmpty = confidence == -1;
            field.SetValueForced(value, confidence);
            if (this.IsDebug && !wasEmpty)
            {
                Log.Info(string.Format("USE  {0} ({1}) = {2}", attribute, confidence, value));
            }

            this.allFields[attribute] = field;
        }

        /// <summary>
        /// The SyntaxError.
        /// </summary>
        /// <param name="output">The output <see cref="TextWriter"/>.</param>
        /// <param name="recognizer">The recognizer<see cref="IRecognizer"/>.</param>
        /// <param name="offendingSymbol">The offendingSymbol<see cref="int"/>.</param>
        /// <param name="line">The line<see cref="int"/>.</param>
        /// <param name="charPositionInLine">The charPositionInLine<see cref="int"/>.</param>
        /// <param name="msg">The msg<see cref="string"/>.</param>
        /// <param name="e">The e<see cref="RecognitionException"/>.</param>
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            this.SyntaxError(output, recognizer, null, line, charPositionInLine, msg, e);
        }

        /// <summary>
        /// The SyntaxError.
        /// </summary>
        /// <param name="output">The output <see cref="TextWriter"/>.</param>
        /// <param name="recognizer">The recognizer<see cref="IRecognizer"/>.</param>
        /// <param name="offendingSymbol">The offendingSymbol<see cref="IToken"/>.</param>
        /// <param name="line">The line<see cref="int"/>.</param>
        /// <param name="charPositionInLine">The charPositionInLine<see cref="int"/>.</param>
        /// <param name="msg">The msg<see cref="string"/>.</param>
        /// <param name="e">The e<see cref="RecognitionException"/>.</param>
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (this.IsDebug)
            {
                Log.Error("Syntax error");
                Log.Error(string.Format("Source : {0}", this.userAgentString));
                Log.Error(string.Format("Message: {0}", msg));
            }

            this.HasSyntaxError = true;
            var syntaxError = new AgentField("false");
            syntaxError.SetValue("true", 1);
            this.allFields[SYNTAX_ERROR] = syntaxError;
        }

        /// <summary>
        /// The ToJson.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToJson()
        {
            var fields = this.GetAvailableFieldNames();
            fields.Add(USERAGENT_FIELDNAME);
            return this.ToJson(fields);
        }

        /// <summary>
        /// The ToJson.
        /// </summary>
        /// <param name="fieldNames">The list of fieldNames.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToJson(IList<string> fieldNames)
        {
            var sb = new StringBuilder(10240);
            sb.Append("{");
            var addSeparator = false;
            foreach (var fieldName in fieldNames)
            {
                if (addSeparator)
                {
                    sb.Append(',');
                }
                else
                {
                    addSeparator = true;
                }

                if (USERAGENT_FIELDNAME.Equals(fieldName))
                {
                    sb
                        .Append("\"Useragent\"")
                        .Append(':')
                        .Append(JsonConvert.ToString(this.UserAgentString));
                }
                else
                {
                    sb
                        .Append(JsonConvert.ToString(fieldName))
                        .Append(':')
                        .Append(JsonConvert.ToString(this.GetValue(fieldName)));
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return this.ToString(this.GetAvailableFieldNamesSorted());
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <param name="fieldNames">The list of fieldNames.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToString(IList<string> fieldNames)
        {
            var sb = new StringBuilder("  - user_agent_string: '\"" + this.userAgentString + "\"'\n");
            var maxLength = 0;
            foreach (var fieldName in fieldNames)
            {
                maxLength = Math.Max(maxLength, fieldName.Length);
            }

            foreach (var fieldName in fieldNames)
            {
                if (!USERAGENT_FIELDNAME.Equals(fieldName))
                {
                    var field = this.allFields[fieldName];
                    if (field.GetValue() != null)
                    {
                        sb.Append("    ").Append(fieldName);
                        for (var l = fieldName.Length; l < maxLength + 2; l++)
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

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <param name="fieldName1">The fieldName1<see cref="string"/>.</param>
        /// <param name="otherFieldNames">The other foeld names"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToString(string fieldName1, params string[] otherFieldNames)
        {
            var l = new List<string> { fieldName1 };
            l.AddRange(otherFieldNames);
            return this.ToString(l);
        }

        /// <summary>
        /// The ToYamlTestCase.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToYamlTestCase()
        {
            return this.ToYamlTestCase(false, null);
        }

        /// <summary>
        /// The ToYamlTestCase.
        /// </summary>
        /// <param name="showConfidence">The showConfidence<see cref="bool"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToYamlTestCase(bool showConfidence)
        {
            return this.ToYamlTestCase(showConfidence, null);
        }

        /// <summary>
        /// The ToYamlTestCase.
        /// </summary>
        /// <param name="showConfidence">The showConfidence<see cref="bool"/>.</param>
        /// <param name="comments">The comments.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string ToYamlTestCase(bool showConfidence, Dictionary<string, string> comments)
        {
            var sb = new StringBuilder(10240);
            sb.Append("\n");
            sb.Append("- test:\n");
            sb.Append("#    options:\n");
            sb.Append("#    - 'verbose'\n");
            sb.Append("#    - 'init'\n");
            sb.Append("#    - 'only'\n");
            sb.Append("    input:\n");
            sb.Append("      user_agent_string: '").Append(this.userAgentString).Append("'\n");
            sb.Append("    expected:\n");

            var fieldNames = this.GetAvailableFieldNamesSorted();

            var maxNameLength = 30;
            var maxValueLength = 0;
            foreach (var fieldName in this.allFields.Keys)
            {
                maxNameLength = Math.Max(maxNameLength, fieldName.Length);
            }

            foreach (var fieldName in fieldNames)
            {
                maxValueLength = Math.Max(maxValueLength, this.Get(fieldName).GetValue().Length);
            }

            foreach (var fieldName in fieldNames)
            {
                sb.Append("      ").Append(fieldName);
                for (var l = fieldName.Length; l < maxNameLength + 7; l++)
                {
                    sb.Append(' ');
                }

                var value = this.Get(fieldName).GetValue();
                sb.Append(": '").Append(value).Append('\'');
                if (showConfidence)
                {
                    for (var l = value.Length; l < maxValueLength + 5; l++)
                    {
                        sb.Append(' ');
                    }

                    sb.Append("# ").Append(string.Format("{0}", this.Get(fieldName).Confidence));
                }

                if (comments != null && comments.ContainsKey(fieldName))
                {
                    sb.Append(" | ").Append(comments[fieldName]);
                }

                sb.Append('\n');
            }

            sb.Append("\n\n");

            return sb.ToString();
        }

        /// <summary>
        /// The IsSystemField.
        /// </summary>
        /// <param name="fieldname">The fieldname<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        internal static bool IsSystemField(string fieldname)
        {
            return SET_ALL_FIELDS.Equals(fieldname) ||
                    SYNTAX_ERROR.Equals(fieldname) ||
                    USERAGENT_FIELDNAME.Equals(fieldname);
        }

        /// <summary>
        /// The Set.
        /// </summary>
        /// <param name="fieldName">The fieldName<see cref="string"/>.</param>
        /// <param name="agentField">The agentField<see cref="AgentField"/>.</param>
        internal void SetImmediateForTesting(string fieldName, AgentField agentField)
        {
            this.allFields[fieldName] = agentField;
        }

        /// <summary>
        /// The Init.
        /// </summary>
        private void Init()
        {
            // Device : Family - Brand - Model
            this.allFields[DEVICE_CLASS] = new AgentField(UNKNOWN_VALUE); // Hacker / Cloud / Server / Desktop / Tablet / Phone / Watch
            this.allFields[DEVICE_BRAND] = new AgentField(UNKNOWN_VALUE); // (Google/AWS/Azure) / ????
            this.allFields[DEVICE_NAME] = new AgentField(UNKNOWN_VALUE); // (Google/AWS/Azure) / ????

            // Operating system
            this.allFields[OPERATING_SYSTEM_CLASS] = new AgentField(UNKNOWN_VALUE); // Cloud, Desktop, Mobile, Embedded
            this.allFields[OPERATING_SYSTEM_NAME] = new AgentField(UNKNOWN_VALUE); // ( Linux / Android / Windows ...)
            this.allFields[OPERATING_SYSTEM_VERSION] = new AgentField(UNKNOWN_VERSION); // 1.2 / 43 / ...

            // Engine : Class (=None/Hacker/Robot/Browser) - Name - Version
            this.allFields[LAYOUT_ENGINE_CLASS] = new AgentField(UNKNOWN_VALUE); // None / Hacker / Robot / Browser /
            this.allFields[LAYOUT_ENGINE_NAME] = new AgentField(UNKNOWN_VALUE); // ( GoogleBot / Bing / ...) / (Trident / Gecko / ...)
            this.allFields[LAYOUT_ENGINE_VERSION] = new AgentField(UNKNOWN_VERSION); // 1.2 / 43 / ...
            this.allFields[LAYOUT_ENGINE_VERSION_MAJOR] = new AgentField(UNKNOWN_VERSION); // 1 / 43 / ...

            // Agent: Class (=Hacker/Robot/Browser) - Name - Version
            this.allFields[AGENT_CLASS] = new AgentField(UNKNOWN_VALUE); // Hacker / Robot / Browser /
            this.allFields[AGENT_NAME] = new AgentField(UNKNOWN_VALUE); // ( GoogleBot / Bing / ...) / ( Firefox / Chrome / ... )
            this.allFields[AGENT_VERSION] = new AgentField(UNKNOWN_VERSION); // 1.2 / 43 / ...
            this.allFields[AGENT_VERSION_MAJOR] = new AgentField(UNKNOWN_VERSION); // 1 / 43 / ...
        }

        /// <summary>
        /// Defines the <see cref="AgentField" />.
        /// </summary>
        [Serializable]
        public class AgentField
        {
            /// <summary>
            /// Defines the defaultValue.
            /// </summary>
            private readonly string defaultValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="AgentField"/> class.
            /// </summary>
            /// <param name="defaultValue">The defaultValue<see cref="string"/>.</param>
            internal AgentField(string defaultValue)
            {
                this.defaultValue = defaultValue;
                this.Reset();
            }

            /// <summary>
            /// Gets or sets the Confidence.
            /// </summary>
            internal long Confidence { get; set; }

            /// <summary>
            /// Gets or sets the Value.
            /// </summary>
            internal string Value { get; set; }

            /// <summary>
            /// The Equals.
            /// </summary>
            /// <param name="obj">The obj<see cref="object"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
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

                var that = (AgentField)obj;
                return this.Confidence == that.Confidence &&
                    Equals(this.defaultValue, that.defaultValue) &&
                    Equals(this.Value, that.Value);
            }

            /// <summary>
            /// The GetConfidence.
            /// </summary>
            /// <returns>The <see cref="long"/>.</returns>
            public long GetConfidence()
            {
                if (this.Value == null)
                {
                    return -1; // Lie in case the value was wiped.
                }

                return this.Confidence;
            }

            /// <summary>
            /// The GetHashCode.
            /// </summary>
            /// <returns>The <see cref="int"/>.</returns>
            public override int GetHashCode()
            {
                return ValueTuple.Create(this.defaultValue, this.Value, this.Confidence).GetHashCode();
            }

            /// <summary>
            /// The GetValue.
            /// </summary>
            /// <returns>The <see cref="string"/>.</returns>
            public string GetValue()
            {
                return this.Value ?? this.defaultValue;
            }

            /// <summary>
            /// The Reset.
            /// </summary>
            public void Reset()
            {
                this.Value = this.defaultValue;
                this.Confidence = -1;
            }

            /// <summary>
            /// The SetValue.
            /// </summary>
            /// <param name="field">The field<see cref="AgentField"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            public bool SetValue(AgentField field)
            {
                return this.SetValue(field.Value, field.Confidence);
            }

            /// <summary>
            /// The SetValue.
            /// </summary>
            /// <param name="newValue">The newValue<see cref="string"/>.</param>
            /// <param name="newConfidence">The newConfidence<see cref="long"/>.</param>
            /// <returns>The <see cref="bool"/>.</returns>
            public bool SetValue(string newValue, long newConfidence)
            {
                if (newConfidence > this.Confidence)
                {
                    this.Confidence = newConfidence;
                    if (NULL_VALUE.Equals(newValue))
                    {
                        this.Value = this.defaultValue;
                    }
                    else
                    {
                        this.Value = newValue;
                    }

                    return true;
                }

                return false;
            }

            /// <summary>
            /// The SetValueForced.
            /// </summary>
            /// <param name="newValue">The newValue<see cref="string"/>.</param>
            /// <param name="newConfidence">The newConfidence<see cref="long"/>.</param>
            public void SetValueForced(string newValue, long newConfidence)
            {
                this.Confidence = newConfidence;

                if (NULL_VALUE.Equals(newValue))
                {
                    this.Value = this.defaultValue;
                }
                else
                {
                    this.Value = newValue;
                }
            }

            /// <summary>
            /// The ToString.
            /// </summary>
            /// <returns>The <see cref="string"/>.</returns>
            public override string ToString()
            {
                if (this.defaultValue == null)
                {
                    return "{ value:'" + this.Value + "', confidence:'" + this.Confidence + "', default:null }";
                }

                return "{ value:'" + this.Value + "', confidence:'" + this.Confidence + "', default:'" + this.defaultValue + "' }";
            }
        }
    }
}
