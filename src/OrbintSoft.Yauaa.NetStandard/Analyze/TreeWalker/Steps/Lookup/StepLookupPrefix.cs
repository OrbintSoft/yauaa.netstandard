//-----------------------------------------------------------------------
// <copyright file="StepLookupPrefix.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2019 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2019 Niels Basjes
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//   https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:48</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// This class defines the LookupPrefix Step, it is used in parsing to find a value in a lookup by a prefix.
    /// </summary>
    [Serializable]
    public class StepLookupPrefix : Step
    {
        /// <summary>
        /// The default value to be used in case no value found in lookup.
        /// </summary>
        private readonly string defaultValue;

        /// <summary>
        /// Defines the name of the lookup.
        /// </summary>
        private readonly string lookupName;

        /// <summary>
        /// Defines the prefix lookup.
        /// </summary>
        private readonly PrefixLookup prefixLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookupPrefix"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepLookupPrefix(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.lookupName = (string)info.GetValue(nameof(this.lookupName), typeof(string));
            this.prefixLookup = (PrefixLookup)info.GetValue(nameof(this.prefixLookup), typeof(PrefixLookup));
            this.defaultValue = (string)info.GetValue(nameof(this.defaultValue), typeof(string));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookupPrefix"/> class.
        /// </summary>
        /// <param name="lookupName">The name of the lookup, a conventional name associated to the lookup, used to define operations.</param>
        /// <param name="prefixList">The dictionary you want use for prefix lookup.</param>
        /// <param name="defaultValue">The default value to be used in case of no lookup found.</param>
        public StepLookupPrefix(string lookupName, IDictionary<string, string> prefixList, string defaultValue)
        {
            this.lookupName = lookupName;
            this.defaultValue = defaultValue;
            this.prefixLookup = new PrefixLookup(prefixList, false);
        }

        /// <summary>
        /// This is used for binary serialization.
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.lookupName), this.lookupName, typeof(string));
            info.AddValue(nameof(this.prefixLookup), this.prefixLookup, typeof(PrefixLookup));
            info.AddValue(nameof(this.defaultValue), this.defaultValue, typeof(string));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LookupPrefix(@{this.lookupName} ; default={this.defaultValue})";
        }

        /// <summary>
        /// It finds the value in the lookup by prefix given the actual value or the default value if not not null
        /// so it will walk into the tree and recurse through all the remaining steps.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var input = this.GetActualValue(tree, value);

            var result = this.prefixLookup.FindLongestMatchingPrefix(input);

            if (result is null)
            {
                if (this.defaultValue is null)
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
