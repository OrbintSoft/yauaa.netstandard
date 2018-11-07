﻿/*
 * Yet Another UserAgent Analyzer .NET Standard
 * Porting realized by Balzarotti Stefano, Copyright (C) OrbintSoft
 * 
 * Original Author and License:
 * 
 * Yet Another UserAgent Analyzer
 * Copyright (C) 2013-2018 Niels Basjes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using Antlr4.Runtime.Tree;
using System;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Compare
{
    [Serializable]
    public class StepIsNull: Step
    {
        public override WalkList.WalkResult Walk(IParseTree tree, string value)
        {
            WalkList.WalkResult actualValue = WalkNextStep(tree, value);

            if (actualValue == null || actualValue.GetValue() == null)
            {
                return new WalkList.WalkResult(tree, "<<<Null Value>>>");
            }
            return null;
        }
    
        public override string ToString()
        {
            return "IsNull()";
        }
    }
}
