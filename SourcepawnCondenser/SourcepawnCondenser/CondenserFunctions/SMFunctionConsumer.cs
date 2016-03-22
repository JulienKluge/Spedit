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
		private int ConsumeSMFunction()
		{
			SMFunctionKind kind = SMFunctionKind.Unknown;
			int startPosition = position;
			int iteratePosition = startPosition + 1;
			switch (t[startPosition].Value)
			{
				case "stock":
					{
						if ((startPosition + 1) < length)
						{
							if (t[startPosition + 1].Kind == TokenKind.FunctionIndicator)
							{
								if (t[startPosition + 1].Value == "static")
								{
									kind = SMFunctionKind.StockStatic;
									++iteratePosition;
									break;
								}
							}
						}
						kind = SMFunctionKind.Stock;
						break;
					}
				case "native":
					{
						kind = SMFunctionKind.Native;
						break;
					}
				case "forward":
					{
						kind = SMFunctionKind.Forward;
						break;
					}
				case "public":
					{
						if ((startPosition + 1) < length)
						{
							if (t[startPosition + 1].Kind == TokenKind.FunctionIndicator)
							{
								if (t[startPosition + 1].Value == "native")
								{
									kind = SMFunctionKind.PublicNative;
									++iteratePosition;
									break;
								}
							}
						}
						kind = SMFunctionKind.Public;
						break;
					}
				case "static":
                    {
						kind = SMFunctionKind.Static;
						break;
					}
				case "normal":
                    {
						kind = SMFunctionKind.Normal;
						break;
					}
			}
			string functionCommentString = string.Empty;
			int commentTokenIndex = BacktraceTestForToken(startPosition - 1, TokenKind.MultiLineComment, true, false);
			if (commentTokenIndex == - 1)
			{
				commentTokenIndex = BacktraceTestForToken(startPosition - 1, TokenKind.SingleLineComment, true, false);
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
			string functionReturnType = string.Empty, functionName = string.Empty;
			for (; iteratePosition < startPosition + 5; ++iteratePosition)
			{
				if (t.Length > (iteratePosition + 1))
				{
					if (t[iteratePosition].Kind == TokenKind.Identifier)
					{
						if (t[iteratePosition + 1].Kind == TokenKind.ParenthesisOpen)
						{
							functionName = t[iteratePosition].Value;
							break;
						}
						else
						{
							functionReturnType = t[iteratePosition].Value;
						}
						continue;
					}
					else if (t[iteratePosition].Kind == TokenKind.Character)
					{
						if (t[iteratePosition].Value.Length > 0)
						{
							char testChar = t[iteratePosition].Value[0];
							if (testChar == ':' || testChar == '[' || testChar == ']')
							{
								continue;
							}
						}
					}
					return -1;
				}
				else
				{
					return -1;
				}
			}
			if (string.IsNullOrEmpty(functionName))
			{
				return -1;
			}
			++iteratePosition;
			List<string> functionParameters = new List<string>();
			int parameterDeclIndexStart = t[iteratePosition].Index;
			int parameterDeclIndexEnd = -1;
			int lastParameterIndex = parameterDeclIndexStart;
			int parenthesisCounter = 0;
			bool gotCommaBreak = false;
			int outTokenIndex = -1;
			int braceState = 0;
			for (; iteratePosition < length; ++iteratePosition)
			{
				if (t[iteratePosition].Kind == TokenKind.ParenthesisOpen)
				{
					++parenthesisCounter;
					continue;
				}
				if (t[iteratePosition].Kind == TokenKind.ParenthesisClose)
				{
					--parenthesisCounter;
					if (parenthesisCounter == 0)
					{
						outTokenIndex = iteratePosition;
						parameterDeclIndexEnd = t[iteratePosition].Index;
						int length = (t[iteratePosition].Index - 1) - (lastParameterIndex + 1);
						if (gotCommaBreak)
						{
							if (length == 0)
							{
								functionParameters.Add(string.Empty);
							}
							else
							{
								functionParameters.Add((source.Substring(lastParameterIndex + 1, length + 1)).Trim());
							}
						}
						else if (length > 0)
						{
							string singleParameterString = source.Substring(lastParameterIndex + 1, length + 1);
							if (!string.IsNullOrWhiteSpace(singleParameterString))
							{
								functionParameters.Add(singleParameterString);
							}
                        }
						break;
					}
					continue;
				}
				if (t[iteratePosition].Kind == TokenKind.BraceOpen)
				{
					++braceState;
				}
				if (t[iteratePosition].Kind == TokenKind.BraceClose)
				{
					--braceState;
				}
				if (t[iteratePosition].Kind == TokenKind.Comma && braceState == 0)
				{
					gotCommaBreak = true;
					int length = (t[iteratePosition].Index - 1) - (lastParameterIndex + 1);
					if (length == 0)
					{
						functionParameters.Add(string.Empty);
					}
					else
					{
						functionParameters.Add((source.Substring(lastParameterIndex + 1, length + 1)).Trim());
					}
					lastParameterIndex = t[iteratePosition].Index;
				}
			}
			if (parameterDeclIndexEnd == -1)
			{
				return -1;
			}
			def.Functions.Add(new SMFunction() {
				FunctionKind = kind,
				Index = t[startPosition].Index,
				Length = (parameterDeclIndexEnd - t[startPosition].Index) + 1,
				Name = functionName,
				ReturnType = functionReturnType,
				CommentString = Condenser.TrimComments(functionCommentString),
				Parameters = functionParameters.ToArray() });
			if ((outTokenIndex + 1) < length)
			{
				if (t[outTokenIndex + 1].Kind == TokenKind.Semicolon)
				{
					return outTokenIndex + 1;
				}
				int nextOpenBraceTokenIndex = FortraceTestForToken(outTokenIndex + 1, TokenKind.BraceOpen, true, false);
				if (nextOpenBraceTokenIndex != -1)
				{
					braceState = 0;
					for (int i = nextOpenBraceTokenIndex; i < length; ++i)
					{
						if (t[i].Kind == TokenKind.BraceOpen)
						{
							++braceState;
						}
						else if (t[i].Kind == TokenKind.BraceClose)
						{
							--braceState;
							if (braceState == 0)
							{
								return i;
							}
						}
					}
				}
			}
			return outTokenIndex;
        }
	}
}
