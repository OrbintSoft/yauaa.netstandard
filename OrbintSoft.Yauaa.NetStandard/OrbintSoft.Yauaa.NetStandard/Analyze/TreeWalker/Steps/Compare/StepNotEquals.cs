//-----------------------------------------------------------------------
// <copyright file="StepNotEquals.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Compare
{
    using Antlr4.Runtime.Tree;
    using System;

    /// <summary>
    /// Defines the <see cref="StepNotEquals" />
    /// </summary>
    [Serializable]
    public class StepNotEquals : Step
    {
        /// <summary>
        /// Defines the desiredValue
        /// </summary>
        private readonly string desiredValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepNotEquals"/> class.
        /// </summary>
        /// <param name="desiredValue">The desiredValue<see cref="string"/></param>
        public StepNotEquals(string desiredValue)
        {
            this.desiredValue = desiredValue.ToLower();
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            string actualValue = GetActualValue(tree, value);

            if (!actualValue.Equals(desiredValue, StringComparison.InvariantCultureIgnoreCase))
            {
                return WalkNextStep(tree, actualValue);
            }
            return null;
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "NotEquals(" + desiredValue + ")";
        }
    }
}
