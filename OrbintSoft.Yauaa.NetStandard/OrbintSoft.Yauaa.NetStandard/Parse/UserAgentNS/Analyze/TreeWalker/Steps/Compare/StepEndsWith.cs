//<copyright file="StepEndsWith.cs" company="OrbintSoft">
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
//<date>2018, 8, 13, 22:27</date>
//<summary></summary>

using Antlr4.Runtime.Tree;
using System;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Compare
{
    [Serializable]
    public class StepEndsWith: Step
    {
        private readonly string desiredValue;

        public StepEndsWith(string desiredValue)
        {
            this.desiredValue = desiredValue.ToLower();
        }

        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            string actualValue = GetActualValue(tree, value);

            if (actualValue.ToLower().EndsWith(desiredValue))
            {
                return WalkNextStep(tree, actualValue);
            }
            return null;
        }

        public override string ToString()
        {
            return "EndsWith(" + desiredValue + ")";
        }
    }
}
