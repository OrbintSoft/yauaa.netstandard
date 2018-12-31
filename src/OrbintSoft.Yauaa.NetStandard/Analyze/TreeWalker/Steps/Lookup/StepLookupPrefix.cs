//-----------------------------------------------------------------------
// <copyright file="StepLookupPrefix.cs" company="OrbintSoft">
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
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// Defines the <see cref="StepLookupPrefix" />
    /// </summary>
    [Serializable]
    public class StepLookupPrefix : Step
    {
        /// <summary>
        /// Defines the defaultValue
        /// </summary>
        private readonly string defaultValue;

        /// <summary>
        /// Defines the lookupName
        /// </summary>
        private readonly string lookupName;

        /// <summary>
        /// Defines the prefixLookup
        /// </summary>
        private readonly PrefixLookup prefixLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookupPrefix"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/></param>
        /// <param name="context">The context<see cref="StreamingContext"/></param>
        public StepLookupPrefix(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.lookupName = (string)info.GetValue("lookupName", typeof(string));
            this.prefixLookup = (PrefixLookup)info.GetValue("prefixLookup", typeof(PrefixLookup));
            this.defaultValue = (string)info.GetValue("defaultValue", typeof(string));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookupPrefix"/> class.
        /// </summary>
        /// <param name="lookupName">The lookupName<see cref="string"/></param>
        /// <param name="prefixList">The prefixList</param>
        /// <param name="defaultValue">The defaultValue<see cref="string"/></param>
        public StepLookupPrefix(string lookupName, IDictionary<string, string> prefixList, string defaultValue)
        {
            this.lookupName = lookupName;
            this.defaultValue = defaultValue;
            this.prefixLookup = new PrefixLookup(prefixList, false);
        }

        /// <summary>
        /// The GetObjectData
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/></param>
        /// <param name="context">The context<see cref="StreamingContext"/></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("lookupName", this.lookupName, typeof(string));
            info.AddValue("prefixLookup", this.prefixLookup, typeof(PrefixLookup));
            info.AddValue("defaultValue", this.defaultValue, typeof(string));
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "LookupPrefix(@" + this.lookupName + " ; default=" + this.defaultValue + ")";
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var input = this.GetActualValue(tree, value);

            var result = this.prefixLookup.FindLongestMatchingPrefix(input);

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

            return this.WalkNextStep(tree, result);
        }
    }
}
