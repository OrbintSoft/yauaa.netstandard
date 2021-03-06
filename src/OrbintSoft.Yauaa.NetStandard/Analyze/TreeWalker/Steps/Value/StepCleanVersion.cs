﻿//-----------------------------------------------------------------------
// <copyright file="StepCleanVersion.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Value
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Parse;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// This class defines the CleanVersion Step, it is used in parsing to format the value of a version node.
    /// </summary>
    [Serializable]
    public class StepCleanVersion : Step
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepCleanVersion"/> class.
        /// </summary>
        public StepCleanVersion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepCleanVersion"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepCleanVersion(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// This step should never fail.
        /// </summary>
        /// <returns>It returns false.</returns>
        public override bool CanFail()
        {
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "CleanVersion()";
        }

        /// <summary>
        /// It cleans the actual value, then it will walk into the tree and recurse through all the remaining steps.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The actual value of the node or null to get the root.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);

            actualValue = Normalize.ReplaceString(actualValue, "_", ".");
            actualValue = Normalize.ReplaceString(actualValue, "/", " ");
            return this.WalkNextStep(tree, actualValue);
        }
    }
}
