using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourcepawnCondenser.SourcemodDefinition
{
	public class SMDefinition
	{
		public List<SMFunction> Functions = new List<SMFunction>();
		public List<SMEnum> Enums = new List<SMEnum>();
		public List<SMStruct> Structs = new List<SMStruct>();
		public List<SMDefine> Defines = new List<SMDefine>();
		public List<SMConstant> Constants = new List<SMConstant>();
		public List<SMMethodmap> Methodmaps = new List<SMMethodmap>();

		public void Sort()
		{
			Functions = Functions.Distinct(new SMFunctionComparer()).ToList();
			Functions.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
			//Enums = Enums.Distinct(new SMEnumComparer()).ToList(); //enums can have the same name but not be the same...
			Enums.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
			Structs = Structs.Distinct(new SMStructComparer()).ToList();
			Structs.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
			Defines = Defines.Distinct(new SMDefineComparer()).ToList();
			Defines.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
			Constants = Constants.Distinct(new SMConstantComparer()).ToList();
			Constants.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
		}



		private class SMFunctionComparer : IEqualityComparer<SMFunction>
		{
			public bool Equals(SMFunction left, SMFunction right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SMFunction sm)
			{ return sm.Name.GetHashCode(); }
		}
		private class SMEnumComparer : IEqualityComparer<SMEnum>
		{
			public bool Equals(SMEnum left, SMEnum right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SMEnum sm)
			{ return sm.Name.GetHashCode(); }
		}
		private class SMStructComparer : IEqualityComparer<SMStruct>
		{
			public bool Equals(SMStruct left, SMStruct right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SMStruct sm)
			{ return sm.Name.GetHashCode(); }
		}
		private class SMDefineComparer : IEqualityComparer<SMDefine>
		{
			public bool Equals(SMDefine left, SMDefine right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SMDefine sm)
			{ return sm.Name.GetHashCode(); }
		}
		private class SMConstantComparer : IEqualityComparer<SMConstant>
		{
			public bool Equals(SMConstant left, SMConstant right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SMConstant sm)
			{ return sm.Name.GetHashCode(); }
		}
		/*private class SMComparer : IEqualityComparer<SM>
		{
			public bool Equals(SM left, SM right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SM sm)
			{ return sm.Name.GetHashCode(); }
		}*/

	}
}
