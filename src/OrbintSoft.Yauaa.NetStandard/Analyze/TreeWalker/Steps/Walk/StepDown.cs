//-----------------------------------------------------------------------
// <copyright file="StepDown.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk
{
    using System;
    using System.Runtime.Serialization;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk.StepDowns;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// This class defines the Down Step, it is used in parsing to go down to children of current node in the tree.
    /// </summary>
    [Serializable]
    public class StepDown : Step
    {
        /// <summary>
        /// Defines the end.
        /// </summary>
        private readonly int end;

        /// <summary>
        /// Defines the name.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Defines the start.
        /// </summary>
        private readonly int start;

        /// <summary>
        /// Defines the userAgentGetChildrenVisitor.
        /// </summary>
        [NonSerialized]
        private UserAgentGetChildrenVisitor userAgentGetChildrenVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDown"/> class.
        /// This is used only for binary deserialization.
        /// </summary>
        /// <param name="info">The info <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context <see cref="StreamingContext"/>.</param>
        public StepDown(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.name = (string)info.GetValue(nameof(this.name), typeof(string));
            this.start = (int)info.GetValue(nameof(this.start), typeof(int));
            this.end = (int)info.GetValue(nameof(this.end), typeof(int));
            this.SetDefaultFieldValues();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDown"/> class.
        /// </summary>
        /// <param name="numberRange">The range<see cref="UserAgentTreeWalkerParser.NumberRangeContext"/> used to get the list of children.</param>
        /// <param name="name">The name of the node.</param>
        public StepDown(UserAgentTreeWalkerParser.NumberRangeContext numberRange, string name)
            : this(NumberRangeVisitor.GetList(numberRange), name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepDown"/> class.
        /// </summary>
        /// <param name="numberRange">The range to get the list of children.</param>
        /// <param name="name">The name of the node.</param>
        private StepDown(NumberRangeList numberRange, string name)
        {
            this.name = name;
            this.start = numberRange.Start;
            this.end = numberRange.End;
            this.SetDefaultFieldValues();
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
            info.AddValue(nameof(this.name), this.name, typeof(string));
            info.AddValue(nameof(this.start), this.start, typeof(int));
            info.AddValue(nameof(this.end), this.end, typeof(int));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Down([{this.start}:{this.end}]{this.name})";
        }

        /// <summary>
        /// It iterates throught children walking to next steps until it find a value, if a value is found it stops iterating.
        /// </summary>
        /// <param name="tree">The tree to walk into.</param>
        /// <param name="value">Not used.</param>
        /// <returns>Either null or the actual value that was found.</returns>
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            using (var children = this.userAgentGetChildrenVisitor.Visit(tree))
            {
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
            }

            return null;
        }

        /// <summary>
        /// Initialize the transient default values.
        /// </summary>
        private void SetDefaultFieldValues()
        {
            this.userAgentGetChildrenVisitor = new UserAgentGetChildrenVisitor(this.name, this.start, this.end);
        }
    }
}
