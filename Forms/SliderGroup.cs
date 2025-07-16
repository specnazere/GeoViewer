using System;
using System.ComponentModel;

namespace LayerSlicer.Forms
{
    internal class SliderGroup : UserControl
    {
        public readonly FloatSlider[] Sliders = new FloatSlider[3];
        public readonly TextBox[] TextBoxes = new TextBox[3];

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
                Sliders[i] = new FloatSlider
                {
                    Minimum = -100,
                    Maximum = 100,
                    Value = 0,
                    Dock = DockStyle.Fill,
                };

                // Поля для ручного ввода
                TextBoxes[i] = new TextBox
                {
                    Dock = DockStyle.Fill,
                    TextAlign = HorizontalAlignment.Center,
                    Font = GlobalConsts.BasicFont,
                    Text = "0"
                };

                // Добавляем в таблицу
                tableLayout.Controls.Add(Sliders[i], i, 0);
                tableLayout.Controls.Add(TextBoxes[i], i, 1);

                // Привязка событий
                int index = i; // Важно для замыкания
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
    }
}
