//-----------------------------------------------------------------------
// <copyright file="UserAgentTreeFlattener.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:49</date>
// <summary></summary>
//-----------------------------------------------------------------------
namespace OrbintSoft.Yauaa.Parse
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="UserAgentTreeFlattener" />
    /// </summary>
    [Serializable]
    public class UserAgentTreeFlattener : UserAgentBaseListener
    {
        private const string AGENT    = "agent";
        private const string PRODUCT  = "product";
        private const string NAME     = "name";
        private const string VERSION  = "version";
        private const string COMMENTS = "comments";
        private const string KEYVALUE = "keyvalue";
        private const string KEY      = "key";
        private const string TEXT     = "text";
        private const string URL      = "url";
        private const string UUID     = "uuid";
        private const string EMAIL    = "email";
        private const string BASE64   = "base64";

        /// <summary>
        /// Defines the WALKER
        /// </summary>
        private static readonly ParseTreeWalker Walker = new ParseTreeWalker();

        /// <summary>
        /// Defines the analyzer
        /// </summary>
        private readonly IAnalyzer analyzer;

        /// <summary>
        /// Defines the state
        /// </summary>
        [NonSerialized]
        private ParseTreeProperty<State> state = null;

#if VERBOSE
        private bool verbose = true;
#else
        /// <summary>
        /// Defines the verbose
        /// </summary>
        private bool verbose = false;

#endif
        public enum PathType
        {
            /// <summary>
            /// Defines the CHILD
            /// </summary>
            CHILD,

            /// <summary>
            /// Defines the COMMENT
            /// </summary>
            COMMENT,

            /// <summary>
            /// Defines the VERSION
            /// </summary>
            VERSION
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentTreeFlattener"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/></param>
        public UserAgentTreeFlattener(IAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        /// <summary>
        /// The SetVerbose
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        public void SetVerbose(bool newVerbose)
        {
            verbose = newVerbose;
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="userAgentString">The userAgentString<see cref="string"/></param>
        /// <returns>The <see cref="UserAgent"/></returns>
        public UserAgent Parse(string userAgentString)
        {
            UserAgent userAgent = new UserAgent(userAgentString);
            return ParseIntoCleanUserAgent(userAgent);
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="UserAgent"/></returns>
        public UserAgent Parse(UserAgent userAgent)
        {
            userAgent.Reset();
            return ParseIntoCleanUserAgent(userAgent);
        }

        /// <summary>
        /// The ParseIntoCleanUserAgent
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="UserAgent"/></returns>
        private UserAgent ParseIntoCleanUserAgent(UserAgent userAgent)
        {
            if (userAgent.UserAgentString == null)
            {
                userAgent.Set(UserAgent.SYNTAX_ERROR, "true", 1);
                return userAgent; // Cannot parse this
            }

            // Parse the userAgent into tree
            UserAgentParser.UserAgentContext userAgentContext = ParseUserAgent(userAgent);

            // Walk the tree an inform the calling analyzer about all the nodes found
            state = new ParseTreeProperty<State>();

            State rootState = new State(this,  AGENT);
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

            Walker.Walk(this, userAgentContext);
            return userAgent;
        }

        // =================================================================================
        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string Inform(IParseTree ctx, string path)
        {
            return Inform(ctx, path, AntlrUtils.GetSourceText((ParserRuleContext)ctx));
        }

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string Inform(IParseTree ctx, string name, string value)
        {
            return Inform(ctx, ctx, name, value, false);
        }

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string Inform(IParseTree ctx, string name, string value, bool fakeChild)
        {
            return Inform(ctx, ctx, name, value, fakeChild);
        }

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="stateCtx">The stateCtx<see cref="IParseTree"/></param>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string Inform(IParseTree stateCtx, IParseTree ctx, string name, string value, bool fakeChild)
        {
            string path = name;
            if (stateCtx == null)
            {
                analyzer.Inform(path, value, ctx);
            }
            else
            {
                State myState = new State(this, stateCtx, name);
                if (!fakeChild)
                {
                    state.Put(stateCtx, myState);
                }
                PathType childType;
                switch (name)
                {
                    case COMMENTS:
                        childType = PathType.COMMENT;
                        break;
                    case VERSION:
                        childType = PathType.VERSION;
                        break;
                    default:
                        childType = PathType.CHILD;
                        break;
                }

                path = myState.CalculatePath(childType, fakeChild);
                analyzer.Inform(path, value, ctx);
            }
            return path;
        }

        //  =================================================================================
        /// <summary>
        /// The ParseUserAgent
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="UserAgentParser.UserAgentContext"/></returns>
        private UserAgentParser.UserAgentContext ParseUserAgent(UserAgent userAgent)
        {
            string userAgentString = EvilManualUseragentStringHacks.FixIt(userAgent.UserAgentString);

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

        /// <summary>
        /// The EnterUserAgent
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.UserAgentContext"/></param>
        public override void EnterUserAgent([NotNull] UserAgentParser.UserAgentContext context)
        {
            // In case of a parse error the 'parsed' version of agent can be incomplete
            Inform(context, AGENT, context.start.TokenSource.InputStream.ToString());
        }

        /// <summary>
        /// The EnterRootText
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.RootTextContext"/></param>
        public override void EnterRootText([NotNull] UserAgentParser.RootTextContext context)
        {
            InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// The EnterProduct
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductContext"/></param>
        public override void EnterProduct([NotNull] UserAgentParser.ProductContext context)
        {
            InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// The EnterCommentProduct
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentProductContext"/></param>
        public override void EnterCommentProduct([NotNull] UserAgentParser.CommentProductContext context)
        {
            InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// The EnterProductNameNoVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameNoVersionContext"/></param>
        public override void EnterProductNameNoVersion([NotNull] UserAgentParser.ProductNameNoVersionContext context)
        {
            InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// The EnterProductNameEmail
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameEmailContext"/></param>
        public override void EnterProductNameEmail([NotNull] UserAgentParser.ProductNameEmailContext context)
        {
            Inform(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameUrl
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameUrlContext"/></param>
        public override void EnterProductNameUrl([NotNull] UserAgentParser.ProductNameUrlContext context)
        {
            Inform(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameWordsContext"/></param>
        public override void EnterProductNameWords([NotNull] UserAgentParser.ProductNameWordsContext context)
        {
            InformSubstrings(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameKeyValue
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameKeyValueContext"/></param>
        public override void EnterProductNameKeyValue([NotNull] UserAgentParser.ProductNameKeyValueContext context)
        {
            Inform(context, "name.(1)keyvalue", context.GetText(), false);
            InformSubstrings(context, NAME, true);
        }

        /// <summary>
        /// The EnterProductNameVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameVersionContext"/></param>
        public override void EnterProductNameVersion([NotNull] UserAgentParser.ProductNameVersionContext context)
        {
            InformSubstrings(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameUuid
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameUuidContext"/></param>
        public override void EnterProductNameUuid([NotNull] UserAgentParser.ProductNameUuidContext context)
        {
            Inform(context, NAME);
        }

        /// <summary>
        /// The EnterProductVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionContext"/></param>
        public override void EnterProductVersion([NotNull] UserAgentParser.ProductVersionContext context)
        {
            EnterProductVersion(context);
        }

        /// <summary>
        /// The EnterProductVersionWithCommas
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionWithCommasContext"/></param>
        public override void EnterProductVersionWithCommas([NotNull] UserAgentParser.ProductVersionWithCommasContext context)
        {
            EnterProductVersion(context);
        }

        /// <summary>
        /// The EnterProductVersion
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        private void EnterProductVersion(IParseTree ctx)
        {
            if (ctx.ChildCount != 1)
            {
                // These are the specials with multiple children like keyvalue, etc.
                Inform(ctx, VERSION);
                return;
            }

            IParseTree child = ctx.GetChild(0);
            // Only for the SingleVersion edition we want to have splits of the version.
            if (child is UserAgentParser.SingleVersionContext || child is UserAgentParser.SingleVersionWithCommasContext)
            {
                return;
            }

            Inform(ctx, VERSION);
        }

        /// <summary>
        /// The EnterProductVersionSingleWord
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionSingleWordContext"/></param>
        public override void EnterProductVersionSingleWord([NotNull] UserAgentParser.ProductVersionSingleWordContext context)
        {
            Inform(context, VERSION);
        }

        /// <summary>
        /// The EnterSingleVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.SingleVersionContext"/></param>
        public override void EnterSingleVersion([NotNull] UserAgentParser.SingleVersionContext context)
        {
            InformSubVersions(context, VERSION);
        }

        /// <summary>
        /// The EnterSingleVersionWithCommas
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.SingleVersionWithCommasContext"/></param>
        public override void EnterSingleVersionWithCommas([NotNull] UserAgentParser.SingleVersionWithCommasContext context)
        {
            InformSubVersions(context, VERSION);
        }

        /// <summary>
        /// The EnterProductVersionWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionWordsContext"/></param>
        public override void EnterProductVersionWords([NotNull] UserAgentParser.ProductVersionWordsContext context)
        {
            InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// The EnterKeyValueProductVersionName
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueProductVersionNameContext"/></param>
        public override void EnterKeyValueProductVersionName([NotNull] UserAgentParser.KeyValueProductVersionNameContext context)
        {
            InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// The EnterCommentBlock
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentBlockContext"/></param>
        public override void EnterCommentBlock([NotNull] UserAgentParser.CommentBlockContext context)
        {
            Inform(context, COMMENTS);
        }

        /// <summary>
        /// The EnterCommentEntry
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentEntryContext"/></param>
        public override void EnterCommentEntry([NotNull] UserAgentParser.CommentEntryContext context)
        {
            InformSubstrings(context, "entry");
        }

        /// <summary>
        /// The InformSubstrings
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        private void InformSubstrings(ParserRuleContext ctx, string name)
        {
            InformSubstrings(ctx, name, false);
        }

        /// <summary>
        /// The InformSubstrings
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
        private void InformSubstrings(ParserRuleContext ctx, string name, bool fakeChild)
        {
            InformSubstrings(ctx, name, fakeChild, WordSplitter.GetInstance());
        }

        /// <summary>
        /// The InformSubVersions
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        private void InformSubVersions(ParserRuleContext ctx, string name)
        {
            InformSubstrings(ctx, name, false, VersionSplitter.GetInstance());
        }

        /// <summary>
        /// The InformSubstrings
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
        /// <param name="splitter">The splitter<see cref="Splitter"/></param>
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
        /// <summary>
        /// The EnterMultipleWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.MultipleWordsContext"/></param>
        public override void EnterMultipleWords([NotNull] UserAgentParser.MultipleWordsContext context)
        {
            InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// The EnterKeyValue
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueContext"/></param>
        public override void EnterKeyValue([NotNull] UserAgentParser.KeyValueContext context)
        {
            Inform(context, KEYVALUE);
        }

        /// <summary>
        /// The EnterKeyWithoutValue
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyWithoutValueContext"/></param>
        public override void EnterKeyWithoutValue([NotNull] UserAgentParser.KeyWithoutValueContext context)
        {
            Inform(context, KEYVALUE);
        }

        /// <summary>
        /// The EnterKeyName
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyNameContext"/></param>
        public override void EnterKeyName([NotNull] UserAgentParser.KeyNameContext context)
        {
            InformSubstrings(context, KEY);
        }

        /// <summary>
        /// The EnterKeyValueVersionName
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueVersionNameContext"/></param>
        public override void EnterKeyValueVersionName([NotNull] UserAgentParser.KeyValueVersionNameContext context)
        {
            InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// The EnterVersionWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.VersionWordsContext"/></param>
        public override void EnterVersionWords([NotNull] UserAgentParser.VersionWordsContext context)
        {
            InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// The EnterSiteUrl
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.SiteUrlContext"/></param>
        public override void EnterSiteUrl([NotNull] UserAgentParser.SiteUrlContext context)
        {
            Inform(context, URL, context.url.Text);
        }

        /// <summary>
        /// The EnterUuId
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.UuIdContext"/></param>
        public override void EnterUuId([NotNull] UserAgentParser.UuIdContext context)
        {
            Inform(context, UUID, context.uuid.Text);
        }

        /// <summary>
        /// The EnterEmailAddress
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.EmailAddressContext"/></param>
        public override void EnterEmailAddress([NotNull] UserAgentParser.EmailAddressContext context)
        {
            Inform(context, EMAIL, context.email.Text);
        }

        /// <summary>
        /// The EnterBase64
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.Base64Context"/></param>
        public override void EnterBase64([NotNull] UserAgentParser.Base64Context context)
        {
            Inform(context, BASE64, context.value.Text);
        }

        /// <summary>
        /// The EnterEmptyWord
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.EmptyWordContext"/></param>
        public override void EnterEmptyWord([NotNull] UserAgentParser.EmptyWordContext context)
        {
            Inform(context, TEXT, "");
        }

        /// <summary>
        /// Defines the <see cref="State" />
        /// </summary>
        [Serializable]
        public class State
        {
            /// <summary>
            /// Defines the child
            /// </summary>
            internal long child = 0;

            /// <summary>
            /// Defines the version
            /// </summary>
            internal long version = 0;

            /// <summary>
            /// Defines the comment
            /// </summary>
            internal long comment = 0;

            /// <summary>
            /// Defines the name
            /// </summary>
            internal readonly string name;

            /// <summary>
            /// Defines the path
            /// </summary>
            internal string path;

            /// <summary>
            /// Defines the ctx
            /// </summary>
            internal IParseTree ctx = null;

            /// <summary>
            /// Defines the userAgentTreeFlattener
            /// </summary>
            private UserAgentTreeFlattener userAgentTreeFlattener;

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="userAgentTreeFlattener">The userAgentTreeFlattener<see cref="UserAgentTreeFlattener"/></param>
            /// <param name="name">The name<see cref="string"/></param>
            public State(UserAgentTreeFlattener userAgentTreeFlattener, string name)
            {
                this.userAgentTreeFlattener = userAgentTreeFlattener;
                this.name = name;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="userAgentTreeFlattener">The userAgentTreeFlattener<see cref="UserAgentTreeFlattener"/></param>
            /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
            /// <param name="name">The name<see cref="string"/></param>
            public State(UserAgentTreeFlattener userAgentTreeFlattener, IParseTree ctx, string name) : this(userAgentTreeFlattener, name)
            {
                this.ctx = ctx;
            }

            /// <summary>
            /// The CalculatePath
            /// </summary>
            /// <param name="type">The type<see cref="PathType"/></param>
            /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
            /// <returns>The <see cref="string"/></returns>
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
    }
}
