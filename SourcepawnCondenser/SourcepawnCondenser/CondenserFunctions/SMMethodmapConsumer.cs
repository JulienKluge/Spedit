using System;
using System.Collections.Generic;
using System.Text;
using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
	    private int ConsumeSMMethodmap()
	    {
	        var startIndex = _t[_position].Index;
	        var iteratePosition = _position + 1;

	        if (_position + 4 >= _length)
	            return -1;

	        var methodMapName = string.Empty;
	        var methodMapType = string.Empty;

	        var methods = new List<SMMethodmapMethod>();
	        var fields = new List<SMMethodmapField>();

	        if (_t[iteratePosition].Kind == TokenKind.Identifier)
	        {
	            if (_t[iteratePosition + 1].Kind == TokenKind.Identifier)
	            {
	                methodMapType = _t[iteratePosition].Value;
	                ++iteratePosition;
	                methodMapName = _t[iteratePosition].Value;
	            }
	            else
	            {
	                methodMapName = _t[iteratePosition].Value;
	            }
	            ++iteratePosition;
	        }

	        var inheriteType = string.Empty;
	        var enteredBlock = false;
	        var braceIndex = 0;
	        var lastIndex = -1;

	        for (; iteratePosition < _length; ++iteratePosition)
	            if (_t[iteratePosition].Kind == TokenKind.BraceOpen)
	            {
	                ++braceIndex;
	                enteredBlock = true;
	            }
	            else if (_t[iteratePosition].Kind == TokenKind.BraceClose)
	            {
	                --braceIndex;

	                if (braceIndex > 0)
	                    continue;

	                lastIndex = iteratePosition;
	                break;
	            }
	            else if (braceIndex == 0 && _t[iteratePosition].Kind == TokenKind.Character)
	            {
	                if (_t[iteratePosition].Value != "<")
	                    continue;

	                if (iteratePosition + 1 >= _length)
	                    continue;

	                if (_t[iteratePosition + 1].Kind != TokenKind.Identifier)
	                    continue;

	                inheriteType = _t[iteratePosition + 1].Value;
	                ++iteratePosition;
	            }
	            else if (enteredBlock)
	            {
	                switch (_t[iteratePosition].Kind)
	                {
	                    case TokenKind.FunctionIndicator:
	                        var mStartIndex = _t[iteratePosition].Index;
	                        var functionCommentString = string.Empty;
	                        var commentTokenIndex = BacktraceTestForToken(iteratePosition - 1, TokenKind.MultiLineComment,
	                            true, false);

	                        if (commentTokenIndex == -1)
	                        {
	                            commentTokenIndex = BacktraceTestForToken(iteratePosition - 1, TokenKind.SingleLineComment,
	                                true, false);
	                            if (commentTokenIndex != -1)
	                            {
	                                var strBuilder = new StringBuilder(_t[commentTokenIndex].Value);
	                                while (
	                                (commentTokenIndex =
	                                    BacktraceTestForToken(commentTokenIndex - 1, TokenKind.SingleLineComment, true,
	                                        false)) != -1)
	                                {
	                                    strBuilder.Insert(0, Environment.NewLine);
	                                    strBuilder.Insert(0, _t[commentTokenIndex].Value);
	                                }
	                                functionCommentString = strBuilder.ToString();
	                            }
	                        }
	                        else
	                        {
	                            functionCommentString = _t[commentTokenIndex].Value;
	                        }

	                        var mEndIndex = mStartIndex;
	                        var functionIndicators = new List<string>();
	                        var parameters = new List<string>();
	                        var methodName = string.Empty;
	                        var methodReturnValue = string.Empty;
	                        var parsingIndicators = true;
	                        var inCodeSection = false;
	                        var parenthesisIndex = 0;
	                        var mBraceIndex = 0;
	                        var awaitingName = true;
	                        var lastFoundParam = string.Empty;
	                        var foundCurentParameter = false;
	                        var inSearchForComma = false;

	                        for (var i = iteratePosition; i < _length; ++i)
	                            if (inCodeSection)
	                            {
	                                if (_t[i].Kind == TokenKind.BraceOpen)
	                                {
	                                    ++mBraceIndex;
	                                }
	                                else if (_t[i].Kind == TokenKind.BraceClose)
	                                {
	                                    --mBraceIndex;

	                                    if (mBraceIndex > 0)
	                                        continue;

	                                    iteratePosition = i;
	                                    break;
	                                }
	                            }
	                            else
	                            {
	                                if (parsingIndicators)
	                                {
	                                    if (_t[i].Kind == TokenKind.FunctionIndicator)
	                                    {
	                                        functionIndicators.Add(_t[i].Value);
	                                        continue;
	                                    }

	                                    parsingIndicators = false;
	                                }
	                                if (_t[i].Kind == TokenKind.Identifier && awaitingName)
	                                {
	                                    if (i + 1 < _length)
	                                    {
	                                        if (_t[i + 1].Kind == TokenKind.Identifier)
	                                        {
	                                            methodReturnValue = _t[i].Value;
	                                            methodName = _t[i + 1].Value;
	                                            ++i;
	                                        }
	                                        else
	                                        {
	                                            methodName = _t[i].Value;
	                                        }
	                                        awaitingName = false;
	                                    }
	                                    continue;
	                                }
	                                if (_t[i].Kind == TokenKind.ParenthesisOpen)
	                                {
	                                    ++parenthesisIndex;
	                                    continue;
	                                }
	                                if (_t[i].Kind == TokenKind.ParenthesisClose)
	                                {
	                                    --parenthesisIndex;
	                                    if (parenthesisIndex == 0)
	                                    {
	                                        if (foundCurentParameter)
	                                        {
	                                            parameters.Add(lastFoundParam);
	                                            lastFoundParam = string.Empty;
	                                        }
	                                        inCodeSection = true;
	                                        if (i + 1 < _length)
	                                        {
	                                            if (_t[i + 1].Kind == TokenKind.Semicolon)
	                                            {
	                                                iteratePosition = i + 1;
	                                                mEndIndex = _t[i + 1].Index;
	                                                break;
	                                            }
	                                            iteratePosition = i;
	                                            mEndIndex = _t[i].Index;
	                                        }
	                                    }
	                                    continue;
	                                }
	                                if (_t[i].Kind == TokenKind.Identifier && !inSearchForComma)
	                                {
	                                    lastFoundParam = _t[i].Value;
	                                    foundCurentParameter = true;
	                                    continue;
	                                }
	                                switch (_t[i].Kind)
	                                {
	                                    case TokenKind.Comma:
	                                        parameters.Add(lastFoundParam);
	                                        lastFoundParam = string.Empty;
	                                        inSearchForComma = false;
	                                        break;
	                                    case TokenKind.Assignment:
	                                        inSearchForComma = true;
	                                        break;
	                                    default:
	                                        // ignored
	                                        break;
	                                }
	                            }
	                        if (mStartIndex < mEndIndex)
	                            methods.Add(new SMMethodmapMethod
	                            {
	                                Index = mStartIndex,
	                                Name = methodName,
	                                ReturnType = methodReturnValue,
	                                MethodKind = functionIndicators.ToArray(),
	                                Parameters = parameters.ToArray(),
	                                FullName = TrimFullname(_source.Substring(mStartIndex, mEndIndex - mStartIndex + 1)),
	                                Length = mEndIndex - mStartIndex + 1,
	                                CommentString = TrimComments(functionCommentString),
	                                MethodmapName = methodMapName,
	                                File = _fileName
	                            });
	                        break;
	                    case TokenKind.Property:
	                        var fStartIndex = _t[iteratePosition].Index;
	                        var fEndIndex = fStartIndex;
	                        if (iteratePosition - 1 >= 0)
	                            if (_t[iteratePosition - 1].Kind == TokenKind.FunctionIndicator)
	                                fStartIndex = _t[iteratePosition - 1].Index;

	                        var fieldName = string.Empty;
	                        var inPureSemicolonSearch = false;
	                        var fBracketIndex = 0;

	                        for (var j = iteratePosition; j < _length; ++j)
	                        {
	                            if (_t[j].Kind == TokenKind.Identifier && !inPureSemicolonSearch)
	                            {
	                                fieldName = _t[j].Value;
	                                continue;
	                            }
	                            if (_t[j].Kind == TokenKind.Assignment)
	                            {
	                                inPureSemicolonSearch = true;
	                                continue;
	                            }
	                            if (_t[j].Kind == TokenKind.Semicolon)
	                                if (fStartIndex == fEndIndex && fBracketIndex == 0)
	                                {
	                                    iteratePosition = j;
	                                    fEndIndex = _t[j].Index;
	                                    break;
	                                }
	                            if (_t[j].Kind == TokenKind.BraceOpen)
	                            {
	                                if (!inPureSemicolonSearch)
	                                {
	                                    inPureSemicolonSearch = true;
	                                    fEndIndex = _t[j].Index - 1;
	                                }
	                                ++fBracketIndex;
	                            }
	                            else if (_t[j].Kind == TokenKind.BraceClose)
	                            {
	                                --fBracketIndex;

	                                if (fBracketIndex != 0)
	                                    continue;

	                                if (j + 1 < _length)
	                                    if (_t[j + 1].Kind == TokenKind.Semicolon)
	                                        iteratePosition = j + 1;
	                                    else
	                                        iteratePosition = j;
	                                break;
	                            }
	                        }
	                        if (fStartIndex < fEndIndex)
	                            fields.Add(new SMMethodmapField
	                            {
	                                Index = fStartIndex,
	                                Length = fEndIndex - fStartIndex + 1,
	                                Name = fieldName,
	                                File = _fileName,
	                                MethodmapName = methodMapName,
	                                FullName = _source.Substring(fStartIndex, fEndIndex - fStartIndex + 1)
	                            });
	                        break;
	                    default:
	                        // ignored
	                        break;
	                }
	            }

	        if (!enteredBlock || braceIndex != 0)
	            return -1;

	        var mm = new SMMethodmap
	        {
	            Index = startIndex,
	            Length = _t[lastIndex].Index - startIndex + 1,
	            Name = methodMapName,
	            File = _fileName,
	            Type = methodMapType,
	            InheritedType = inheriteType
	        };

	        mm.Methods.AddRange(methods);
	        mm.Fields.AddRange(fields);
	        _def.Methodmaps.Add(mm);
	        _position = lastIndex;

	        return -1;
	    }
	}
}
