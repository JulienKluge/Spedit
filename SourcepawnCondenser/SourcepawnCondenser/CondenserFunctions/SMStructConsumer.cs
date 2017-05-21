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
		private int ConsumeSMStruct()
		{
			int startIndex = t[position].Index;
			if ((position + 1) < length)
			{
				int iteratePosition = position;
				string structName = string.Empty;
				while ((iteratePosition + 1) < length && t[iteratePosition].Kind != TokenKind.BraceOpen)
				{
					if (t[iteratePosition].Kind == TokenKind.Identifier)
					{
						structName = t[iteratePosition].Value;
					}
					++iteratePosition;
				}
				int braceState = 0;
				int endTokenIndex = -1;
				for (; iteratePosition < length; ++iteratePosition)
				{
					if (t[iteratePosition].Kind == TokenKind.BraceOpen)
					{
						++braceState;
						continue;
					}
					if (t[iteratePosition].Kind == TokenKind.BraceClose)
					{
						--braceState;
						if (braceState == 0)
						{
							endTokenIndex = iteratePosition;
							break;
						}
						continue;
					}
				}
				if (endTokenIndex == -1)
				{
					return -1;
				}
				def.Structs.Add(new SMStruct() { Index = startIndex, Length = (t[endTokenIndex].Index - startIndex) + 1, File = FileName, Name = structName });
				return endTokenIndex;
			}
			return -1;
		}
	}
}
