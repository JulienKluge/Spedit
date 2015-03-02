using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Spedit.SPCondenser
{
    public static class SourcepawnCondenser
    {
        public static CondensedSourcepawnDefinition Condense(string Path)
        {
            SourcepawnDefinitionCondeser csd = new SourcepawnDefinitionCondeser();
            if (!Directory.Exists(Path))
            {
                return csd.FinalCondense();
            }
            StringBuilder wholeSource = new StringBuilder();
            string[] files = Directory.GetFiles(Path, "*.inc", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                wholeSource.AppendLine(File.ReadAllText(files[i]));
            }
            string source = wholeSource.ToString();
            FunctionsCondenser.Condense(source, ref csd);
            Regex removeMultilineComments = new Regex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
            source = removeMultilineComments.Replace(source, string.Empty);
            EnumCondenser.Condense(source, ref csd);
            ConstantsCondenser.Condense(source, ref csd);
            return csd.FinalCondense();
        }
    }

    public class SourcepawnDefinitionCondeser
    {
        public List<string> _Types = new List<string>();
        public List<string> _Constants = new List<string>();
        public List<SPFunction> _Functions = new List<SPFunction>();
        public List<string> _FunctionNames = new List<string>();
        public List<string> _MethodNames = new List<string>();
        public List<string> _Properties = new List<string>();

        public CondensedSourcepawnDefinition FinalCondense()
        {
            var strComparer = new StringEqualityComparer();
            _Types = _Types.Distinct(strComparer).ToList();
            _Types.Sort((a, b) => { return string.Compare(a, b); });
            _Constants = _Constants.Distinct(strComparer).ToList();
            _Constants.Sort((a, b) => { return string.Compare(a, b); });
            _Functions = _Functions.Distinct(new SPFunctionEqualityComparer()).ToList();
            _Functions.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
            _FunctionNames = _FunctionNames.Distinct(strComparer).ToList();
            _FunctionNames.Sort((a, b) => { return string.Compare(a, b); });
            _MethodNames = _MethodNames.Distinct(strComparer).ToList();
            _MethodNames.Sort((a, b) => { return string.Compare(a, b); });
            _Properties = _Properties.Distinct(strComparer).ToList();
            _Properties.Sort((a, b) => { return string.Compare(a, b); });
            var csd = new CondensedSourcepawnDefinition()
            {
                Types = _Types.ToArray(),
                Constants = _Constants.ToArray(),
                Functions = _Functions.ToArray(),
                FunctionNames = _FunctionNames.ToArray(),
                MethodNames = _MethodNames.ToArray(),
                Properties = _Properties.ToArray()
            };
            List<ACNode> acNodeList = new List<ACNode>();
            acNodeList.AddRange(ACNode.ConvertFromStringArray(csd.Types, false));
            acNodeList.AddRange(ACNode.ConvertFromStringArray(csd.Constants, false));
            acNodeList.AddRange(ACNode.ConvertFromStringArray(csd.FunctionNames, true));
            //isNodeList.AddRange(ISNode.ConvertFromStringArray(csd.MethodNames, true));
            acNodeList = acNodeList.Distinct(new ACNodeEqualityComparer()).ToList();
            acNodeList.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
            csd.ACNodes = acNodeList.ToArray();
            List<ISNode> isNodeList = new List<ISNode>();
            isNodeList.AddRange(ISNode.ConvertFromStringArray(csd.MethodNames, true));
            isNodeList.AddRange(ISNode.ConvertFromStringArray(csd.Properties, false));
            isNodeList = isNodeList.Distinct(new ISNodeEqualityComparer()).ToList();
            isNodeList.Sort((a, b) => { return string.Compare(a.Name, b.Name); });
            csd.ISNodes = isNodeList.ToArray();
            return csd;
        }
    }

    public class CondensedSourcepawnDefinition
    {
        public string[] Types;
        public string[] Constants;
        public SPFunction[] Functions;
        public string[] FunctionNames;
        public string[] MethodNames;
        public ACNode[] ACNodes;
        public string[] Properties;
        public ISNode[] ISNodes;
    }

    public class SPFunction
    {
        public string Name;
        public string FullName;
        public string Comment;
    }

    public class StringEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string strA, string strB)
        { return strA == strB; }

        public int GetHashCode(string str)
        { return str.GetHashCode(); }
    }
    public class SPFunctionEqualityComparer : IEqualityComparer<SPFunction>
    {
        public bool Equals(SPFunction funcA, SPFunction funcB)
        { return funcA.Name == funcB.Name; }

        public int GetHashCode(SPFunction func)
        { return func.Name.GetHashCode(); }
    }
    public class ACNodeEqualityComparer : IEqualityComparer<ACNode>
    {
        public bool Equals(ACNode nodeA, ACNode nodeB)
        { return nodeA.Name == nodeB.Name; }

        public int GetHashCode(ACNode node)
        { return node.Name.GetHashCode(); }
    }
    public class ISNodeEqualityComparer : IEqualityComparer<ISNode>
    {
        public bool Equals(ISNode nodeA, ISNode nodeB)
        { return nodeA.Name == nodeB.Name; }

        public int GetHashCode(ISNode node)
        { return node.Name.GetHashCode(); }
    }

    public class ACNode
    {
        public string Name;
        public bool IsExecuteable;

        public static List<ACNode> ConvertFromStringArray(string[] strings, bool Executable)
        {
            List<ACNode> nodeList = new List<ACNode>();
            int length = strings.Length;
            for (int i = 0; i < length; ++i)
            {
                nodeList.Add(new ACNode() { Name = strings[i], IsExecuteable = Executable });
            }
            return nodeList;
        }
    }
    public class ISNode
    {
        public string Name;
        public bool IsExecuteable;

        public static List<ISNode> ConvertFromStringArray(string[] strings, bool Executable)
        {
            List<ISNode> nodeList = new List<ISNode>();
            int length = strings.Length;
            for (int i = 0; i < length; ++i)
            {
                nodeList.Add(new ISNode() { Name = strings[i], IsExecuteable = Executable });
            }
            return nodeList;
        }
    }

}
