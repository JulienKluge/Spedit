using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Spedit.SPCondenser
{
    public static class FunctionsCondenser
    {
        public static void Condense(string source, ref SourcepawnDefinitionCondeser sdc)
        {
            int length = source.Length;
            Regex regex = new Regex(@"\bmethodmap\s+(([a-zA-z_][a-zA-z1-9_]+\s+[a-zA-z_][a-zA-z1-9_]+)|([a-zA-z_][a-zA-z1-9_]+\s*\<\s*[a-zA-z_][a-zA-z1-9_]+))"
                , RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            MatchCollection mc = regex.Matches(source, 0);
            List<TextMarker> methodmapBlocks = new List<TextMarker>();
            for (int i = 0; i < mc.Count; ++i)
            {
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
                                methodmapBlocks.Add(new TextMarker() { Position = startIndex, EndPosition = j, Value = source.Substring(startIndex, j - startIndex + 1) });
                                break;
                            }
                        }
                    }
                }
            }
            regex = new Regex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
            mc = regex.Matches(source, 0);
            List<TextMarker> commentBlocks = new List<TextMarker>();
            for (int i = 0; i < mc.Count; ++i)
            {
                commentBlocks.Add(new TextMarker() { Position = mc[i].Index, EndPosition = mc[i].Index + mc[i].Length, Value = mc[i].Value });
            }
            regex = new Regex(@"(?<fullname>\b(public|stock|native|forward|normal)\s+((public|stock|native|static|forward|normal)\s+){0,2}((([a-zA-z]+\:)|([a-zA-z]+\s+)))?(?<name>[a-zA-Z][a-zA-Z1-9_]+)(\(.*?\))(\s*\=.+?)?(?=(\s*(\;|\{))))"
                , RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
            mc = regex.Matches(source, 0);
            for (int i = 0; i < mc.Count; ++i)
            {
                int foundIndex = mc[i].Index;
                bool isMethod = false;
                for (int j = 0; j < methodmapBlocks.Count; ++j)
                {
                    if (foundIndex >= methodmapBlocks[j].Position)
                    {
                        if (foundIndex <= methodmapBlocks[j].EndPosition)
                        {
                            isMethod = true;
                            break;
                        }
                    }
                }
                if (isMethod)
                {
                    sdc._MethodNames.Add(mc[i].Groups["name"].Value);
                }
                else
                {
                    string commentString = string.Empty;
                    int possibleCommentIndex = Math.Max(foundIndex - 3, 0);
                    for (int j = 0; j < commentBlocks.Count; ++j)
                    {
                        int testPosition = commentBlocks[j].EndPosition;
                        if ((possibleCommentIndex <= testPosition) && (foundIndex >= testPosition))
                        {
                            commentString = PreParseCommentString(commentBlocks[j].Value);
                            break;
                        }
                    }
                    string FuncName = mc[i].Groups["name"].Value;
                    sdc._FunctionNames.Add(FuncName);
                    sdc._Functions.Add(new SPFunction() { Name = FuncName, FullName = PreParseFullFunctionName(mc[i].Groups["fullname"].Value), Comment = commentString });
                }
            }
        }

        private class TextMarker
        {
            public int Position = 0;
            public int EndPosition = 0;
            public string Value = string.Empty;
        }

        private static string PreParseCommentString(string comment)
        {
            string[] lines = comment.Split('\n');
            StringBuilder outString = new StringBuilder();
            int length = lines.Length;
            char[] seperator = new char[] { ' ', '\r', '\t', '*', '/' };
            //bool LastLineWasNullLine = false;
            for (int i = 0; i < length; ++i)
            {
                string newLine = lines[i].Trim(seperator);
                if (string.IsNullOrWhiteSpace(newLine))
                {
                    continue;
                }
                outString.AppendLine(newLine);
            }
            return (outString.ToString()).Trim(new char[] { '\n', '\r' });
        }

        private static string PreParseFullFunctionName(string name)
        {
            string[] lines = name.Split(',');
            StringBuilder outString = new StringBuilder();
            int length = lines.Length;
            char[] seperator = new char[] { ' ', '\n', '\r', '\t', '*', '/' };
            for (int i = 0; i < length; ++i)
            {
                string newLine = lines[i].Trim(seperator);
                if (string.IsNullOrWhiteSpace(newLine))
                {
                    continue;
                }
                if (i != 0)
                {
                    outString.Append(", ");
                }
                outString.Append(newLine);
            }
            return outString.ToString();
        }
    }
}
