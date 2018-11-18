//<copyright file="StepLookup.cs" company="OrbintSoft">
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
//<date>2018, 8, 13, 23:45</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Lookup
{
    using Antlr4.Runtime.Tree;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="StepLookup" />
    /// </summary>
    [Serializable]
    public class StepLookup : Step
    {
        /// <summary>
        /// Defines the lookupName
        /// </summary>
        private readonly string lookupName;

        /// <summary>
        /// Defines the lookup
        /// </summary>
        private readonly IDictionary<string, string> lookup;

        /// <summary>
        /// Defines the defaultValue
        /// </summary>
        private readonly string defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookup"/> class.
        /// </summary>
        /// <param name="lookupName">The lookupName<see cref="string"/></param>
        /// <param name="lookup">The lookup<see cref="IDictionary{string, string}"/></param>
        /// <param name="defaultValue">The defaultValue<see cref="string"/></param>
        public StepLookup(string lookupName, IDictionary<string, string> lookup, string defaultValue)
        {
            this.lookupName = lookupName;
            this.lookup = lookup;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            string input = GetActualValue(tree, value).ToLower();

            string result = lookup.ContainsKey(input) ? lookup[input] : null;

            if (result == null)
            {
                if (defaultValue == null)
                {
                    return null;
                }
                else
                {
                    return WalkNextStep(tree, defaultValue);
                }
            }
            return WalkNextStep(tree, result);
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return string.Format("Lookup(@{0} ; default={1})", lookupName, defaultValue ?? "null");
        }
    }
}
