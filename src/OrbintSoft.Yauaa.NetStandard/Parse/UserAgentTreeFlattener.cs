//-----------------------------------------------------------------------
// <copyright file="UserAgentTreeFlattener.cs" company="OrbintSoft">
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
// <date>2018, 11, 24, 12:49</date>
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
    /// Implements <see cref="UserAgentBaseListener"/>, used to flatten the parsing tree.
    /// </summary>
    [Serializable]
    public class UserAgentTreeFlattener : UserAgentBaseListener
    {
        /// <summary>
        /// Name of the agent node.
        /// </summary>
        internal const string AGENT = "agent";

        /// <summary>
        /// Name of the base64 node.
        /// </summary>
        internal const string BASE64 = "base64";

        /// <summary>
        /// Name of the comments node.
        /// </summary>
        internal const string COMMENTS = "comments";

        /// <summary>
        /// Name of the email node.
        /// </summary>
        internal const string EMAIL = "email";

        /// <summary>
        /// Name of the entry node.
        /// </summary>
        internal const string ENTRY = "entry";

        /// <summary>
        /// Name of the key node.
        /// </summary>
        internal const string KEY = "key";

        /// <summary>
        /// Name of the keyvalue node.
        /// </summary>
        internal const string KEYVALUE = "keyvalue";

        /// <summary>
        /// Name of the name node.
        /// </summary>
        internal const string NAME = "name";

        /// <summary>
        /// Name of the product node.
        /// </summary>
        internal const string PRODUCT = "product";

        /// <summary>
        /// Name of the text node.
        /// </summary>
        internal const string TEXT = "text";

        /// <summary>
        /// Name of the url node.
        /// </summary>
        internal const string URL = "url";

        /// <summary>
        /// Name of the uuid node.
        /// </summary>
        internal const string UUID = "uuid";

        /// <summary>
        /// Name of the value node.
        /// </summary>
        internal const string VALUE = "value";

        /// <summary>
        /// Name of the version node.
        /// </summary>
        internal const string VERSION = "version";

        /// <summary>
        /// Defines the tree walker.
        /// </summary>
        private static readonly ParseTreeWalker Walker = new ParseTreeWalker();

        /// <summary>
        /// Defines the analyzer.
        /// </summary>
        private readonly IAnalyzer analyzer;

        /// <summary>
        /// Defines the state of the parse tree property.
        /// </summary>
        [NonSerialized]
        private ParseTreeProperty<State> state = null;

        /// <summary>
        /// Defines whether verbose logging is enabled.
        /// </summary>
        private bool verbose = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentTreeFlattener"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer<see cref="IAnalyzer"/>.</param>
        public UserAgentTreeFlattener(IAnalyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        /// <summary>
        /// Defines the path type.
        /// </summary>
        public enum PathType
        {
            /// <summary>
            /// Child node
            /// </summary>
            CHILD,

            /// <summary>
            /// Comment
            /// </summary>
            COMMENT,

            /// <summary>
            /// Version
            /// </summary>
            VERSION,
        }

        /// <summary>
        /// When a base64 node is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterBase64([NotNull] UserAgentParser.Base64Context context)
        {
            this.Inform(context, BASE64, context.value.Text);
        }

        /// <summary>
        /// When a comment node is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterCommentBlock([NotNull] UserAgentParser.CommentBlockContext context)
        {
            this.Inform(context, COMMENTS);
        }

        /// <summary>
        /// When a comment entry is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterCommentEntry([NotNull] UserAgentParser.CommentEntryContext context)
        {
            this.InformSubstrings(context, "entry");
        }

        /// <summary>
        /// When a comment product is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterCommentProduct([NotNull] UserAgentParser.CommentProductContext context)
        {
            this.InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// When an e-mail is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterEmailAddress([NotNull] UserAgentParser.EmailAddressContext context)
        {
            this.Inform(context, EMAIL, context.email.Text);
        }

        /// <summary>
        /// When an empty word is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterEmptyWord([NotNull] UserAgentParser.EmptyWordContext context)
        {
            this.Inform(context, TEXT, string.Empty);
        }

        /// <summary>
        /// When a key name is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterKeyName([NotNull] UserAgentParser.KeyNameContext context)
        {
            this.InformSubstrings(context, KEY);
        }

        /// <summary>
        /// When a key value is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterKeyValue([NotNull] UserAgentParser.KeyValueContext context)
        {
            this.Inform(context, KEYVALUE);
        }

        /// <summary>
        /// When a product version name is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterKeyValueProductVersionName([NotNull] UserAgentParser.KeyValueProductVersionNameContext context)
        {
            this.InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// When a key value version name is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterKeyValueVersionName([NotNull] UserAgentParser.KeyValueVersionNameContext context)
        {
            this.InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// When a key without value is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterKeyWithoutValue([NotNull] UserAgentParser.KeyWithoutValueContext context)
        {
            this.Inform(context, KEYVALUE);
        }

        /// <summary>
        /// When multiple words are found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterMultipleWords([NotNull] UserAgentParser.MultipleWordsContext context)
        {
            this.InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// When a product is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProduct([NotNull] UserAgentParser.ProductContext context)
        {
            this.InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// When a product name email is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameEmail([NotNull] UserAgentParser.ProductNameEmailContext context)
        {
            this.Inform(context, NAME);
        }

        /// <summary>
        /// When a product name key value is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameKeyValue([NotNull] UserAgentParser.ProductNameKeyValueContext context)
        {
            this.Inform(context, "name.(1)keyvalue", context.GetText(), false);
            this.InformSubstrings(context, NAME, true);
        }

        /// <summary>
        /// When a product name without version is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameNoVersion([NotNull] UserAgentParser.ProductNameNoVersionContext context)
        {
            this.InformSubstrings(context, PRODUCT);
        }

        /// <summary>
        /// When a product name url is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameUrl([NotNull] UserAgentParser.ProductNameUrlContext context)
        {
            this.Inform(context, NAME);
        }

        /// <summary>
        /// When a product name uuid is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameUuid([NotNull] UserAgentParser.ProductNameUuidContext context)
        {
            this.Inform(context, NAME);
        }

        /// <summary>
        /// When a product name version is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameVersion([NotNull] UserAgentParser.ProductNameVersionContext context)
        {
            this.InformSubstrings(context, NAME);
        }

        /// <summary>
        /// When product name words are found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductNameWords([NotNull] UserAgentParser.ProductNameWordsContext context)
        {
            this.InformSubstrings(context, NAME);
        }

        /// <summary>
        /// When a product version is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductVersion([NotNull] UserAgentParser.ProductVersionContext context)
        {
            this.EnterProductVersion(context);
        }

        /// <summary>
        /// When a product name version with single word is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductVersionSingleWord([NotNull] UserAgentParser.ProductVersionSingleWordContext context)
        {
            this.Inform(context, VERSION);
        }

        /// <summary>
        /// When a product version with commas is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductVersionWithCommas([NotNull] UserAgentParser.ProductVersionWithCommasContext context)
        {
            this.EnterProductVersion(context);
        }

        /// <summary>
        /// When a product name version with multiple words is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterProductVersionWords([NotNull] UserAgentParser.ProductVersionWordsContext context)
        {
            this.InformSubstrings(context, VERSION);
        }

        /// <summary>
        /// When a root text is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterRootText([NotNull] UserAgentParser.RootTextContext context)
        {
            this.InformSubstrings(context, TEXT);
        }

        /// <summary>
        /// When a single version is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterSingleVersion([NotNull] UserAgentParser.SingleVersionContext context)
        {
            this.InformSubVersions(context, VERSION);
        }

        /// <summary>
        /// When a single version with commas is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterSingleVersionWithCommas([NotNull] UserAgentParser.SingleVersionWithCommasContext context)
        {
            this.InformSubVersions(context, VERSION);
        }

        /// <summary>
        /// When a site url is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterSiteUrl([NotNull] UserAgentParser.SiteUrlContext context)
        {
            this.Inform(context, URL, context.url.Text);
        }

        /// <summary>
        /// When a user agent is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterUserAgent([NotNull] UserAgentParser.UserAgentContext context)
        {
            // In case of a parse error the 'parsed' version of agent can be incomplete
            this.Inform(context, AGENT, context.Start.TokenSource.InputStream.ToString());
        }

        /// <summary>
        /// When a uuid is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterUuId([NotNull] UserAgentParser.UuIdContext context)
        {
            this.Inform(context, UUID, context.uuid.Text);
        }

        /// <summary>
        /// When aversion words is found.
        /// </summary>
        /// <param name="context">The context node.</param>
        public override void EnterVersionWords([NotNull] UserAgentParser.VersionWordsContext context)
        {
            this.InformSubstrings(context, TEXT);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"UserAgentTreeFlattener{{verbose={this.verbose}}} ";
        }

        /// <summary>
        /// Parse the user agen string.
        /// </summary>
        /// <param name="userAgentString">The user agent string.</param>
        /// <returns>The parsed <see cref="UserAgent"/>.</returns>
        public UserAgent Parse(string userAgentString)
        {
            var userAgent = new UserAgent(userAgentString);
            return this.ParseIntoCleanUserAgent(userAgent);
        }

        /// <summary>
        /// Parse a <see cref="UserAgent"/>.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/> to be parsed.</param>
        /// <returns>The parsed <see cref="UserAgent"/>.</returns>
        public UserAgent Parse(UserAgent userAgent)
        {
            userAgent.Reset();
            return this.ParseIntoCleanUserAgent(userAgent);
        }

        /// <summary>
        /// Set true to enable verbose logging.
        /// </summary>
        /// <param name="newVerbose">True to enable verbose logging.</param>
        public void SetVerbose(bool newVerbose)
        {
            this.verbose = newVerbose;
        }

        /// <summary>
        /// Handles the product version special cases.
        /// </summary>
        /// <param name="ctx">The context.</param>
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
        /// Inform about a node.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The inform result.</returns>
        private string Inform(IParseTree ctx, string name, string value)
        {
            return this.Inform(ctx, ctx, name, value, false);
        }

        /// <summary>
        /// Inform about a node.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="fakeChild">True if it's a fake child.</param>
        /// <returns>The inform result.</returns>
        private string Inform(IParseTree ctx, string name, string value, bool fakeChild)
        {
            return this.Inform(ctx, ctx, name, value, fakeChild);
        }

        /// <summary>
        /// Inform about a node.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="path">The name.</param>
        /// <returns>The inform result.</returns>
        private string Inform(IParseTree ctx, string path)
        {
            return this.Inform(ctx, path, AntlrUtils.GetSourceText((ParserRuleContext)ctx));
        }

        /// <summary>
        /// Inform about a node.
        /// </summary>
        /// <param name="stateCtx">The context state.</param>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="fakeChild">True if it's a fake child.</param>
        /// <returns>The inform result.</returns>
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
        /// Inform about substrings.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        private void InformSubstrings(ParserRuleContext ctx, string name)
        {
            this.InformSubstrings(ctx, name, false);
        }

        /// <summary>
        /// Inform about substrings.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        /// <param name="fakeChild">True if it's a fake child.</param>
        private void InformSubstrings(ParserRuleContext ctx, string name, bool fakeChild)
        {
            this.InformSubstrings(ctx, name, fakeChild, WordSplitter.GetInstance());
        }

        /// <summary>
        /// Inform about substrings.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        /// <param name="fakeChild">True if it's a fake child.</param>
        /// <param name="splitter">The <see cref="Splitter"/>.</param>
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
        /// Inform about subversions.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="name">The name.</param>
        private void InformSubVersions(ParserRuleContext ctx, string name)
        {
            this.InformSubstrings(ctx, name, false, VersionSplitter.Instance);
        }

        /// <summary>
        /// Parse the <see cref="UserAgent"/> into a clean one.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/> to parse.</param>
        /// <returns>The claened <see cref="UserAgent"/>.</returns>
        private UserAgent ParseIntoCleanUserAgent(UserAgent userAgent)
        {
            if (userAgent.UserAgentString == null)
            {
                userAgent.Set(DefaultUserAgentFields.SYNTAX_ERROR, "true", 1);
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
                this.Inform(null, DefaultUserAgentFields.SYNTAX_ERROR, "true");
            }
            else
            {
                this.Inform(null, DefaultUserAgentFields.SYNTAX_ERROR, "false");
            }

            Walker.Walk(this, userAgentContext);
            return userAgent;
        }

        /// <summary>
        /// Parse the <see cref="UserAgent"/>.
        /// </summary>
        /// <param name="userAgent">The <see cref="UserAgent"/> to parse.</param>
        /// <returns>The parsing context.</returns>
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
        /// Defines the state of the node.
        /// </summary>
        [Serializable]
        public class State
        {
            /// <summary>
            /// Defines the context.
            /// </summary>
            private readonly IParseTree ctx = null;

            /// <summary>
            /// Defines the name.
            /// </summary>
            private readonly string name;

            /// <summary>
            /// Defines the user agent tree flattener.
            /// </summary>
            private readonly UserAgentTreeFlattener userAgentTreeFlattener;

            /// <summary>
            /// Defines the child count.
            /// </summary>
            private long child = 0;

            /// <summary>
            /// Defines the comments count.
            /// </summary>
            private long comment = 0;

            /// <summary>
            /// Defines the path.
            /// </summary>
            private string path;

            /// <summary>
            /// Defines the version count.
            /// </summary>
            private long version = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="userAgentTreeFlattener">The <see cref="UserAgentTreeFlattener"/>.</param>
            /// <param name="ctx">The context.</param>
            /// <param name="name">The name of the node.</param>
            public State(UserAgentTreeFlattener userAgentTreeFlattener, IParseTree ctx, string name)
                : this(userAgentTreeFlattener, name)
            {
                this.ctx = ctx;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="userAgentTreeFlattener">The <see cref="UserAgentTreeFlattener"/>.</param>
            /// <param name="name">The name of the node.</param>
            public State(UserAgentTreeFlattener userAgentTreeFlattener, string name)
            {
                this.userAgentTreeFlattener = userAgentTreeFlattener;
                this.name = name;
            }

            /// <summary>
            /// Calculate the path of the current node based on the type.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="fakeChild">The fake child.</param>
            /// <returns>The resulting path.</returns>
            public string CalculatePath(PathType type, bool fakeChild)
            {
                var node = this.ctx;
                this.path = this.name;
                if (node is null)
                {
                    return this.path;
                }

                State parentState = null;

                while (parentState == null)
                {
                    node = node.Parent;
                    if (node is null)
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
