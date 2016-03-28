using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
		public List<SMTypedef> Typedefs = new List<SMTypedef>();

		public string[] FunctionStrings = new string[0];
		//public string[] EnumStrings = new string[0]; NOT NEEDED
		//public string[] StructStrings = new string[0]; NOT NEEDED
		//public string[] DefinesStrings = new string[0]; NOT NEEDED
		public string[] ConstantsStrings = new string[0]; //ATTENTION: THIS IS NOT THE LIST OF ALL CONSTANTS - IT INCLUDES MUCH MORE
		public string[] MethodmapsStrings = new string[0];
		public string[] MethodsStrings = new string[0];
		public string[] FieldStrings = new string[0];
		public string[] TypeStrings = new string[0];

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

		public void AppendFiles(string[] paths)
		{
			for (int i = 0; i < paths.Length; ++i)
			{
				if (Directory.Exists(paths[i]))
				{
					string[] files = Directory.GetFiles(paths[i], "*.inc", SearchOption.AllDirectories);
					for (int j = 0; j < files.Length; ++j)
					{
						FileInfo fInfo = new FileInfo(files[j]);
						Condenser subCondenser = new Condenser(File.ReadAllText(fInfo.FullName), fInfo.Name);
						var subDefinition = subCondenser.Condense();
						Functions.AddRange(subDefinition.Functions);
						Enums.AddRange(subDefinition.Enums);
						Structs.AddRange(subDefinition.Structs);
						Defines.AddRange(subDefinition.Defines);
						Constants.AddRange(subDefinition.Constants);
						Methodmaps.AddRange(subDefinition.Methodmaps);
						Typedefs.AddRange(subDefinition.Typedefs);
					}
				}
			}
			Sort();
			ProduceStringArrays();
		}

		public void ProduceStringArrays()
		{
			FunctionStrings = new string[Functions.Count];
			for (int i = 0; i < Functions.Count; ++i)
			{
				FunctionStrings[i] = Functions[i].Name;
			}
			List<string> methodNames = new List<string>();
			List<string> fieldNames = new List<string>();
			List<string> methodmapNames = new List<string>();
			foreach (var mm in Methodmaps)
			{
				methodmapNames.Add(mm.Name);
				foreach (var m in mm.Methods)
				{
					methodNames.Add(m.Name);
				}
				foreach (var f in mm.Fields)
				{
					fieldNames.Add(f.Name);
				}
			}
			MethodsStrings = methodNames.ToArray();
			FieldStrings = fieldNames.ToArray();
			MethodmapsStrings = methodmapNames.ToArray();
			List<string> constantNames = new List<string>();
			foreach (var i in Constants) { constantNames.Add(i.Name); }
			foreach (var e in Enums) { constantNames.AddRange(e.Entries); }
			foreach (var i in Defines) { constantNames.Add(i.Name); }
			constantNames.Sort((a, b) => string.Compare(a, b));
			ConstantsStrings = constantNames.ToArray();
			List<string> typeNames = new List<string>();
			typeNames.Capacity = Enums.Count + Structs.Count + Methodmaps.Count;
			foreach (var i in Enums) { typeNames.Add(i.Name); }
			foreach (var i in Structs) { typeNames.Add(i.Name); }
			foreach (var i in Methodmaps) { typeNames.Add(i.Name); }
			foreach (var i in Typedefs) { typeNames.Add(i.Name); }
			typeNames.Sort((a, b) => string.Compare(a, b));
			TypeStrings = typeNames.ToArray();
		}

		public ACNode[] ProduceACNodes()
		{
			List<ACNode> nodes = new List<ACNode>();
			nodes.Capacity = Enums.Count + Structs.Count + Constants.Count + Functions.Count;
			nodes.AddRange(ACNode.ConvertFromStringArray(FunctionStrings, true, "▲ "));
			nodes.AddRange(ACNode.ConvertFromStringArray(TypeStrings, false, "♦ "));
			nodes.AddRange(ACNode.ConvertFromStringArray(ConstantsStrings, false, "• "));
			nodes.AddRange(ACNode.ConvertFromStringArray(MethodmapsStrings, false, "↨ "));
			//nodes = nodes.Distinct(new ACNodeEqualityComparer()).ToList(); Methodmaps and Functions can and will be the same.
			nodes.Sort((a, b) => { return string.Compare(a.EntryName, b.EntryName); });
			return nodes.ToArray();
		}
		public ISNode[] ProduceISNodes()
		{
			List<ISNode> nodes = new List<ISNode>();
			nodes.AddRange(ISNode.ConvertFromStringArray(MethodsStrings, true, "▲ "));
			nodes.AddRange(ISNode.ConvertFromStringArray(FieldStrings, true, "• "));
			nodes = nodes.Distinct(new ISNodeEqualityComparer()).ToList();
			nodes.Sort((a, b) => { return string.Compare(a.EntryName, b.EntryName); });
			return nodes.ToArray();
		}

		public void MergeDefinitions(SMDefinition def)
		{
			Functions.AddRange(def.Functions);
			Enums.AddRange(def.Enums);
			Structs.AddRange(def.Structs);
			Defines.AddRange(def.Defines);
			Constants.AddRange(def.Constants);
			Methodmaps.AddRange(def.Methodmaps);
		}

		public SMDefinition ProduceTemporaryExpandedDefinition(SMDefinition[] definitions)
		{
			SMDefinition def = new SMDefinition();
			def.MergeDefinitions(this);
			for (int i = 0; i < definitions.Length; ++i)
			{
				if (definitions[i] != null)
				{
					def.MergeDefinitions(definitions[i]);
				}
			}
			def.Sort();
			def.ProduceStringArrays();
			return def;
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
		private class SMMethodmapsComparer : IEqualityComparer<SMMethodmap>
		{
			public bool Equals(SMMethodmap left, SMMethodmap right)
			{ return left.Name == right.Name; }

			public int GetHashCode(SMMethodmap sm)
			{ return sm.Name.GetHashCode(); }
		}

		/*public class ACNodeEqualityComparer : IEqualityComparer<ACNode>
		{
			public bool Equals(ACNode nodeA, ACNode nodeB)
			{ return nodeA.EntryName == nodeB.EntryName; }

			public int GetHashCode(ACNode node)
			{ return node.EntryName.GetHashCode(); }
		}*/
		public class ISNodeEqualityComparer : IEqualityComparer<ISNode>
		{
			public bool Equals(ISNode nodeA, ISNode nodeB)
			{ return nodeA.EntryName == nodeB.EntryName; }

			public int GetHashCode(ISNode node)
			{ return node.EntryName.GetHashCode(); }
		}
	}

	public class ACNode
	{
		public string Name;
		public string EntryName;
		public bool IsExecuteable;

		public static List<ACNode> ConvertFromStringArray(string[] strings, bool Executable, string prefix = "")
		{
			List<ACNode> nodeList = new List<ACNode>();
			int length = strings.Length;
			for (int i = 0; i < length; ++i)
			{
				nodeList.Add(new ACNode() { Name = prefix + strings[i], EntryName = strings[i], IsExecuteable = Executable });
			}
			return nodeList;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}

	public class ISNode
	{
		public string Name;
		public string EntryName;
		public bool IsExecuteable = false;

		public static List<ISNode> ConvertFromStringArray(string[] strings, bool Executable, string prefix = "")
		{
			List<ISNode> nodeList = new List<ISNode>();
			int length = strings.Length;
			for (int i = 0; i < length; ++i)
			{
				nodeList.Add(new ISNode() { Name = prefix + strings[i], EntryName = strings[i], IsExecuteable = Executable });
			}
			return nodeList;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
