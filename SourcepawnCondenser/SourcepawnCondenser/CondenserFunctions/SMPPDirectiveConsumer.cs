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
		private int ConsumeSMPPDirective()
		{
			if (t[position].Value == "#define")
			{
				if ((position + 1) < length)
				{
					if (t[position + 1].Kind == TokenKind.Identifier)
					{
                        def.Defines.Add(new SMDefine() { Index = t[position].Index, Length = (t[position + 1].Index - t[position].Index) + t[position + 1].Length, File = FileName,
							Name = t[position + 1].Value });
						for (int j = position + 1; j < length; ++j)
						{
							if (t[j].Kind == TokenKind.EOL)
							{
								return j;
							}
						}
						return position + 1;
					}
				}
			}
			return -1;
		}
	}
}
