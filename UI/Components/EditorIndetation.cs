using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;

namespace Spedit.UI.Components
{
    public class EditorIndetationStrategy : IIndentationStrategy
    {
        public void IndentLine(TextDocument document, DocumentLine line)
        {
            if (document == null || line == null)
            {
                return;
            }
            DocumentLine previousLine = line.PreviousLine;
            if (previousLine != null)
            {
                ISegment indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
                string indentation = document.GetText(indentationSegment);
                if (Program.OptionsObject.Editor_AgressiveIndentation)
                {
                    string currentLineTextTrimmed = (document.GetText(line)).Trim();
                    string lastLineTextTrimmed = (document.GetText(previousLine)).Trim();
                    if (lastLineTextTrimmed == "{" && currentLineTextTrimmed != "}")
                    {
                        indentation += "\t";
                    }
                    else if (currentLineTextTrimmed == "}")
                    {
                        if (indentation.Length > 0)
                        {
                            indentation = indentation.Substring(0, indentation.Length - 1);
                        }
                        else
                        {
                            indentation = string.Empty;
                        }
                    }
                }
                indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
                document.Replace(indentationSegment, indentation);
            }
        }

        public void IndentLines(TextDocument document, int beginLine, int endLine)
        { }
    }
}
