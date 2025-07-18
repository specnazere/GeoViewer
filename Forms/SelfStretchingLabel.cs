using System.ComponentModel;
using System.Text;

namespace LayerSlicer.Forms
{
    internal class SelfStretchingLabel : Label
    {
        // Межстрочный отступ в пикселях (можно задавать извне)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParagraphSpacing { get; set; } = GlobalConsts.LayerStepWidth;

        public SelfStretchingLabel() : base()
        {
            AutoSize = false; // Отключаем авторазмер, чтобы управлять вручную
            TextChanged += (s, e) => RecalibrateHeight();
            FontChanged += (s, e) => RecalibrateHeight();
            SizeChanged += (s, e) => RecalibrateHeight();
        }

        private void RecalibrateHeight()
        {
            if (IsDisposed || string.IsNullOrEmpty(Text)) return;

            using (var g = CreateGraphics())
            {
                // 1. Разделяем текст на абзацы
                var paragraphs = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var totalHeight = 0f;

                // 2. Рассчитываем высоту для каждого абзаца
                foreach (var paragraph in paragraphs)
                {
                    if (string.IsNullOrEmpty(paragraph)) continue;

                    // Измеряем высоту текущего абзаца (с учётом переноса слов)
                    var lines = GetWrappedLines(g, paragraph, Width);
                    totalHeight += lines.Count * Font.Height + Math.Max(0, lines.Count - 1);// * LineSpacing;

                    // Добавляем отступ после абзаца (кроме последнего)
                    if (paragraph != paragraphs.Last())
                        totalHeight += ParagraphSpacing;
                }

                Height = (int)Math.Ceiling(totalHeight);
            }
        }

        private List<string> GetWrappedLines(Graphics g, string text, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();

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
            base.OnPaintBackground(e); // Очищаем фон

            float yPos = 0;
            var paragraphs = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrEmpty(paragraph)) continue;

                var lines = GetWrappedLines(e.Graphics, paragraph, Width - Padding.Horizontal);
                foreach (var line in lines)
                {
                    e.Graphics.DrawString(
                        line,
                        Font,
                        new SolidBrush(ForeColor),
                        new RectangleF(Padding.Left, yPos, Width - Padding.Horizontal, Font.Height)
                    );
                    yPos += Font.Height;// + (line != lines.Last() ? 0 : 0);
                }

                // Добавляем отступ после абзаца (кроме последнего)
                if (paragraph != paragraphs.Last())
                    yPos += ParagraphSpacing;
            }
        }
    }
}
