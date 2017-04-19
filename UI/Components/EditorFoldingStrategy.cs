using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;

namespace Spedit.UI.Components
{
    public class SPFoldingStrategy
    {
        public char OpeningBrace { get; set; }
        public char ClosingBrace { get; set; }

        public SPFoldingStrategy()
		{
			OpeningBrace = '{';
			ClosingBrace = '}';
		}

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            var newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(ITextSource document)
        {
            var newFoldings = new List<NewFolding>();

            var startOffsets = new Stack<int>();
            var lastNewLineOffset = 0;
            var inCommentMode = false;

            for (var i = 0; i < document.TextLength; ++i)
            {
                var c = document.GetCharAt(i);

                if (c == '\n' || c == '\r')
                    lastNewLineOffset = i + 1;

                else if (inCommentMode)
                {
                    if (c != '/')
                        continue;

                    if (i <= 0)
                        continue;

                    if (document.GetCharAt(i - 1) != '*')
                        continue;

                    var startOffset = startOffsets.Pop();
                    inCommentMode = false;

                    if (startOffset < lastNewLineOffset)
                        newFoldings.Add(new NewFolding(startOffset, i + 1));
                }
                else switch (c)
                {
                    case '/':
                        if ((i + 1) < document.TextLength)
                            if (document.GetCharAt(i + 1) == '*')
                            {
                                inCommentMode = true;
                                startOffsets.Push(i);
                            }
                        break;
                    case '{':
                        startOffsets.Push(i);
                        break;
                    default:
                        if (c == '}' && startOffsets.Count > 0)
                        {
                            var startOffset = startOffsets.Pop();

                            if (startOffset < lastNewLineOffset)
                                newFoldings.Add(new NewFolding(startOffset, i + 1));
                        }
                        break;
                }
            }

            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}
