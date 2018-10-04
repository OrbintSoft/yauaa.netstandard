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
using OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Antlr4Source;
using System.Collections.Generic;
using System.Linq;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.Analyze.TreeWalker.Steps.Walk.StepDowns
{
    public class UserAgentGetChildrenVisitor: UserAgentBaseVisitor<IEnumerator<ParserRuleContext>>
    { 
        private readonly string name;
        private readonly ChildIterable childIterable;

        public UserAgentGetChildrenVisitor(string name, int start, int end)
        {
            this.name = name;
            switch (name)
            {
                case "keyvalue":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.KeyValueContext ||
                        clazz is UserAgentParser.KeyWithoutValueContext ||
                        clazz is UserAgentParser.ProductNameKeyValueContext));
                    break;

                case "product":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.ProductContext ||
                        clazz is UserAgentParser.CommentProductContext ||
                        clazz is UserAgentParser.ProductNameNoVersionContext));
                    break;

                case "uuid":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.UuIdContext ||
                        clazz is UserAgentParser.ProductNameUuidContext));
                    break;

                case "base64":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.Base64Context));
                    break;

                case "url":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.SiteUrlContext ||
                        clazz is UserAgentParser.ProductNameUrlContext));
                    break;

                case "email":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.EmailAddressContext ||
                        clazz is UserAgentParser.ProductNameEmailContext));
                    break;

                case "text":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.MultipleWordsContext ||
                        clazz is UserAgentParser.VersionWordsContext ||
                        clazz is UserAgentParser.EmptyWordContext ||
                        clazz is UserAgentParser.RootTextContext ||
                        clazz is UserAgentParser.KeyValueVersionNameContext));
                    break;

                case "name":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.ProductNameContext));
                    break;

                case "version":
                    childIterable = new ChildIterable(true, start, end, clazz => (
                        clazz is UserAgentParser.ProductVersionContext ||
                        clazz is UserAgentParser.ProductVersionWithCommasContext ||
                        clazz is UserAgentParser.ProductVersionWordsContext ||
                        clazz is UserAgentParser.ProductVersionSingleWordContext));
                    break;

                case "comments":
                    childIterable = new ChildIterable(true, start, end, clazz => (
                        clazz is UserAgentParser.CommentBlockContext));
                    break;

                case "key":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.KeyNameContext));
                    break;

                case "value":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.UuIdContext ||
                        clazz is UserAgentParser.MultipleWordsContext ||
                        clazz is UserAgentParser.SiteUrlContext ||
                        clazz is UserAgentParser.EmailAddressContext ||
                        clazz is UserAgentParser.KeyValueVersionNameContext ||
                        clazz is UserAgentParser.KeyValueProductVersionNameContext));
                    break;

                case "entry":
                    childIterable = new ChildIterable(false, start, end, clazz => (
                        clazz is UserAgentParser.CommentEntryContext));
                    break;

                default:
                    childIterable = new ChildIterable(false, start, end, clazz => (false));
                    break;
            }
        }

        private static readonly IEnumerator<ParserRuleContext> EMPTY = null;

        protected override IEnumerator<ParserRuleContext> DefaultResult
        {
            get
            {
                return EMPTY;
            }            
        }

        IEnumerator<ParserRuleContext> GetChildrenByName(ParserRuleContext ctx)
        {
            return childIterable.Iterator(ctx);
        }

        public override IEnumerator<ParserRuleContext> VisitUserAgent([NotNull] UserAgentParser.UserAgentContext context)
        {
            return base.VisitUserAgent(context);
        }

        public override IEnumerator<ParserRuleContext> VisitRootElements([NotNull] UserAgentParser.RootElementsContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitProduct([NotNull] UserAgentParser.ProductContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitProductNameNoVersion([NotNull] UserAgentParser.ProductNameNoVersionContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitCommentProduct([NotNull] UserAgentParser.CommentProductContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitProductName([NotNull] UserAgentParser.ProductNameContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitProductNameKeyValue([NotNull] UserAgentParser.ProductNameKeyValueContext context)
        {
            switch (name)
            {
                case "key":
                    var list = new List<ParserRuleContext>() { context.key };
                    return list.GetEnumerator();
                case "value":
                    List<ParserRuleContext > children = context.multipleWords().Select(s => s as ParserRuleContext).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.keyValueProductVersionName().Select(s => s as ParserRuleContext).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.siteUrl().Select(s => s as ParserRuleContext).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.emailAddress().Select(s => s as ParserRuleContext).ToList();
                    if (children.Count != 0)
                    {
                        return children.GetEnumerator();
                    }

                    children = context.uuId().Select(s => s as ParserRuleContext).ToList();
                    return children.GetEnumerator();
                default:
                    return GetChildrenByName(context);
            }
        }

        public override IEnumerator<ParserRuleContext> VisitProductVersion([NotNull] UserAgentParser.ProductVersionContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitProductVersionWithCommas([NotNull] UserAgentParser.ProductVersionWithCommasContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitKeyValue([NotNull] UserAgentParser.KeyValueContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitKeyWithoutValue([NotNull] UserAgentParser.KeyWithoutValueContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitCommentBlock([NotNull] UserAgentParser.CommentBlockContext context)
        {
            return GetChildrenByName(context);
        }

        public override IEnumerator<ParserRuleContext> VisitCommentEntry([NotNull] UserAgentParser.CommentEntryContext context)
        {
            return GetChildrenByName(context);
        }
    }
}
