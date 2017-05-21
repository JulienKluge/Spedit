namespace SourcepawnCondenser.Tokenizer
{
	public class Token
	{
		public Token(string Value_, TokenKind Kind_, int Index_)
		{
			this.Value = Value_;
			this.Kind = Kind_;
			this.Index = Index_;
			this.Length = Value_.Length;
		}
		public Token(char Value_, TokenKind Kind_, int Index_)
		{
			this.Value = Value_.ToString();
			this.Kind = Kind_;
			this.Index = Index_;
			this.Length = 1;
		}
		public string Value;
		public TokenKind Kind;
		public int Index;
		public int Length;
	}
}
