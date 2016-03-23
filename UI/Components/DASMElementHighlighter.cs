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
    public class DASMHighlighting : IHighlightingDefinition
    {
        public string Name { get { return "SM"; } }

        public HighlightingRuleSet MainRuleSet
        {
            get
            {
                HighlightingRuleSet commentMarkerSet = new HighlightingRuleSet();
                commentMarkerSet.Name = "CommentMarkerSet";
                HighlightingRuleSet excludeInnerSingleLineComment = new HighlightingRuleSet();
                excludeInnerSingleLineComment.Spans.Add(new HighlightingSpan() { StartExpression = new Regex(@"\;"), EndExpression = new Regex(@".") });
                HighlightingRuleSet rs = new HighlightingRuleSet();
                SimpleHighlightingBrush commentBrush = new SimpleHighlightingBrush(Program.OptionsObject.SH_Comments);
                rs.Spans.Add(new HighlightingSpan() //singleline comments
                {
                    StartExpression = new Regex(@"\;", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    EndExpression = new Regex(@"$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
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
                rs.Rules.Add(new HighlightingRule() //opcodes
                {
                    Regex = RegexKeywordsHelper.GetRegexFromKeywords(opcodestrings, true),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_CommentsMarker) }
                });
                rs.Rules.Add(new HighlightingRule() //hexnumbers
                {
                    Regex = new Regex(@"\b0[xX][0-9a-fA-F]+", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Program.OptionsObject.SH_Numbers) }
                });
                var def = Program.Configs[Program.SelectedConfig].GetSMDef();
                if (def.TypeStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //Types
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.TypeStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Colors.Gray) }
                    });
                }
                if (def.ConstantsStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //constants
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.ConstantsStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Colors.Gray) }
                    });
                }
                if (def.FunctionStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //Functions
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.FunctionStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Colors.Gray) }
                    });
                }
                if (def.MethodsStrings.Length > 0)
                {
                    rs.Rules.Add(new HighlightingRule() //Methods
                    {
                        Regex = RegexKeywordsHelper.GetRegexFromKeywords(def.MethodsStrings, true),
                        Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(Colors.Gray) }
                    });
                }
                rs.Name = "MainRule";
                return rs;
            }
        }

        public static string[] opcodestrings = new string[] { "none", "load.pri", "load.alt", "load.s.pri", "load.s.alt", "lref.pri", "lref.alt", "lref.s.pri",
            "lref.s.alt", "load.i", "lodb.i", "const.pri", "const.alt", "addr.pri", "addr.alt", "stor.pri", "stor.alt", "stor.s.pri", "stor.s.alt", "sref.pri",
            "sref.alt", "sref.s.pri", "sref.s.alt", "stor.i", "strb.i", "lidx", "lidx.b", "idxaddr", "idxaddr.b", "align.pri", "align.alt", "lctrl", "sctrl",
            "move.pri", "move.alt", "xchg", "push.pri", "push.alt", "push.r", "push.c", "push", "push.s", "pop.pri", "pop.alt", "stack", "heap", "proc", "ret",
            "retn", "call", "call.pri", "jump", "jrel", "jzer", "jnz", "jeq", "jneq", "jsless", "jleq", "jgrtr", "jgeq", "jsless", "jsleq", "jsgrtr", "jsgeq",
            "shl", "shr", "sshr", "shl.c.pri", "shl.c.alt", "shr.c.pri", "shr.c.alt", "smul", "sdiv", "sdiv.alt", "umul", "udiv", "udiv.alt", "add", "sub",
            "sub.alt", "and", "or", "xor", "not", "neg", "invert", "add.c", "smul.c", "zero.pri", "zero.alt", "zero", "zero.s", "sign.pri", "sign.alt", "eq",
            "neq", "less", "leq", "grtr", "geq", "sless", "sleq", "sgrtr", "sgeq", "eq.c.pri", "eq.c.alt", "inc.pri", "inc.alt", "inc", "inc.s", "inc.i", "dec.pri",
            "dec.alt", "dec", "dec.s", "dec.i", "movs", "cmps", "fill", "halt", "bounds", "sysreq.pri", "sysreq.c", "file", "line", "symbol", "srange", "jump.pri",
            "switch", "casetbl", "swap.pri", "swap.alt", "push.adr", "nop", "sysreq.n", "symtag", "break", "push2.c", "push2", "push2.s", "push2.adr", "push3.c",
            "push3", "push3.s", "push3.adr", "push4.c", "push4", "push4.s", "push4.adr", "push5.c", "push5", "push5.s", "push5.adr", "load.both", "load.s.both",
            "const", "const.s", "sysreq.d", "sysreq.nd", "trk.push.c", "trk.pop", "genarray", "genarray.z", "stradjust.pri", "stackadjust", "endproc", "ldgfn.pri",
            "fabs", "float", "float.add", "float.sub", "float.mul", "float.div", "round", "floor", "ceil", "rndtozero", "float.cmp", "float.gt", "float.ge",
            "float.lt", "float.le", "float.ne", "float.eq", "float.not" };

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
}
