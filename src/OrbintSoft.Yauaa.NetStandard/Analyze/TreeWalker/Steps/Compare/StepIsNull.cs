//-----------------------------------------------------------------------
// <copyright file="StepIsNull.cs" company="OrbintSoft">
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
    /// This class defines the IsNull Step, it is used in parsing to check if the next step is null.
    /// </summary>
    [Serializable]
    public class StepIsNull : Step
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsNull"/> class.
        /// </summary>
        public StepIsNull()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepIsNull"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepIsNull(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "IsNull()";
        }

        /// <summary>
        /// It checks if the next step is null in that case it will return a &lt;&lt;&lt;Null Value&gt;&gt;&gt; <see cref="WalkList.WalkResult"/>
        /// othewise it will return null.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">The value of the step.</param>
        /// <returns>Either null or a <see cref="WalkList.WalkResult"/>.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.WalkNextStep(tree, value);

            if (actualValue is null || actualValue.Value is null)
            {
                return new WalkList.WalkResult(tree, "<<<Null Value>>>");
            }

            return null;
        }
    }
}
