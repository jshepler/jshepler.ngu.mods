using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace jshepler.ngu.mods
{
    public class TextTable
    {
        public enum TextAlign
        {
            Left,
            Right,
            Center
        }

        public class Cell
        {
            public string Text { get; set; }
            public int ColSpan { get; set; } = 1;
            public TextAlign? Alignment { get; set; } = null; // null = use default column alignment

            public Cell(string text, int colSpan = 1, TextAlign? alignment = null)
            {
                Text = text;
                ColSpan = colSpan;
                Alignment = alignment;
            }
        }

        private readonly List<List<Cell>> _rows = new();
        private readonly int _spacing;
        private readonly char _separatorChar;
        private TextAlign[] _alignments = [];
        private TextAlign _defaultAlignment = TextAlign.Left;

        public TextTable(int spacing = 2, char separatorChar = '-', TextAlign defaultAlignment = TextAlign.Left)
        {
            _spacing = spacing;
            _separatorChar = separatorChar;
            _defaultAlignment = defaultAlignment;
        }

        public void SetAlignments(params TextAlign[] alignments)
        {
            _alignments = alignments;
        }

        public void AddRow(params string[] cols)
        {
            var cells = cols.Select(s => new Cell(s)).ToList();
            AddRow(cells);
        }

        public void AddRow(List<Cell> cells)
        {
            _rows.Add(cells);
        }

        public void AddRow(params Cell[] cells)
        {
            _rows.Add(cells.ToList());
        }

        public void AddSeparatorRow()
        {
            _rows.Add(null); // null = separator
        }

        public override string ToString()
        {
            if (_rows.Count == 0)
                return string.Empty;

            int[] widths = ComputeColumnWidths();
            int cols = widths.Length;
            var sb = new StringBuilder();
            string colSpace = new string(' ', _spacing);

            foreach (var row in _rows)
            {
                // separator row
                if (row == null)
                {
                    for (int i = 0; i < cols; i++)
                    {
                        sb.Append(new string(_separatorChar, widths[i]));
                        if (i < cols - 1)
                            sb.Append(colSpace);
                    }

                    sb.AppendLine();
                    continue;
                }

                int colIndex = 0;
                for(var i = 0; i < row.Count; i++)
                {
                    var cell = row[i];
                    int cellWidth = 0;
                    for (int j = colIndex; j < colIndex + cell.ColSpan; j++)
                    {
                        cellWidth += widths[j];
                        if (j < colIndex + cell.ColSpan - 1)
                            cellWidth += _spacing;
                    }

                    TextAlign align = cell.Alignment ?? GetColumnAlignment(colIndex);
                    sb.Append(Align(cell.Text, cellWidth, align));

                    if (i < row.Count - 1)
                        sb.Append(new string(' ', _spacing));

                    colIndex += cell.ColSpan;
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private int[] ComputeColumnWidths()
        {
            if (_rows.Count == 0)
                return Array.Empty<int>();

            int cols = _rows.Max(r => r?.Sum(c => c.ColSpan) ?? 0);
            int[] widths = new int[cols];

            // First pass: single-column cells
            for (int c = 0; c < cols; c++)
            {
                int maxWidth = 0;
                foreach (var row in _rows)
                {
                    if (row == null)
                        continue;

                    int colIndex = 0;
                    foreach (var cell in row)
                    {
                        if (cell.ColSpan == 1 && colIndex == c)
                        {
                            int len = StripRichTextTags(cell.Text).Length;
                            maxWidth = Math.Max(maxWidth, len);
                        }

                        colIndex += cell.ColSpan;
                    }
                }

                widths[c] = maxWidth;
            }

            // Second pass: multi-column cells
            foreach (var row in _rows)
            {
                if (row == null) continue;
                int colIndex = 0;
                foreach (var cell in row)
                {
                    if (cell.ColSpan <= 1)
                    {
                        colIndex += cell.ColSpan;
                        continue;
                    }

                    int totalWidth = 0;
                    for (int i = colIndex; i < colIndex + cell.ColSpan; i++)
                    {
                        totalWidth += widths[i];
                        if (i < colIndex + cell.ColSpan - 1) totalWidth += _spacing;
                    }

                    int visibleLength = StripRichTextTags(cell.Text).Length;
                    if (visibleLength > totalWidth)
                    {
                        int extra = visibleLength - totalWidth;
                        int perCol = extra / cell.ColSpan;
                        int remainder = extra % cell.ColSpan;

                        for (int i = colIndex; i < colIndex + cell.ColSpan; i++)
                        {
                            widths[i] += perCol + (i - colIndex < remainder ? 1 : 0);
                        }
                    }

                    colIndex += cell.ColSpan;
                }
            }

            return widths;
        }

        private TextAlign GetColumnAlignment(int column)
        {
            if (column < _alignments.Length)
                return _alignments[column];

            return _defaultAlignment;
        }

        private static string Align(string text, int width, TextAlign align)
        {
            string stripped = StripRichTextTags(text);
            int visibleLength = stripped.Length;
            int padding = width - visibleLength;
            if (padding <= 0)
                return text;

            // can't use padleft or padright as those would count the rich text tags,
            // so we have to manually add the right number of spaces before/after the text
            return align switch
            {
                TextAlign.Left => text + new string(' ', padding),
                TextAlign.Right => new string(' ', padding) + text,
                TextAlign.Center => new string(' ', padding / 2) + text + new string(' ', padding - padding / 2),
                _ => text
            };
        }

        private static readonly Regex RichTextTagRegex = new Regex("<.*?>", RegexOptions.Compiled);

        private static string StripRichTextTags(string s)
        {
            return string.IsNullOrEmpty(s) ? s : RichTextTagRegex.Replace(s, "");
        }
    }
}
