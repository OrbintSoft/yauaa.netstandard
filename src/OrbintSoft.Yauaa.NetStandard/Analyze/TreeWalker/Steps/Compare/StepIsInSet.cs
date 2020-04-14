//-----------------------------------------------------------------------
// <copyright file="StepIsInSet.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Compare
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines the IsInSet Step, it is used in parsing to check if the actual value is the set of provided values.
    /// </summary>
    [Serializable]
    public class StepIsInSet : Step
    {
        /// <summary>
        /// Defines the set of values that you want use for matching.
        /// </summary>
        private readonly ISet<string> list;

        /// <summary>
        /// Defines the name of the list.
        /// </summary>
        private readonly string listName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsInSet"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepIsInSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.listName = (string)info.GetValue(nameof(this.listName), typeof(string));
            this.list = (ISet<string>)info.GetValue(nameof(this.list), typeof(ISet<string>));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsInSet"/> class.
        /// </summary>
        /// <param name="listName">The listName, a conventional name associated to the list, used to define operations.</param>
        /// <param name="list">The set of values you want use to match.</param>
        public StepIsInSet(string listName, ISet<string> list)
        {
            this.listName = listName;
            this.list = list;
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
            info.AddValue(nameof(this.listName), this.listName, typeof(string));
            info.AddValue(nameof(this.list), this.list, typeof(ISet<string>));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"IsInSet(@{this.listName})";
        }

        /// <summary>
        /// It checks if the actual value is contained in the set of desidered values, in that case:
        /// this will walk into the tree and recurse through all the remaining steps.
        /// Otherwise it will return null, since no other steps to walk.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);

            if (this.list.Contains(actualValue.ToLower()))
            {
                return this.WalkNextStep(tree, actualValue);
            }
            else
            {
                return null;
            }
        }
    }
}
