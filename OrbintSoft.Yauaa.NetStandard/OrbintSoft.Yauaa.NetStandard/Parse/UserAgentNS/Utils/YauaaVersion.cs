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


using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;
using System.Linq;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils
{
    public sealed class YauaaVersion
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(YauaaVersion));

        private YauaaVersion()
        {
        }

        public static void LogVersion(params string[] extraLines)
        {
            string[] lines = {
                "For more information: https://github.com/OrbintSoft/yauaa.netstandard",
                ThisVersion.GetCopyright() + " - " + ThisVersion.GetLicense()
            };
            string version = GetVersion();
            int width = version.Length;
            foreach (string line in lines)
            {
                width = Math.Max(width, line.Length);
            }
            foreach (string line in extraLines)
            {
                width = Math.Max(width, line.Length);
            }

            LOG.Info("");
            LOG.Info(string.Format("/-{0}-\\", Padding('-', width)));
            LogLine(version, width);
            LOG.Info(string.Format("+-{0}-+", Padding('-', width)));
            foreach (string line in lines)
            {
                LogLine(line, width);
            }
            if (extraLines.Length > 0)
            {
                LOG.Info(string.Format("+-{0}-+", Padding('-', width)));
                foreach (string line in extraLines)
                {
                    LogLine(line, width);
                }
            }

            LOG.Info(string.Format("\\-{0}-/", Padding('-', width)));
            LOG.Info("");
        }

        private static string Padding(char letter, int count)
        {
            StringBuilder sb = new StringBuilder(128);
            for (int i = 0; i < count; i++)
            {
                sb.Append(letter);
            }
            return sb.ToString();
        }

        private static void LogLine(string line, int width)
        {
            LOG.Info(string.Format("| {0}{1} |", line, Padding(' ', width - line.Length)));
        }

        public static string GetVersion()
        {
            return GetVersion(ThisVersion.GetProjectVersion(), ThisVersion.GetGitCommitIdDescribeShort(), ThisVersion.GetBuildTimestamp());
        }

        public static string GetVersion(string projectVersion, string gitCommitIdDescribeShort, string buildTimestamp)
        {
            return "Yauaa " + projectVersion + " (" + gitCommitIdDescribeShort + " @ " + buildTimestamp + ")";
        }

        public static void AssertSameVersion(KeyValuePair<YamlNode, YamlNode> versionNodeTuple, string filename)
        {
            // Check the version information from the Yaml files
            YamlSequenceNode versionNode = YamlUtils.GetValueAsSequenceNode(versionNodeTuple, filename);
            string gitCommitIdDescribeShort = null;
            string buildTimestamp = null;
            string projectVersion = null;

            List<YamlNode> versionList = versionNode.Children.ToList();
            foreach (YamlNode versionEntry in versionList)
            {
                YamlUtils.RequireNodeInstanceOf(typeof(YamlMappingNode), versionEntry, filename, "The entry MUST be a mapping");
                KeyValuePair<YamlNode, YamlNode> entry = YamlUtils.GetExactlyOneNodeTuple((YamlMappingNode)versionEntry, filename);
                string key = YamlUtils.GetKeyAsString(entry, filename);
                string value = YamlUtils.GetValueAsString(entry, filename);
                switch (key)
                {
                    case "git_commit_id_describe_short":
                        gitCommitIdDescribeShort = value;
                        break;
                    case "build_timestamp":
                        buildTimestamp = value;
                        break;
                    case "project_version":
                        projectVersion = value;
                        break;
                    case "copyright":
                    case "license":
                        // Ignore those two when comparing.
                        break;
                    default:
                        throw new InvalidParserConfigurationException(
                            "Yaml config.(" + filename + ":" + versionNode.Start.Line + "): " +
                                "Found unexpected config entry: " + key + ", allowed are " +
                                "'git_commit_id_describe_short', 'build_timestamp' and 'project_version'");
                }
            }
            AssertSameVersion(gitCommitIdDescribeShort, buildTimestamp, projectVersion);
        }

        public static void AssertSameVersion(string gitCommitIdDescribeShort, string buildTimestamp, string projectVersion)
        {
            string libraryGitCommitIdDescribeShort = ThisVersion.GetGitCommitIdDescribeShort();
            string libraryBuildTimestamp = ThisVersion.GetBuildTimestamp();
            string libraryProjectVersion = ThisVersion.GetProjectVersion();
            if (libraryGitCommitIdDescribeShort.Equals(gitCommitIdDescribeShort) &&
                libraryBuildTimestamp.Equals(buildTimestamp) &&
                libraryProjectVersion.Equals(projectVersion))
            {
                return;
            }

            string libraryVersion = GetVersion(libraryProjectVersion, libraryGitCommitIdDescribeShort, libraryBuildTimestamp);
            string rulesVersion = GetVersion(projectVersion, gitCommitIdDescribeShort, buildTimestamp);

            LOG.Error("===============================================");
            LOG.Error("==========        FATAL ERROR       ===========");
            LOG.Error("vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
            LOG.Error("");
            LOG.Error("Two different Yauaa versions have been loaded:");
            LOG.Error(string.Format("Runtime Library: {0}", libraryVersion));
            LOG.Error(string.Format("Rule sets      : {0}", rulesVersion));
            LOG.Error("");
            LOG.Error("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            LOG.Error("===============================================");

            throw new InvalidParserConfigurationException("Two different Yauaa versions have been loaded: \n" +
                "Runtime Library: " + libraryVersion + "\n" +
                "Rule sets      : " + rulesVersion + "\n");
        }
    }
}