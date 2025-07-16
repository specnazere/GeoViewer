namespace LayerSlicer.Forms
{
    internal class InteractForm : CollapsiblePanel
    {
        private ScrollBar _scrollBar;
        private bool _recalHeight = false;

        private int _lastInnerWidth = 0;
        private int _innerWidth = 0;

        public InteractForm() : base("Интерфейс")
        {
            // Base setup
            Text = "Interact";

            _scrollBar = new ScrollBar()
            {
                Visible = false,
                Width = GlobalConsts.ScrollBarWidth,
            };

            Controls.Add(_scrollBar);

            _scrollBar.Scrolled += (sender, e) =>
            {
                //RecalibrateHeight();
                _content.Location = new Point(GlobalConsts.LayerStepWidth, _button.Height + GlobalConsts.Padding - _scrollBar.Offset);
            };

            _scrollBar.VisibleChanged += (sender, e) =>
            {
                _scrollBar.Height = Program.GetRenderSize().Height - _button.Height;
                _scrollBar.Location = new Point(Width - GlobalConsts.ScrollBarWidth, _button.Height);
            };

            _scrollBar.SetScrollable(_content);

            //TopLevel = false;
            BackColor = Color.Black;
            BorderStyle = BorderStyle.FixedSingle;
            _innerWidth = GlobalConsts.InteractFormWidth;
            Size = new Size(GlobalConsts.InteractFormWidth, GlobalConsts.ButtonHeight);

            // Control setup
            var ControlGuideTextBox = new SelfStretchingTextBox();
            SetupTextBox(ControlGuideTextBox);
            ControlGuideTextBox.Text = "Для переключения между режимами управления нажми \"F\"";
            var ControlGuidePanel = new CollapsiblePanel("Гайд на перемещение");
            ControlGuidePanel.Add(ControlGuideTextBox);

            var FreeCameraControlPanel = new CollapsiblePanel("Cвободная камера");
            var FreeCameraControlText = new SelfStretchingTextBox();
            SetupTextBox(FreeCameraControlText);
            FreeCameraControlText.Text = "WASD для пермещения вперёд влево вправо и назад соответственно" + Environment.NewLine + "Прокрутить колёсико для изменения поля зрения";
            FreeCameraControlPanel.Add(FreeCameraControlText);
            ControlGuidePanel.Add(FreeCameraControlPanel);

            var MouseControlPanel = new CollapsiblePanel("Управление мышью");
            var MouseControlText = new SelfStretchingTextBox();
            SetupTextBox(MouseControlText);
            MouseControlText.Text = "ЛКМ для вращения камеры вокруг центра сцены" + Environment.NewLine + "СКМ для пермещения камеры" + Environment.NewLine + "Прокрутить колёсико для изменения поля зрения";
            MouseControlPanel.Add(MouseControlText);
            ControlGuidePanel.Add(MouseControlPanel);

            // Scene interact setup
            var SceneControlPanel = new CollapsiblePanel("Панель управления");

            var ModelPositionControlTextBox = new SelfStretchingTextBox();
            SetupTextBox(ModelPositionControlTextBox);
            ModelPositionControlTextBox.Text = "Слайдеры для перемещения по осям X, Y и Z соотвественно \"F\"";
            var PositionControlSliders = new SliderGroup()
            {
                Value = 0,
                Minimum = -100,
                Maximum = 100,
            };
            PositionControlSliders.Sliders[0].ValueChanged += (sender, e) =>
            {
                GlobalModel.Position.X = PositionControlSliders.Sliders[0].Value;
            };
            PositionControlSliders.Sliders[1].ValueChanged += (sender, e) =>
            {
                GlobalModel.Position.Y = PositionControlSliders.Sliders[1].Value;
            };
            PositionControlSliders.Sliders[2].ValueChanged += (sender, e) =>
            {
                GlobalModel.Position.Z = PositionControlSliders.Sliders[2].Value;
            };
            var ModelPositionControlPanel = new CollapsiblePanel("Положение модели");
            ModelPositionControlPanel.Add(ModelPositionControlTextBox);
            ModelPositionControlPanel.Add(PositionControlSliders);

            var ModelRotationControlTextBox = new SelfStretchingTextBox();
            SetupTextBox(ModelRotationControlTextBox);
            ModelRotationControlTextBox.Text = "Слайдеры для вращения по осям X, Y и Z соотвественно \"F\"";
            var RotationControlSliders = new SliderGroup()
            {
                Value = 0,
                Minimum = -180,
                Maximum = 180,
            };
            RotationControlSliders.Sliders[0].ValueChanged += (sender, e) =>
            {
                GlobalModel.Rotation.X = RotationControlSliders.Sliders[0].Value * GlobalConsts.DegreesToRadians;
            };
            RotationControlSliders.Sliders[1].ValueChanged += (sender, e) =>
            {
                GlobalModel.Rotation.Y = RotationControlSliders.Sliders[1].Value * GlobalConsts.DegreesToRadians;
            };
            RotationControlSliders.Sliders[2].ValueChanged += (sender, e) =>
            {
                GlobalModel.Rotation.Z = RotationControlSliders.Sliders[2].Value * GlobalConsts.DegreesToRadians;
            };
            var ModelRotationControlPanel = new CollapsiblePanel("Вращение модели");
            ModelRotationControlPanel.Add(ModelRotationControlTextBox);
            ModelRotationControlPanel.Add(RotationControlSliders);

            var ModelScaleControlTextBox = new SelfStretchingTextBox();
            SetupTextBox(ModelScaleControlTextBox);
            ModelScaleControlTextBox.Text = "Слайдеры для масштабирования по осям X, Y и Z соотвественно \"F\"";
            var ScaleControlSliders = new SliderGroup()
            {
                Value = 1,
                Minimum = -10,
                Maximum = 10,
            };
            ScaleControlSliders.Sliders[0].ValueChanged += (sender, e) =>
            {
                GlobalModel.Scale.X = ScaleControlSliders.Sliders[0].Value;
            };
            ScaleControlSliders.Sliders[1].ValueChanged += (sender, e) =>
            {
                GlobalModel.Scale.Y = ScaleControlSliders.Sliders[1].Value;
            };
            ScaleControlSliders.Sliders[2].ValueChanged += (sender, e) =>
            {
                GlobalModel.Scale.Z = ScaleControlSliders.Sliders[2].Value;
            };
            var ModelScaleControlPanel = new CollapsiblePanel("Масштабирование модели");
            ModelScaleControlPanel.Add(ModelScaleControlTextBox);
            ModelScaleControlPanel.Add(ScaleControlSliders);

            SceneControlPanel.Add(ModelPositionControlPanel);
            SceneControlPanel.Add(ModelRotationControlPanel);
            SceneControlPanel.Add(ModelScaleControlPanel);

            // Add elements to form
            Add(ControlGuidePanel);
            Add(SceneControlPanel);
        }

        private void SetupTextBox(TextBox textBox)
        {
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.Font = GlobalConsts.BasicFont;
            textBox.ForeColor = Color.White;
            textBox.BackColor = Color.Black;
            textBox.Location = new Point(GlobalConsts.Padding, GlobalConsts.Padding);
            textBox.BorderStyle = BorderStyle.None;
            textBox.Margin = Padding.Empty;
        }

        protected override void RecalibrateHeight()
        {
            _recalHeight = true;
            base.RecalibrateHeight();

            Height += GlobalConsts.Padding;
            _content.Height += GlobalConsts.Padding;

            if (_scrollBar != null)
            {
                if (Height > Program.GetRenderSize().Height)
                {
                    ActivateScrollBar();
                }
                else
                {
                    DeactivateScrollBar();
                }
            }

            _recalHeight = false;
        }

        protected override void OnSizeChanged(object? sender, EventArgs e)
        {
            if (_recalHeight) return;

            RecalibrateHeight();
            UpdateInnersWidth();
        }

        protected void UpdateInnersWidth()
        {
            if (_lastInnerWidth == _innerWidth) return;

            _header.Width = Width - GlobalConsts.Padding - _button.Width;
            _content.Width = _innerWidth;

            foreach (Control control in _content.Controls)
            {
                control.Width = _innerWidth;

                if (control.GetType() == typeof(SelfStretchingTextBox))
                {
                    control.Width -= GlobalConsts.Padding;
                }
                else if (control.GetType() == typeof(SliderGroup))
                {
                    control.Width -= 4;
                }
            }
            _lastInnerWidth = _innerWidth;
        }

        protected void ActivateScrollBar()
        {
            if (_scrollBar.Visible) return;

            _scrollBar.Visible = true;
            _innerWidth = Width - _scrollBar.Width - GlobalConsts.LayerStepWidth;

            UpdateInnersWidth();
        }

        protected void DeactivateScrollBar()
        {
            if (!_scrollBar.Visible) return;

            _scrollBar.Visible = false;
            _innerWidth = Width - GlobalConsts.LayerStepWidth;

            UpdateInnersWidth();
        }
    }
}
