using System.ComponentModel;

namespace LayerSlicer.Forms
{
    internal class SliderGroup : UserControl
    {
        public readonly SliderWrap[] SliderWraps = new SliderWrap[3];

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Minimum
        {
            get => SliderWraps[0].Slider.Minimum;
            set
            {
                foreach (var sliderWrap in SliderWraps)
                {
                    sliderWrap.Slider.Minimum = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Maximum
        {
            get => SliderWraps[0].Slider.Maximum;
            set
            {
                foreach (var sliderWrap in SliderWraps)
                {
                    sliderWrap.Slider.Maximum = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            set
            {
                foreach (var sliderWrap in SliderWraps)
                {
                    sliderWrap.Slider.Value = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SetReset
        {
            set
            {
                foreach (var sliderWrap in SliderWraps)
                {
                    sliderWrap.Slider.SetReset = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Integer
        {
            set
            {
                foreach (var sliderWrap in SliderWraps)
                {
                    sliderWrap.Integer = value;
                }
            }
        }

        public SliderGroup()
        {
            Text = "Таблица слайдеров";
            ClientSize = new Size(450, 300);

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for (int i = 0; i < 3; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
                SliderWraps[i] = new SliderWrap()
                {
                    Dock = DockStyle.Fill,
                };
                tableLayout.Controls.Add(SliderWraps[i], i, 0);
            }

            Controls.Add(tableLayout);
        }

        public void Reset()
        {
            foreach (var sliderWrap in SliderWraps)
            {
                sliderWrap.Reset();
            }
        }
    }
}