using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Spedit.UI.Components
{
    public class AeonEditorHighlighting : IHighlightingDefinition
    {
        public string Name { get { return "SM"; } }

        public HighlightingRuleSet MainRuleSet
        {
            get
            {
                HighlightingRuleSet commentMarkerSet = new HighlightingRuleSet();
                commentMarkerSet.Name = "CommentMarkerSet";
                commentMarkerSet.Rules.Add(new HighlightingRule()
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "TODO", "FIX", "FIXME", "HACK", "WORKAROUND", "BUG" }),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_CommentsMarker), FontWeight = FontWeights.Bold }
                });
                HighlightingRuleSet excludeInnerSingleLineComment = new HighlightingRuleSet();
                excludeInnerSingleLineComment.Spans.Add(new HighlightingSpan() { StartExpression = new Regex(@"\\"), EndExpression = new Regex(@".") });
                HighlightingRuleSet rs = new HighlightingRuleSet();
                SimpleHighlightingBrush commentBrush = new SimpleHighlightingBrush(Program.OptionsObject.SH_Comments);
                rs.Spans.Add(new HighlightingSpan() //singleline comments
                {
                    StartExpression = new Regex(@"//", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    EndExpression = new Regex(@"$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    SpanColor = new HighlightingColor() { Foreground = commentBrush },
                    StartColor = new HighlightingColor() { Foreground = commentBrush },
                    EndColor = new HighlightingColor() { Foreground = commentBrush },
                    RuleSet = commentMarkerSet
                });
                rs.Spans.Add(new HighlightingSpan() //multiline comments
                {
                    StartExpression = new Regex(@"/\*", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline),
                    EndExpression = new Regex(@"\*/", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline),
                    SpanColor = new HighlightingColor() { Foreground = commentBrush },
                    StartColor = new HighlightingColor() { Foreground = commentBrush },
                    EndColor = new HighlightingColor() { Foreground = commentBrush },
                    RuleSet = commentMarkerSet
                });
                SimpleHighlightingBrush stringBrush = new SimpleHighlightingBrush(Program.OptionsObject.SH_Strings);
                rs.Spans.Add(new HighlightingSpan() //strings
                {
                    StartExpression = new Regex(@"(?<!')""", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    EndExpression = new Regex(@"""", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    SpanColor = new HighlightingColor() { Foreground = stringBrush },
                    StartColor = new HighlightingColor() { Foreground = stringBrush },
                    EndColor = new HighlightingColor() { Foreground = stringBrush },
                    RuleSet = excludeInnerSingleLineComment
                });
                if (Program.OptionsObject.SH_HighlightDeprecateds)
                {
                    rs.Rules.Add(new HighlightingRule() //deprecated variable declaration
                    {
                        Regex = new Regex(@"^\s*(decl|new)\s+([a-zA-z_][a-zA-z1-9_]*:)?", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.ExplicitCapture),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Deprecated) }
                    });
                    rs.Rules.Add(new HighlightingRule() //deprecated function declaration
                    {
                        Regex = new Regex(@"^(public|stock|forward)\s+[a-zA-z_][a-zA-z1-9_]*:", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.ExplicitCapture),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Deprecated) }
                    });
                    rs.Rules.Add(new HighlightingRule() //deprecated taggings (from std types)
                    {
                        Regex = new Regex(@"\b(bool|Float|float|Handle|String|char|void|int):", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.ExplicitCapture),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Deprecated) }
                    });
                    rs.Rules.Add(new HighlightingRule() //deprecated keywords
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "decl", "String", "Float", "functag", "funcenum" }),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Deprecated) }
                    });
                }
                rs.Rules.Add(new HighlightingRule() //preprocessor keywords
                {
                    //Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "#include", "#if", "#else", "#elif", "#endif", "#define", "#undef", "#pragma", "#endinput" }),
                    Regex = new Regex(@"\#[a-zA-Z_][a-zA-Z0-9_]+", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_PreProcessor) }
                });
                rs.Rules.Add(new HighlightingRule() //type-values keywords
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "sizeof", "true", "false", "null" }),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_TypesValues) }
                });
                rs.Rules.Add(new HighlightingRule() //main keywords
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "if", "else", "switch", "case", "default", "for", "while", "do", "break", "continue", "return", "new", "view_as", "delete" }),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Keywords) }
                });
                rs.Rules.Add(new HighlightingRule() //context keywords
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "stock", "normal", "native", "public", "static", "const", "methodmap", "enum", "forward", "function", "struct", "property", "get", "set", "typeset", "typedef", "this" }),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_ContextKeywords) }
                });
                rs.Rules.Add(new HighlightingRule() //value types
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "bool", "char", "float", "int", "void", "any", "Handle" }),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Types) }
                });
                rs.Rules.Add(new HighlightingRule() //char type
                {
                    Regex = new Regex(@"'\\?.?'", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Chars) }
                });
                rs.Rules.Add(new HighlightingRule() //numbers
                {
                    Regex = new Regex(@"\b0[x][0-9a-fA-F]+|\b0[b][01]+|\b0[o][0-7]+|([+-]?\b[0-9]+(\.[0-9]+)?|\.[0-9]+)([eE][+-]?[0-9]+)?", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Numbers) }
                });
                rs.Rules.Add(new HighlightingRule() //special characters
                {
                    Regex = new Regex(@"[?.;()\[\]{}+\-/%*&<>^+~!|&]+", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_SpecialCharacters) }
                });
                rs.Rules.Add(new HighlightingRule() //std includes - string color!
                {
                    Regex = new Regex(@"\s[<][\w\\/\-]+(\.[\w\-]+)?[>]", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = stringBrush }
                });
                var def = Program.Configs[Program.SelectedConfig].GetSMDef();
                if (def.TypeStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //types
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.TypeStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Types) }
                    });
                }
                if (def.ConstantsStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //constants
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.ConstantsStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Constants) }
                    });
                }
                if (def.FunctionStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //Functions
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.FunctionStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Functions) }
                    });
                }
                if (def.MethodsStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //Methods
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.MethodsStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Methods) }
                    });
                }
                rs.Rules.Add(new HighlightingRule() //unknown function calls
                {
                    Regex = new Regex(@"\b\w+(?=\s*\()", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_UnkownFunctions) }
                });
                
                rs.Name = "MainRule";
                return rs;
            }
        }

        public HighlightingRuleSet GetNamedRuleSet(string name) { return null; }
        public HighlightingColor GetNamedColor(string name) { return null; }
        public IEnumerable<HighlightingColor> NamedHighlightingColors { get; set; }

        public IDictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> propertiesDictionary = new Dictionary<string, string>();
                propertiesDictionary.Add("DocCommentMarker", "///");
                return propertiesDictionary;
            }
        }
    }

    [Serializable]
    public sealed class SimpleHighlightingBrush : HighlightingBrush, ISerializable
    {
        readonly SolidColorBrush brush;

        internal SimpleHighlightingBrush(SolidColorBrush brush)
        {
            brush.Freeze();
            this.brush = brush;
        }

        public SimpleHighlightingBrush(Color color) : this(new SolidColorBrush(color)) { }

        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            return brush;
        }

        public override string ToString()
        {
            return brush.ToString();
        }

        SimpleHighlightingBrush(SerializationInfo info, StreamingContext context)
        {
            this.brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(info.GetString("color")));
            brush.Freeze();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("color", brush.Color.ToString(CultureInfo.InvariantCulture));
        }

        public override bool Equals(object obj)
        {
            SimpleHighlightingBrush other = obj as SimpleHighlightingBrush;
            if (other == null)
                return false;
            return this.brush.Color.Equals(other.brush.Color);
        }

        public override int GetHashCode()
        {
            return brush.Color.GetHashCode();
        }
    }

    public static class RegexKeywordsHelper
    {
        public static Regex GetRegexFromKeywords(string[] keywords, bool ForceAtomicRegex = false)
        {
            if (ForceAtomicRegex)
            {
                keywords = RegexKeywordsHelper.ConvertToAtomicRegexAbleStringArray(keywords);
            }
            if (keywords.Length == 0)
            {
                return new Regex("SPEdit_Error"); //hehe 
            }
            bool UseAtomicRegex = true;
            for (int j = 0; j < keywords.Length; ++j)
            {
                if ((!char.IsLetterOrDigit((keywords[j])[0])) || (!char.IsLetterOrDigit((keywords[j])[keywords[j].Length - 1])))
                {
                    UseAtomicRegex = false;
                    break;
                }
            }
            StringBuilder regexBuilder = new StringBuilder();
            if (UseAtomicRegex)
            { regexBuilder.Append(@"\b(?>"); }
            else
            { regexBuilder.Append(@"("); }
            List<string> orderedKeyWords = new List<string>(keywords);
            int i = 0;
            foreach (string keyword in orderedKeyWords.OrderByDescending(w => w.Length))
            {
                if ((i++) > 0)
                { regexBuilder.Append('|'); }
                if (UseAtomicRegex)
                { regexBuilder.Append(Regex.Escape(keyword)); }
                else
                {
                    if (char.IsLetterOrDigit(keyword[0])) { regexBuilder.Append(@"\b"); }
                    regexBuilder.Append(Regex.Escape(keyword));
                    if (char.IsLetterOrDigit(keyword[keyword.Length - 1])) { regexBuilder.Append(@"\b"); }
                }
            }
            if (UseAtomicRegex)
            { regexBuilder.Append(@")\b"); }
            else
            { regexBuilder.Append(@")"); }
            return new Regex(regexBuilder.ToString(), RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        }

        public static string[] ConvertToAtomicRegexAbleStringArray(string[] keywords)
        {
            List<string> atomicRegexAbleList = new List<string>();
            for (int j = 0; j < keywords.Length; ++j)
            {
				if (keywords[j].Length > 0)
				{
					if ((char.IsLetterOrDigit((keywords[j])[0])) && (char.IsLetterOrDigit((keywords[j])[keywords[j].Length - 1])))
					{
						atomicRegexAbleList.Add(keywords[j]);
					}
				}
            }
            return atomicRegexAbleList.ToArray();
        }
    }
}
