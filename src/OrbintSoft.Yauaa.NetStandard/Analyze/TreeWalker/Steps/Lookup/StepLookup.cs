//-----------------------------------------------------------------------
// <copyright file="StepLookup.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:48</date>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines the Lookup Step, it is used in parsing to find a value in a lookup.
    /// </summary>
    [Serializable]
    public class StepLookup : Step
    {
        /// <summary>
        /// The default value to be used in case no value found in lookup.
        /// </summary>
        private readonly string defaultValue;

        /// <summary>
        /// Defines the lookup dictionary.
        /// </summary>
        private readonly IDictionary<string, string> lookup;

        /// <summary>
        /// Defines the name of the lookup.
        /// </summary>
        private readonly string lookupName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookup"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepLookup(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.lookupName = (string)info.GetValue(nameof(this.lookupName), typeof(string));
            this.lookup = (IDictionary<string, string>)info.GetValue(nameof(this.lookup), typeof(IDictionary<string, string>));
            this.defaultValue = (string)info.GetValue(nameof(this.defaultValue), typeof(string));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepLookup"/> class.
        /// </summary>
        /// <param name="lookupName">The name of the lookup, a conventional name associated to the lookup, used to define operations.</param>
        /// <param name="lookup">The dictionary you want use for lookup.</param>
        /// <param name="defaultValue">The default value to be used in case of no lookup found.</param>
        public StepLookup(string lookupName, IDictionary<string, string> lookup, string defaultValue)
        {
            this.lookupName = lookupName;
            this.lookup = lookup;
            this.defaultValue = defaultValue;
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
            info.AddValue(nameof(this.lookup), this.lookup, typeof(IDictionary<string, string>));
            info.AddValue(nameof(this.defaultValue), this.defaultValue, typeof(string));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Lookup(@{this.lookupName} ; default={this.defaultValue ?? "null"})";
        }

        /// <summary>
        /// It finds the value in the lookup given the actual value or the default value if not not null
        /// so it will walk into the tree and recurse through all the remaining steps.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var input = this.GetActualValue(tree, value).ToLower();

            var result = this.lookup.ContainsKey(input) ? this.lookup[input] : null;
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
