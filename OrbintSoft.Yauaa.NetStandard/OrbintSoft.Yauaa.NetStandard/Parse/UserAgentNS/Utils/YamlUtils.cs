//<copyright file="YamlUtils.cs" company="OrbintSoft">
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
//<date>2018, 7, 26, 23:05</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils
{
    using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Defines the <see cref="YamlUtils" />
    /// </summary>
    public sealed class YamlUtils
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="YamlUtils"/> class from being created.
        /// </summary>
        private YamlUtils()
        {
        }

        /// <summary>
        /// The RequireNodeInstanceOf
        /// </summary>
        /// <param name="clazz">The clazz<see cref="Type"/></param>
        /// <param name="node">The node<see cref="YamlNode"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <param name="error">The error<see cref="string"/></param>
        public static void RequireNodeInstanceOf(Type clazz, YamlNode node, string filename, string error)
        {
            if (!clazz.IsInstanceOfType(node))
            {
                throw new InvalidParserConfigurationException(
                    CreateErrorString(node, filename, error));
            }
        }

        /// <summary>
        /// The Require
        /// </summary>
        /// <param name="condition">The condition<see cref="bool"/></param>
        /// <param name="node">The node<see cref="YamlNode"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <param name="error">The error<see cref="string"/></param>
        public static void Require(bool condition, YamlNode node, string filename, string error)
        {
            if (!condition)
            {
                throw new InvalidParserConfigurationException(
                    CreateErrorString(node, filename, error));
            }
        }

        /// <summary>
        /// The CreateErrorString
        /// </summary>
        /// <param name="node">The node<see cref="YamlNode"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <param name="error">The error<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string CreateErrorString(YamlNode node, string filename, string error)
        {
            return "Yaml config problem.(" + filename + ":" + node.Start.Line + "): " + error;
        }

        /// <summary>
        /// The GetExactlyOneNodeTuple
        /// </summary>
        /// <param name="mappingNode">The mappingNode<see cref="YamlMappingNode"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="KeyValuePair{YamlNode,YamlNode}"/></returns>
        public static KeyValuePair<YamlNode, YamlNode> GetExactlyOneNodeTuple(YamlMappingNode mappingNode, string filename)
        {
            var nodeTupleList = mappingNode.ToList();
            Require(nodeTupleList.Count == 1, mappingNode, filename, "There must be exactly 1 value in the list");
            return nodeTupleList.FirstOrDefault();
        }

        /// <summary>
        /// The GetKeyAsString
        /// </summary>
        /// <param name="tuple">The tuple<see cref="KeyValuePair{YamlNode, YamlNode}"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetKeyAsString(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode keyNode = tuple.Key;
            RequireNodeInstanceOf(typeof(YamlScalarNode), tuple.Key, filename, "The key should be a string but it is a " + keyNode.NodeType.ToString());
            return ((YamlScalarNode)keyNode).Value;
        }

        /// <summary>
        /// The GetValueAsString
        /// </summary>
        /// <param name="tuple">The tuple<see cref="KeyValuePair{YamlNode, YamlNode}"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetValueAsString(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode valueNode = tuple.Value;
            RequireNodeInstanceOf(typeof(YamlScalarNode), tuple.Value, filename, "The value should be a string but it is a " + valueNode.NodeType.ToString());
            return ((YamlScalarNode)valueNode).Value;
        }

        /// <summary>
        /// The GetValueAsMappingNode
        /// </summary>
        /// <param name="tuple">The tuple<see cref="KeyValuePair{YamlNode, YamlNode}"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="YamlMappingNode"/></returns>
        public static YamlMappingNode GetValueAsMappingNode(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode valueNode = tuple.Value;
            RequireNodeInstanceOf(typeof(YamlMappingNode), tuple.Value, filename, "The value should be a map but it is a " + valueNode.NodeType.ToString());
            return ((YamlMappingNode)valueNode);
        }

        /// <summary>
        /// The GetValueAsSequenceNode
        /// </summary>
        /// <param name="tuple">The tuple<see cref="KeyValuePair{YamlNode, YamlNode}"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="YamlSequenceNode"/></returns>
        public static YamlSequenceNode GetValueAsSequenceNode(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode valueNode = tuple.Value;
            RequireNodeInstanceOf(typeof(YamlSequenceNode), tuple.Value, filename, "The value should be a sequence but it is a " + valueNode.NodeType.ToString());
            return ((YamlSequenceNode)valueNode);
        }

        /// <summary>
        /// The GetStringValues
        /// </summary>
        /// <param name="sequenceNode">The sequenceNode<see cref="YamlNode"/></param>
        /// <param name="filename">The filename<see cref="string"/></param>
        /// <returns>The <see cref="List{string}"/></returns>
        public static List<string> GetStringValues(YamlNode sequenceNode, string filename)
        {
            RequireNodeInstanceOf(typeof(YamlSequenceNode), sequenceNode, filename, "The provided node must be a sequence but it is a " + sequenceNode.NodeType.ToString());
            List<YamlNode> valueNodes = ((YamlSequenceNode)sequenceNode).ToList();
            List<string> values = new List<string>();
            foreach (YamlNode node in valueNodes)
            {
                RequireNodeInstanceOf(typeof(YamlScalarNode), node, filename, "The value should be a string but it is a " + node.NodeType.ToString());
                values.Add(((YamlScalarNode)node).Value);
            }
            return values;
        }
    }
}
