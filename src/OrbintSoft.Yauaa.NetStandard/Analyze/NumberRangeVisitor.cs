//-----------------------------------------------------------------------
// <copyright file="NumberRangeVisitor.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze
{
    using System.Collections.Generic;
    using Antlr4.Runtime.Misc;
    using OrbintSoft.Yauaa.Antlr4Source;

    /// <summary>
    /// Implements an antlr visitor for parsing number ranges.
    /// </summary>
    public sealed class NumberRangeVisitor : UserAgentTreeWalkerBaseVisitor<NumberRangeList>
    {
        /// <summary>
        /// Defines the singleton instance.
        /// </summary>
        internal static readonly NumberRangeVisitor Instance = new NumberRangeVisitor();

        /// <summary>
        /// Defines default max number that can be used for a range.
        /// </summary>
        private const int DEFAULT_MAX = 10;

        /// <summary>
        /// Defines the default min number that can be used for a range.
        /// </summary>
        private const int DEFAULT_MIN = 1;

        /// <summary>
        /// Defines a dictionary of max range per user agent section.
        /// </summary>
        private static readonly IDictionary<string, int> MaxRange = new Dictionary<string, int>();

        /// <summary>
        /// Initializes static members of the <see cref="NumberRangeVisitor"/> class.
        /// </summary>
        static NumberRangeVisitor()
        {
            // Hardcoded maximum values because of the parsing rules
            MaxRange["agent"] = 1;
            MaxRange["name"] = 1;
            MaxRange["key"] = 1;

            // Did statistics on over 200K real useragents from 2015.
            // These are the maximum values from that test set (+ a little margin)
            MaxRange["value"] = 2; // Max was 2
            MaxRange["version"] = 4; // Max was 4
            MaxRange["comments"] = 2; // Max was 2
            MaxRange["entry"] = 20; // Max was much higher
            MaxRange["product"] = 10; // Max was much higher

            MaxRange["email"] = 2;
            MaxRange["keyvalue"] = 3;
            MaxRange["text"] = 8;
            MaxRange["url"] = 2;
            MaxRange["uuid"] = 2;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NumberRangeVisitor"/> class from being created.
        /// </summary>
        private NumberRangeVisitor()
        {
        }

        /// <summary>
        /// Gets the <see cref="NumberRangeList"/> after parsing the user agent.
        /// </summary>
        /// <param name="ctx">The ctx<see cref="UserAgentTreeWalkerParser.NumberRangeContext"/>.</param>
        /// <returns>The <see cref="NumberRangeList"/>.</returns>
        public static NumberRangeList GetList(UserAgentTreeWalkerParser.NumberRangeContext ctx)
        {
            return Instance.Visit(ctx);
        }

        /// <summary>
        /// Visits a number range all (from <see cref="DEFAULT_MIN"/> to max).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The resulting <see cref="NumberRangeList"/>.</returns>
        public override NumberRangeList VisitNumberRangeAll([NotNull] UserAgentTreeWalkerParser.NumberRangeAllContext context)
        {
            return new NumberRangeList(DEFAULT_MIN, GetMaxRange(context));
        }

        /// <summary>
        /// Visits an empty range, returns as default from <see cref="DEFAULT_MIN"/> to max.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The resulting <see cref="NumberRangeList"/>.</returns>
        public override NumberRangeList VisitNumberRangeEmpty([NotNull] UserAgentTreeWalkerParser.NumberRangeEmptyContext context)
        {
            return new NumberRangeList(DEFAULT_MIN, GetMaxRange(context));
        }

        /// <summary>
        /// Visits a range with open start from <see cref="DEFAULT_MIN"/> to end value.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.NumberRangeOpenStartToEndContext"/>.</param>
        /// <returns>The resulting <see cref="NumberRangeList"/>.</returns>
        public override NumberRangeList VisitNumberRangeOpenStartToEnd([NotNull] UserAgentTreeWalkerParser.NumberRangeOpenStartToEndContext context)
        {
            return new NumberRangeList(DEFAULT_MIN, int.Parse(context.rangeEnd.Text));
        }

        /// <summary>
        /// Vists a range with single value (from value to value).
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.NumberRangeSingleValueContext"/>.</param>
        /// <returns>The resulting <see cref="NumberRangeList"/>.</returns>
        public override NumberRangeList VisitNumberRangeSingleValue([NotNull] UserAgentTreeWalkerParser.NumberRangeSingleValueContext context)
        {
            var value = int.Parse(context.count.Text);
            return new NumberRangeList(value, value);
        }

        /// <summary>
        /// Visits a range with start and end (from start value to end value(.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.NumberRangeStartToEndContext"/>.</param>
        /// <returns>The resulting <see cref="NumberRangeList"/>.</returns>
        public override NumberRangeList VisitNumberRangeStartToEnd([NotNull] UserAgentTreeWalkerParser.NumberRangeStartToEndContext context)
        {
            return new NumberRangeList(
                int.Parse(context.rangeStart.Text),
                int.Parse(context.rangeEnd.Text));
        }

        /// <summary>
        /// Visits a range with open end from start value to max value.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentTreeWalkerParser.NumberRangeStartToOpenEndContext"/>.</param>
        /// <returns>The resulting <see cref="NumberRangeList"/>.</returns>
        public override NumberRangeList VisitNumberRangeStartToOpenEnd([NotNull] UserAgentTreeWalkerParser.NumberRangeStartToOpenEndContext context)
        {
            return new NumberRangeList(int.Parse(context.rangeStart.Text), GetMaxRange(context));
        }

        /// <summary>
        /// Gets the max range based on the context.
        /// If not faound <see cref="DEFAULT_MAX"/> is used.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <returns>The max value for the range.</returns>
        private static int GetMaxRange(UserAgentTreeWalkerParser.NumberRangeContext ctx)
        {
            var parent = ctx.Parent;
            var name = ((UserAgentTreeWalkerParser.StepDownContext)parent).name.Text;
            var maxRange = MaxRange.ContainsKey(name) ? (int?)MaxRange[name] : null;
            if (maxRange is null)
            {
                return DEFAULT_MAX;
            }

            return maxRange ?? 0;
        }
    }
}
