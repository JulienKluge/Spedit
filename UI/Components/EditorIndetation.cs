using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;

namespace Spedit.UI.Components
{
    public class EditorIndetationStrategy : IIndentationStrategy
    {
        public void IndentLine(TextDocument document, DocumentLine line)
        {
            if (document == null || line == null)
                return;

            var previousLine = line.PreviousLine;

            if (previousLine == null)
                return;

            var indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
            var indentation = document.GetText(indentationSegment);

            if (Program.OptionsObject.EditorAgressiveIndentation)
            {
                var currentLineTextTrimmed = (document.GetText(line)).Trim();
                var lastLineTextTrimmed = (document.GetText(previousLine)).Trim();
                var currentLineFirstNonWhitespaceChar = ' ';

                if (currentLineTextTrimmed.Length > 0)
                    currentLineFirstNonWhitespaceChar = currentLineTextTrimmed[0];

                var lastLineLastNonWhitespaceChar = ' ';

                if (lastLineTextTrimmed.Length > 0)
                    lastLineLastNonWhitespaceChar = lastLineTextTrimmed[lastLineTextTrimmed.Length - 1];

                if (lastLineLastNonWhitespaceChar == '{' && currentLineFirstNonWhitespaceChar != '}')
                    indentation += "\t";

                else if (currentLineFirstNonWhitespaceChar == '}')
                    indentation = indentation.Length > 0 ? indentation.Substring(0, indentation.Length - 1) : string.Empty;
            }

            indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
            document.Replace(indentationSegment, indentation);
        }

        public void IndentLines(TextDocument document, int beginLine, int endLine)
        {

        }
    }
}
