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

using OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Analyze;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgent.Utils
{
    public sealed class YamlUtils
    {
        private YamlUtils() { }

        public static void Fail(YamlNode node, string filename, string error)
        {
            throw new InvalidParserConfigurationException(CreateErrorString(node, filename, error));
        }

        private static string CreateErrorString(YamlNode node, string filename, string error)
        {
            return "Yaml config problem.(" + filename + ":" + node.Start.Line + "): " + error;
        }

        public static KeyValuePair<YamlNode,YamlNode> GetExactlyOneNodeTuple(YamlMappingNode mappingNode, string filename)
        {
            var nodeTupleList = mappingNode.ToList();
            if (nodeTupleList.Count != 1)
            {
                Fail(mappingNode, filename, "There must be exactly 1 value in the list");
            }
            return nodeTupleList.FirstOrDefault();
        }

        public static string GetKeyAsString(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode keyNode = tuple.Key;
            if (!(keyNode is YamlScalarNode)) {
                Fail(tuple.Key, filename, "The key should be a string but it is a " + keyNode.NodeType.ToString());
            }
            return ((YamlScalarNode)keyNode).Value;
        }

        public static string GetValueAsString(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode valueNode = tuple.Value;
            if (!(valueNode is YamlScalarNode)) {
                Fail(tuple.Value, filename, "The value should be a string but it is a " + valueNode.NodeType.ToString());
            }
            return ((YamlScalarNode)valueNode).Value;
        }

        public static YamlMappingNode GetValueAsMappingNode(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode valueNode = tuple.Value;
            if (!(valueNode is YamlMappingNode)) {
                Fail(tuple.Value, filename, "The value should be a map but it is a " + valueNode.NodeType.ToString());
            }
            return ((YamlMappingNode)valueNode);
        }

        public static YamlSequenceNode GetValueAsSequenceNode(KeyValuePair<YamlNode, YamlNode> tuple, string filename)
        {
            YamlNode valueNode = tuple.Value;
            if (!(valueNode is YamlSequenceNode)) {
                Fail(tuple.Value, filename, "The value should be a sequence but it is a " + valueNode.NodeType.ToString());
            }
            return ((YamlSequenceNode)valueNode);
        }

        public static List<string> GetStringValues(YamlNode sequenceNode, string filename)
        {
            if (!(sequenceNode is YamlSequenceNode)) {
                Fail(sequenceNode, filename, "The provided node must be a sequence but it is a " + sequenceNode.NodeType.ToString());
            }

            List<YamlNode> valueNodes = ((YamlSequenceNode)sequenceNode).ToList();
            List<string> values = new List<string>();
            foreach (YamlNode node in valueNodes)
            {
                if (!(node is YamlScalarNode)) {
                    Fail(node, filename, "The value should be a string but it is a " + node.NodeType.ToString());
            }
            values.Add(((YamlScalarNode)node).Value);
        }
        return values;
    }
}
}
