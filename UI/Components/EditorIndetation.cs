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
                    string lastLineTextTrimmed = (document.GetText(previousLine)).Trim();
                    if (lastLineTextTrimmed == "{")
                    {
                        indentation += "\t";
                    }
                    else if (lastLineTextTrimmed == "}")
                    {
                        if (indentation.Length > 0)
                        {
                            indentation = indentation.Substring(0, indentation.Length - 1);
                        }
                        else
                        {
                            indentation = string.Empty;
                        }
                        indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
                        document.Replace(indentationSegment, indentation);
                        return;
                    }
                }
                indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
                document.Replace(indentationSegment, indentation);
            }
        }

        public void IndentLines(TextDocument document, int beginLine, int endLine) { }
    }

    /*public class TabHighlighter : VisualLineElementGenerator
    {
        protected ITextRunConstructionContext CurrentContext { get; private set; }

        public virtual void StartGeneration(ITextRunConstructionContext context)
        {
            if (context == null)
            { return; }
            this.CurrentContext = context;
        }

        public virtual void FinishGeneration()
        {
            this.CurrentContext = null;
        }

        internal int cachedInterest;

        public int GetFirstInterestedOffset(int startOffset)
        {
            DocumentLine line = CurrentContext.VisualLine.LastDocumentLine;
            string str = CurrentContext.GetText(startOffset, line.EndOffset - startOffset).Text;
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                if (str[i] == '\t')
                {
                    return i + startOffset;
                }
            }
            return -1;
        }

        public VisualLineElement ConstructElement(int offset)
        {
            return null;
        }
    }*/
}
