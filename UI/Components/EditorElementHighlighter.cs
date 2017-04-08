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
        public string Name => "SM";

        public HighlightingRuleSet MainRuleSet
        {
            get
            {
                var commentMarkerSet = new HighlightingRuleSet();
                commentMarkerSet.Name = "CommentMarkerSet";
                commentMarkerSet.Rules.Add(new HighlightingRule()
                {
                    Regex =
                        RegexKeywordsHelper.GetRegexFromKeywords(new string[]
                            {"TODO", "FIX", "FIXME", "HACK", "WORKAROUND", "BUG"}),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHCommentsMarker),
                            FontWeight = FontWeights.Bold
                        }
                });
                var excludeInnerSingleLineComment = new HighlightingRuleSet();
                excludeInnerSingleLineComment.Spans.Add(new HighlightingSpan()
                {
                    StartExpression = new Regex(@"\\"),
                    EndExpression = new Regex(@".")
                });
                var rs = new HighlightingRuleSet();
                var commentBrush = new SimpleHighlightingBrush(Program.OptionsObject.SHComments);
                rs.Spans.Add(new HighlightingSpan() //singleline comments
                {
                    StartExpression = new Regex(@"//", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    EndExpression = new Regex(@"$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    SpanColor = new HighlightingColor() {Foreground = commentBrush},
                    StartColor = new HighlightingColor() {Foreground = commentBrush},
                    EndColor = new HighlightingColor() {Foreground = commentBrush},
                    RuleSet = commentMarkerSet
                });
                rs.Spans.Add(new HighlightingSpan() //multiline comments
                {
                    StartExpression =
                        new Regex(@"/\*",
                            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline),
                    EndExpression =
                        new Regex(@"\*/",
                            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline),
                    SpanColor = new HighlightingColor() {Foreground = commentBrush},
                    StartColor = new HighlightingColor() {Foreground = commentBrush},
                    EndColor = new HighlightingColor() {Foreground = commentBrush},
                    RuleSet = commentMarkerSet
                });
                var stringBrush = new SimpleHighlightingBrush(Program.OptionsObject.SHStrings);
                rs.Spans.Add(new HighlightingSpan() //strings
                {
                    StartExpression =
                        new Regex(@"(?<!')""", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    EndExpression = new Regex(@"""", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    SpanColor = new HighlightingColor() {Foreground = stringBrush},
                    StartColor = new HighlightingColor() {Foreground = stringBrush},
                    EndColor = new HighlightingColor() {Foreground = stringBrush},
                    RuleSet = excludeInnerSingleLineComment
                });
                if (Program.OptionsObject.SHHighlightDeprecateds)
                {
                    rs.Rules.Add(new HighlightingRule() //deprecated variable declaration
                    {
                        Regex =
                            new Regex(@"^\s*(decl|new)\s+([a-zA-z_][a-zA-z1-9_]*:)?",
                                RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.ExplicitCapture),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHDeprecated)
                            }
                    });
                    rs.Rules.Add(new HighlightingRule() //deprecated function declaration
                    {
                        Regex =
                            new Regex(@"^(public|stock|forward)\s+[a-zA-z_][a-zA-z1-9_]*:",
                                RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.ExplicitCapture),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHDeprecated)
                            }
                    });
                    rs.Rules.Add(new HighlightingRule() //deprecated taggings (from std types)
                    {
                        Regex =
                            new Regex(@"\b(bool|Float|float|Handle|String|char|void|int):",
                                RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.ExplicitCapture),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHDeprecated)
                            }
                    });
                    rs.Rules.Add(new HighlightingRule() //deprecated keywords
                    {
                        Regex =
                            RegexKeywordsHelper.GetRegexFromKeywords(new string[]
                                {"decl", "String", "Float", "functag", "funcenum"}),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHDeprecated)
                            }
                    });
                }
                rs.Rules.Add(new HighlightingRule() //preprocessor keywords
                {
                    //Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] { "#include", "#if", "#else", "#elif", "#endif", "#define", "#undef", "#pragma", "#endinput" }),
                    Regex =
                        new Regex(@"\#[a-zA-Z_][a-zA-Z0-9_]+",
                            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHPreProcessor)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //type-values keywords
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(new string[] {"sizeof", "true", "false", "null"}),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHTypesValues)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //main keywords
                {
                    Regex =
                        RegexKeywordsHelper.GetRegexFromKeywords(new string[]
                        {
                            "if", "else", "switch", "case", "default", "for", "while", "do", "break", "continue",
                            "return", "new", "view_as", "delete"
                        }),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHKeywords)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //context keywords
                {
                    Regex =
                        RegexKeywordsHelper.GetRegexFromKeywords(new string[]
                        {
                            "stock", "normal", "native", "public", "static", "const", "methodmap", "enum", "forward",
                            "function", "struct", "property", "get", "set", "typeset", "typedef", "this"
                        }),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHContextKeywords)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //value types
                {
                    Regex =
                        RegexKeywordsHelper.GetRegexFromKeywords(new string[]
                            {"bool", "char", "float", "int", "void", "any", "Handle"}),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHTypes)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //char type
                {
                    Regex = new Regex(@"'\\?.?'", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHChars)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //numbers
                {
                    Regex =
                        new Regex(
                            @"\b0[x][0-9a-fA-F]+|\b0[b][01]+|\b0[o][0-7]+|([+-]?\b[0-9]+(\.[0-9]+)?|\.[0-9]+)([eE][+-]?[0-9]+)?",
                            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHNumbers)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //special characters
                {
                    Regex =
                        new Regex(@"[?.;()\[\]{}+\-/%*&<>^+~!|&]+",
                            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHSpecialCharacters)
                        }
                });
                rs.Rules.Add(new HighlightingRule() //std includes - string color!
                {
                    Regex =
                        new Regex(@"\s[<][\w\\/\-]+(\.[\w\-]+)?[>]",
                            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() {Foreground = stringBrush}
                });
                var def = Program.Configs[Program.SelectedConfig].GetSMDef();
                if (def.TypeStrings.Length > 0)
                    rs.Rules.Add(new HighlightingRule() //types
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.TypeStrings, true),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHTypes)
                            }
                    });
                if (def.ConstantsStrings.Length > 0)
                    rs.Rules.Add(new HighlightingRule() //constants
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.ConstantsStrings, true),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHConstants)
                            }
                    });
                if (def.FunctionStrings.Length > 0)
                    rs.Rules.Add(new HighlightingRule() //Functions
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.FunctionStrings, true),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHFunctions)
                            }
                    });
                if (def.MethodsStrings.Length > 0)
                    rs.Rules.Add(new HighlightingRule() //Methods
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.MethodsStrings, true),
                        Color =
                            new HighlightingColor()
                            {
                                Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHMethods)
                            }
                    });
                rs.Rules.Add(new HighlightingRule() //unknown function calls
                {
                    Regex = new Regex(@"\b\w+(?=\s*\()", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color =
                        new HighlightingColor()
                        {
                            Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SHUnkownFunctions)
                        }
                });

                rs.Name = "MainRule";
                return rs;
            }
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            return null;
        }

        public HighlightingColor GetNamedColor(string name)
        {
            return null;
        }

        public IEnumerable<HighlightingColor> NamedHighlightingColors { get; set; }

        public IDictionary<string, string> Properties
        {
            get
            {
                var propertiesDictionary = new Dictionary<string, string>();
                propertiesDictionary.Add("DocCommentMarker", "///");
                return propertiesDictionary;
            }
        }
    }

    [Serializable]
    public sealed class SimpleHighlightingBrush : HighlightingBrush, ISerializable
    {
        private readonly SolidColorBrush _brush;

        internal SimpleHighlightingBrush(SolidColorBrush brush)
        {
            brush.Freeze();
            _brush = brush;
        }

        public SimpleHighlightingBrush(Color color) : this(new SolidColorBrush(color)) { }

        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            return _brush;
        }

        public override string ToString()
        {
            return _brush.ToString();
        }

        private SimpleHighlightingBrush(SerializationInfo info, StreamingContext context)
        {
            var convertFromString = ColorConverter.ConvertFromString(info.GetString("color"));

            if (convertFromString != null)
                _brush = new SolidColorBrush((Color)convertFromString);

            _brush.Freeze();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("color", _brush.Color.ToString(CultureInfo.InvariantCulture));
        }

        public override bool Equals(object obj)
        {
            var other = obj as SimpleHighlightingBrush;
            return other != null && _brush.Color.Equals(other._brush.Color);
        }

        public override int GetHashCode()
        {
            return _brush.Color.GetHashCode();
        }
    }

    public static class RegexKeywordsHelper
    {
        public static Regex GetRegexFromKeywords(string[] keywords, bool forceAtomicRegex = false)
        {
            if (forceAtomicRegex)
                keywords = ConvertToAtomicRegexAbleStringArray(keywords);

            if (keywords.Length == 0)
                return new Regex("SPEdit_Error"); //hehe 

            var useAtomicRegex = keywords.All(t => char.IsLetterOrDigit(t[0]) && char.IsLetterOrDigit(t[t.Length - 1]));

            var regexBuilder = new StringBuilder();

            regexBuilder.Append(useAtomicRegex ? @"\b(?>" : @"(");

            var orderedKeyWords = new List<string>(keywords);
            var i = 0;

            foreach (var keyword in orderedKeyWords.OrderByDescending(w => w.Length))
            {
                if (i++ > 0)
                    regexBuilder.Append('|');

                if (useAtomicRegex)
                    regexBuilder.Append(Regex.Escape(keyword));
                else
                {
                    if (char.IsLetterOrDigit(keyword[0]))
                        regexBuilder.Append(@"\b");

                    regexBuilder.Append(Regex.Escape(keyword));

                    if (char.IsLetterOrDigit(keyword[keyword.Length - 1]))
                        regexBuilder.Append(@"\b");
                }
            }

            regexBuilder.Append(useAtomicRegex ? @")\b" : @")");

            return new Regex(regexBuilder.ToString(), RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        }

        public static string[] ConvertToAtomicRegexAbleStringArray(string[] keywords)
        {
            return keywords.Where(t => t.Length > 0).Where(t => char.IsLetterOrDigit(t[0]) && char.IsLetterOrDigit(t[t.Length - 1])).ToArray();
        }
    }
}
