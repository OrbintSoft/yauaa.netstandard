//-----------------------------------------------------------------------
// <copyright file="AntlrUtils.cs" company="OrbintSoft">
// Yet Another User Agent Analyzer for .NET Standard
// porting realized by Stefano Balzarotti, Copyright 2019 (C) OrbintSoft
//
// Original Author and License:
//
// Yet Another UserAgent Analyzer
// Copyright(C) 2013-2019 Niels Basjes
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Stefano Balzarotti, Niels Basjes</author>
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Utils
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;

    /// <summary>
    /// Defines the <see cref="AntlrUtils" />.
    /// </summary>
    public sealed class AntlrUtils
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="AntlrUtils"/> class from being created.
        /// </summary>
        private AntlrUtils()
        {
        }

        /// <summary>
        /// The GetSourceText.
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetSourceText(ParserRuleContext ctx)
        {
            if (ctx.Start == null || ctx.Stop == null)
            {
                return ctx.GetText();
            }

            var startIndex = ctx.Start.StartIndex;
            var stopIndex = ctx.Stop.StopIndex;
            if (stopIndex < startIndex)
            {
                return string.Empty; // Just return the empty string.
            }

            var inputStream = ctx.Start.InputStream;
            return inputStream.GetText(new Interval(startIndex, stopIndex));
        }
    }
}
