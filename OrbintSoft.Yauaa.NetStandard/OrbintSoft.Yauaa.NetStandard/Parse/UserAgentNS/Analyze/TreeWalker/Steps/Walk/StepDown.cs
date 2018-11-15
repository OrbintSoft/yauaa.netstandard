//<copyright file="StepDown.cs" company="OrbintSoft">
//	Yet Another UserAgent Analyzer.NET Standard
//	Porting realized by Stefano Balzarotti, Copyright (C) OrbintSoft
//
//	Original Author and License:
//
//	Yet Another UserAgent Analyzer
//	Copyright(C) 2013-2018 Niels Basjes
//
//	Licensed under the Apache License, Version 2.0 (the "License");
//	you may not use this file except in compliance with the License.
//	You may obtain a copy of the License at
//
//	https://www.apache.org/licenses/LICENSE-2.0
//
//	Unless required by applicable law or agreed to in writing, software
//	distributed under the License is distributed on an "AS IS" BASIS,
//	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//	See the License for the specific language governing permissions and
//	limitations under the License.
//
//</copyright>
//<author>Stefano Balzarotti, Niels Basjes</author>
//<date>2018, 8, 16, 01:17</date>
//<summary></summary>

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Walk.StepDowns;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System;
using System.Collections.Generic;
using System.IO;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Walk
{
    [Serializable]
    public class StepDown: Step
    {
        private readonly int start;
        private readonly int end;
        private readonly string name;
        private UserAgentGetChildrenVisitor userAgentGetChildrenVisitor;

        public StepDown(UserAgentTreeWalkerParser.NumberRangeContext numberRange, string name) : this(NumberRangeVisitor.GetList(numberRange), name)
        {

        }

        private StepDown(NumberRangeList numberRange, string name)
        {
            this.name = name;
            start = numberRange.GetStart();
            end = numberRange.GetEnd();
            SetDefaultFieldValues();
        }

        /// <summary>
        /// Initialize the transient default values
        /// </summary>
        private void SetDefaultFieldValues()
        {
            userAgentGetChildrenVisitor = new UserAgentGetChildrenVisitor(name, start, end);
        }

        private void ReadObject(Stream stream)
        {
            SetDefaultFieldValues();
        }        

        public override string ToString()
        {
            return "Down([" + start + ":" + end + "]" + name + ")";
        }

        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            IEnumerator<ParserRuleContext> children = userAgentGetChildrenVisitor.Visit(tree);
            if (children != null)
            {
                do
                {
                    if (children.Current != null || children.MoveNext())
                    {
                        ParserRuleContext child = children.Current;
                        WalkList.WalkResult childResult = WalkNextStep(child, null);
                        if (childResult != null)
                        {
                            return childResult;
                        }
                    }
                } while (children.MoveNext());
            }           
            return null;
        }
    }
}
