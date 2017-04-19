using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
		private int ConsumeSMTypedef()
		{
			var startIndex = _t[_position].Index;

		    if ((_position + 2) >= _length)
                return -1;

		    ++_position;

		    if (_t[_position].Kind != TokenKind.Identifier)
                return -1;

		    var name = _t[_position].Value;

		    for (var iteratePosition = _position + 1; iteratePosition < _length; ++iteratePosition)
		    {
		        if (_t[iteratePosition].Kind != TokenKind.Semicolon)
                    continue;

		        _def.Typedefs.Add(new SMTypedef
		        {
		            Index = startIndex,
		            Length = _t[iteratePosition].Index - startIndex + 1,
		            File = _fileName,
		            Name = name,
		            FullName = _source.Substring(startIndex, _t[iteratePosition].Index - startIndex + 1)
		        });

		        return iteratePosition;
		    }

		    return -1;
		}

	    private int ConsumeSMTypeset()
	    {
	        var startIndex = _t[_position].Index;

	        if (_position + 2 >= _length)
	            return -1;

	        ++_position;

	        if (_t[_position].Kind != TokenKind.Identifier)
	            return -1;

	        var name = _t[_position].Value;
	        var bracketIndex = 0;

	        for (var iteratePosition = _position + 1; iteratePosition < _length; ++iteratePosition)
	            switch (_t[iteratePosition].Kind)
	            {
	                case TokenKind.BraceClose:
	                    --bracketIndex;
	                    if (bracketIndex == 0)
	                    {
	                        _def.Typedefs.Add(new SMTypedef
	                        {
	                            Index = startIndex,
	                            Length = _t[iteratePosition].Index - startIndex + 1,
	                            File = _fileName,
	                            Name = name,
	                            FullName = _source.Substring(startIndex, _t[iteratePosition].Index - startIndex + 1)
	                        });

	                        return iteratePosition;
	                    }
	                    break;
	                case TokenKind.BraceOpen:
	                    ++bracketIndex;
	                    break;
	                default:
	                    // ignored
	                    break;
	            }
	        return -1;
	    }
	}
}
