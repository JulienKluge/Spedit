using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spedit.SPCondenser
{
    public static class ConstantsCondenser
    {
        public static void Condense(string source, ref SourcepawnDefinitionCondeser sdc)
        {
            //defines
            Regex regex = new Regex(@"^[ \f\t\v]*\#define\s+(?<name>[a-zA-Z_][a-zA-Z1-9_]+)"
                , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                sdc._Constants.Add(mc[i].Groups["name"].Value);
            }
            //constants and dynamic variables
            regex = new Regex(@"\b(public|const)(\s+)(([a-zA-Z]+\s+)|([a-zA-Z]+:))?(?<name>[a-zA-Z_][a-zA-Z1-9_]+)(\[[a-zA-Z0-9_]+\])?;" //(\s*=\s*[a-zA-Z0-9_()<>\s]+)?
                            , RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                sdc._Constants.Add(mc[i].Groups["name"].Value);
            }

        }
    }
}
