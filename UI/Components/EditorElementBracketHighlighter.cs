using System;
using System.Collections.Generic;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace Spedit.UI.Components
{
    public class BracketHighlightRenderer : IBackgroundRenderer
    {
        private BracketSearchResult _result;
        private readonly Brush _backgroundBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x88, 0x88, 0x88));
        private readonly TextView _textView;

        public void SetHighlight(BracketSearchResult result)
        {
            if (_result == result)
                return;

            _result = result;
            _textView.InvalidateLayer(Layer);
        }

        public BracketHighlightRenderer(TextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException(nameof(textView));

            _textView = textView;
            _textView.BackgroundRenderers.Add(this);
        }

        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_result == null)
                return;

            var builder = new BackgroundGeometryBuilder
            {
                CornerRadius = 1,
                AlignToWholePixels = true,
                BorderThickness = 0.0
            };

            builder.AddSegment(textView, new TextSegment() { StartOffset = _result.OpeningBracketOffset, Length = _result.OpeningBracketLength });
            builder.CloseFigure();
            builder.AddSegment(textView, new TextSegment() { StartOffset = _result.ClosingBracketOffset, Length = _result.ClosingBracketLength });

            var geometry = builder.CreateGeometry();

            if (geometry != null)
                drawingContext.DrawGeometry(_backgroundBrush, null, geometry);
        }
    }

    public interface IBracketSearcher
    {
        BracketSearchResult SearchBracket(IDocument document, int offset);
    }

    public class BracketSearchResult
    {
        public int OpeningBracketOffset { get; }

        public int OpeningBracketLength { get; }

        public int ClosingBracketOffset { get; }

        public int ClosingBracketLength { get; }

        public int DefinitionHeaderOffset { get; set; }

        public int DefinitionHeaderLength { get; set; }

        public BracketSearchResult(int openingBracketOffset, int openingBracketLength,
                                   int closingBracketOffset, int closingBracketLength)
        {
            OpeningBracketOffset = openingBracketOffset;
            OpeningBracketLength = openingBracketLength;
            ClosingBracketOffset = closingBracketOffset;
            ClosingBracketLength = closingBracketLength;
        }
    }

    public class SPBracketSearcher : IBracketSearcher
    {
        private const string OpeningBrackets = "([{";
        private const string ClosingBrackets = ")]}";

        public BracketSearchResult SearchBracket(IDocument document, int offset)
        {
            if (offset <= 0)
                return null;

            var c = document.GetCharAt(offset - 1);
            var index = OpeningBrackets.IndexOf(c);
            var otherOffset = -1;

            if (index > -1)
                otherOffset = SearchBracketForward(document, offset, OpeningBrackets[index], ClosingBrackets[index]);

            index = ClosingBrackets.IndexOf(c);

            if (index > -1)
                otherOffset = SearchBracketBackward(document, offset - 2, OpeningBrackets[index], ClosingBrackets[index]);

            if (otherOffset <= -1)
                return null;

            var result = new BracketSearchResult(Math.Min(offset - 1, otherOffset), 1,
                Math.Max(offset - 1, otherOffset), 1);
            SearchDefinition(document, result);

            return result;
        }

        private void SearchDefinition(IDocument document, BracketSearchResult result)
        {
            if (document.GetCharAt(result.OpeningBracketOffset) != '{')
                return;

            var documentLine = document.GetLineByOffset(result.OpeningBracketOffset);

            while (documentLine != null && IsBracketOnly(document, documentLine))
                documentLine = documentLine.PreviousLine;

            if (documentLine == null)
                return;

            result.DefinitionHeaderOffset = documentLine.Offset;
            result.DefinitionHeaderLength = documentLine.Length;
        }

        private static bool IsBracketOnly(ITextSource document, ISegment documentLine)
        {
            var lineText = document.GetText(documentLine).Trim();

            return lineText == "{" || string.IsNullOrEmpty(lineText)
                || lineText.StartsWith("//", StringComparison.Ordinal)
                || lineText.StartsWith("/*", StringComparison.Ordinal)
                || lineText.StartsWith("*", StringComparison.Ordinal)
                || lineText.StartsWith("'", StringComparison.Ordinal);
        }

        #region SearchBracket helper functions

        private static int ScanLineStart(ITextSource document, int offset)
        {
            for (var i = offset - 1; i > 0; --i)
                if (document.GetCharAt(i) == '\n')
                    return i + 1;

            return 0;
        }

        private static int GetStartType(ITextSource document, int linestart, int offset)
        {
            var inString = false;
            var inChar = false;
            var verbatim = false;
            var result = 0;

            for (var i = linestart; i < offset; i++)
            {
                switch (document.GetCharAt(i))
                {
                    case '/':
                        if (!inString && !inChar && i + 1 < document.TextLength)
                            if (document.GetCharAt(i + 1) == '/')
                                result = 1;
                        break;
                    case '"':
                        if (!inChar)
                        {
                            if (inString && verbatim)
                                if (i + 1 < document.TextLength && document.GetCharAt(i + 1) == '"')
                                {
                                    ++i;
                                    inString = false;
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            else if (!inString && i > 0 && document.GetCharAt(i - 1) == '@')
                                verbatim = true;
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!inString) inChar = !inChar;
                        break;
                    case '\\':
                        if (inString && !verbatim || inChar)
                            ++i;
                        break;
                }
            }

            return (inString || inChar) ? 2 : result;
        }
        #endregion

        #region SearchBracketBackward

        private int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
        {
            if (offset + 1 >= document.TextLength)
                return -1;

            var quickResult = QuickSearchBracketBackward(document, offset, openBracket, closingBracket);

            if (quickResult >= 0)
                return quickResult;

            var linestart = ScanLineStart(document, offset + 1);
            var starttype = GetStartType(document, linestart, offset + 1);

            if (starttype == 1)
                return -1;

            var bracketStack = new Stack<int>();
            var blockComment = false;
            var lineComment = false;
            var inChar = false;
            var inString = false;
            var verbatim = false;

            for (var i = 0; i <= offset; ++i)
            {
                var ch = document.GetCharAt(i);

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
                            if (document.GetCharAt(i - 1) == '*')
                                blockComment = false;
                        if (!inString && !inChar && i + 1 < document.TextLength)
                        {
                            if (!blockComment && document.GetCharAt(i + 1) == '/')
                                lineComment = true;
                            if (!lineComment && document.GetCharAt(i + 1) == '*')
                                blockComment = true;
                        }
                        break;
                    case '"':
                        if (!(inChar || lineComment || blockComment))
                        {
                            if (inString && verbatim)
                                if (i + 1 < document.TextLength && document.GetCharAt(i + 1) == '"')
                                {
                                    ++i; // skip escaped quote
                                    inString = false; // let the string go
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            else if (i > 0) //FIX CRASH ON SELECTING
                                if (!inString && offset > 0 && document.GetCharAt(i - 1) == '@')
                                    verbatim = true;
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!(inString || lineComment || blockComment))
                            inChar = !inChar;
                        break;
                    case '\\':
                        if (inString && !verbatim || inChar)
                            ++i;
                        break;
                    default:
                        if (ch == openBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                                bracketStack.Push(i);
                        }
                        else if (ch == closingBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                                if (bracketStack.Count > 0)
                                    bracketStack.Pop();
                        }
                        break;
                }
            }

            if (bracketStack.Count > 0)
                return bracketStack.Pop();

            return -1;
        }
        #endregion

        #region SearchBracketForward

        private static int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
        {
            var inString = false;
            var inChar = false;
            var verbatim = false;
            var lineComment = false;
            var blockComment = false;
            var brackets = 1;

            if (offset < 0)
                return -1;

            var quickResult = QuickSearchBracketForward(document, offset, openBracket, closingBracket);

            if (quickResult >= 0)
                return quickResult;

            var linestart = ScanLineStart(document, offset);
            var starttype = GetStartType(document, linestart, offset);

            if (starttype != 0)
                return -1;

            while (offset < document.TextLength)
            {
                var ch = document.GetCharAt(offset);

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
                            if (document.GetCharAt(offset - 1) == '*')
                                blockComment = false;
                        if (!inString && !inChar && offset + 1 < document.TextLength)
                        {
                            if (!blockComment && document.GetCharAt(offset + 1) == '/')
                                lineComment = true;
                            if (!lineComment && document.GetCharAt(offset + 1) == '*')
                                blockComment = true;
                        }
                        break;
                    case '"':
                        if (!(inChar || lineComment || blockComment))
                        {
                            if (inString && verbatim)
                                if (offset + 1 < document.TextLength && document.GetCharAt(offset + 1) == '"')
                                {
                                    ++offset; // skip escaped quote
                                    inString = false; // let the string go
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            else if (!inString && offset > 0 && document.GetCharAt(offset - 1) == '@')
                                verbatim = true;
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!(inString || lineComment || blockComment))
                            inChar = !inChar;
                        break;
                    case '\\':
                        if (inString && !verbatim || inChar)
                            ++offset; // skip next character
                        break;
                    default:
                        if (ch == openBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                                ++brackets;
                        }
                        else if (ch == closingBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                --brackets;
                                if (brackets == 0)
                                    return offset;
                            }
                        }
                        break;
                }
                ++offset;
            }
            return -1;
        }
        #endregion

        private static int QuickSearchBracketBackward(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            var brackets = -1;

            for (var i = offset; i >= 0; --i)
            {
                var ch = document.GetCharAt(i);

                if (ch == openBracket)
                {
                    ++brackets;
                    if (brackets == 0) return i;
                }
                else if (ch == closingBracket)
                    --brackets;
                else if (ch == '"')
                    break;
                else if (ch == '\'')
                    break;
                else if (ch == '/' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/')
                        break;
                    if (document.GetCharAt(i - 1) == '*')
                        break;
                }
            }
            return -1;
        }

        private static int QuickSearchBracketForward(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            var brackets = 1;
            // try "quick find" - find the matching bracket if there is no string/comment in the way

            for (var i = offset; i < document.TextLength; ++i)
            {
                var ch = document.GetCharAt(i);

                if (ch == openBracket)
                    ++brackets;
                else if (ch == closingBracket)
                {
                    --brackets;

                    if (brackets == 0)
                        return i;
                }
                else if (ch == '"')
                    break;
                else if (ch == '\'')
                    break;
                else if (ch == '/' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/')
                        break;
                }
                else if (ch == '*' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/')
                        break;
                }
            }
            return -1;
        }
    }
}
