//-----------------------------------------------------------------------
// <copyright file="StepNormalizeBrand.cs" company="OrbintSoft">
//    Yet Another User Agent Analyzer for .NET Standard
//    porting realized by Stefano Balzarotti, Copyright 2018 (C) OrbintSoft
//
//    Original Author and License:
//
//    Yet Another UserAgent Analyzer
//    Copyright(C) 2013-2018 Niels Basjes
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//    https://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:48</date>
// <summary></summary>
//-----------------------------------------------------------------------

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Value
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// Defines the <see cref="StepNormalizeBrand" />
    /// </summary>
    [Serializable]
    public class StepNormalizeBrand : Step
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepNormalizeBrand"/> class.
        /// </summary>
        public StepNormalizeBrand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepNormalizeBrand"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/></param>
        /// <param name="context">The context<see cref="StreamingContext"/></param>
        public StepNormalizeBrand(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// The CanFail
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public override bool CanFail()
        {
            return false;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "StepNormalizeBrand()";
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var actualValue = this.GetActualValue(tree, value);
            var filteredValue = Normalize.Brand(actualValue);
            return this.WalkNextStep(tree, filteredValue);
        }
    }
}
