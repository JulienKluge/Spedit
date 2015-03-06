using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spedit.SPCondenser
{
    public static class EnumCondenser
    {
        public static void Condense(string source, ref SourcepawnDefinitionCondeser sdc)
        {
            int length = source.Length;
            Regex regex = new Regex(@"\benum(\s+(?<name>[a-zA-Z_][a-zA-Z_1-9]+\:?))?(\s*//.+)?\s*\{"
                , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //^[ \f\t\v]*(?<name>[a-zA-Z_][a-zA-Z1-9_]+)(\s*=\s*[a-zA-Z0-9-\+\s\"]+)?,
            Regex inEnumRegex = new Regex(@"^[ \f\t\v]*(?<name>[a-zA-Z_][a-zA-Z1-9_]+)"
                , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                string matchedName = mc[i].Groups["name"].Value;
                if (!string.IsNullOrWhiteSpace(matchedName))
                {
                    sdc._Types.Add(matchedName);
                }
                bool canCountDown = false;
                int scopeLevel = 0;
                int startIndex = (mc[i].Index + mc[i].Length) - 1;
                for (int j = startIndex; j < length; ++j)
                {
                    if (source[j] == '{')
                    {
                        canCountDown = true;
                        scopeLevel++;
                    }
                    else if (canCountDown)
                    {
                        if (source[j] == '}')
                        {
                            scopeLevel--;
                            if (scopeLevel == 0)
                            {
                                string inEnumString = source.Substring(startIndex, (j - startIndex) + 1);
                                MatchCollection inEnumMC = inEnumRegex.Matches(inEnumString);
                                for (int k = 0; k < inEnumMC.Count; ++k)
                                {
                                    /*if (inEnumMC[k].Groups["name"].Value == "enum")
                                    {
                                        int nul = 0;
                                    }*/
                                    sdc._Constants.Add(inEnumMC[k].Groups["name"].Value);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            regex = new Regex(@"\bstruct\s+(?<name>[a-zA-Z_][a-zA-Z_1-9]+)"
                , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                sdc._Types.Add(mc[i].Groups["name"].Value);
            }
            regex = new Regex(@"\bproperty(\s+[a-zA-Z_][a-zA-Z_1-9]+)?(\s+(?<name>[a-zA-Z_][a-zA-Z_1-9]+))\s*\{"
                , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                sdc._Properties.Add(mc[i].Groups["name"].Value);
            }
            regex = new Regex(@"\b(functag|funcenum|typeset|typedef)(\s+(?<name>[a-zA-Z_][a-zA-Z_1-9]+))"
                            , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                sdc._Types.Add(mc[i].Groups["name"].Value);
            }
        }
    }
}
