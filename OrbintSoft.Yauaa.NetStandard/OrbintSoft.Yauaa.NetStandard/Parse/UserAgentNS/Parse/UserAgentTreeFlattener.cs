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

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Utils;
using System;
using System.Collections.Generic;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Parse
{
    public class UserAgentTreeFlattener: UserAgentBaseListener 
    {
        private static readonly ParseTreeWalker WALKER = new ParseTreeWalker();
        private readonly IAnalyzer analyzer;

        public enum PathType
        {
            CHILD,
            COMMENT,
            VERSION
        }

        public class State
        {
            internal long child = 0;
            internal long version = 0;
            internal long comment = 0;
            internal readonly string name;
            internal string path;
            internal IParseTree ctx = null;

            private UserAgentTreeFlattener userAgentTreeFlattener;

            public State(UserAgentTreeFlattener userAgentTreeFlattener, string name)
            {
                this.userAgentTreeFlattener = userAgentTreeFlattener;
                this.name = name;
            }

            public State(UserAgentTreeFlattener userAgentTreeFlattener, IParseTree ctx, string name): this(userAgentTreeFlattener, name)
            {
                this.ctx = ctx;
            }

            public string CalculatePath(PathType type, bool fakeChild)
            {
                IParseTree node = ctx;
                path = name;
                if (node == null)
                {
                    return path;
                }
                State parentState = null;

                while (parentState == null)
                {
                    node = node.Parent;
                    if (node == null)
                    {
                        return path;
                    }
                    parentState = userAgentTreeFlattener.state.Get(node);
                }

                long counter = 0;
                switch (type)
                {
                    case PathType.CHILD:
                        if (!fakeChild)
                        {
                            parentState.child++;
                        }
                        counter = parentState.child;
                        break;
                    case PathType.COMMENT:
                        if (!fakeChild)
                        {
                            parentState.comment++;
                        }
                        counter = parentState.comment;
                        break;
                    case PathType.VERSION:
                        if (!fakeChild)
                        {
                            parentState.version++;
                        }
                        counter = parentState.version;
                        break;
                    default:
                        break;
                }

                path = parentState.path + ".(" + counter + ')' + name;

                return path;
            }
        }

        private ParseTreeProperty<State> state;

        public UserAgentTreeFlattener(IAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

#if DEBUG
        private bool verbose = true;
#else
        private bool verbose = false;
#endif
        public void SetVerbose(bool newVerbose)
        {
            verbose = newVerbose;
        }

        public UserAgent Parse(string userAgentString)
        {
            UserAgent userAgent = new UserAgent(userAgentString);
            return ParseIntoCleanUserAgent(userAgent);
        }

        public UserAgent Parse(UserAgent userAgent)
        {
            userAgent.Reset();
            return ParseIntoCleanUserAgent(userAgent);
        }

        private UserAgent ParseIntoCleanUserAgent(UserAgent userAgent)
        {
            if (userAgent.GetUserAgentString() == null)
            {
                userAgent.Set(UserAgent.SYNTAX_ERROR, "true", 1);
                return userAgent; // Cannot parse this
            }

            // Parse the userAgent into tree
            UserAgentParser.UserAgentContext userAgentContext = ParseUserAgent(userAgent);

            // Walk the tree an inform the calling analyzer about all the nodes found
            state = new ParseTreeProperty<State>();

            State rootState = new State(this, "agent");
            rootState.CalculatePath(PathType.CHILD, false);
            state.Put(userAgentContext, rootState);

            if (userAgent.HasSyntaxError)
            {
                Inform(null, UserAgent.SYNTAX_ERROR, "true");
            }
            else
            {
                Inform(null, UserAgent.SYNTAX_ERROR, "false");
            }

            WALKER.Walk(this, userAgentContext);
            return userAgent;
        }

        // =================================================================================

        private string Inform(IParseTree ctx, string path)
        {
            return Inform(ctx, path, AntlrUtils.GetSourceText((ParserRuleContext)ctx));
        }

        private string Inform(IParseTree ctx, string name, string value)
        {
            return Inform(ctx, ctx, name, value, false);
        }

        private string Inform(IParseTree ctx, string name, string value, bool fakeChild)
        {
            return Inform(ctx, ctx, name, value, fakeChild);
        }

        private string Inform(IParseTree stateCtx, IParseTree ctx, string name, string value, bool fakeChild)
        {
            State myState = new State(this, stateCtx, name);

            if (!fakeChild && stateCtx != null)
            {
                state.Put(stateCtx, myState);
            }

            PathType childType;
            switch (name)
            {
                case "comments":
                    childType = PathType.COMMENT;
                    break;
                case "version":
                    childType = PathType.VERSION;
                    break;
                default:
                    childType = PathType.CHILD;
                    break;
            }

            string path = myState.CalculatePath(childType, fakeChild);
            analyzer.Inform(path, value, ctx);
            return path;
        }

        //  =================================================================================

        private UserAgentParser.UserAgentContext ParseUserAgent(UserAgent userAgent)
        {
            string userAgentString = EvilManualUseragentStringHacks.FixIt(userAgent.GetUserAgentString());

            AntlrInputStream input = new AntlrInputStream(userAgentString);
            UserAgentLexer lexer = new UserAgentLexer(input);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            UserAgentParser parser = new UserAgentParser(tokens);

            if (!verbose)
            {
                lexer.RemoveErrorListeners();
                parser.RemoveErrorListeners();
            }
            lexer.AddErrorListener(userAgent);
            parser.AddErrorListener(userAgent);

            return parser.userAgent();
        }

        public override void EnterUserAgent([NotNull] UserAgentParser.UserAgentContext context)
        {
            // In case of a parse error the 'parsed' version of agent can be incomplete
            Inform(context, "agent", context.start.TokenSource.InputStream.ToString());
        }

        public override void EnterRootText([NotNull] UserAgentParser.RootTextContext context)
        {
            InformSubstrings(context, "text");
        }

        public override void EnterProduct([NotNull] UserAgentParser.ProductContext context)
        {
            InformSubstrings(context, "product");
        }

        public override void EnterCommentProduct([NotNull] UserAgentParser.CommentProductContext context)
        {
            InformSubstrings(context, "product");
        }

        public override void EnterProductNameNoVersion([NotNull] UserAgentParser.ProductNameNoVersionContext context)
        {
            InformSubstrings(context, "product");
        }

        public override void EnterProductNameEmail([NotNull] UserAgentParser.ProductNameEmailContext context)
        {
            Inform(context, "name");
        }

        public override void EnterProductNameUrl([NotNull] UserAgentParser.ProductNameUrlContext context)
        {
            Inform(context, "name");
        }

        public override void EnterProductNameWords([NotNull] UserAgentParser.ProductNameWordsContext context)
        {
            InformSubstrings(context, "name");
        }

        public override void EnterProductNameKeyValue([NotNull] UserAgentParser.ProductNameKeyValueContext context)
        {
            Inform(context, "name.(1)keyvalue", context.GetText(), false);
            InformSubstrings(context, "name", true);
        }

        public override void EnterProductNameVersion([NotNull] UserAgentParser.ProductNameVersionContext context)
        {
            InformSubstrings(context, "name");
        }

        public override void EnterProductNameUuid([NotNull] UserAgentParser.ProductNameUuidContext context)
        {
            Inform(context, "name");
        }

        public override void EnterProductVersion([NotNull] UserAgentParser.ProductVersionContext context)
        {
            EnterProductVersion(context);
        }

        public override void EnterProductVersionWithCommas([NotNull] UserAgentParser.ProductVersionWithCommasContext context)
        {
            EnterProductVersion(context);
        }

        private void EnterProductVersion(IParseTree ctx)
        {
            if (ctx.ChildCount != 1)
            {
                // These are the specials with multiple children like keyvalue, etc.
                Inform(ctx, "version");
                return;
            }

            IParseTree child = ctx.GetChild(0);
            // Only for the SingleVersion edition we want to have splits of the version.
            if (child is UserAgentParser.SingleVersionContext || child is UserAgentParser.SingleVersionWithCommasContext) {
                return;
            }

            Inform(ctx, "version");
        }

        public override void EnterProductVersionSingleWord([NotNull] UserAgentParser.ProductVersionSingleWordContext context)
        {
            Inform(context, "version");
        }

        public override void EnterSingleVersion([NotNull] UserAgentParser.SingleVersionContext context)
        {
            InformSubVersions(context, "version");
        }

        public override void EnterSingleVersionWithCommas([NotNull] UserAgentParser.SingleVersionWithCommasContext context)
        {
            InformSubVersions(context, "version");
        }

        public override void EnterProductVersionWords([NotNull] UserAgentParser.ProductVersionWordsContext context)
        {
            InformSubstrings(context, "version");
        }

        public override void EnterKeyValueProductVersionName([NotNull] UserAgentParser.KeyValueProductVersionNameContext context)
        {
            InformSubstrings(context, "version");
        }

        public override void EnterCommentBlock([NotNull] UserAgentParser.CommentBlockContext context)
        {
            Inform(context, "comments");
        }

        public override void EnterCommentEntry([NotNull] UserAgentParser.CommentEntryContext context)
        {
            InformSubstrings(context, "entry");
        }

        private void InformSubstrings(ParserRuleContext ctx, string name)
        {
            InformSubstrings(ctx, name, false);
        }

        private void InformSubstrings(ParserRuleContext ctx, string name, bool fakeChild)
        {
            InformSubstrings(ctx, name, fakeChild, WordSplitter.GetInstance());
        }

        private void InformSubVersions(ParserRuleContext ctx, string name)
        {
            InformSubstrings(ctx, name, false, VersionSplitter.GetInstance());
        }

        private void InformSubstrings(ParserRuleContext ctx, string name, bool fakeChild, Splitter splitter)
        {
            string text = AntlrUtils.GetSourceText(ctx);
            string path = Inform(ctx, name, text, fakeChild);
            ISet<WordRangeVisitor.Range> ranges = analyzer.GetRequiredInformRanges(path);

            if (ranges.Count > 4)
            { // Benchmarks showed this to be the breakeven point. (see below)
                List<Tuple<int, int>> splitList = splitter.CreateSplitList(text);
                foreach (WordRangeVisitor.Range range in ranges)
                {
                    string value = splitter.GetSplitRange(text, splitList, range);
                    if (value != null)
                    {
                        Inform(ctx, ctx, name + range, value, true);
                    }
                }
            }
            else
            {
                foreach (WordRangeVisitor.Range range in ranges)
                {
                    string value = splitter.GetSplitRange(text, range);
                    if (value != null)
                    {
                        Inform(ctx, ctx, name + range, value, true);
                    }
                }
            }
        }

        // # Ranges | Direct                   |  SplitList
        // 1        |    1.664 ± 0.010  ns/op  |    99.378 ± 1.548  ns/op
        // 2        |   38.103 ± 0.479  ns/op  |   115.808 ± 1.055  ns/op
        // 3        |  109.023 ± 0.849  ns/op  |   141.473 ± 6.702  ns/op
        // 4        |  162.917 ± 1.842  ns/op  |   166.120 ± 7.166  ns/op  <-- Break even
        // 5        |  264.877 ± 6.264  ns/op  |   176.334 ± 3.999  ns/op
        // 6        |  356.914 ± 2.573  ns/op  |   196.640 ± 1.306  ns/op
        // 7        |  446.930 ± 3.329  ns/op  |   215.499 ± 3.410  ns/op
        // 8        |  533.153 ± 2.250  ns/op  |   233.241 ± 5.311  ns/op
        // 9        |  519.130 ± 3.495  ns/op  |   250.921 ± 6.107  ns/op


        public override void EnterMultipleWords([NotNull] UserAgentParser.MultipleWordsContext context)
        {
            InformSubstrings(context, "text");
        }

        public override void EnterKeyValue([NotNull] UserAgentParser.KeyValueContext context)
        {
            Inform(context, "keyvalue");
        }

        public override void EnterKeyWithoutValue([NotNull] UserAgentParser.KeyWithoutValueContext context)
        {
            Inform(context, "keyvalue");            
        }

        public override void EnterKeyName([NotNull] UserAgentParser.KeyNameContext context)
        {
            InformSubstrings(context, "key");
        }

        public override void EnterKeyValueVersionName([NotNull] UserAgentParser.KeyValueVersionNameContext context)
        {
            InformSubstrings(context, "version");
        }

        public override void EnterVersionWords([NotNull] UserAgentParser.VersionWordsContext context)
        {
            InformSubstrings(context, "text");
        }

        public override void EnterSiteUrl([NotNull] UserAgentParser.SiteUrlContext context)
        {
            Inform(context, "url", context.url.Text);
        }

        public override void EnterUuId([NotNull] UserAgentParser.UuIdContext context)
        {
            Inform(context, "uuid", context.uuid.Text);
        }

        public override void EnterEmailAddress([NotNull] UserAgentParser.EmailAddressContext context)
        {
            Inform(context, "email", context.email.Text);
        }

        public override void EnterBase64([NotNull] UserAgentParser.Base64Context context)
        {
            Inform(context, "base64", context.value.Text);
        }

        public override void EnterEmptyWord([NotNull] UserAgentParser.EmptyWordContext context)
        {
            Inform(context, "text", "");
        } 
    }
}
