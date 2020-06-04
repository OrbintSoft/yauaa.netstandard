using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrbintSoft.Yauaa.WebSample.AspNetCore.Exceptions;
using OrbintSoft.Yauaa.WebSample.AspNetCore.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.TagHelpers
{
    [OutputElementHint("link")]
    [HtmlTargetElement("link-style")]
    public class LinkStyleTagHelper : LinkTagHelper
    {
        public LinkStyleTagHelper(IWebHostEnvironment hostingEnvironment, TagHelperMemoryCacheProvider cacheProvider, IFileVersionProvider fileVersionProvider, HtmlEncoder htmlEncoder, JavaScriptEncoder javaScriptEncoder, IUrlHelperFactory urlHelperFactory) : base(hostingEnvironment, cacheProvider, fileVersionProvider, htmlEncoder, javaScriptEncoder, urlHelperFactory)
        {

        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagName = "link";
            output.Attributes.SetAttribute("rel", "stylesheet");
            output.Attributes.TryGetAttribute("href", out var hrefAttribute);
            if (hrefAttribute != null)
            {
                var value = hrefAttribute.Value;
                if (value != null && value is string)
                {
                    if (!Debugger.IsAttached)
                    {
                        var strValue = (string)value;
                        output.Attributes.SetAttribute("href", strValue.ReplaceLastOccurrence(".css", ".min.css"));
                    }
                }
                else
                {
                    new InvalidHtmlAttributeException("href value is not set");
                }
            }
            else
            {
                throw new InvalidHtmlAttributeException("href Attribute is not set");
            }

        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            return base.ProcessAsync(context, output);
        }
    }
}
