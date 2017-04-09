using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
		public string[] ConstantsStrings = new string[0]; //ATTENTION: THIS IS NOT THE LIST OF ALL CONSTANTS - IT INCLUDES MUCH MORE
		public string[] MethodmapsStrings = new string[0];
		public string[] MethodsStrings = new string[0];
		public string[] FieldStrings = new string[0];
		public string[] TypeStrings = new string[0];

	    public void Sort()
	    {
	        try
	        {
	            Functions = Functions.Distinct(new SMFunctionComparer()).ToList();
	            Functions.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
	            Enums.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
	            Structs = Structs.Distinct(new SMStructComparer()).ToList();
	            Structs.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
	            Defines = Defines.Distinct(new SMDefineComparer()).ToList();
	            Defines.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
	            Constants = Constants.Distinct(new SMConstantComparer()).ToList();
	            Constants.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
	        }
	        catch (Exception)
	        {
	            // ignored
	        } //racing condition on save when the thread closes first or not..
	    }

		public void AppendFiles(string[] paths)
		{
			foreach (var path in paths)
			{
			    if (!Directory.Exists(path))
                    continue;

			    var files = Directory.GetFiles(path, "*.inc", SearchOption.AllDirectories);

			    foreach (var file in files)
			    {
			        var fInfo = new FileInfo(file);
                    var subCondenser = new Condenser(File.ReadAllText(fInfo.FullName), fInfo.Name);
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

			Sort();
			ProduceStringArrays();
		}

	    public void ProduceStringArrays()
	    {
	        FunctionStrings = new string[Functions.Count];

	        for (var i = 0; i < Functions.Count; ++i)
	            FunctionStrings[i] = Functions[i].Name;

	        var methodNames = new List<string>();
	        var fieldNames = new List<string>();
	        var methodmapNames = new List<string>();

	        foreach (var mm in Methodmaps)
	        {
	            methodmapNames.Add(mm.Name);
	            methodNames.AddRange(mm.Methods.Select(m => m.Name));
	            fieldNames.AddRange(mm.Fields.Select(f => f.Name));
	        }

	        MethodsStrings = methodNames.ToArray();
	        FieldStrings = fieldNames.ToArray();
	        MethodmapsStrings = methodmapNames.ToArray();

	        var constantNames = Constants.Select(i => i.Name).ToList();
	        foreach (var e in Enums)
	        {
	            constantNames.AddRange(e.Entries);
	        }
	        constantNames.AddRange(Defines.Select(i => i.Name));
	        constantNames.Sort(string.CompareOrdinal);
	        ConstantsStrings = constantNames.ToArray();
	        var typeNames = new List<string> {Capacity = Enums.Count + Structs.Count + Methodmaps.Count};
	        typeNames.AddRange(Enums.Select(i => i.Name));
	        typeNames.AddRange(Structs.Select(i => i.Name));
	        typeNames.AddRange(Methodmaps.Select(i => i.Name));
	        typeNames.AddRange(Typedefs.Select(i => i.Name));
	        typeNames.Sort(string.CompareOrdinal);
	        TypeStrings = typeNames.ToArray();
	    }

		public AcNode[] ProduceAcNodes()
		{
			var nodes = new List<AcNode>();

			try
			{
				nodes.Capacity = Enums.Count + Structs.Count + Constants.Count + Functions.Count;
				nodes.AddRange(AcNode.ConvertFromStringArray(FunctionStrings, true, "▲ "));
				nodes.AddRange(AcNode.ConvertFromStringArray(TypeStrings, false, "♦ "));
				nodes.AddRange(AcNode.ConvertFromStringArray(ConstantsStrings, false, "• "));
				nodes.AddRange(AcNode.ConvertFromStringArray(MethodmapsStrings, false, "↨ "));
				nodes.Sort((a, b) => string.CompareOrdinal(a.EntryName, b.EntryName));
			}
		    catch (Exception)
		    {
		        // ignored
		    }

		    return nodes.ToArray();
		}

		public IsNode[] ProduceIsNodes()
		{
			var nodes = new List<IsNode>();

			try
			{
				nodes.AddRange(IsNode.ConvertFromStringArray(MethodsStrings, true, "▲ "));
				nodes.AddRange(IsNode.ConvertFromStringArray(FieldStrings, true, "• "));
				nodes = nodes.Distinct(new IsNodeEqualityComparer()).ToList();
				nodes.Sort((a, b) => string.CompareOrdinal(a.EntryName, b.EntryName));
			}
		    catch (Exception)
		    {
		        // ignored
		    }

		    return nodes.ToArray();
		}

		public void MergeDefinitions(SMDefinition def)
		{
			try
			{
				Functions.AddRange(def.Functions);
				Enums.AddRange(def.Enums);
				Structs.AddRange(def.Structs);
				Defines.AddRange(def.Defines);
				Constants.AddRange(def.Constants);
				Methodmaps.AddRange(def.Methodmaps);
			}
		    catch (Exception)
		    {
		        // ignored
		    }
		}

		public SMDefinition ProduceTemporaryExpandedDefinition(SMDefinition[] definitions)
		{
			var def = new SMDefinition();

			try
			{
				def.MergeDefinitions(this);

				foreach (var smdef in definitions)
				    if (smdef != null)
				        def.MergeDefinitions(smdef);

			    def.Sort();
				def.ProduceStringArrays();
			}
		    catch (Exception)
		    {
		        // ignored
		    }

		    return def;
		}

	    private class SMFunctionComparer : IEqualityComparer<SMFunction>
	    {
	        public bool Equals(SMFunction left, SMFunction right)
	        {
	            return left.Name == right.Name;
	        }

	        public int GetHashCode(SMFunction sm)
	        {
	            return sm.Name.GetHashCode();
	        }
	    }

		private class SMEnumComparer : IEqualityComparer<SMEnum>
		{
		    public bool Equals(SMEnum left, SMEnum right)
		    {
		        return left.Name == right.Name;
		    }

		    public int GetHashCode(SMEnum sm)
		    {
		        return sm.Name.GetHashCode();
		    }
		}

		private class SMStructComparer : IEqualityComparer<SMStruct>
		{
		    public bool Equals(SMStruct left, SMStruct right)
		    {
		        return left.Name == right.Name;
		    }

		    public int GetHashCode(SMStruct sm)
		    {
		        return sm.Name.GetHashCode();
		    }
		}

		private class SMDefineComparer : IEqualityComparer<SMDefine>
		{
		    public bool Equals(SMDefine left, SMDefine right)
		    {
		        return left.Name == right.Name;
		    }

		    public int GetHashCode(SMDefine sm)
		    {
		        return sm.Name.GetHashCode();
		    }
		}

		private class SMConstantComparer : IEqualityComparer<SMConstant>
		{
		    public bool Equals(SMConstant left, SMConstant right)
		    {
		        return left.Name == right.Name;
		    }

		    public int GetHashCode(SMConstant sm)
		    {
		        return sm.Name.GetHashCode();
		    }
		}

		private class SMMethodmapsComparer : IEqualityComparer<SMMethodmap>
		{
		    public bool Equals(SMMethodmap left, SMMethodmap right)
		    {
		        return left.Name == right.Name;
		    }

		    public int GetHashCode(SMMethodmap sm)
		    {
		        return sm.Name.GetHashCode();
		    }
		}

		public class IsNodeEqualityComparer : IEqualityComparer<IsNode>
		{
		    public bool Equals(IsNode nodeA, IsNode nodeB)
		    {
		        return nodeA.EntryName == nodeB.EntryName;
		    }

		    public int GetHashCode(IsNode node)
		    {
		        return node.EntryName.GetHashCode();
		    }
		}
	}

	public class AcNode
	{
		public string Name;
		public string EntryName;
		public bool IsExecuteable;

		public static List<AcNode> ConvertFromStringArray(string[] strings, bool executable, string prefix = "")
		{
			var nodeList = new List<AcNode>();
			var length = strings.Length;

			for (var i = 0; i < length; ++i)
				nodeList.Add(new AcNode() { Name = prefix + strings[i], EntryName = strings[i], IsExecuteable = executable });

			return nodeList;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class IsNode
	{
		public string Name;
		public string EntryName;
		public bool IsExecuteable;

		public static List<IsNode> ConvertFromStringArray(string[] strings, bool executable, string prefix = "")
		{
			var nodeList = new List<IsNode>();
			var length = strings.Length;

			for (var i = 0; i < length; ++i)
				nodeList.Add(new IsNode() { Name = prefix + strings[i], EntryName = strings[i], IsExecuteable = executable });

			return nodeList;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
