using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
	    private int ConsumeSMStruct()
	    {
	        var startIndex = _t[_position].Index;

	        if (_position + 1 >= _length)
	            return -1;

	        var iteratePosition = _position;
	        var structName = string.Empty;

	        while (iteratePosition + 1 < _length && _t[iteratePosition].Kind != TokenKind.BraceOpen)
	        {
	            if (_t[iteratePosition].Kind == TokenKind.Identifier)
	                structName = _t[iteratePosition].Value;
	            ++iteratePosition;
	        }

	        var braceState = 0;
	        var endTokenIndex = -1;

	        for (; iteratePosition < _length; ++iteratePosition)
	        {
	            if (_t[iteratePosition].Kind == TokenKind.BraceOpen)
	            {
	                ++braceState;
	                continue;
	            }

	            if (_t[iteratePosition].Kind != TokenKind.BraceClose)
	                continue;

	            --braceState;

	            if (braceState != 0)
	                continue;

	            endTokenIndex = iteratePosition;
	            break;
	        }
	        if (endTokenIndex == -1)
	            return -1;

	        _def.Structs.Add(new SMStruct
	        {
	            Index = startIndex,
	            Length = _t[endTokenIndex].Index - startIndex + 1,
	            File = _fileName,
	            Name = structName
	        });
	        return endTokenIndex;
	    }
	}
}
