using System.ComponentModel;

namespace LayerSlicer.Forms
{
    internal class SliderGroup : UserControl
    {
        public readonly TrackBar[] Sliders = new TrackBar[3];
        public readonly TextBox[] TextBoxes = new TextBox[3];

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Minimum
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
        public int Maximum
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
        public int Step
        {
            get
            {
                return Sliders[0].TickFrequency;
            }
            set
            {
                foreach (var slider in Sliders)
                {
                    slider.TickFrequency = value;
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

        public SliderGroup()
        {
            // Настройка формы
            Text = "Таблица слайдеров";
            ClientSize = new Size(450, 300);

            // Создаем табличную компоновку
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            // Настройка столбцов
            for (int i = 0; i < 3; i++)
            {
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            }

            // Добавляем строки
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 85)); // Для слайдеров
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // Для полей ввода

            // Создаем и настраиваем элементы
            for (int i = 0; i < 3; i++)
            {
                // Вертикальные слайдеры
                Sliders[i] = new TrackBar
                {
                    Orientation = Orientation.Vertical,
                    Minimum = -100,
                    Maximum = 100,
                    Value = 0,
                    TickFrequency = 1,
                    Dock = DockStyle.Fill,
                };

                // Поля для ручного ввода
                TextBoxes[i] = new TextBox
                {
                    Dock = DockStyle.Fill,
                    TextAlign = HorizontalAlignment.Center,
                    Text = "0"
                };

                // Добавляем в таблицу
                tableLayout.Controls.Add(Sliders[i], i, 0);
                tableLayout.Controls.Add(TextBoxes[i], i, 1);

                // Привязка событий
                int index = i; // Важно для замыкания
                Sliders[index].ValueChanged += (s, e) =>
                {
                    TextBoxes[index].Text = Sliders[index].Value.ToString();
                };

                TextBoxes[index].Validated += (s, e) =>
                {
                    if (int.TryParse(TextBoxes[index].Text, out int value))
                    {
                        Sliders[index].Value = Math.Max(Sliders[index].Minimum, Math.Min(Sliders[index].Maximum, value));
                    }
                    else
                    {
                        TextBoxes[index].Text = Sliders[index].Value.ToString();
                    }
                };
            }

            Controls.Add(tableLayout);
        }
    }
}
