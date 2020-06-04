using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Extensions
{
    public static class StringExtension
    {
        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            var place = source.LastIndexOf(find);
            if (place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static string ReplaceFirstOccurrence(this string source, string find, string replace)
        {
            var place = source.IndexOf(find);
            if (place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }
    }
}
