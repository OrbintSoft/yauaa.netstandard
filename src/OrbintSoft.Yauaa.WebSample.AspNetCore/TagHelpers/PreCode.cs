using Microsoft.AspNetCore.Razor.TagHelpers;
using OrbintSoft.Yauaa.WebSample.AspNetCore.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.TagHelpers
{
    [OutputElementHint("code")]
    [HtmlTargetElement("pre-code")]
    public class PreCode : TagHelper
    {
        private readonly Regex spaceatBegin = new Regex(@"^[\t ]+", RegexOptions.Compiled);

        public PreCode() : base()
        {
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "pre";
            output.TagMode = TagMode.StartTagAndEndTag;
            var uglycode = (await output.GetChildContentAsync().ConfigureAwait(false)).GetContent();
            var stringBuilder = new StringBuilder();
            var emptyStrings = new StringBuilder();
            using (var reader = new StringReader(uglycode))
            {
                string line = null;
                string toBeReplaced = null;
                //Skip empty at beginning
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    else
                    {
                        var match = spaceatBegin.Match(line);
                        if (match.Success)
                        {
                            toBeReplaced = match.Value;
                        }
                        break;
                    }
                }

                //Create content
                if (line != null)
                {
                    do
                    {
                        if (toBeReplaced != null)
                        {
                            line = line.ReplaceFirstOccurrence(toBeReplaced, string.Empty);
                        }
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            emptyStrings.AppendLine(line);
                        }
                        else
                        {
                            stringBuilder.Append(emptyStrings);
                            stringBuilder.AppendLine(line);
                            emptyStrings.Clear();
                        }
                    } while ((line = reader.ReadLine()) != null);
                }
            }
            var attributes = string.Join(' ', context.AllAttributes.Select(a => $"{a.Name}=\"{a.Value}\"").ToArray());
            output.Content.SetHtmlContent($"<code {attributes}>{stringBuilder}</code>");
        }
    }
}
