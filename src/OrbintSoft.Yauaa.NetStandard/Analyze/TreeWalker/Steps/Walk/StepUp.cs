//-----------------------------------------------------------------------
// <copyright file="StepUp.cs" company="OrbintSoft">
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
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    ///  This class defines the Up Step, it is used in parsing to go up to the parent of current node in the tree.
    /// </summary>
    [Serializable]
    public class StepUp : Step
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepUp"/> class.
        /// </summary>
        public StepUp()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepUp"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepUp(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Up()";
        }

        /// <summary>
        /// If there is parent return that, otherwise returns null.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">Not used.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var parent = this.Up(tree);
            if (parent is null)
            {
                return null;
            }

            return this.WalkNextStep(parent, null);
        }
    }
}
