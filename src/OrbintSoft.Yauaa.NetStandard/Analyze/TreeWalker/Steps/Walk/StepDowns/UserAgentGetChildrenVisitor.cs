//-----------------------------------------------------------------------
// <copyright file="UserAgentGetChildrenVisitor.cs" company="OrbintSoft">
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

namespace OrbintSoft.Yauaa.Analyze.TreeWalker.Steps.Walk.StepDowns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using OrbintSoft.Yauaa.Antlr4Source;
    using OrbintSoft.Yauaa.Parse;

    /// <summary>
    /// This class is used to visit the children of a <see cref="IEnumerator{IParseTree}"/>.
    /// </summary>
    [Serializable]
    public class UserAgentGetChildrenVisitor : UserAgentBaseVisitor<IEnumerator<IParseTree>>, ISerializable
    {
        /// <summary>
        /// The default enumerator of an empty node.
        /// </summary>
        private static readonly IEnumerator<IParseTree> Empty = null;

        /// <summary>
        /// Defines the start index.
        /// </summary>
        private readonly int start;

        /// <summary>
        /// Defines the end index.
        /// </summary>
        private readonly int end;

        /// <summary>
        /// Defines the name of the visitor.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Defines the child iterable.
        /// </summary>
        private ChildIterable childIterable;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentGetChildrenVisitor"/> class.
        /// It is used for bonary deserialization.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public UserAgentGetChildrenVisitor(SerializationInfo info, StreamingContext context)
        {
            this.name = (string)info.GetValue(nameof(this.name), typeof(string));
            this.start = (int)info.GetValue(nameof(this.start), typeof(int));
            this.end = (int)info.GetValue(nameof(this.end), typeof(int));
            this.Init(this.name, this.start, this.end);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentGetChildrenVisitor"/> class.
        /// </summary>
        /// <param name="name">The name of the visitor.</param>
        /// <param name="start">The start index to iterate.</param>
        /// <param name="end">The end index to iterate.</param>
        public UserAgentGetChildrenVisitor(string name, int start, int end)
        {
            this.name = name;
            this.start = start;
            this.end = end;
            this.Init(name, start, end);
        }

        /// <summary>
        /// Gets the default result.
        /// </summary>
        protected override IEnumerator<IParseTree> DefaultResult
        {
            get
            {
                return Empty;
            }
        }

        /// <summary>
        /// This is used for binary serialization.
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.name), this.name, typeof(string));
            info.AddValue(nameof(this.start), this.start, typeof(int));
            info.AddValue(nameof(this.end), this.end, typeof(int));
        }

        /// <summary>
        /// When visiting a comment block node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentBlockContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitCommentBlock([NotNull] UserAgentParser.CommentBlockContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a comment entry node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentEntryContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitCommentEntry([NotNull] UserAgentParser.CommentEntryContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a comment for a product node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.CommentProductContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitCommentProduct([NotNull] UserAgentParser.CommentProductContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a key with value node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyValueContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitKeyValue([NotNull] UserAgentParser.KeyValueContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a key without a value node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.KeyWithoutValueContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitKeyWithoutValue([NotNull] UserAgentParser.KeyWithoutValueContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a product node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitProduct([NotNull] UserAgentParser.ProductContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a product name node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context<see cref="UserAgentParser.ProductNameContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitProductName([NotNull] UserAgentParser.ProductNameContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a product name with key and value node, in case the current vistor is searching for the key, returns the key.
        /// In case it's a value, tries to find children using different strategies.
        /// </summary>
        /// <param name="context">The context of the current node<see cref="UserAgentParser.ProductNameKeyValueContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitProductNameKeyValue([NotNull] UserAgentParser.ProductNameKeyValueContext context)
        {
            switch (this.name)
            {
                case UserAgentTreeFlattener.KEY:
                    var list = new List<ParserRuleContext>() { context.key };
                    return list.GetEnumerator();
                case UserAgentTreeFlattener.VALUE:
                    var children = context.multipleWords().Select(s => s as IParseTree).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.keyValueProductVersionName().Select(s => s as IParseTree).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.siteUrl().Select(s => s as IParseTree).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.emailAddress().Select(s => s as IParseTree).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.uuId().Select(s => s as IParseTree).ToList();
                    return children.GetEnumerator();
                default:
                    return this.GetChildrenByName(context);
            }
        }

        /// <summary>
        /// When visiting a product name without version node find the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context node <see cref="UserAgentParser.ProductNameNoVersionContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitProductNameNoVersion([NotNull] UserAgentParser.ProductNameNoVersionContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a product name with a version node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context of the current node <see cref="UserAgentParser.ProductVersionContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitProductVersion([NotNull] UserAgentParser.ProductVersionContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting a product name with a version that contains commas node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context of the current node <see cref="UserAgentParser.ProductVersionWithCommasContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitProductVersionWithCommas([NotNull] UserAgentParser.ProductVersionWithCommasContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting root node finds the children by name appling the parsing rule for the context.
        /// </summary>
        /// <param name="context">The context of the current node <see cref="UserAgentParser.RootElementsContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitRootElements([NotNull] UserAgentParser.RootElementsContext context)
        {
            return this.GetChildrenByName(context);
        }

        /// <summary>
        /// When visiting the user agent node tries to find children skipping root node by name.
        /// Otherwise calls tries to get children using children visitor.
        /// </summary>
        /// <param name="context">The context of the current node <see cref="UserAgentParser.UserAgentContext"/>.</param>
        /// <returns>The found children as <see cref="IEnumerator{IParseTree}"/>.</returns>
        public override IEnumerator<IParseTree> VisitUserAgent([NotNull] UserAgentParser.UserAgentContext context)
        {
            var children = this.GetChildrenByName(context);
            if (!children.MoveNext() && children.Current == null)
            {
                return this.VisitChildren(context);
            }

            return children;
        }

        /// <summary>
        /// Gets the children enumerator of the context node.
        /// </summary>
        /// <param name="ctx">The context of the current node<see cref="ParserRuleContext"/>.</param>
        /// <returns>The <see cref="IEnumerator{IParseTree}"/>.</returns>
        internal IEnumerator<IParseTree> GetChildrenByName(ParserRuleContext ctx)
        {
            return this.childIterable.GetEnumerator(ctx);
        }

        /// <summary>
        /// Initialize the <see cref="UserAgentGetChildrenVisitor"/> by the name and provided range.
        /// </summary>
        /// <param name="name">The name of the visitor based on the kind of node we need to iterate children.</param>
        /// <param name="start">The start range.</param>
        /// <param name="end">>The end range.</param>
        private void Init(string name, int start, int end)
        {
            switch (name)
            {
                case UserAgentTreeFlattener.KEYVALUE:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.KeyValueContext ||
                        clazz is UserAgentParser.KeyWithoutValueContext ||
                        clazz is UserAgentParser.ProductNameKeyValueContext));
                    break;

                case UserAgentTreeFlattener.PRODUCT:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.ProductContext ||
                        clazz is UserAgentParser.CommentProductContext ||
                        clazz is UserAgentParser.ProductNameNoVersionContext));
                    break;

                case UserAgentTreeFlattener.UUID:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.UuIdContext ||
                        clazz is UserAgentParser.ProductNameUuidContext));
                    break;

                case UserAgentTreeFlattener.BASE64:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.Base64Context));
                    break;

                case UserAgentTreeFlattener.URL:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.SiteUrlContext ||
                        clazz is UserAgentParser.ProductNameUrlContext));
                    break;

                case UserAgentTreeFlattener.EMAIL:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.EmailAddressContext ||
                        clazz is UserAgentParser.ProductNameEmailContext));
                    break;

                case UserAgentTreeFlattener.TEXT:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.MultipleWordsContext ||
                        clazz is UserAgentParser.VersionWordsContext ||
                        clazz is UserAgentParser.EmptyWordContext ||
                        clazz is UserAgentParser.RootTextContext ||
                        clazz is UserAgentParser.KeyValueVersionNameContext));
                    break;

                case UserAgentTreeFlattener.NAME:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.ProductNameContext));
                    break;

                case UserAgentTreeFlattener.VERSION:
                    this.childIterable = new ChildIterable(true, start, end, clazz => (
                        clazz is UserAgentParser.ProductVersionContext ||
                        clazz is UserAgentParser.ProductVersionWithCommasContext ||
                        clazz is UserAgentParser.ProductVersionWordsContext ||
                        clazz is UserAgentParser.ProductVersionSingleWordContext));
                    break;

                case UserAgentTreeFlattener.COMMENTS:
                    this.childIterable = new ChildIterable(true, start, end, clazz => (
                        clazz is UserAgentParser.CommentBlockContext));
                    break;

                case UserAgentTreeFlattener.KEY:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.KeyNameContext));
                    break;

                case UserAgentTreeFlattener.VALUE:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.UuIdContext ||
                        clazz is UserAgentParser.MultipleWordsContext ||
                        clazz is UserAgentParser.SiteUrlContext ||
                        clazz is UserAgentParser.EmailAddressContext ||
                        clazz is UserAgentParser.KeyValueVersionNameContext ||
                        clazz is UserAgentParser.KeyValueProductVersionNameContext));
                    break;

                case UserAgentTreeFlattener.ENTRY:
                    this.childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.CommentEntryContext));
                    break;

                default:
                    this.childIterable = new ChildIterable(false, start, end, clazz => false);
                    break;
            }
        }
    }
}
