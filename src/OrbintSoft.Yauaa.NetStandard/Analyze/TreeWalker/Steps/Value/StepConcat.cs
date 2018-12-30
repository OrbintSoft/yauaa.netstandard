//-----------------------------------------------------------------------
// <copyright file="StepConcat.cs" company="OrbintSoft">
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
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;

    /// <summary>
    /// Defines the <see cref="StepConcat" />
    /// </summary>
    public class StepConcat : Step
    {
        /// <summary>
        /// Defines the postfix
        /// </summary>
        private readonly string postfix;

        /// <summary>
        /// Defines the prefix
        /// </summary>
        private readonly string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepConcat"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/></param>
        /// <param name="context">The context<see cref="StreamingContext"/></param>
        public StepConcat(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.prefix = (string)info.GetValue("prefix", typeof(string));
            this.postfix = (string)info.GetValue("postfix", typeof(string));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepConcat"/> class.
        /// </summary>
        /// <param name="prefix">The prefix<see cref="string"/></param>
        /// <param name="postfix">The postfix<see cref="string"/></param>
        public StepConcat(string prefix, string postfix)
        {
            this.prefix = prefix;
            this.postfix = postfix;
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
        /// The GetObjectData
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/></param>
        /// <param name="context">The context<see cref="StreamingContext"/></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("prefix", this.prefix, typeof(string));
            info.AddValue("postfix", this.postfix, typeof(string));
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "Concat(" + this.prefix + ";" + this.postfix + ")";
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
            var filteredValue = this.prefix + actualValue + this.postfix;
            return this.WalkNextStep(tree, filteredValue);
        }
    }
}
