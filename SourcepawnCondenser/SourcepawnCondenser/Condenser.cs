using System;
using System.Text;
using SourcepawnCondenser.SourcemodDefinition;
using SourcepawnCondenser.Tokenizer;

namespace SourcepawnCondenser
{
	public partial class Condenser
	{
	    private readonly Token[] _t;
	    private int _position;
	    private readonly int _length;
	    private readonly SMDefinition _def;
	    private readonly string _source;
	    private readonly string _fileName;

		public Condenser(string sourceCode, string fileName)
		{
			_t = Tokenizer.Tokenizer.TokenizeString(sourceCode, true).ToArray();
			_position = 0;
			_length = _t.Length;
			_def = new SMDefinition();
			_source = sourceCode;

			if (fileName.EndsWith(".inc", StringComparison.InvariantCultureIgnoreCase))
				fileName = fileName.Substring(0, fileName.Length - 4);

			_fileName = fileName;
		}

	    public SMDefinition Condense()
	    {
	        Token ct;

	        while ((ct = _t[_position]).Kind != TokenKind.Eof)
	        {
	            if (ct.Kind == TokenKind.FunctionIndicator)
	            {
	                var newIndex = ConsumeSMFunction();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.Enum)
	            {
	                var newIndex = ConsumeSMEnum();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.Struct)
	            {
	                var newIndex = ConsumeSMStruct();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.PrePocessorDirective)
	            {
	                var newIndex = ConsumeSmppDirective();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.Constant)
	            {
	                var newIndex = ConsumeSMConstant();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.MethodMap)
	            {
	                var newIndex = ConsumeSMMethodmap();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.TypeSet)
	            {
	                var newIndex = ConsumeSMTypeset();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            if (ct.Kind == TokenKind.TypeDef)
	            {
	                var newIndex = ConsumeSMTypedef();

	                if (newIndex != -1)
	                {
	                    _position = newIndex + 1;
	                    continue;
	                }
	            }

	            ++_position;
	        }

	        _def.Sort();
	        return _def;
	    }

		private int BacktraceTestForToken(int startPosition, TokenKind testKind, bool ignoreEol, bool ignoreOtherTokens)
		{
		    for (var i = startPosition; i >= 0; --i)
		    {
		        if (_t[i].Kind == testKind)
		            return i;

		        if (ignoreOtherTokens)
		            continue;

		        if (_t[i].Kind == TokenKind.Eol && ignoreEol)
		            continue;

		        return -1;
		    }
		    return -1;
		}

		private int FortraceTestForToken(int startPosition, TokenKind testKind, bool ignoreEol, bool ignoreOtherTokens)
		{
		    for (var i = startPosition; i < _length; ++i)
		    {
		        if (_t[i].Kind == testKind)
		            return i;

		        if (ignoreOtherTokens)
		            continue;

		        if (_t[i].Kind == TokenKind.Eol && ignoreEol)
		            continue;

		        return -1;
		    }
		    return -1;
		}

        public static string TrimComments(string comment)
        {
            var outString = new StringBuilder();
            var lines = comment.Split('\r', '\n');

            for (var i = 0; i < lines.Length; ++i)
            {
                var line = (lines[i].Trim()).TrimStart('/', '*', ' ', '\t');

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (i > 0)
                    outString.AppendLine();

                outString.Append(line.StartsWith("@param") ? FormatParamLineString(line) : line);
            }

            return outString.ToString().Trim();
        }

		public static string TrimFullname(string name)
		{
			var outString = new StringBuilder();
            var lines = name.Split('\r', '\n');

			for (var i = 0; i < lines.Length; ++i)
			{
			    if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

			    if (i > 0)
			        outString.Append(" ");

			    outString.Append(lines[i].Trim(' ', '\t'));
			}

			return outString.ToString();
		}

		private static string FormatParamLineString(string line)
		{
			var split = line.Replace('\t', ' ').Split(new[] { ' ' }, 3);

			if (split.Length > 2)
				return ("@param " + split[1]).PadRight(24, ' ') + " " + split[2].Trim(' ', '\t');

			return line;
		}
	}
}
