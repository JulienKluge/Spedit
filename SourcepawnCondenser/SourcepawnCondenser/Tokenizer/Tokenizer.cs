using System;
using System.Collections.Generic;

namespace SourcepawnCondenser.Tokenizer
{
	public static class Tokenizer
	{
		public static List<Token> TokenizeString(string Source, bool IgnoreMultipleEOL)
		{
			char[] sArray = Source.ToCharArray();
			int sArrayLength = sArray.Length;
			List<Token> token = new List<Token>((int)Math.Ceiling((float)sArrayLength * 0.20f));
			//the reservation of the capacity is an empirical measured optimization. The average token to text length is 0.19 (with multiple EOL)
			//To hopefully never extend the inner array, we use 2.3  |  performance gain: around 20%
			char c;
			for (int i = 0; i < sArrayLength; ++i)
			{
				c = sArray[i];

				#region Whitespace
				if (c == ' ' || c == '\t')
				{
					continue;
				}

				if (c == '\n' || c == '\r')
				{
					token.Add(new Token("\r\n", TokenKind.EOL, i));
					if (IgnoreMultipleEOL)
					{
						while ((i + 1) < sArrayLength)
						{
							if (sArray[i + 1] == '\n' || sArray[i + 1] == '\r')
							{
								++i;
							}
							else
							{
								break;
							}
						}
					}
					else if (c == '\r')
					{
						if ((i + 1) < sArrayLength)
						{
							if (sArray[i + 1] == '\n')
							{
								++i;
							}
						}
					}
					continue;
				}
				#endregion

				#region Special characters
				if (c == '{')
				{
					token.Add(new Token("{", TokenKind.BraceOpen, i));
					continue;
				}

				if (c == '}')
				{
					token.Add(new Token("}", TokenKind.BraceClose, i));
					continue;
				}

				if (c == '(')
				{
					token.Add(new Token("(", TokenKind.ParenthesisOpen, i));
					continue;
				}

				if (c == ')')
				{
					token.Add(new Token(")", TokenKind.ParenthesisClose, i));
					continue;
				}

				if (c == ';')
				{
					token.Add(new Token(";", TokenKind.Semicolon, i));
					continue;
				}

				if (c == ',')
				{
					token.Add(new Token(",", TokenKind.Comma, i));
					continue;
				}

				if (c == '=')
				{
					token.Add(new Token("=", TokenKind.Assignment, i));
					continue;
				}
				#endregion

				#region Comments
				if (c== '/')
				{
					if ((i + 1) < sArrayLength)
					{
						if (sArray[i + 1] == '/') //singleline comment
						{
							int startIndex = i;
							int endIndex = -1;
							for (int j = i + 1; j < sArrayLength; ++j)
							{
								if (sArray[j] == '\r' || sArray[j] == '\n')
								{
									endIndex = j;
									break;
								}
							}
							if (endIndex == -1)
							{
								token.Add(new Token(Source.Substring(startIndex), TokenKind.SingleLineComment, startIndex));
								i = sArrayLength;
							}
							else
							{
								token.Add(new Token(Source.Substring(startIndex, endIndex - startIndex), TokenKind.SingleLineComment, startIndex));
								i = endIndex - 1;
							}
							continue;
						}
						else if (sArray[i + 1] == '*') //multiline comment
						{
							if ((i + 3) < sArrayLength)
							{
								int startIndex = i;
								int endIndex = -1;
								for (int j = i + 3; j < sArrayLength; ++j)
								{
									if (sArray[j] == '/')
									{
										if (sArray[j - 1] == '*')
										{
											endIndex = j;
											break;
										}
									}
								}
								if (endIndex == -1)
								{
									i = sArrayLength;
									token.Add(new Token(Source.Substring(startIndex), TokenKind.MultiLineComment, startIndex));
								}
								else
								{
									i = endIndex;
									token.Add(new Token(Source.Substring(startIndex, endIndex - startIndex + 1), TokenKind.MultiLineComment, startIndex));
								}
								continue;
							}
						}
					}
				}
				#endregion

				#region Quotes
				if (c == '"' && ((i + 1) < sArrayLength))
				{
					int startIndex = i;
					int endIndex = -1;
					for (int j = i + 1; j < sArrayLength; ++j)
					{
						if (sArray[j] == '"')
						{
							if (sArray[j - 1] != '\\')
							{
								endIndex = j;
								break;
							}
						}
					}
					if (endIndex != -1)
					{
						token.Add(new Token(Source.Substring(startIndex, endIndex - startIndex + 1), TokenKind.Quote, startIndex));
						i = endIndex;
						continue;
					}
				}
				#endregion

				#region Identifier
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_') //identifier
				{
					int startIndex = i, endIndex = i + 1;
					char nextChar = '\0';
					if ((i + 1) < sArrayLength)
					{
						nextChar = sArray[i + 1];
						endIndex = -1;
						for (int j = i + 1; j < sArrayLength; ++j)
						{
							if (!((sArray[j] >= 'a' && sArray[j] <= 'z') || (sArray[j] >= 'A' && sArray[j] <= 'Z') || (sArray[j] >= '0' && sArray[j] <= '9') || sArray[j] == '_'))
							{
								endIndex = j;
								break;
							}
						}
						if (endIndex == -1)
						{ endIndex = sArrayLength; }
					}
					if ((c != '_') || (c == '_' && ((nextChar >= 'a' && nextChar <= 'z') || (nextChar >= 'A' && nextChar <= 'Z') || (nextChar >= '0' && nextChar <= '9') || nextChar == '_')))
					{
						string identString = Source.Substring(startIndex, endIndex - startIndex);
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
					int startIndex = i;
					int endIndex = -1;
					bool gotDecimal = false;
					bool gotExponent = false;
					for (int j = i + 1; j < sArrayLength; ++j)
					{
						if (sArray[j] == '.')
						{
							if (!gotDecimal)
							{
								if ((j + 1) < sArrayLength)
								{
									if (sArray[j + 1] >= '0' && sArray[j + 1] <= '9')
									{
										gotDecimal = true;
										continue;
									}
								}
							}
							endIndex = j - 1;
							break;
						}
						if (sArray[j] == 'e' || sArray[j] == 'E')
						{
							if (!gotExponent)
							{
								if ((j + 1) < sArrayLength)
								{
									if (sArray[j + 1] == '+' || sArray[j + 1] == '-')
									{
										if ((j + 2) < sArrayLength)
										{
											if (sArray[j + 2] >= '0' && sArray[j + 2] <= '9')
											{
												++j;
												gotDecimal = gotExponent = true;
												continue;
											}
										}
									}
									else if (sArray[j + 1] >= '0' && sArray[j + 1] <= '9')
									{
										gotDecimal = gotExponent = true;
										continue;
									}
								}
							}
							endIndex = j - 1;
							break;
						}

						if (!(sArray[j] >= '0' && sArray[j] <= '9'))
						{
							endIndex = j - 1;
							break;
						}
					}
					if (endIndex == -1)
					{ endIndex = sArrayLength - 1; }
					token.Add(new Token(Source.Substring(startIndex, endIndex - startIndex + 1), TokenKind.Number, startIndex));
					i = endIndex;
					continue;
				}
				#endregion

				#region Preprocessor Directives
				if (c == '#')
				{
					int startIndex = i;
					if ((i + 1) < sArrayLength)
					{
						char testChar = sArray[i + 1];
						if ((testChar >= 'a' && testChar <= 'z') || (testChar >= 'A' && testChar <= 'Z'))
						{
							int endIndex = i + 1;
							for (int j = i + 1; j < sArrayLength; ++j)
							{
								if (!((sArray[j] >= 'a' && sArray[j] <= 'z') || (sArray[j] >= 'A' && sArray[j] <= 'Z')))
								{
									endIndex = j;
									break;
								}
							}
							string directiveString = Source.Substring(startIndex, endIndex - startIndex);
							token.Add(new Token(directiveString, TokenKind.PrePocessorDirective, startIndex));
							i = endIndex - 1;
							continue;
						}
					}
				}
				#endregion


				token.Add(new Token(c, TokenKind.Character, i));
			}
			token.Add(new Token("", TokenKind.EOF, sArrayLength));
			return token;
		}
	}
}
