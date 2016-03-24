using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
		private int ConsumeSMMethodmap()
		{
			int startIndex = t[position].Index;
            int iteratePosition = position + 1;
			if ((position + 4) < length)
            {
                string methodMapName = string.Empty;
                string methodMapType = string.Empty;
                List<SMMethodmapMethod> methods = new List<SMMethodmapMethod>();
                if (t[iteratePosition].Kind == TokenKind.Identifier)
                {
                    if (t[iteratePosition + 1].Kind == TokenKind.Identifier)
                    {
                        methodMapType = t[iteratePosition].Value;
                        ++iteratePosition;
                        methodMapName = t[iteratePosition].Value;
                    }
                    else
                    {
                        methodMapName = t[iteratePosition].Value;
                    }
                    ++iteratePosition;
                }
                string inheriteType = string.Empty;
                bool enteredBlock = false;
                int braceIndex = 0;
                int lastIndex = -1;
                for (; iteratePosition < length; ++iteratePosition)
                {
                    if (t[iteratePosition].Kind == TokenKind.BraceOpen)
                    {
                        ++braceIndex;
                        enteredBlock = true;
                        continue;
                    }
                    else if (t[iteratePosition].Kind == TokenKind.BraceClose)
                    {
                        --braceIndex;
                        if (braceIndex <= 0)
                        {
                            lastIndex = iteratePosition;
                            break;
                        }
                    }
                    else if (braceIndex == 0 && t[iteratePosition].Kind == TokenKind.Character)
                    {
                        if (t[iteratePosition].Value == "<")
                        {
                            if ((iteratePosition + 1) < length)
                            {
                                if (t[iteratePosition + 1].Kind == TokenKind.Identifier)
                                {
                                    inheriteType = t[iteratePosition + 1].Value;
                                    ++iteratePosition;
                                    continue;
                                }
                            }
                        }
                    }
                    else if (enteredBlock)
                    {
                        if (t[iteratePosition].Kind == TokenKind.FunctionIndicator)
                        {
                            int mStartIndex = t[iteratePosition].Index;
                            string functionCommentString = string.Empty;
                            int commentTokenIndex = BacktraceTestForToken(iteratePosition - 1, TokenKind.MultiLineComment, true, false);
                            if (commentTokenIndex == -1)
                            {
                                commentTokenIndex = BacktraceTestForToken(iteratePosition - 1, TokenKind.SingleLineComment, true, false);
                                if (commentTokenIndex != -1)
                                {
                                    StringBuilder strBuilder = new StringBuilder(t[commentTokenIndex].Value);
                                    while ((commentTokenIndex = BacktraceTestForToken(commentTokenIndex - 1, TokenKind.SingleLineComment, true, false)) != -1)
                                    {
                                        strBuilder.Insert(0, Environment.NewLine);
                                        strBuilder.Insert(0, t[commentTokenIndex].Value);
                                    }
                                    functionCommentString = strBuilder.ToString();
                                }
                            }
                            else
                            {
                                functionCommentString = t[commentTokenIndex].Value;
                            }
                            int mEndIndex = mStartIndex;
                            List<string> functionIndicators = new List<string>();
                            List<string> parameters = new List<string>();
                            string methodName = string.Empty;
                            string methodReturnValue = string.Empty;
                            bool ParsingIndicators = true;
                            bool InCodeSection = false;
                            int ParenthesisIndex = 0;
                            int mBraceIndex = 0;
                            bool AwaitingName = true;
                            string lastFoundParam = string.Empty;
                            bool foundCurentParameter = false;
                            bool InSearchForComma = false;
                            for (int i = iteratePosition; i < length; ++i)
                            {
                                if (InCodeSection)
                                {
                                    if (t[i].Kind == TokenKind.BraceOpen)
                                    {
                                        ++mBraceIndex;
                                    }
                                    else if (t[i].Kind == TokenKind.BraceClose)
                                    {
                                        --mBraceIndex;
                                        if (mBraceIndex <= 0)
                                        {
                                            iteratePosition = i;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (ParsingIndicators)
                                    {
                                        if (t[i].Kind == TokenKind.FunctionIndicator)
                                        {
                                            functionIndicators.Add(t[i].Value);
                                            continue;
                                        }
                                        else
                                        {
                                            ParsingIndicators = false;
                                        }
                                    }
                                    if (t[i].Kind == TokenKind.Identifier && AwaitingName)
                                    {
                                        if ((i + 1) < length)
                                        {
                                            if (t[i + 1].Kind == TokenKind.Identifier)
                                            {
                                                methodReturnValue = t[i].Value;
                                                methodName = t[i + 1].Value;
                                                ++i;
                                            }
                                            else
                                            {
                                                methodName = t[i].Value;
                                            }
                                            AwaitingName = false;
                                        }
                                        continue;
                                    }
                                    if (t[i].Kind == TokenKind.ParenthesisOpen)
                                    {
                                        ++ParenthesisIndex;
                                        continue;
                                    }
                                    if (t[i].Kind == TokenKind.ParenthesisClose)
                                    {
                                        --ParenthesisIndex;
                                        if (ParenthesisIndex == 0)
                                        {
                                            if (foundCurentParameter)
                                            {
                                                parameters.Add(lastFoundParam);
                                                lastFoundParam = string.Empty;
                                            }
                                            InCodeSection = true;
                                            if ((i + 1) < length)
                                            {
                                                if (t[i + 1].Kind == TokenKind.Semicolon)
                                                {
                                                    iteratePosition = i + 1;
                                                    mEndIndex = t[i + 1].Index;
                                                    break;
                                                }
                                                iteratePosition = i;
                                                mEndIndex = t[i].Index;
                                            }
                                        }
                                        continue;
                                    }
                                    if ((t[i].Kind == TokenKind.Identifier) && (!InSearchForComma))
                                    {
                                        lastFoundParam = t[i].Value;
                                        foundCurentParameter = true;
                                        continue;
                                    }
                                    if (t[i].Kind == TokenKind.Comma)
                                    {
                                        parameters.Add(lastFoundParam);
                                        lastFoundParam = string.Empty;
                                        InSearchForComma = false;
                                    }
                                    else if (t[i].Kind == TokenKind.Assignment)
                                    {
                                        InSearchForComma = true;
                                    }
                                }
                            }
                            if (mStartIndex < mEndIndex)
                            {
                                methods.Add(new SMMethodmapMethod() { Index = mStartIndex, Name = methodName, ReturnType = methodReturnValue, MethodKind = functionIndicators.ToArray(),
                                    Parameters = parameters.ToArray(), FullName = TrimFullname(source.Substring(mStartIndex, (mEndIndex - mStartIndex) + 1)),
									Length = (mEndIndex - mStartIndex) +1, CommentString = Condenser.TrimComments(functionCommentString), MethodmapName = methodMapName, File = FileName });
                            }
                        }
                    }
                    //TODO: ADD FIELDS AND METHOD PARSING
                }
                if (enteredBlock && braceIndex == 0)
                {
                    var mm = new SMMethodmap() { Index = startIndex, Length = t[lastIndex].Index - startIndex + 1, Name = methodMapName, File = FileName,
						Type = methodMapType, InheritedType = inheriteType };
                    mm.Methods.AddRange(methods);
                    def.Methodmaps.Add(mm);
                }
            }
			return -1;
		}
	}
}
