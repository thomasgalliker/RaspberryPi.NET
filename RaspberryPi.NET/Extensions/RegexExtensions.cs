using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaspberryPi.Extensions
{
    internal static class RegexExtensions
    {
        internal static Regex GetKeyValueRegex(string diveder)
        {
            return new Regex(@$"(?<PropertyName>^.*?)(?={diveder})|(?<PropertyValue>[^{diveder}\n\r].*$)");
        }

        internal static bool ParseValueYesNo(IEnumerable<Match> matches, string propertyName)
        {
            return ParseValue(matches, propertyName, value => value == "yes" ? true : false);
        }

        internal static string ParseValue(IEnumerable<Match> matches, string propertyName)
        {
            return ParseValue(matches, propertyName, value => value);
        }

        internal static T ParseValue<T>(IEnumerable<Match> matches, string propertyName, Func<string, T> convert)
        {
            var group = matches.Where(m => m.Groups["PropertyName"].Value == propertyName).Select(m => m.Groups).SingleOrDefault();
            if (group == null)
            {
                return convert(null);
            }

            var value = group["PropertyValue"].Value.Trim();
            return convert(value);
        }

        //internal static bool TryParseValue(Match match, string propertyName, out string value)
        //{
        //    var group = match.Groups["PropertyName"];
        //    if (group.Value != propertyName)
        //    {
        //        value = null;
        //        return false;
        //    }

        //    value = match.Groups["PropertyValue"].Value.Trim();
        //    return true;
        //}

        internal static bool TryParseValue(MatchCollection matches, string propertyName, out string value)
        {
            var match = matches.OfType<Match>().Where(m => m.Groups["Key"].Value == propertyName).SingleOrDefault();
            if (match == null)
            {
                value = null;
                return false;
            }

            value = match.Groups["Value"].Value.Trim();
            return true;
        }
    }
}
