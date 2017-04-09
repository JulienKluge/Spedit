using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
		private int ConsumeSmppDirective()
		{
		    if (_t[_position].Value != "#define")
                return -1;

		    if (_position + 1 >= _length)
                return -1;

		    if (_t[_position + 1].Kind != TokenKind.Identifier)
                return -1;

		    _def.Defines.Add(new SMDefine
		    {
		        Index = _t[_position].Index,
		        Length = _t[_position + 1].Index - _t[_position].Index + _t[_position + 1].Length,
		        File = _fileName,
		        Name = _t[_position + 1].Value
		    });

		    for (var j = _position + 1; j < _length; ++j)
		        if (_t[j].Kind == TokenKind.Eol)
		            return j;

		    return _position + 1;
		}
	}
}
