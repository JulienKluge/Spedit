using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace Spedit.UI.Components
{
    public class BracketHighlightRenderer : IBackgroundRenderer
    {
        BracketSearchResult result;
        Brush backgroundBrush = Brushes.LightGray;
        TextView textView;

        public void SetHighlight(BracketSearchResult result)
        {
            if (this.result != result)
            {
                this.result = result;
                textView.InvalidateLayer(this.Layer);
            }
        }

        public BracketHighlightRenderer(TextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException("textView");

            this.textView = textView;

            this.textView.BackgroundRenderers.Add(this);
        }

        public KnownLayer Layer
        {
            get
            {
                return KnownLayer.Selection;
            }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (this.result == null)
                return;

            BackgroundGeometryBuilder builder = new BackgroundGeometryBuilder();

            builder.CornerRadius = 1;
            builder.AlignToMiddleOfPixels = true;

            builder.AddSegment(textView, new TextSegment() { StartOffset = result.OpeningBracketOffset, Length = result.OpeningBracketLength });
            builder.CloseFigure();
            builder.AddSegment(textView, new TextSegment() { StartOffset = result.ClosingBracketOffset, Length = result.ClosingBracketLength });

            Geometry geometry = builder.CreateGeometry();
            if (geometry != null)
            {
                drawingContext.DrawGeometry(backgroundBrush, null, geometry);
            }
        }
    }

    public interface IBracketSearcher
    {
        BracketSearchResult SearchBracket(IDocument document, int offset);
    }
    public class BracketSearchResult
    {
        public int OpeningBracketOffset { get; private set; }

        public int OpeningBracketLength { get; private set; }

        public int ClosingBracketOffset { get; private set; }

        public int ClosingBracketLength { get; private set; }

        public int DefinitionHeaderOffset { get; set; }

        public int DefinitionHeaderLength { get; set; }

        public BracketSearchResult(int openingBracketOffset, int openingBracketLength,
                                   int closingBracketOffset, int closingBracketLength)
        {
            this.OpeningBracketOffset = openingBracketOffset;
            this.OpeningBracketLength = openingBracketLength;
            this.ClosingBracketOffset = closingBracketOffset;
            this.ClosingBracketLength = closingBracketLength;
        }
    }

    public class SPBracketSearcher : IBracketSearcher
    {
        string openingBrackets = "([{";
        string closingBrackets = ")]}";

        public BracketSearchResult SearchBracket(IDocument document, int offset)
        {
            if (offset > 0)
            {
                char c = document.GetCharAt(offset - 1);
                int index = openingBrackets.IndexOf(c);
                int otherOffset = -1;
                if (index > -1)
                    otherOffset = SearchBracketForward(document, offset, openingBrackets[index], closingBrackets[index]);

                index = closingBrackets.IndexOf(c);
                if (index > -1)
                    otherOffset = SearchBracketBackward(document, offset - 2, openingBrackets[index], closingBrackets[index]);

                if (otherOffset > -1)
                {
                    var result = new BracketSearchResult(Math.Min(offset - 1, otherOffset), 1,
                                                         Math.Max(offset - 1, otherOffset), 1);
                    SearchDefinition(document, result);
                    return result;
                }
            }

            return null;
        }

        void SearchDefinition(IDocument document, BracketSearchResult result)
        {
            if (document.GetCharAt(result.OpeningBracketOffset) != '{')
                return;
            var documentLine = document.GetLineByOffset(result.OpeningBracketOffset);
            while (documentLine != null && IsBracketOnly(document, documentLine))
                documentLine = documentLine.PreviousLine;
            if (documentLine != null)
            {
                result.DefinitionHeaderOffset = documentLine.Offset;
                result.DefinitionHeaderLength = documentLine.Length;
            }
        }

        bool IsBracketOnly(IDocument document, IDocumentLine documentLine)
        {
            string lineText = document.GetText(documentLine).Trim();
            return lineText == "{" || string.IsNullOrEmpty(lineText)
                || lineText.StartsWith("//", StringComparison.Ordinal)
                || lineText.StartsWith("/*", StringComparison.Ordinal)
                || lineText.StartsWith("*", StringComparison.Ordinal)
                || lineText.StartsWith("'", StringComparison.Ordinal);
        }

        #region SearchBracket helper functions
        static int ScanLineStart(IDocument document, int offset)
        {
            for (int i = offset - 1; i > 0; --i)
            {
                if (document.GetCharAt(i) == '\n')
                    return i + 1;
            }
            return 0;
        }

        static int GetStartType(IDocument document, int linestart, int offset)
        {
            bool inString = false;
            bool inChar = false;
            bool verbatim = false;
            int result = 0;
            for (int i = linestart; i < offset; i++)
            {
                switch (document.GetCharAt(i))
                {
                    case '/':
                        if (!inString && !inChar && i + 1 < document.TextLength)
                        {
                            if (document.GetCharAt(i + 1) == '/')
                            {
                                result = 1;
                            }
                        }
                        break;
                    case '"':
                        if (!inChar)
                        {
                            if (inString && verbatim)
                            {
                                if (i + 1 < document.TextLength && document.GetCharAt(i + 1) == '"')
                                {
                                    ++i;
                                    inString = false;
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && i > 0 && document.GetCharAt(i - 1) == '@')
                            {
                                verbatim = true;
                            }
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!inString) inChar = !inChar;
                        break;
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++i;
                        break;
                }
            }

            return (inString || inChar) ? 2 : result;
        }
        #endregion

        #region SearchBracketBackward
        int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
        {
            if (offset + 1 >= document.TextLength) return -1;

            int quickResult = QuickSearchBracketBackward(document, offset, openBracket, closingBracket);
            if (quickResult >= 0) return quickResult;

            int linestart = ScanLineStart(document, offset + 1);

            int starttype = GetStartType(document, linestart, offset + 1);
            if (starttype == 1)
            {
                return -1;
            }
            Stack<int> bracketStack = new Stack<int>();
            bool blockComment = false;
            bool lineComment = false;
            bool inChar = false;
            bool inString = false;
            bool verbatim = false;

            for (int i = 0; i <= offset; ++i)
            {
                char ch = document.GetCharAt(i);
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        lineComment = false;
                        inChar = false;
                        if (!verbatim) inString = false;
                        break;
                    case '/':
                        if (blockComment)
                        {
                            if (document.GetCharAt(i - 1) == '*')
                            {
                                blockComment = false;
                            }
                        }
                        if (!inString && !inChar && i + 1 < document.TextLength)
                        {
                            if (!blockComment && document.GetCharAt(i + 1) == '/')
                            {
                                lineComment = true;
                            }
                            if (!lineComment && document.GetCharAt(i + 1) == '*')
                            {
                                blockComment = true;
                            }
                        }
                        break;
                    case '"':
                        if (!(inChar || lineComment || blockComment))
                        {
                            if (inString && verbatim)
                            {
                                if (i + 1 < document.TextLength && document.GetCharAt(i + 1) == '"')
                                {
                                    ++i; // skip escaped quote
                                    inString = false; // let the string go
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && offset > 0 && document.GetCharAt(i - 1) == '@')
                            {
                                verbatim = true;
                            }
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!(inString || lineComment || blockComment))
                        {
                            inChar = !inChar;
                        }
                        break;
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++i;
                        break;
                    default:
                        if (ch == openBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                bracketStack.Push(i);
                            }
                        }
                        else if (ch == closingBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                if (bracketStack.Count > 0)
                                    bracketStack.Pop();
                            }
                        }
                        break;
                }
            }
            if (bracketStack.Count > 0) return (int)bracketStack.Pop();
            return -1;
        }
        #endregion

        #region SearchBracketForward
        int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
        {
            bool inString = false;
            bool inChar = false;
            bool verbatim = false;

            bool lineComment = false;
            bool blockComment = false;

            if (offset < 0) return -1;

            int quickResult = QuickSearchBracketForward(document, offset, openBracket, closingBracket);
            if (quickResult >= 0) return quickResult;

            int linestart = ScanLineStart(document, offset);

            int starttype = GetStartType(document, linestart, offset);
            if (starttype != 0) return -1;

            int brackets = 1;

            while (offset < document.TextLength)
            {
                char ch = document.GetCharAt(offset);
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        lineComment = false;
                        inChar = false;
                        if (!verbatim) inString = false;
                        break;
                    case '/':
                        if (blockComment)
                        {
                            if (document.GetCharAt(offset - 1) == '*')
                            {
                                blockComment = false;
                            }
                        }
                        if (!inString && !inChar && offset + 1 < document.TextLength)
                        {
                            if (!blockComment && document.GetCharAt(offset + 1) == '/')
                            {
                                lineComment = true;
                            }
                            if (!lineComment && document.GetCharAt(offset + 1) == '*')
                            {
                                blockComment = true;
                            }
                        }
                        break;
                    case '"':
                        if (!(inChar || lineComment || blockComment))
                        {
                            if (inString && verbatim)
                            {
                                if (offset + 1 < document.TextLength && document.GetCharAt(offset + 1) == '"')
                                {
                                    ++offset; // skip escaped quote
                                    inString = false; // let the string go
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && offset > 0 && document.GetCharAt(offset - 1) == '@')
                            {
                                verbatim = true;
                            }
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!(inString || lineComment || blockComment))
                        {
                            inChar = !inChar;
                        }
                        break;
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++offset; // skip next character
                        break;
                    default:
                        if (ch == openBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                ++brackets;
                            }
                        }
                        else if (ch == closingBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                --brackets;
                                if (brackets == 0)
                                {
                                    return offset;
                                }
                            }
                        }
                        break;
                }
                ++offset;
            }
            return -1;
        }
        #endregion

        int QuickSearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
        {
            int brackets = -1;
            for (int i = offset; i >= 0; --i)
            {
                char ch = document.GetCharAt(i);
                if (ch == openBracket)
                {
                    ++brackets;
                    if (brackets == 0) return i;
                }
                else if (ch == closingBracket)
                {
                    --brackets;
                }
                else if (ch == '"')
                {
                    break;
                }
                else if (ch == '\'')
                {
                    break;
                }
                else if (ch == '/' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/') break;
                    if (document.GetCharAt(i - 1) == '*') break;
                }
            }
            return -1;
        }

        int QuickSearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
        {
            int brackets = 1;
            // try "quick find" - find the matching bracket if there is no string/comment in the way
            for (int i = offset; i < document.TextLength; ++i)
            {
                char ch = document.GetCharAt(i);
                if (ch == openBracket)
                {
                    ++brackets;
                }
                else if (ch == closingBracket)
                {
                    --brackets;
                    if (brackets == 0) return i;
                }
                else if (ch == '"')
                {
                    break;
                }
                else if (ch == '\'')
                {
                    break;
                }
                else if (ch == '/' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/') break;
                }
                else if (ch == '*' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/') break;
                }
            }
            return -1;
        }
    }
}
