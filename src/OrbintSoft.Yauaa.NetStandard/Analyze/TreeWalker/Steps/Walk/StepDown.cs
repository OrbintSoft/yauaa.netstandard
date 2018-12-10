//-----------------------------------------------------------------------
// <copyright file="StepDown.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk
{
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk.StepDowns;
    using OrbintSoft.Yauaa.Antlr4Source;
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the <see cref="StepDown" />
    /// </summary>
    [Serializable]
    public class StepDown : Step
    {
        /// <summary>
        /// Defines the end
        /// </summary>
        private readonly int end;

        /// <summary>
        /// Defines the name
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Defines the start
        /// </summary>
        private readonly int start;

        /// <summary>
        /// Defines the userAgentGetChildrenVisitor
        /// </summary>
        [NonSerialized]
        private UserAgentGetChildrenVisitor userAgentGetChildrenVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDown"/> class.
        /// </summary>
        /// <param name="numberRange">The numberRange<see cref="UserAgentTreeWalkerParser.NumberRangeContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        public StepDown(UserAgentTreeWalkerParser.NumberRangeContext numberRange, string name) : this(NumberRangeVisitor.GetList(numberRange), name)
        {
        }

        public StepDown(SerializationInfo info, StreamingContext context): base(info, context)
        {
            this.name = (string)info.GetValue("name", typeof(string));
            this.start = (int)info.GetValue("start", typeof(int));
            this.end = (int)info.GetValue("end", typeof(int));
            this.SetDefaultFieldValues();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDown"/> class.
        /// </summary>
        /// <param name="numberRange">The numberRange<see cref="NumberRangeList"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        private StepDown(NumberRangeList numberRange, string name)
        {
            this.name = name;
            this.start = numberRange.Start;
            this.end = numberRange.End;
            this.SetDefaultFieldValues();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("name", this.name, typeof(string));
            info.AddValue("start", this.start, typeof(int));
            info.AddValue("end", this.end, typeof(int));           
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return "Down([" + this.start + ":" + this.end + "]" + this.name + ")";
        }

        /// <summary>
        /// The Walk
        /// </summary>
        /// <param name="tree">The tree<see cref="IParseTree"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="WalkList.WalkResult"/></returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            var children = this.userAgentGetChildrenVisitor.Visit(tree);
            if (children != null)
            {
                do
                {
                    if (children.Current != null || children.MoveNext())
                    {
                        var child = children.Current;
                        var childResult = this.WalkNextStep(child, null);
                        if (childResult != null)
                        {
                            return childResult;
                        }
                    }
                }
                while (children.MoveNext());
            }

            return null;
        }

        /// <summary>
        /// The ReadObject
        /// </summary>
        /// <param name="stream">The stream<see cref="Stream"/></param>
        private void ReadObject(Stream stream)
        {
            this.SetDefaultFieldValues();
        }

        /// <summary>
        /// Initialize the transient default values
        /// </summary>
        private void SetDefaultFieldValues()
        {
            this.userAgentGetChildrenVisitor = new UserAgentGetChildrenVisitor(this.name, this.start, this.end);
        }
    }
}
