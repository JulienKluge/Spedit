using System.Collections.Generic;

namespace SourcepawnCondenser.SourcemodDefinition
{
	public class SMMethodmap
	{
		public int Index = -1;
		public int Length = 0;
		public string File = string.Empty;
		public string Name = string.Empty;
        public string Type = string.Empty;
        public string InheritedType = string.Empty;
        public List<SMMethodmapField> Fields = new List<SMMethodmapField>();
		public List<SMMethodmapMethod> Methods = new List<SMMethodmapMethod>();
	}

	public class SMMethodmapField
	{
		public int Index = -1;
		public int Length = 0;
		public string File = string.Empty;
		public string Name = string.Empty;
		public string MethodmapName = string.Empty;
		public string FullName = string.Empty;
	}

	public class SMMethodmapMethod
	{
		public int Index = -1;
		public int Length = 0;
		public string File = string.Empty;
		public string Name = string.Empty;
		public string MethodmapName = string.Empty;
        public string FullName = string.Empty;
		public string ReturnType = string.Empty;
		public string CommentString = string.Empty;
		public string[] Parameters = new string[0];
		public string[] MethodKind = new string[0];
	}
}
