using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DarkRP_DiscordBot.Lua
{
    public static class Parser
    {
        public static MatchCollection GetLuaEntries(string text)
        {
            return Regex.Matches(text, @".*?((""|')(.*?)(""|').*?{.*?\s(.*?)\n})", RegexOptions.Singleline);
        }

        public static MatchCollection GetEntriesKeysAndValues(string text)
        {
            return Regex.Matches(text, @".*?(\w+) = (.*?.+?(?=\n\s+\w+ = )|.*?$)", RegexOptions.Multiline | RegexOptions.Singleline);
        }

        public static string GetQuotedString(string text)
        {
            var commandMatch = Regex.Match(text, @"(""|')(.*?)(""|')", RegexOptions.Singleline);

            return commandMatch.Groups[2].Value;
        }

        public static int GetIntFromValue(string text)
        {
            if (text.Contains(','))
            {
                var intMatch = Regex.Match(text, @"(\d+)(,.*|),");
                text = intMatch.Groups[1].Value;
            }

            return int.Parse(text);
        }
    }
}
