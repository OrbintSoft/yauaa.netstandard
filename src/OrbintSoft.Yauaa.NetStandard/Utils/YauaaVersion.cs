//-----------------------------------------------------------------------
// <copyright file="YauaaVersion.cs" company="OrbintSoft">
//  Yet Another User Agent Analyzer for .NET Standard
//  porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//  Original Author and License:
//
//  Yet Another UserAgent Analyzer
//  Copyright(C) 2013-2019 Niels Basjes
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  https://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using log4net;
    using OrbintSoft.Yauaa.Analyze;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// This class is an helper to get, compare and log the current version of Yauaa library.
    /// </summary>
    public sealed class YauaaVersion
    {
        /// <summary>
        /// Defines the Logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(YauaaVersion));

        /// <summary>
        /// Prevents a default instance of the <see cref="YauaaVersion"/> class from being created.
        /// </summary>
        private YauaaVersion()
        {
        }

        /// <summary>
        /// The AssertSameVersion.
        /// </summary>
        /// <param name="versionNodeTuple">The versionNodeTuple<see cref="KeyValuePair{YamlNode, YamlNode}"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        public static void AssertSameVersion(KeyValuePair<YamlNode, YamlNode> versionNodeTuple, string filename)
        {
            // Check the version information from the Yaml files
            var versionNode = YamlUtils.GetValueAsSequenceNode(versionNodeTuple, filename);
            string gitCommitIdDescribeShort = null;
            string buildTimestamp = null;
            string projectVersion = null;

            var versionList = versionNode.Children.ToList();
            foreach (var versionEntry in versionList)
            {
                YamlUtils.RequireNodeInstanceOf(typeof(YamlMappingNode), versionEntry, filename, "The entry MUST be a mapping");
                var entry = YamlUtils.GetExactlyOneNodeTuple((YamlMappingNode)versionEntry, filename);
                var key = YamlUtils.GetKeyAsString(entry, filename);
                var value = YamlUtils.GetValueAsString(entry, filename);
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

        /// <summary>
        /// The AssertSameVersion.
        /// </summary>
        /// <param name="gitCommitIdDescribeShort">The gitCommitIdDescribeShort<see cref="string"/>.</param>
        /// <param name="buildTimestamp">The buildTimestamp<see cref="string"/>.</param>
        /// <param name="projectVersion">The projectVersion<see cref="string"/>.</param>
        public static void AssertSameVersion(string gitCommitIdDescribeShort, string buildTimestamp, string projectVersion)
        {
            var libraryGitCommitIdDescribeShort = ThisVersion.GitCommitIdDescribeShort;
            var libraryBuildTimestamp = ThisVersion.BuildTimestamp;
            var libraryProjectVersion = ThisVersion.ProjectVersion;
            if (libraryGitCommitIdDescribeShort.Equals(gitCommitIdDescribeShort) &&
                libraryBuildTimestamp.Equals(buildTimestamp) &&
                libraryProjectVersion.Equals(projectVersion))
            {
                return;
            }

            var libraryVersion = GetVersion(libraryProjectVersion, libraryGitCommitIdDescribeShort, libraryBuildTimestamp);
            var rulesVersion = GetVersion(projectVersion, gitCommitIdDescribeShort, buildTimestamp);

            Log.Error("===============================================");
            Log.Error("==========        FATAL ERROR       ===========");
            Log.Error("vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
            Log.Error(string.Empty);
            Log.Error("Two different Yauaa versions have been loaded:");
            Log.Error(string.Format("Runtime Library: {0}", libraryVersion));
            Log.Error(string.Format("Rule sets      : {0}", rulesVersion));
            Log.Error(string.Empty);
            Log.Error("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            Log.Error("===============================================");

            throw new InvalidParserConfigurationException("Two different Yauaa versions have been loaded: \n" +
                "Runtime Library: " + libraryVersion + "\n" +
                "Rule sets      : " + rulesVersion + "\n");
        }

        /// <summary>
        /// The GetVersion.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetVersion()
        {
            return GetVersion(ThisVersion.ProjectVersion, ThisVersion.GitCommitIdDescribeShort, ThisVersion.BuildTimestamp);
        }

        /// <summary>
        /// The GetVersion.
        /// </summary>
        /// <param name="projectVersion">The projectVersion<see cref="string"/>.</param>
        /// <param name="gitCommitIdDescribeShort">The gitCommitIdDescribeShort<see cref="string"/>.</param>
        /// <param name="buildTimestamp">The buildTimestamp<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetVersion(string projectVersion, string gitCommitIdDescribeShort, string buildTimestamp)
        {
            return $"Yauaa {projectVersion}({gitCommitIdDescribeShort}@{buildTimestamp})";
        }

        /// <summary>
        /// The LogVersion.
        /// </summary>
        /// <param name="extraLines">The extraLines.</param>
        public static void LogVersion(params string[] extraLines)
        {
            string[] lines =
            {
                "For more information: https://github.com/OrbintSoft/yauaa.netstandard",
                ThisVersion.Copyright + " - " + ThisVersion.License,
            };

            var version = GetVersion();
            var width = version.Length;
            foreach (var line in lines)
            {
                width = Math.Max(width, line.Length);
            }

            foreach (var line in extraLines)
            {
                width = Math.Max(width, line.Length);
            }

            Log.Info(string.Empty);
            Log.Info(string.Format("/-{0}-\\", Padding('-', width)));
            LogLine(version, width);
            Log.Info(string.Format("+-{0}-+", Padding('-', width)));
            foreach (var line in lines)
            {
                LogLine(line, width);
            }

            if (extraLines.Any())
            {
                Log.Info(string.Format("+-{0}-+", Padding('-', width)));
                foreach (var line in extraLines)
                {
                    LogLine(line, width);
                }
            }

            Log.Info(string.Format("\\-{0}-/", Padding('-', width)));
            Log.Info(string.Empty);
        }

        /// <summary>
        /// The LogLine.
        /// </summary>
        /// <param name="line">The line<see cref="string"/>.</param>
        /// <param name="width">The width<see cref="int"/>.</param>
        private static void LogLine(string line, int width)
        {
            Log.Info(string.Format("| {0}{1} |", line, Padding(' ', width - line.Length)));
        }

        /// <summary>
        /// The Padding.
        /// </summary>
        /// <param name="letter">The letter<see cref="char"/>.</param>
        /// <param name="count">The count<see cref="int"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string Padding(char letter, int count)
        {
            var sb = new StringBuilder(128);
            for (var i = 0; i < count; i++)
            {
                sb.Append(letter);
            }

            return sb.ToString();
        }
    }
}
