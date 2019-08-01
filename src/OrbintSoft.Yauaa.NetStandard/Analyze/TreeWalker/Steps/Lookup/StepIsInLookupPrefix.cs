//-----------------------------------------------------------------------
// <copyright file="StepIsInLookupPrefix.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
    /// Defines the <see cref="StepIsInLookupPrefix" />.
    /// </summary>
    [Serializable]
    public class StepIsInLookupPrefix : Step
    {
        /// <summary>
        /// Defines the lookupName.
        /// </summary>
        private readonly string lookupName;

        /// <summary>
        /// Defines the prefixLookup.
        /// </summary>
        private readonly PrefixLookup prefixLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsInLookupPrefix"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public StepIsInLookupPrefix(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.lookupName = (string)info.GetValue("lookupName", typeof(string));
            this.prefixLookup = (PrefixLookup)info.GetValue("prefixLookup", typeof(PrefixLookup));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsInLookupPrefix"/> class.
        /// </summary>
        /// <param name="lookupName">The lookupName<see cref="string"/>.</param>
        /// <param name="prefixList">The prefixList.</param>
        public StepIsInLookupPrefix(string lookupName, IDictionary<string, string> prefixList)
        {
            this.lookupName = lookupName;
            this.prefixLookup = new PrefixLookup(prefixList, false);
        }

        /// <summary>
        /// The GetObjectData.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("lookupName", this.lookupName, typeof(string));
            info.AddValue("prefixLookup", this.prefixLookup, typeof(PrefixLookup));
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return "IsInLookupPrefix(@" + this.lookupName + ")";
        }

        /// <summary>
        /// The Walk.
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/>.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <returns>The <see cref="WalkList.WalkResult"/>.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var input = this.GetActualValue(tree, value);

            var result = this.prefixLookup.FindLongestMatchingPrefix(input);

            if (result == null)
            {
                return null;
            }

            return this.WalkNextStep(tree, input);
        }
    }
}
