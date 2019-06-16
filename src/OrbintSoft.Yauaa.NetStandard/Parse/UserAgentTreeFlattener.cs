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
    using System;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Analyze;
    using OrbintSoft.Yauaa.Analyzer;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Utils;

    /// <summary>
    /// Defines the <see cref="UserAgentTreeFlattener" />
    /// </summary>
    [Serializable]
    public class UserAgentTreeFlattener : UserAgentBaseListener
    {
        /// <summary>
        /// Defines the AGENT
        /// </summary>
        private const string AGENT = "agent";

        /// <summary>
        /// Defines the BASE64
        /// </summary>
        private const string BASE64 = "base64";

        /// <summary>
        /// Defines the COMMENTS
        /// </summary>
        private const string COMMENTS = "comments";

        /// <summary>
        /// Defines the EMAIL
        /// </summary>
        private const string EMAIL = "email";

        /// <summary>
        /// Defines the KEY
        /// </summary>
        private const string KEY = "key";

        /// <summary>
        /// Defines the KEYVALUE
        /// </summary>
        private const string KEYVALUE = "keyvalue";

        /// <summary>
        /// Defines the NAME
        /// </summary>
        private const string NAME = "name";

        /// <summary>
        /// Defines the PRODUCT
        /// </summary>
        private const string PRODUCT = "product";

        /// <summary>
        /// Defines the TEXT
        /// </summary>
        private const string TEXT = "text";

        /// <summary>
        /// Defines the URL
        /// </summary>
        private const string URL = "url";

        /// <summary>
        /// Defines the UUID
        /// </summary>
        private const string UUID = "uuid";

        /// <summary>
        /// Defines the VERSION
        /// </summary>
        private const string VERSION = "version";

        /// <summary>
        /// Defines the Walker
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

        /// <summary>
        /// Defines the verbose
        /// </summary>
        private bool verbose = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentTreeFlattener"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/></param>
        public UserAgentTreeFlattener(IAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        /// <summary>
        /// Defines the PathType
        /// </summary>
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
        /// The EnterBase64
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.Base64Context"/></param>
        public override void EnterBase64([NotNull] UserAgentParser.Base64Context context)
        {
            this.Inform(context, BASE64, context.value.Text);
        }

        /// <summary>
        /// The EnterCommentBlock
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentBlockContext"/></param>
        public override void EnterCommentBlock([NotNull] UserAgentParser.CommentBlockContext context)
        {
            this.Inform(context, COMMENTS);
        }

        /// <summary>
        /// The EnterCommentEntry
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentEntryContext"/></param>
        public override void EnterCommentEntry([NotNull] UserAgentParser.CommentEntryContext context)
        {
            this.InformSubstrings(context, "entry");
        }

        /// <summary>
        /// The EnterCommentProduct
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentProductContext"/></param>
        public override void EnterCommentProduct([NotNull] UserAgentParser.CommentProductContext context)
        {
            this.InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// The EnterEmailAddress
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.EmailAddressContext"/></param>
        public override void EnterEmailAddress([NotNull] UserAgentParser.EmailAddressContext context)
        {
            this.Inform(context, EMAIL, context.email.Text);
        }

        /// <summary>
        /// The EnterEmptyWord
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.EmptyWordContext"/></param>
        public override void EnterEmptyWord([NotNull] UserAgentParser.EmptyWordContext context)
        {
            this.Inform(context, TEXT, string.Empty);
        }

        /// <summary>
        /// The EnterKeyName
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyNameContext"/></param>
        public override void EnterKeyName([NotNull] UserAgentParser.KeyNameContext context)
        {
            this.InformSubstrings(context, KEY);
        }

        /// <summary>
        /// The EnterKeyValue
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueContext"/></param>
        public override void EnterKeyValue([NotNull] UserAgentParser.KeyValueContext context)
        {
            this.Inform(context, KEYVALUE);
        }

        /// <summary>
        /// The EnterKeyValueProductVersionName
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueProductVersionNameContext"/></param>
        public override void EnterKeyValueProductVersionName([NotNull] UserAgentParser.KeyValueProductVersionNameContext context)
        {
            this.InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// The EnterKeyValueVersionName
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueVersionNameContext"/></param>
        public override void EnterKeyValueVersionName([NotNull] UserAgentParser.KeyValueVersionNameContext context)
        {
            this.InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// The EnterKeyWithoutValue
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyWithoutValueContext"/></param>
        public override void EnterKeyWithoutValue([NotNull] UserAgentParser.KeyWithoutValueContext context)
        {
            this.Inform(context, KEYVALUE);
        }

        /// <summary>
        /// The EnterMultipleWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.MultipleWordsContext"/></param>
        public override void EnterMultipleWords([NotNull] UserAgentParser.MultipleWordsContext context)
        {
            this.InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// The EnterProduct
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductContext"/></param>
        public override void EnterProduct([NotNull] UserAgentParser.ProductContext context)
        {
            this.InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// The EnterProductNameEmail
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameEmailContext"/></param>
        public override void EnterProductNameEmail([NotNull] UserAgentParser.ProductNameEmailContext context)
        {
            this.Inform(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameKeyValue
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameKeyValueContext"/></param>
        public override void EnterProductNameKeyValue([NotNull] UserAgentParser.ProductNameKeyValueContext context)
        {
            this.Inform(context, "name.(1)keyvalue", context.GetText(), false);
            this.InformSubstrings(context, NAME, true);
        }

        /// <summary>
        /// The EnterProductNameNoVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameNoVersionContext"/></param>
        public override void EnterProductNameNoVersion([NotNull] UserAgentParser.ProductNameNoVersionContext context)
        {
            this.InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// The EnterProductNameUrl
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameUrlContext"/></param>
        public override void EnterProductNameUrl([NotNull] UserAgentParser.ProductNameUrlContext context)
        {
            this.Inform(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameUuid
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameUuidContext"/></param>
        public override void EnterProductNameUuid([NotNull] UserAgentParser.ProductNameUuidContext context)
        {
            this.Inform(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameVersionContext"/></param>
        public override void EnterProductNameVersion([NotNull] UserAgentParser.ProductNameVersionContext context)
        {
            this.InformSubstrings(context, NAME);
        }

        /// <summary>
        /// The EnterProductNameWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameWordsContext"/></param>
        public override void EnterProductNameWords([NotNull] UserAgentParser.ProductNameWordsContext context)
        {
            this.InformSubstrings(context, NAME);
        }

        /// <summary>
        /// The EnterProductVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionContext"/></param>
        public override void EnterProductVersion([NotNull] UserAgentParser.ProductVersionContext context)
        {
            this.EnterProductVersion(context);
        }

        /// <summary>
        /// The EnterProductVersionSingleWord
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionSingleWordContext"/></param>
        public override void EnterProductVersionSingleWord([NotNull] UserAgentParser.ProductVersionSingleWordContext context)
        {
            this.Inform(context, VERSION);
        }

        /// <summary>
        /// The EnterProductVersionWithCommas
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionWithCommasContext"/></param>
        public override void EnterProductVersionWithCommas([NotNull] UserAgentParser.ProductVersionWithCommasContext context)
        {
            this.EnterProductVersion(context);
        }

        /// <summary>
        /// The EnterProductVersionWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductVersionWordsContext"/></param>
        public override void EnterProductVersionWords([NotNull] UserAgentParser.ProductVersionWordsContext context)
        {
            this.InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// The EnterRootText
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.RootTextContext"/></param>
        public override void EnterRootText([NotNull] UserAgentParser.RootTextContext context)
        {
            this.InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// The EnterSingleVersion
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.SingleVersionContext"/></param>
        public override void EnterSingleVersion([NotNull] UserAgentParser.SingleVersionContext context)
        {
            this.InformSubVersions(context, VERSION);
        }

        /// <summary>
        /// The EnterSingleVersionWithCommas
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.SingleVersionWithCommasContext"/></param>
        public override void EnterSingleVersionWithCommas([NotNull] UserAgentParser.SingleVersionWithCommasContext context)
        {
            this.InformSubVersions(context, VERSION);
        }

        /// <summary>
        /// The EnterSiteUrl
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.SiteUrlContext"/></param>
        public override void EnterSiteUrl([NotNull] UserAgentParser.SiteUrlContext context)
        {
            this.Inform(context, URL, context.url.Text);
        }

        /// <summary>
        /// The EnterUserAgent
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.UserAgentContext"/></param>
        public override void EnterUserAgent([NotNull] UserAgentParser.UserAgentContext context)
        {
            // In case of a parse error the 'parsed' version of agent can be incomplete
            this.Inform(context, AGENT, context.Start.TokenSource.InputStream.ToString());
        }

        /// <summary>
        /// The EnterUuId
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.UuIdContext"/></param>
        public override void EnterUuId([NotNull] UserAgentParser.UuIdContext context)
        {
            this.Inform(context, UUID, context.uuid.Text);
        }

        /// <summary>
        /// The EnterVersionWords
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.VersionWordsContext"/></param>
        public override void EnterVersionWords([NotNull] UserAgentParser.VersionWordsContext context)
        {
            this.InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="userAgentString">The userAgentString<see cref="string"/></param>
        /// <returns>The <see cref="UserAgent"/></returns>
        public UserAgent Parse(string userAgentString)
        {
            var userAgent = new UserAgent(userAgentString);
            return this.ParseIntoCleanUserAgent(userAgent);
        }

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="UserAgent"/></returns>
        public UserAgent Parse(UserAgent userAgent)
        {
            userAgent.Reset();
            return this.ParseIntoCleanUserAgent(userAgent);
        }

        /// <summary>
        /// The SetVerbose
        /// </summary>
        /// <param name="newVerbose">The newVerbose<see cref="bool"/></param>
        public void SetVerbose(bool newVerbose)
        {
            this.verbose = newVerbose;
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
                this.Inform(ctx, VERSION);
                return;
            }

            var child = ctx.GetChild(0);

            // Only for the SingleVersion edition we want to have splits of the version.
            if (child is UserAgentParser.SingleVersionContext || child is UserAgentParser.SingleVersionWithCommasContext)
            {
                return;
            }

            this.Inform(ctx, VERSION);
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
            return this.Inform(ctx, ctx, name, value, false);
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
            return this.Inform(ctx, ctx, name, value, fakeChild);
        }

        /// <summary>
        /// The Inform
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
        /// <param name="path">The path<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string Inform(IParseTree ctx, string path)
        {
            return this.Inform(ctx, path, AntlrUtils.GetSourceText((ParserRuleContext)ctx));
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
            var path = name;
            if (stateCtx == null)
            {
                this.analyzer.Inform(path, value, ctx);
            }
            else
            {
                var myState = new State(this, stateCtx, name);
                if (!fakeChild)
                {
                    this.state.Put(stateCtx, myState);
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
                this.analyzer.Inform(path, value, ctx);
            }

            return path;
        }

        /// <summary>
        /// The InformSubstrings
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        private void InformSubstrings(ParserRuleContext ctx, string name)
        {
            this.InformSubstrings(ctx, name, false);
        }

        /// <summary>
        /// The InformSubstrings
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
        private void InformSubstrings(ParserRuleContext ctx, string name, bool fakeChild)
        {
            this.InformSubstrings(ctx, name, fakeChild, WordSplitter.GetInstance());
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
            var text = AntlrUtils.GetSourceText(ctx);
            var path = this.Inform(ctx, name, text, fakeChild);
            var ranges = this.analyzer.GetRequiredInformRanges(path);

            if (ranges.Count > 4)
            { // Benchmarks showed this to be the breakeven point. (see below)
                var splitList = splitter.CreateSplitList(text);
                foreach (var range in ranges)
                {
                    var value = splitter.GetSplitRange(text, splitList, range);
                    if (value != null)
                    {
                        this.Inform(ctx, ctx, name + range, value, true);
                    }
                }
            }
            else
            {
                foreach (var range in ranges)
                {
                    var value = splitter.GetSplitRange(text, range);
                    if (value != null)
                    {
                        this.Inform(ctx, ctx, name + range, value, true);
                    }
                }
            }
        }

        /// <summary>
        /// The InformSubVersions
        /// </summary>
        /// <param name="ctx">The ctx<see cref="ParserRuleContext"/></param>
        /// <param name="name">The name<see cref="string"/></param>
        private void InformSubVersions(ParserRuleContext ctx, string name)
        {
            this.InformSubstrings(ctx, name, false, VersionSplitter.GetInstance());
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
            var userAgentContext = this.ParseUserAgent(userAgent);

            // Walk the tree an inform the calling analyzer about all the nodes found
            this.state = new ParseTreeProperty<State>();

            var rootState = new State(this, AGENT);
            rootState.CalculatePath(PathType.CHILD, false);
            this.state.Put(userAgentContext, rootState);

            if (userAgent.HasSyntaxError)
            {
                this.Inform(null, UserAgent.SYNTAX_ERROR, "true");
            }
            else
            {
                this.Inform(null, UserAgent.SYNTAX_ERROR, "false");
            }

            Walker.Walk(this, userAgentContext);
            return userAgent;
        }

        /// <summary>
        /// The ParseUserAgent
        /// </summary>
        /// <param name="userAgent">The userAgent<see cref="UserAgent"/></param>
        /// <returns>The <see cref="UserAgentParser.UserAgentContext"/></returns>
        private UserAgentParser.UserAgentContext ParseUserAgent(UserAgent userAgent)
        {
            var userAgentString = EvilManualUseragentStringHacks.FixIt(userAgent.UserAgentString);

            var input = new AntlrInputStream(userAgentString);
            var lexer = new UserAgentLexer(input);

            var tokens = new CommonTokenStream(lexer);

            var parser = new UserAgentParser(tokens);

            if (!this.verbose)
            {
                lexer.RemoveErrorListeners();
                parser.RemoveErrorListeners();
            }

            lexer.AddErrorListener(userAgent);
            parser.AddErrorListener(userAgent);

            return parser.userAgent();
        }

        /// <summary>
        /// Defines the <see cref="State" />
        /// </summary>
        [Serializable]
        public class State
        {
            /// <summary>
            /// Defines the ctx
            /// </summary>
            private readonly IParseTree ctx = null;

            /// <summary>
            /// Defines the name
            /// </summary>
            private readonly string name;

            /// <summary>
            /// Defines the userAgentTreeFlattener
            /// </summary>
            private readonly UserAgentTreeFlattener userAgentTreeFlattener;

            /// <summary>
            /// Defines the child
            /// </summary>
            private long child = 0;

            /// <summary>
            /// Defines the comment
            /// </summary>
            private long comment = 0;

            /// <summary>
            /// Defines the path
            /// </summary>
            private string path;

            /// <summary>
            /// Defines the version
            /// </summary>
            private long version = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="userAgentTreeFlattener">The userAgentTreeFlattener<see cref="UserAgentTreeFlattener"/></param>
            /// <param name="ctx">The ctx<see cref="IParseTree"/></param>
            /// <param name="name">The name<see cref="string"/></param>
            public State(UserAgentTreeFlattener userAgentTreeFlattener, IParseTree ctx, string name)
                : this(userAgentTreeFlattener, name)
            {
                this.ctx = ctx;
            }

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
            /// The CalculatePath
            /// </summary>
            /// <param name="type">The type<see cref="PathType"/></param>
            /// <param name="fakeChild">The fakeChild<see cref="bool"/></param>
            /// <returns>The <see cref="string"/></returns>
            public string CalculatePath(PathType type, bool fakeChild)
            {
                var node = this.ctx;
                this.path = this.name;
                if (node == null)
                {
                    return this.path;
                }

                State parentState = null;

                while (parentState == null)
                {
                    node = node.Parent;
                    if (node == null)
                    {
                        return this.path;
                    }

                    parentState = this.userAgentTreeFlattener.state.Get(node);
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

                this.path = parentState.path + ".(" + counter + ')' + this.name;

                return this.path;
            }
        }
    }
}
