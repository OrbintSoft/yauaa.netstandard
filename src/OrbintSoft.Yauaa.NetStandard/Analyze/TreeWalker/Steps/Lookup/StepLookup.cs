//-----------------------------------------------------------------------
// <copyright file="StepLookup.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Lookup
{
    using Antlr4.Runtime.Tree;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the <see cref="StepLookup" />
    /// </summary>
    [Serializable]
    public class StepLookup : Step
    {
        /// <summary>
        /// Defines the defaultValue
        /// </summary>
        private readonly string defaultValue;

        /// <summary>
        /// Defines the lookup
        /// </summary>
        private readonly IDictionary<string, string> lookup;

        /// <summary>
        /// Defines the lookupName
        /// </summary>
        private readonly string lookupName;

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

        public StepLookup(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.lookupName = (string)info.GetValue("lookupName", typeof(string));
            this.lookup = (IDictionary<string, string>)info.GetValue("lookup", typeof(IDictionary<string, string>));
            this.defaultValue = (string)info.GetValue("defaultValue", typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("lookupName", this.lookupName, typeof(string));
            info.AddValue("lookup", this.lookup, typeof(IDictionary<string, string>));
            info.AddValue("defaultValue", this.defaultValue, typeof(string));
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return string.Format("Lookup(@{0} ; default={1})", this.lookupName, this.defaultValue ?? "null");
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var input = this.GetActualValue(tree, value).ToLower();

            var result = this.lookup.ContainsKey(input) ? this.lookup[input] : null;
            if (result == null)
            {
                if (this.defaultValue == null)
                {
                    return null;
                }
                else
                {
                    return this.WalkNextStep(tree, this.defaultValue);
                }
            }
            else
            {
                return this.WalkNextStep(tree, result);
            }
        }
    }
}
