using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spedit.Utils.SPSyntaxTidy
{
    public static class SPTokenizer
    {
        public static SPToken[] Tokenize(string source)
        {
            List<SPToken> token = new List<SPToken>();
            char[] buffer = source.ToCharArray();
            int length = buffer.Length;
            bool AllowLTOperator = true;
            bool AllowGTOperator = true;
            for (int i = 0; i < length; ++i)
            {
                char c = buffer[i];
                #region Newline
                if (c == '\n') //just fetch \n. \r will be killed by the whitestrip but it's reintroduced in Environment.NewLine
                {
                    token.Add(new SPToken() { Kind = SPTokenKind.Newline, Value = Environment.NewLine }); //add them before the whitestrip-killer will get them ^^
                    continue;
                }
                #endregion
                #region Whitespace
                if (char.IsWhiteSpace(c)) { continue; } //this are the fuckers we do this here for...
                #endregion
                #region Quotes
                if (c == '"') //sigh...
                {
                    int startIndex = i;
                    bool foundOccurence = false; //these suckers are here because we want to continue the main-for-loop but cannot do it from the for-loop in the nextline
                    for (int j = i + 1; j < length; ++j)
                    {
                        if (buffer[j] == '"')
                        {
                            if (buffer[j - 1] != '\\') //is the quote not escaped?
                            {
                                token.Add(new SPToken() { Kind = SPTokenKind.Quote, Value = source.Substring(startIndex, j - startIndex + 1) });
                                foundOccurence = true;
                                i = j; //skip it in the main loop
                                break;
                            }
                        }
                    }
                    if (foundOccurence)
                    {
                        continue;
                    }
                }
                if (c == '\'') //I sell that as a quote...kill me right?
                {
                    int startIndex = i;
                    bool foundOccurence = false;
                    for (int j = i + 1; j < length; ++j)
                    {
                        if (buffer[j] == '\'')
                        {
                            if (buffer[j - 1] != '\\') //is the quote not escaped?
                            {
                                token.Add(new SPToken() { Kind = SPTokenKind.Quote, Value = source.Substring(startIndex, j - startIndex + 1) });
                                foundOccurence = true;
                                i = j;
                                break;
                            }
                        }
                    }
                    if (foundOccurence)
                    {
                        continue;
                    }
                }
                #endregion
                #region Comments
                if (c == '/') //lets find comments...
                {
                    if ((i + 1) < length) //is a next char even possible? Because both have at least one next char.
                    {
                        if (buffer[i + 1] == '/') //I see you singlelinecomment ^^
                        {
                            int startIndex = i;
                            int endIndex = i; // this is here, because if we reach the end of the document, this is still a comment
                            //so when we fall out of the for-loop without lineending match, we'll just use this as the endoffset.
                            ++i;
                            for (int j = i; j < length; ++j)
                            {
                                if (buffer[j] == '\r' || buffer[j] == '\n') //different line ending specifications...horribly...
                                {
                                    break;
                                }
                                endIndex = j;
                            }
                            i = endIndex;
                            token.Add(new SPToken() { Kind = SPTokenKind.SingleLineComment, Value = source.Substring(startIndex, endIndex - startIndex + 1) });
                            continue;
                        }
                        else if (buffer[i + 3] < length) //this have to be true because of the closing phrase '*/'
                        {
                            if (buffer[i + 1] == '*') //aaaaaand, multilinecomment...
                            {
                                int startIndex = i;
                                ++i;
                                bool foundOccurence = false;
                                for (int j = i; j < length; ++j)
                                {
                                    if (buffer[j] == '/')
                                    {
                                        if (buffer[j - 1] == '*')
                                        {
                                            i = j;
                                            foundOccurence = true;
                                            token.Add(new SPToken() { Kind = SPTokenKind.MultilineComment, Value = source.Substring(startIndex, j - startIndex + 1) });
                                            break;
                                        }
                                    }
                                }
                                if (foundOccurence)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
                #endregion
                #region Names
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_')
                {
                    int startIndex = i;
                    int endindex = i;
                    for (int j = i + 1; j < length; ++j)
                    {
                        c = buffer[j];
                        if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_'))
                        {
                            break;
                        }
                        endindex = j;
                    }
                    i = endindex;
                    string strValue = source.Substring(startIndex, endindex - startIndex + 1);
                    if (strValue == "view_as")
                    {
                        AllowGTOperator = AllowLTOperator = false;
                    }
                    token.Add(new SPToken() { Kind = SPTokenKind.Name, Value = strValue });
                    continue;
                }
                #endregion
                #region Brackets
                if (c == '{')
                {
                    token.Add(new SPToken() { Kind = SPTokenKind.BracketOpen, Value = "{" });
                    continue;
                }
                if (c == '}')
                {
                    token.Add(new SPToken() { Kind = SPTokenKind.BracketClose, Value = "}" });
                    continue;
                }
                #endregion
                #region Operators
                if (c == '=')
                {
                    if ((i + 1) < length)
                    {
                        if (buffer[i + 1] == '=')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "==" });
                            i++;
                            continue;
                        }
                    }
                    token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "=" });
                    continue;
                }
                if (c == '?' || c == '%')
                {
                    token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = c.ToString() });
                    continue;
                }
                if (c == ':')
                {
                    if (i > 0)
                    {
                        if (buffer[i - 1] == ' ' || buffer[i - 1] == '\t')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = ":" });
                            continue;
                        }
                    }
                }
                if (c == '<' || c == '>' || c == '!' || c == '|' || c == '&' || c == '+' || c == '-' || c == '*' || c == '/' || c == '^')
                {
                    if ((i + 1) < length)
                    {
                        if (buffer[i + 1] == '=')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = source.Substring(i, 2) });
                            i++;
                            continue;
                        }
                    }
                    if (c != '!' && c != '|' && c != '&' && c != '+' && c != '-' && c != '<' && c != '>') //they can have another meaning so they are handled on their own
                    {
                        token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = source.Substring(i, 1) });
                        continue;
                    }
                }
                if (c == '|')
                {
                    if ((i + 1) < length)
                    {
                        if (buffer[i + 1] == '|')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "||" });
                            ++i;
                            continue;
                        }
                    }
                    token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "|" });
                    continue;
                }
                if (c == '>')
                {
                    if ((i + 1) < length)
                    {
                        if (buffer[i + 1] == '>')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = ">>" });
                            ++i;
                            continue;
                        }
                    }
                    if (AllowGTOperator)
                    {
                        token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = ">" });
                        continue;
                    }
                    else
                    {
                        AllowGTOperator = true;
                    }
                }
                if (c == '<')
                {
                    if ((i + 1) < length)
                    {
                        if (buffer[i + 1] == '<')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "<<" });
                            ++i;
                            continue;
                        }
                    }
                    if (AllowLTOperator)
                    {
                        token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "<" });
                        continue;
                    }
                    else
                    {
                        AllowLTOperator = true;
                    }
                }
                if (c == '&') //the & operator is a little bit problematic. It can mean bitwise AND or address of variable. This is not easy to determinate
                {
                    bool canMatchSingle = true;
                    if ((i + 1) < length)
                    {
                        if (buffer[i + 1] == '&')
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "&&" });
                            ++i;
                            continue;
                        }
                        //if next to the single & is a function valid char, prepend its the addressof-operator | this can be lead to formatting-errors, but hey, thats not my fault..
                        if (((buffer[i + 1] >= 'a' && buffer[i + 1] <= 'z') || buffer[i + 1] >= 'A' && buffer[i + 1] <= 'Z') || buffer[i + 1] == '_')
                        {
                            canMatchSingle = false;
                        }
                    }
                    if (canMatchSingle)
                    {
                        token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "&" });
                        continue;
                    }
                }
                if (c == '+')
                {
                    bool isMatched = true;
                    if ((i + 1) < length)
                    {
                        isMatched = buffer[i + 1] != '+';
                    }
                    if (isMatched)
                    {
                        if ((i - 1) < length && (i - 1) >= 0)
                        {
                            isMatched = buffer[i - 1] != '+';
                        }
                        if (isMatched)
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "+" });
                            continue;
                        }
                    }
                }
                if (c == '-')
                {
                    bool isMatched = true;
                    if ((i + 1) < length)
                    {
                        isMatched = buffer[i + 1] != '-';
                    }
                    if (isMatched)
                    {
                        if ((i - 1) < length && (i - 1) >= 0)
                        {
                            isMatched = buffer[i - 1] != '-';
                        }
                        if (isMatched)
                        {
                            token.Add(new SPToken() { Kind = SPTokenKind.Operator, Value = "-" });
                            continue;
                        }
                    }
                }
                #endregion
                #region PreProcessorLine
                if (c == '#') //lets just overtake Lines of Preprocessing-directives
                {
                    int startIndex = i;
                    int endIndex = i;
                    for (int j = i + 1; j < length; ++j)
                    {
                        if (buffer[j] == '\r' || buffer[j] == '\n')
                        {
                            break;
                        }
                        endIndex = j;
                    }
                    i = endIndex;
                    token.Add(new SPToken() { Kind = SPTokenKind.PreProcessorLine, Value = source.Substring(startIndex, endIndex - startIndex + 1) });
                    continue;
                }
                #endregion
                #region Symbols
                if (c == ',')
                {
                    token.Add(new SPToken() { Kind = SPTokenKind.Comma, Value = "," });
                    continue;
                }
                if (c == ';')
                {
                    token.Add(new SPToken() { Kind = SPTokenKind.Semicolon, Value = ";" });
                    continue;
                }
                token.Add(new SPToken() { Kind = SPTokenKind.Symbol, Value = c.ToString() });
                #endregion
            }
            return token.ToArray();
        }
    }

    public class SPToken
    {
        public SPTokenKind Kind;
        public string Value;
    }

    public enum SPTokenKind
    {
        Name,
        Symbol,
        Newline,
        Quote,
        SingleLineComment,
        MultilineComment,
        BracketOpen,
        BracketClose,
        Operator,
        PreProcessorLine,
        Comma,
        Semicolon,
        Invalid
    }
}
