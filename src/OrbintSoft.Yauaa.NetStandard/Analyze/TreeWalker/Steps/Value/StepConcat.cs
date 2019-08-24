//-----------------------------------------------------------------------
// <copyright file="StepConcat.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Value
{
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines the Concat Step, it is used in parsing to concatenate a prefix and a postfix to the value of the node.
    /// </summary>
    public class StepConcat : Step
    {
        /// <summary>
        /// Defines the postfix to concatenate.
        /// </summary>
        private readonly string postfix;

        /// <summary>
        /// Defines the prefix to concatenate.
        /// </summary>
        private readonly string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepConcat"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepConcat(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.prefix = (string)info.GetValue(nameof(this.prefix), typeof(string));
            this.postfix = (string)info.GetValue(nameof(this.postfix), typeof(string));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepConcat"/> class.
        /// </summary>
        /// <param name="prefix">The prefix that shoukd be concatenated.</param>
        /// <param name="postfix">The postfix that should be concatenated.</param>
        public StepConcat(string prefix, string postfix)
        {
            this.prefix = prefix;
            this.postfix = postfix;
        }

        /// <summary>
        /// This step should never fail.
        /// </summary>
        /// <returns>It returns false.</returns>
        public override bool CanFail()
        {
            return false;
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
            info.AddValue(nameof(this.prefix), this.prefix, typeof(string));
            info.AddValue(nameof(this.postfix), this.postfix, typeof(string));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Concat({this.prefix};{this.postfix})";
        }

        /// <summary>
        /// It concatenates a prefix and a postfix to the actual value, then it will walk into the tree and recurse through all the remaining steps.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);
            var filteredValue = this.prefix + actualValue + this.postfix;
            return this.WalkNextStep(tree, filteredValue);
        }
    }
}
