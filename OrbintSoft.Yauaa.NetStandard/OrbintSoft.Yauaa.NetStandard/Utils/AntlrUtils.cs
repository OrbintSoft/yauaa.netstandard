//<copyright file="AntlrUtils.cs" company="OrbintSoft">
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
//<date>2018, 8, 12, 15:52</date>
//<summary></summary>

namespace OrbintSoft.Yauaa.Utils
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;

    /// <summary>
    /// Defines the <see cref="AntlrUtils" />
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
        /// The GetSourceText
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetSourceText(ParserRuleContext ctx)
        {
            if (ctx.start == null || ctx.stop == null)
            {
                return ctx.GetText();
            }
            int startIndex = ctx.start.StartIndex;
            int stopIndex = ctx.stop.StopIndex;
            if (stopIndex < startIndex)
            {
                return ""; // Just return the empty string.
            }
            ICharStream inputStream = ctx.start.InputStream;
            return inputStream.GetText(new Interval(startIndex, stopIndex));
        }
    }
}
