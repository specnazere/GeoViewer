using System.ComponentModel;
using System.Numerics;

namespace LayerSlicer.Forms
{
    internal class SliderGroup : UserControl
    {
        // Table containment
        public readonly FloatSlider[] Sliders = new FloatSlider[3];
        public readonly TextBox[] TextBoxes = new TextBox[3];
        public readonly Button[] ResetButtons = new Button[3];

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Minimum
        {
            get
            {
                return Sliders[0].Minimum;
            }
            set
            {
                foreach (var slider in Sliders)
                {
                    slider.Minimum = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Maximum
        {
            get
            {
                return Sliders[0].Maximum;
            }
            set
            {
                foreach (var slider in Sliders)
                {
                    slider.Maximum = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            set
            {
                foreach (var slider in Sliders)
                {
                    slider.Value = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SetReset
        {
            set
            {
                foreach (var slider in Sliders)
                {
                    slider.SetReset = value;
                }
            }
        }

        public SliderGroup()
        {
            // Base setup
            Text = "Таблица слайдеров";
            ClientSize = new Size(450, 300);

            // Table layout setup
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for (int i = 0; i < 3; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            }

            // Rows
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70)); // Sliders
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // Text boxes
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // Reset buttons

            // Creating per-column elements
            for (int i = 0; i < 3; i++)
            {
                // Setup button
                ResetButtons[i] = new Button
                {
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.White,
                    ForeColor = Color.Black,
                    Dock = DockStyle.Fill,
                    Text = "RESET",
                    Font = GlobalConsts.BasicFont,
                };

                // Setup slider
                Sliders[i] = new FloatSlider
                {
                    Minimum = -100,
                    Maximum = 100,
                    Value = 0,
                    Dock = DockStyle.Fill,
                };

                // Hand input box
                TextBoxes[i] = new TextBox
                {
                    Dock = DockStyle.Fill,
                    TextAlign = HorizontalAlignment.Center,
                    Font = GlobalConsts.BasicFont,
                    Text = "0"
                };

                // Reset buttons connecting
                var indexCopy = i;
                ResetButtons[i].Click += (sender, e) =>
                {
                    Sliders[indexCopy].Reset();
                };

                // Fill column
                tableLayout.Controls.Add(Sliders[i], i, 0);
                tableLayout.Controls.Add(TextBoxes[i], i, 1);
                tableLayout.Controls.Add(ResetButtons[i], i, 2);

                // Events handling
                int index = i; // Index copy
                bool _isUpdating = false;

                Sliders[index].ValueChanged += (s, e) =>
                {
                    if (_isUpdating) return;
    
                    _isUpdating = true;
                    TextBoxes[index].Text = Sliders[index].Value.ToString();
                    _isUpdating = false;
                };

                TextBoxes[index].TextChanged += (s, e) =>
                {
                    if (_isUpdating) return;
    
                    _isUpdating = true;
                    if (float.TryParse(TextBoxes[index].Text, out float value))
                    {
                        Sliders[index].Value = Math.Clamp(value, Sliders[index].Minimum, Sliders[index].Maximum);
                    }
                    else
                    {
                        TextBoxes[index].Text = Sliders[index].Value.ToString();
                    }
                    _isUpdating = false;
                };
            }

            Controls.Add(tableLayout);
        }

        public void Reset()
        {
            foreach (var slider in Sliders)
            {
                slider.Reset();
            }
        }
    }
}
