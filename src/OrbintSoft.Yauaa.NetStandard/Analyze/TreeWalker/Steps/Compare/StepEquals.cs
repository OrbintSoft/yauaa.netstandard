//-----------------------------------------------------------------------
// <copyright file="StepEquals.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Compare
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// This class defines the Equals Step, it is used in parsing to check if the actual value is equal to the desidered value.
    /// </summary>
    [Serializable]
    public class StepEquals : Step
    {
        /// <summary>
        /// Defines the desired value you want compare.
        /// </summary>
        private readonly string desiredValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepEquals"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepEquals(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.desiredValue = (string)info.GetValue(nameof(this.desiredValue), typeof(string));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepEquals"/> class.
        /// </summary>
        /// <param name="desiredValue">The desired value you want check if equals to the actual value.</param>
        public StepEquals(string desiredValue)
        {
            this.desiredValue = desiredValue.ToLower();
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
            info.AddValue(nameof(this.desiredValue), this.desiredValue, typeof(string));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Equals({this.desiredValue})";
        }

        /// <summary>
        /// It checks if tha actual value equals to the desired value, in that case:
        /// this will walk into the tree and recurse through all the remaining steps.
        /// Otherwise it will return null, since no other steps to walk.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The value<see cref="string"/>.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);

            if (actualValue.Equals(this.desiredValue, StringComparison.InvariantCultureIgnoreCase))
            {
                return this.WalkNextStep(tree, actualValue);
            }

            return null;
        }
    }
}
