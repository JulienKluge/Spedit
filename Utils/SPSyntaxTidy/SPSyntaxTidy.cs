using System.Text;

namespace Spedit.Utils.SPSyntaxTidy
{
    public static class SPSyntaxTidy
    {
        public static string TidyUp(string source)
        {
            var lookForSingleIndentationSegment = false;
            var singleIndentationSegmentScope = 0;
            var indentationLevel = 0;
            var outString = new StringBuilder();
            var token = SPTokenizer.Tokenize(source);
            var length = token.Length;

            for (var i = 0; i < length; ++i)
            {
                SPToken lastToken;
                switch (token[i].Kind)
                {
                    case SPTokenKind.Newline:
                    {
                        outString.AppendLine();
                        var subIndentLevel = indentationLevel;
                        var nextToken = GetTokenSave(i + 1, token, length);
                        if (nextToken.Kind == SPTokenKind.BracketClose)
                        {
                            --subIndentLevel;
                            if (subIndentLevel < 0)
                                subIndentLevel = 0;
                        }
                        else if (nextToken.Kind != SPTokenKind.BracketOpen)
                        {
                            if (lookForSingleIndentationSegment)
                                if (singleIndentationSegmentScope == 0)
                                    subIndentLevel++;
                        }
                        else if (nextToken.Kind == SPTokenKind.PreProcessorLine)
                            //preporcessor directives should not indented
                        {
                            subIndentLevel = 0;
                        }
                        lookForSingleIndentationSegment = false;
                        singleIndentationSegmentScope = 0;
                        for (var j = 0; j < subIndentLevel; ++j)
                            outString.Append('\t');
                        continue;
                    }
                    case SPTokenKind.BracketOpen:
                        lookForSingleIndentationSegment = false;
                        lastToken = GetTokenSave(i - 1, token, length);
                        if (lastToken.Kind != SPTokenKind.Newline && lastToken.Kind != SPTokenKind.Comma)
                            outString.Append(" ");
                        outString.Append("{");
                        if (GetTokenSave(i + 1, token, length).Kind != SPTokenKind.Newline)
                            outString.Append(" ");
                        ++indentationLevel;
                        continue;
                    case SPTokenKind.BracketClose:
                    {
                        if (GetTokenSave(i - 1, token, length).Kind != SPTokenKind.Newline)
                            outString.Append(" ");
                        outString.Append("}");
                        var nextToken = GetTokenSave(i + 1, token, length);
                        if (nextToken.Kind != SPTokenKind.Newline && nextToken.Kind != SPTokenKind.Comma &&
                            nextToken.Kind != SPTokenKind.Semicolon
                            && nextToken.Kind != SPTokenKind.SingleLineComment &&
                            nextToken.Kind != SPTokenKind.BracketClose)
                            outString.Append(" ");
                        --indentationLevel;
                        if (indentationLevel < 0)
                            indentationLevel = 0;
                        continue;
                    }
                    case SPTokenKind.PreProcessorLine:
                        outString.Append(token[i].Value);
                        continue;
                    case SPTokenKind.SingleLineComment:
                        if (GetTokenSave(i - 1, token, length).Kind != SPTokenKind.Newline)
                            outString.Append(" ");
                        outString.Append(token[i].Value);
                        continue;
                    case SPTokenKind.Operator:
                        if (token[i].Value == "-")
                        {
                            lastToken = GetTokenSave(i - 1, token, length);
                            var nextToken = GetTokenSave(i + 1, token, length);
                            var lastTokenIsName = lastToken.Kind == SPTokenKind.Name;
                            var lastTokenValid = lastTokenIsName || IsTokenNumber(lastToken);
                            if (!lastTokenValid)
                                if (lastToken.Kind == SPTokenKind.Symbol)
                                    lastTokenValid = lastToken.Value == ")" || lastToken.Value == "]";
                            if (lastTokenIsName)
                                lastTokenValid = lastToken.Value != "e" && lastToken.Value != "return";
                            var nextTokenValid = nextToken.Kind == SPTokenKind.Name || IsTokenNumber(nextToken);
                            if (!nextTokenValid)
                                if (nextToken.Kind == SPTokenKind.Symbol)
                                    nextTokenValid = nextToken.Value == "(";
                            if (nextTokenValid && lastTokenValid)
                                outString.Append(" - ");
                            else
                                outString.Append("-");
                            continue;
                        }
                        outString.Append(" " + token[i].Value + " ");
                        continue;
                    case SPTokenKind.Name:
                        if (token[i].Value == "return" &&
                            GetTokenSave(i + 1, token, length).Kind != SPTokenKind.Semicolon)
                        {
                            outString.Append("return ");
                            continue;
                        }
                        if (token[i].Value == "if" || token[i].Value == "else" || token[i].Value == "for" ||
                            token[i].Value == "while")
                        {
                            lookForSingleIndentationSegment = true;
                            singleIndentationSegmentScope = 0;
                        }
                        outString.Append(token[i].Value);
                        if (GetTokenSave(i + 1, token, length).Kind == SPTokenKind.Name)
                            outString.Append(" ");
                        else if (IsPreWhiteSpaceName(token[i].Value))
                            outString.Append(" ");
                        continue;
                    case SPTokenKind.Comma:
                        outString.Append(", ");
                        continue;
                    case SPTokenKind.Semicolon:
                    {
                        lookForSingleIndentationSegment = false;
                        outString.Append(";");
                        var nextToken = GetTokenSave(i + 1, token, length);
                        if (nextToken.Kind != SPTokenKind.Newline && nextToken.Kind != SPTokenKind.BracketClose &&
                            nextToken.Kind != SPTokenKind.SingleLineComment)
                            outString.Append(" ");
                        continue;
                    }
                    case SPTokenKind.Symbol:
                        if (token[i].Value == "]")
                            if (GetTokenSave(i + 1, token, length).Kind == SPTokenKind.Name)
                            {
                                outString.Append("] ");
                                continue;
                            }
                        switch (token[i].Value)
                        {
                            case "(":
                                ++singleIndentationSegmentScope;
                                ++indentationLevel;
                                break;
                            case ")":
                                --singleIndentationSegmentScope;
                                --indentationLevel;
                                break;
                            default:
                                // ignored
                                break;
                        }
                        if (token[i].Value == "&") //addressof operator
                            if (GetTokenSave(i - 1, token, length).Kind == SPTokenKind.Name &&
                                GetTokenSave(i + 1, token, length).Kind == SPTokenKind.Name)
                            {
                                outString.Append(" &");
                                continue;
                            }
                        break;
                    case SPTokenKind.Quote:
                        // ignored
                        break;
                    case SPTokenKind.MultilineComment:
                        // ignored
                        break;
                    case SPTokenKind.Invalid:
                        // ignored
                        break;
                    default:
                        // ignored
                        break;
                }
                outString.Append(token[i].Value);
            }
            return outString.ToString();
        }

        public static SPToken GetTokenSave(int index, SPToken[] token, int length)
        {
            if (index < 0 || index >= length)
                return new SPToken {Kind = SPTokenKind.Invalid};

            return token[index];
        }

        public static bool IsPreWhiteSpaceName(string name)
        {
            switch (name)
            {
                case "if":
                    return true;
                case "for":
                    return true;
                case "while":
                    return true;
                case "switch":
                    return true;
                case "case":
                    return true;
                default:
                    // ignored
                    break;
            }
            return false;
        }

        public static bool IsTokenNumber(SPToken token)
        {
            if (token == null)
                return false;

            if (token.Kind == SPTokenKind.Invalid)
                return false;

            if (token.Value.Length != 1)
                return false;

            return token.Value[0] >= '0' && token.Value[0] <= '9';
        }
    }
}
