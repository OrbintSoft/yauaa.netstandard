/*
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
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * All rights should be reserved to the original author Niels Basjes
 */

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze
{
    public sealed class NumberRangeVisitor: UserAgentTreeWalkerBaseVisitor<NumberRangeList>
    {
        private const int DEFAULT_MIN = 1;
        private const int DEFAULT_MAX = 10;

        internal static readonly NumberRangeVisitor Instance = new NumberRangeVisitor();

        private static readonly Dictionary<string, int> MaxRange = new Dictionary<string, int>();

        static NumberRangeVisitor() {
            // Hardcoded maximum values because of the parsing rules
            MaxRange["agent"] = 1;
            MaxRange["name"] = 1;
            MaxRange["key"] = 1;

            // Did statistics on over 200K real useragents from 2015.
            // These are the maximum values from that test set (+ a little margin)
            MaxRange["value"] = 2; // Max was 2
            MaxRange["version"] = 5; // Max was 4
            MaxRange["comments"] = 2; // Max was 2
            MaxRange["entry"] = 20; // Max was much higher
            MaxRange["product"] = 10; // Max was much higher

            MaxRange["email"] = 2;
            MaxRange["keyvalue"] = 3;
            MaxRange["text"] = 8;
            MaxRange["url"] = 3;
            MaxRange["uuid"] = 4;
        }

        private NumberRangeVisitor()
        {
        }

        private static int GetMaxRange(UserAgentTreeWalkerParser.NumberRangeContext ctx)
        {
            RuleContext parent = ctx.Parent;
            // The antlr rules force this to always be true.
            //if (!(parent is UserAgentTreeWalkerParser.StepDownContext)) {
            //    return DEFAULT_MAX;
            //}
            string name = ((UserAgentTreeWalkerParser.StepDownContext)parent).name.Text;
            int? maxRange = MaxRange.ContainsKey(name) ? (int?) MaxRange[name] : null;
            if (maxRange == null)
            {
                return DEFAULT_MAX;
            }
            return maxRange ?? 0;
        }
       
        public static NumberRangeList GetList(UserAgentTreeWalkerParser.NumberRangeContext ctx)
        {
            return Instance.Visit(ctx);
        }

        public override NumberRangeList VisitNumberRangeStartToEnd([NotNull] UserAgentTreeWalkerParser.NumberRangeStartToEndContext context)
        {
            return new NumberRangeList(
                int.Parse(context.rangeStart.Text),
                int.Parse(context.rangeEnd.Text));
        }

        public override NumberRangeList VisitNumberRangeSingleValue([NotNull] UserAgentTreeWalkerParser.NumberRangeSingleValueContext context)
        {
            int value = int.Parse(context.count.Text);
            return new NumberRangeList(value, value);
        }

        public override NumberRangeList VisitNumberRangeAll([NotNull] UserAgentTreeWalkerParser.NumberRangeAllContext context)
        {
            return new NumberRangeList(DEFAULT_MIN, GetMaxRange(context));
        }

        public override NumberRangeList VisitNumberRangeEmpty([NotNull] UserAgentTreeWalkerParser.NumberRangeEmptyContext context)
        {
            return new NumberRangeList(DEFAULT_MIN, GetMaxRange(context));
        }
    }
}
