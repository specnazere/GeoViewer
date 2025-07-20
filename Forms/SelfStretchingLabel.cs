using System.ComponentModel;
using System.Text;

namespace LayerSlicer.Forms
{
    internal class SelfStretchingLabel : Label
    {
        public int ParagraphSpacing = GlobalConsts.LayerStepWidth;

        public SelfStretchingLabel() : base()
        {
            // Base setup
            AutoSize = false;

            // Handling events that can change size of inner text
            TextChanged += (s, e) => RecalibrateHeight();
            FontChanged += (s, e) => RecalibrateHeight();
            SizeChanged += (s, e) => RecalibrateHeight();
        }

        private void RecalibrateHeight()
        {
            if (IsDisposed || string.IsNullOrEmpty(Text)) return;

            using (var g = CreateGraphics())
            {
                // Splitting by paragraphs
                var paragraphs = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var totalHeight = 0f;

                // Height for every paragraph
                foreach (var paragraph in paragraphs)
                {
                    if (string.IsNullOrEmpty(paragraph)) continue;

                    // Paragraph height
                    var lines = GetWrappedLines(g, paragraph, Width);
                    totalHeight += lines.Count * Font.Height + Math.Max(0, lines.Count - 1);

                    // Adding inter-paragraph spacing
                    if (paragraph != paragraphs.Last())
                        totalHeight += ParagraphSpacing;
                }

                Height = (int)Math.Ceiling(totalHeight);
            }
        }

        private List<string> GetWrappedLines(Graphics g, string text, float maxWidth)
        {
            // Splitting
            var lines = new List<string>();
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();

            // Splitting paragraph for lines with width less than width of label
            foreach (var word in words)
            {
                var testLine = currentLine.Length > 0 ? $"{currentLine} {word}" : word;
                if (g.MeasureString(testLine, Font).Width <= maxWidth)
                {
                    currentLine.Append(currentLine.Length > 0 ? " " + word : word);
                }
                else
                {
                    if (currentLine.Length > 0) lines.Add(currentLine.ToString());
                    currentLine.Clear().Append(word);
                }
            }

            if (currentLine.Length > 0) lines.Add(currentLine.ToString());
            return lines;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Setting text before drawing
            float yPos = 0;
            var paragraphs = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Drawing every paragraph
            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph)) continue;

                // Draw separate lines
                var lines = GetWrappedLines(e.Graphics, paragraph, Width - Padding.Horizontal);
                foreach (var line in lines)
                {
                    e.Graphics.DrawString(
                        line,
                        Font,
                        new SolidBrush(ForeColor),
                        new RectangleF(Padding.Left, yPos, Width - Padding.Horizontal, Font.Height)
                    );
                    yPos += Font.Height;
                }

                // Добавляем отступ после абзаца (кроме последнего)
                if (paragraph != paragraphs.Last())
                    yPos += ParagraphSpacing;
            }
        }
    }
}
