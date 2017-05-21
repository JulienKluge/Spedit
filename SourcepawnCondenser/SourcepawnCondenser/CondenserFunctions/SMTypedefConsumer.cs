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
		private int ConsumeSMTypedef()
		{
			int startIndex = t[position].Index;
			if ((position + 2) < length)
			{
				++position;
				string name = string.Empty;
				if (t[position].Kind == TokenKind.Identifier)
				{
					name = t[position].Value;
					for (int iteratePosition = position + 1; iteratePosition < length; ++iteratePosition)
					{
						if (t[iteratePosition].Kind == TokenKind.Semicolon)
						{
							def.Typedefs.Add(new SMTypedef()
							{
								Index = startIndex,
								Length = t[iteratePosition].Index - startIndex + 1,
								File = FileName,
								Name = name,
								FullName = source.Substring(startIndex, t[iteratePosition].Index - startIndex + 1)
							});
							return iteratePosition;
						}
					}
				}
			}
			return -1;
		}

		private int ConsumeSMTypeset()
		{
			int startIndex = t[position].Index;
			if ((position + 2) < length)
			{
				++position;
				string name = string.Empty;
				if (t[position].Kind == TokenKind.Identifier)
				{
					name = t[position].Value;
					int bracketIndex = 0;
					for (int iteratePosition = position + 1; iteratePosition < length; ++iteratePosition)
					{
						if (t[iteratePosition].Kind == TokenKind.BraceClose)
						{
							--bracketIndex;
							if (bracketIndex == 0)
							{
								def.Typedefs.Add(new SMTypedef()
								{
									Index = startIndex,
									Length = t[iteratePosition].Index - startIndex + 1,
									File = FileName,
									Name = name,
									FullName = source.Substring(startIndex, t[iteratePosition].Index - startIndex + 1)
								});
								return iteratePosition;
							}
						}
						else if (t[iteratePosition].Kind == TokenKind.BraceOpen)
						{
							++bracketIndex;
						}
					}
				}
			}
			return -1;
		}
	}
}
