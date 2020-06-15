//-----------------------------------------------------------------------
// <copyright file="StepIsInLookupContains.cs" company="OrbintSoft">
//   Yet Another User Agent Analyzer for .NET Standard
//   porting realized by Stefano Balzarotti, Copyright 2018-2020 (C) OrbintSoft
//
//   Original Author and License:
//
//   Yet Another UserAgent Analyzer
//   Copyright(C) 2013-2020 Niels Basjes
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
// <date>2020, 06, 06, 23:02</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines the StepIsInLookupContains Step, it is used in parsing to check if the actual value exist in a lookup.
    /// </summary>
    [Serializable]
    public class StepIsInLookupContains : Step
    {
        private readonly string lookupName;
        private readonly HashSet<string> lookupKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsInLookupContains"/> class.
        /// </summary>
        /// <param name="lookupName">The name of the lookup.</param>
        /// <param name="lookup">The lookup values.</param>
        public StepIsInLookupContains(string lookupName, IDictionary<string, string> lookup)
        {
            this.lookupName = lookupName;
            this.lookupKeys = new HashSet<string>(lookup.Keys);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsInLookupContains"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepIsInLookupContains(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.lookupName = (string)info.GetValue(nameof(this.lookupName), typeof(string));
            this.lookupKeys = (HashSet<string>)info.GetValue(nameof(this.lookupKeys), typeof(HashSet<string>));
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
            info.AddValue(nameof(this.lookupKeys), this.lookupKeys, typeof(HashSet<string>));
        }

        /// <summary>
        /// It finds if the actual value exist in the lookup, in that case:
        /// this will walk into the tree and recurse through all the remaining steps.
        /// Otherwise it will return null, since no other steps to walk.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            string input = this.GetActualValue(tree, value);

            string compareInput = input.ToLower();
            foreach (string key in this.lookupKeys)
            {
                if (compareInput.Contains(key))
                {
                    return this.WalkNextStep(tree, input);
                }
            }

            // Not found:
            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"IsInLookupContains(@{this.lookupName})";
        }
    }
}
