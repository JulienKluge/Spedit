using System;
using System.Collections.Generic;

namespace SourcepawnCondenser.Tokenizer
{
	public static class Tokenizer
	{
	    public static List<Token> TokenizeString(string source, bool ignoreMultipleEol)
	    {
	        var sArray = source.ToCharArray();
	        var sArrayLength = sArray.Length;
	        var token = new List<Token>((int) Math.Ceiling(sArrayLength * 0.20f));

	        //the reservation of the capacity is an empirical measured optimization. The average token to text length is 0.19 (with multiple EOL)
	        //To hopefully never extend the inner array, we use 2.3  |  performance gain: around 20%

	        for (var i = 0; i < sArrayLength; ++i)
	        {
	            var c = sArray[i];

	            #region Whitespace

	            switch (c)
	            {
	                case ' ':
	                case '\t':
	                    continue;
	                case '\n':
	                case '\r':
	                    token.Add(new Token("\r\n", TokenKind.Eol, i));
	                    if (ignoreMultipleEol)
	                        while (i + 1 < sArrayLength)
	                            if (sArray[i + 1] == '\n' || sArray[i + 1] == '\r')
	                                ++i;
	                            else
	                                break;
	                    else if (c == '\r')
	                        if (i + 1 < sArrayLength)
	                            if (sArray[i + 1] == '\n')
	                                ++i;
	                    continue;
	                case '{':
	                    token.Add(new Token("{", TokenKind.BraceOpen, i));
	                    continue;
	                case '}':
	                    token.Add(new Token("}", TokenKind.BraceClose, i));
	                    continue;
	                case '(':
	                    token.Add(new Token("(", TokenKind.ParenthesisOpen, i));
	                    continue;
	                case ')':
	                    token.Add(new Token(")", TokenKind.ParenthesisClose, i));
	                    continue;
	                case ';':
	                    token.Add(new Token(";", TokenKind.Semicolon, i));
	                    continue;
	                case ',':
	                    token.Add(new Token(",", TokenKind.Comma, i));
	                    continue;
	                case '=':
	                    token.Add(new Token("=", TokenKind.Assignment, i));
	                    continue;
	                case '/':
	                    if (i + 1 < sArrayLength)
	                        switch (sArray[i + 1])
	                        {
	                            case '/':
	                            {
	                                var startIndex = i;
	                                var endIndex = -1;

	                                for (var j = i + 1; j < sArrayLength; ++j)
	                                {
	                                    if (sArray[j] != '\r' && sArray[j] != '\n')
	                                        continue;

	                                    endIndex = j;
	                                    break;
	                                }

	                                if (endIndex == -1)
	                                {
	                                    token.Add(new Token(source.Substring(startIndex), TokenKind.SingleLineComment,
	                                        startIndex));
	                                    i = sArrayLength;
	                                }
	                                else
	                                {
	                                    token.Add(new Token(source.Substring(startIndex, endIndex - startIndex),
	                                        TokenKind.SingleLineComment, startIndex));
	                                    i = endIndex - 1;
	                                }

	                                continue;
	                            }
	                            case '*':
	                                if (i + 3 < sArrayLength)
	                                {
	                                    var startIndex = i;
	                                    var endIndex = -1;
	                                    for (var j = i + 3; j < sArrayLength; ++j)
	                                    {
	                                        if (sArray[j] != '/')
	                                            continue;

	                                        if (sArray[j - 1] != '*')
	                                            continue;

	                                        endIndex = j;
	                                        break;
	                                    }

	                                    if (endIndex == -1)
	                                    {
	                                        i = sArrayLength;
	                                        token.Add(new Token(source.Substring(startIndex), TokenKind.MultiLineComment,
	                                            startIndex));
	                                    }
	                                    else
	                                    {
	                                        i = endIndex;
	                                        token.Add(new Token(source.Substring(startIndex, endIndex - startIndex + 1),
	                                            TokenKind.MultiLineComment, startIndex));
	                                    }

	                                    continue;
	                                }
	                                break;
	                        }
	                    break;
	                default:
	                    // ignored
	                    break;
	            }

	            #endregion

	            #region Special characters

	            #endregion

	            #region Comments

	            #endregion

	            #region Quotes

	            if (c == '"' && i + 1 < sArrayLength)
	            {
	                var startIndex = i;
	                var endIndex = -1;

	                for (var j = i + 1; j < sArrayLength; ++j)
	                {
	                    if (sArray[j] != '"')
	                        continue;

	                    if (sArray[j - 1] == '\\')
	                        continue;

	                    endIndex = j;
	                    break;
	                }

	                if (endIndex != -1)
	                {
	                    token.Add(new Token(source.Substring(startIndex, endIndex - startIndex + 1), TokenKind.Quote,
	                        startIndex));
	                    i = endIndex;
	                    continue;
	                }
	            }

	            #endregion

	            #region Identifier

	            if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_') //identifier
	            {
	                int startIndex = i, endIndex = i + 1;
	                var nextChar = '\0';

	                if (i + 1 < sArrayLength)
	                {
	                    nextChar = sArray[i + 1];
	                    endIndex = -1;

	                    for (var j = i + 1; j < sArrayLength; ++j)
	                    {
	                        if (sArray[j] >= 'a' && sArray[j] <= 'z' || sArray[j] >= 'A' && sArray[j] <= 'Z' ||
	                            sArray[j] >= '0' && sArray[j] <= '9' || sArray[j] == '_') continue;
	                        endIndex = j;
	                        break;
	                    }

	                    if (endIndex == -1)
	                        endIndex = sArrayLength;
	                }
	                if (c != '_' ||
	                    c == '_' &&
	                    (nextChar >= 'a' && nextChar <= 'z' || nextChar >= 'A' && nextChar <= 'Z' ||
	                     nextChar >= '0' && nextChar <= '9' || nextChar == '_'))
	                {
	                    var identString = source.Substring(startIndex, endIndex - startIndex);

	                    switch (identString)
	                    {
	                        case "native":
	                        case "stock":
	                        case "forward":
	                        case "public":
	                        case "normal":
	                        case "static":
	                        {
	                            token.Add(new Token(identString, TokenKind.FunctionIndicator, startIndex));
	                            break;
	                        }
	                        case "enum":
	                        {
	                            token.Add(new Token(identString, TokenKind.Enum, startIndex));
	                            break;
	                        }
	                        case "struct":
	                        {
	                            token.Add(new Token(identString, TokenKind.Struct, startIndex));
	                            break;
	                        }
	                        case "const":
	                        {
	                            token.Add(new Token(identString, TokenKind.Constant, startIndex));
	                            break;
	                        }
	                        case "methodmap":
	                        {
	                            token.Add(new Token(identString, TokenKind.MethodMap, startIndex));
	                            break;
	                        }
	                        case "property":
	                        {
	                            token.Add(new Token(identString, TokenKind.Property, startIndex));
	                            break;
	                        }
	                        case "typeset":
	                        case "funcenum":
	                        {
	                            token.Add(new Token(identString, TokenKind.TypeSet, startIndex));
	                            break;
	                        }
	                        case "typedef":
	                        case "functag":
	                        {
	                            token.Add(new Token(identString, TokenKind.TypeDef, startIndex));
	                            break;
	                        }
	                        default:
	                        {
	                            token.Add(new Token(identString, TokenKind.Identifier, startIndex));
	                            break;
	                        }
	                    }

	                    i = endIndex - 1;
	                    continue;
	                }
	            }

	            #endregion

	            #region Numbers

	            if (c >= '0' && c <= '9') //numbers
	            {
	                var startIndex = i;
	                var endIndex = -1;
	                var gotDecimal = false;
	                var gotExponent = false;

	                for (var j = i + 1; j < sArrayLength; ++j)
	                {
	                    if (sArray[j] == '.')
	                    {
	                        if (!gotDecimal)
	                            if (j + 1 < sArrayLength)
	                                if (sArray[j + 1] >= '0' && sArray[j + 1] <= '9')
	                                {
	                                    gotDecimal = true;
	                                    continue;
	                                }
	                        endIndex = j - 1;
	                        break;
	                    }
	                    if (sArray[j] == 'e' || sArray[j] == 'E')
	                    {
	                        if (!gotExponent)
	                            if (j + 1 < sArrayLength)
	                                if (sArray[j + 1] == '+' || sArray[j + 1] == '-')
	                                {
	                                    if (j + 2 < sArrayLength)
	                                        if (sArray[j + 2] >= '0' && sArray[j + 2] <= '9')
	                                        {
	                                            ++j;
	                                            gotDecimal = gotExponent = true;
	                                            continue;
	                                        }
	                                }
	                                else if (sArray[j + 1] >= '0' && sArray[j + 1] <= '9')
	                                {
	                                    gotDecimal = gotExponent = true;
	                                    continue;
	                                }
	                        endIndex = j - 1;
	                        break;
	                    }

	                    if (sArray[j] >= '0' && sArray[j] <= '9')
	                        continue;

	                    endIndex = j - 1;
	                    break;
	                }

	                if (endIndex == -1)
	                    endIndex = sArrayLength - 1;
	                token.Add(new Token(source.Substring(startIndex, endIndex - startIndex + 1), TokenKind.Number,
	                    startIndex));
	                i = endIndex;
	                continue;
	            }

	            #endregion

	            #region Preprocessor Directives

	            if (c == '#')
	            {
	                var startIndex = i;

	                if (i + 1 < sArrayLength)
	                {
	                    var testChar = sArray[i + 1];

	                    if (testChar >= 'a' && testChar <= 'z' || testChar >= 'A' && testChar <= 'Z')
	                    {
	                        var endIndex = i + 1;

	                        for (var j = i + 1; j < sArrayLength; ++j)
	                        {
	                            if (sArray[j] >= 'a' && sArray[j] <= 'z' || sArray[j] >= 'A' && sArray[j] <= 'Z')
	                                continue;

	                            endIndex = j;
	                            break;
	                        }

	                        var directiveString = source.Substring(startIndex, endIndex - startIndex);
	                        token.Add(new Token(directiveString, TokenKind.PrePocessorDirective, startIndex));
	                        i = endIndex - 1;
	                        continue;
	                    }
	                }
	            }

	            #endregion

	            token.Add(new Token(c, TokenKind.Character, i));
	        }
	        token.Add(new Token("", TokenKind.Eof, sArrayLength));
	        return token;
	    }
	}
}
