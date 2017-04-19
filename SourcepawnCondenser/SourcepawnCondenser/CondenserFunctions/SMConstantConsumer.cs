using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
	    private int ConsumeSMConstant()
	    {
	        if (_position + 2 >= _length)
	            return -1;

	        var startIndex = _t[_position].Index;
	        var foundIdentifier = false;
	        var foundAssignment = false;
	        var constantName = string.Empty;

	        for (var i = _position + 2; i < _length; ++i)
	            switch (_t[i].Kind)
	            {
	                case TokenKind.Semicolon:
	                    if (!foundIdentifier)
	                        if (_t[i - 1].Kind == TokenKind.Identifier)
	                        {
	                            constantName = _t[i - 1].Value;
	                            foundIdentifier = true;
	                        }
	                    if (!string.IsNullOrWhiteSpace(constantName))
	                        _def.Constants.Add(new SMConstant
	                        {
	                            Index = startIndex,
	                            Length = _t[i].Index - startIndex,
	                            File = _fileName,
	                            Name = constantName
	                        });
	                    return i;
	                case TokenKind.Assignment:
	                    foundAssignment = true;
	                    if (_t[i - 1].Kind == TokenKind.Identifier)
	                    {
	                        foundIdentifier = true;
	                        constantName = _t[i - 1].Value;
	                    }
	                    break;
	                case TokenKind.Identifier:
	                    break;
	                case TokenKind.Number:
	                    break;
	                case TokenKind.Character:
	                    break;
	                case TokenKind.BraceOpen:
	                    break;
	                case TokenKind.BraceClose:
	                    break;
	                case TokenKind.ParenthesisOpen:
	                    break;
	                case TokenKind.ParenthesisClose:
	                    break;
	                case TokenKind.Quote:
	                    break;
	                case TokenKind.SingleLineComment:
	                    break;
	                case TokenKind.MultiLineComment:
	                    break;
	                case TokenKind.Comma:
	                    break;
	                case TokenKind.FunctionIndicator:
	                    break;
	                case TokenKind.Constant:
	                    break;
	                case TokenKind.Enum:
	                    break;
	                case TokenKind.Struct:
	                    break;
	                case TokenKind.MethodMap:
	                    break;
	                case TokenKind.Property:
	                    break;
	                case TokenKind.PrePocessorDirective:
	                    break;
	                case TokenKind.TypeDef:
	                    break;
	                case TokenKind.TypeSet:
	                    break;
	                case TokenKind.Eol:
	                    break;
	                case TokenKind.Eof:
	                    break;
	                default:
	                    if (_t[i].Kind == TokenKind.Character && !foundAssignment)
	                    {
	                        if (_t[i].Value == "[")
	                            if (_t[i - 1].Kind == TokenKind.Identifier)
	                            {
	                                foundIdentifier = true;
	                                constantName = _t[i - 1].Value;
	                            }
	                    }
	                    else if (_t[i].Kind == TokenKind.Eol) //failsafe
	                    {
	                        return i;
	                    }
	                    break;
	            }
	        return -1;
	    }
	}
}
