namespace LayerSlicer.Forms
{
    internal class InteractForm : CollapsiblePanel
    {
        private ScrollBar _scrollBar;

        protected int _innerWidth;

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

            MainWindow.GetRenderForm().SizeChanged += (sender, e) =>
            {
                _scrollBar.Height = Program.GetRenderSize().Height - _button.Height;

                if (Height > Program.GetRenderSize().Height)
                {
                    ActivateScrollBar();
                }
                else
                {
                    DeactivateScrollBar();
                }

                _scrollBar.Invalidate();
            };

            _scrollBar.SetScrollable(_content);

            //TopLevel = false;
            BackColor = Color.Black;
            BorderStyle = BorderStyle.FixedSingle;
            _innerWidth = GlobalConsts.InteractFormWidth - GlobalConsts.LayerStepWidth - GlobalConsts.Padding;
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
                SetReset = 0,
                Minimum = -100,
                Maximum = 100,
            };
            PositionControlSliders.Reset();
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
                SetReset = 0,
                Minimum = -180,
                Maximum = 180,
            };
            RotationControlSliders.Reset();
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
                SetReset = 1,
                Minimum = -10,
                Maximum = 10,
            };
            ScaleControlSliders.Reset();
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

            OnSizeChanged(null, EventArgs.Empty);
        }

        private void SetupTextBox(TextBox textBox)
        {
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.Font = GlobalConsts.BasicFont;
            textBox.ForeColor = Color.White;
            textBox.BackColor = Color.Black;
            textBox.Location = new Point(GlobalConsts.Padding, 0);
            textBox.BorderStyle = BorderStyle.None;
            textBox.Padding = new Padding(0, 0, 0, 50);
            textBox.Margin = Padding.Empty;
        }

        protected override void RecalibrateHeight()
        {
            base.RecalibrateHeight();

            if (_scrollBar != null && !_isCollapsed)
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

        protected override int GetInnerWidth()
        {
            return _innerWidth;
        }
    }
}
