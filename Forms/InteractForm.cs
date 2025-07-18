using System.Numerics;

namespace LayerSlicer.Forms
{
    internal class InteractForm : CollapsiblePanel
    {
        private ScrollBar _scrollBar;

        protected int _innerWidth;

        public InteractForm() : base("Интерактивная панель")
        {
            // Base setup
            Text = "Interact";
            
            // Scroll bar setup
            _scrollBar = new ScrollBar()
            {
                Visible = false,
                Width = GlobalConsts.ScrollBarWidth,
            };

            Controls.Add(_scrollBar);

            _scrollBar.Scrolled += (sender, e) =>
            {
                //RecalibrateHeight();
                _content.Location = new Point(
                    GlobalConsts.LayerStepWidth, 
                    _button.Height + GlobalConsts.Padding - _scrollBar.Offset);
            };

            _scrollBar.VisibleChanged += (sender, e) =>
            {
                _scrollBar.Height = Program.GetRenderSize().Height - _button.Height;
                _scrollBar.Location = new Point(
                    Width - GlobalConsts.ScrollBarWidth, 
                    _button.Height);
            };

            _scrollBar.SetScrollable(_content);

            // Handling window sizes changing
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

            BackColor = Color.Black;
            BorderStyle = BorderStyle.FixedSingle;
            _innerWidth = 
                GlobalConsts.InteractFormWidth 
                    - GlobalConsts.LayerStepWidth 
                    - GlobalConsts.Padding;
            Size = new Size(GlobalConsts.InteractFormWidth, GlobalConsts.ButtonHeight);

            // Control setup
            var ControlInfoPanel = new CollapsiblePanel("Справка по интерфейсу");
            Add(ControlInfoPanel);
            {
                var HotkeysInfoPanel = new CollapsiblePanel("Горячие клавиши");
                ControlInfoPanel.Add(HotkeysInfoPanel);
                {
                    var ControlInfoLabel = new SelfStretchingLabel();
                    SetupLabel(ControlInfoLabel);
                    ControlInfoLabel.Text =
                        "\"F\" для переключения между режимами управления" + 
                        Environment.NewLine +
                        "\"R\" для восстановления изначального состояния сцены " +
                            "(положение, поворот и поле зрения камеры, положение, " +
                            "поворот и масштаб модели)";
                    HotkeysInfoPanel.Add(ControlInfoLabel);
                }

                var CameraControlModesInfoPanel = new CollapsiblePanel("Справка по камере");
                ControlInfoPanel.Add(CameraControlModesInfoPanel);
                {
                    var FreeCameraControlPanel = new CollapsiblePanel("Cвободная камера");
                    CameraControlModesInfoPanel.Add(FreeCameraControlPanel);
                    {
                        var FreeCameraControlText = new SelfStretchingLabel();
                        SetupLabel(FreeCameraControlText);
                        FreeCameraControlText.Text =
                            "WASD для пермещения вперёд, влево, вправо и назад " +
                                "соответственно" +
                                Environment.NewLine +
                            "Прокрутить колёсико для изменения поля зрения";
                        FreeCameraControlPanel.Add(FreeCameraControlText);
                    }

                    var MouseControlPanel = new CollapsiblePanel("Центрированная камера");
                    CameraControlModesInfoPanel.Add(MouseControlPanel);
                    {
                        var MouseControlText = new SelfStretchingLabel();
                        SetupLabel(MouseControlText);
                        MouseControlText.Text =
                            "ЛКМ для вращения камеры вокруг центра сцены" +
                            Environment.NewLine +
                            "СКМ для пермещения камеры" +
                            Environment.NewLine +
                            "Прокрутить колёсико для изменения поля зрения";
                        MouseControlPanel.Add(MouseControlText);
                    }
                }

                var SlidersInfoPanel = new CollapsiblePanel("Справка по слайдерам");
                ControlInfoPanel.Add(SlidersInfoPanel);
                {
                    var SlidersInfoText = new SelfStretchingLabel();
                    SetupLabel(SlidersInfoText);
                    SlidersInfoText.Text = "ЛКМ для перемещения ползунка" + 
                        Environment.NewLine +
                        "Значение сверху отражает максимальное значение слайдера" + 
                        Environment.NewLine +
                        "Значение снизу отражает минимальное значение слайдера" + 
                        Environment.NewLine +
                        "Красная полоса показывает значение после сброса";
                    SlidersInfoPanel.Add(SlidersInfoText);
                }

                var HeadersInfoPanel = new CollapsiblePanel("Справка по заголовкам");
                ControlInfoPanel.Add(HeadersInfoPanel);
                {
                    var HeadersInfoText = new SelfStretchingLabel();
                    SetupLabel(HeadersInfoText);
                    HeadersInfoText.Text =
                        "ЛКМ на кнопку слева от заголовка, что бы развернуть/свернуть панель";
                    HeadersInfoPanel.Add(HeadersInfoText);
                }
            }

            // Scene interact setup
            var SceneControlPanel = new CollapsiblePanel("Панель управления");
            Add(SceneControlPanel);
            {
                var ModelControlPanel = new CollapsiblePanel("Управление моделью");
                SceneControlPanel.Add(ModelControlPanel);
                {
                    var ModelPositionControlPanel = new CollapsiblePanel("Положение модели");
                    ModelControlPanel.Add(ModelPositionControlPanel);
                    {
                        var ModelPositionControlLabel = new SelfStretchingLabel();
                        ModelPositionControlPanel.Add(ModelPositionControlLabel);
                        {
                            SetupLabel(ModelPositionControlLabel);
                            ModelPositionControlLabel.Text =
                                "Слайдеры для перемещения вдоль осей X, Y и Z соотвественно";
                        }

                        var PositionControlSliders = new SliderGroup()
                        {
                            SetReset = 0,
                            Minimum = -50,
                            Maximum = 50,
                        };
                        ModelPositionControlPanel.Add(PositionControlSliders);
                        {
                            PositionControlSliders.Reset();

                            bool positionChanging = false;
                            PositionControlSliders.Sliders[0].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalModel.SetPosition(
                                    PositionControlSliders.Sliders[0].Value, null, null);
                                positionChanging = false;
                            };
                            PositionControlSliders.Sliders[1].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalModel.SetPosition(null, 
                                    PositionControlSliders.Sliders[1].Value, null);
                                positionChanging = false;
                            };
                            PositionControlSliders.Sliders[2].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalModel.SetPosition(null, null, 
                                    PositionControlSliders.Sliders[2].Value);
                                positionChanging = false;
                            };

                            GlobalModel.PositionChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                PositionControlSliders.Sliders[0].Value = GlobalModel.Position.X;
                                PositionControlSliders.Sliders[1].Value = GlobalModel.Position.Y;
                                PositionControlSliders.Sliders[2].Value = GlobalModel.Position.Z;
                                positionChanging = false;
                            };
                        }
                    }

                    var ModelRotationControlPanel = new CollapsiblePanel("Поворот модели");
                    ModelControlPanel.Add(ModelRotationControlPanel);
                    {
                        var ModelRotationControlLabel = new SelfStretchingLabel();
                        ModelRotationControlPanel.Add(ModelRotationControlLabel);
                        {
                            SetupLabel(ModelRotationControlLabel);
                            ModelRotationControlLabel.Text =
                                "Слайдеры для вращения по осям X, Y и Z соотвественно";
                        }

                        var RotationControlSliders = new SliderGroup()
                        {
                            SetReset = 0,
                            Minimum = -180,
                            Maximum = 180,
                        };
                        ModelRotationControlPanel.Add(RotationControlSliders);
                        {
                            RotationControlSliders.Reset();

                            bool rotationChanging = false;
                            RotationControlSliders.Sliders[0].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalModel.SetRotation(
                                    RotationControlSliders.Sliders[0].Value
                                        * GlobalConsts.DegreesToRadians, null, null);
                                rotationChanging = false;
                            };
                            RotationControlSliders.Sliders[1].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalModel.SetRotation(null, null,
                                    RotationControlSliders.Sliders[1].Value
                                        * GlobalConsts.DegreesToRadians);
                                rotationChanging = false;
                            };
                            RotationControlSliders.Sliders[2].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalModel.SetRotation(null,
                                    RotationControlSliders.Sliders[2].Value
                                        * GlobalConsts.DegreesToRadians, null);
                                rotationChanging = false;
                            };

                            GlobalModel.RotationChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                RotationControlSliders.Sliders[0].Value = GlobalModel.Rotation.X;
                                RotationControlSliders.Sliders[1].Value = GlobalModel.Rotation.Y;
                                RotationControlSliders.Sliders[2].Value = GlobalModel.Rotation.Z;
                                rotationChanging = false;
                            };
                        }
                    }

                    var ModelScaleControlPanel = new CollapsiblePanel("Масштаб модели");
                    ModelControlPanel.Add(ModelScaleControlPanel);
                    {
                        var ModelScaleControlLabel = new SelfStretchingLabel();
                        ModelScaleControlPanel.Add(ModelScaleControlLabel);
                        {
                            SetupLabel(ModelScaleControlLabel);
                            ModelScaleControlLabel.Text =
                                "Слайдеры для масштабирования " +
                                    "вдоль осей X, Y и Z соотвественно";
                        }
                        var ScaleControlSliders = new SliderGroup()
                        {
                            SetReset = 1,
                            Minimum = -10,
                            Maximum = 10,
                        };
                        ModelScaleControlPanel.Add(ScaleControlSliders);
                        {
                            ScaleControlSliders.Reset();

                            bool scaleChanging = false;
                            ScaleControlSliders.Sliders[0].ValueChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                GlobalModel.SetScale(
                                    ScaleControlSliders.Sliders[0].Value, null, null);
                                scaleChanging = false;
                            };
                            ScaleControlSliders.Sliders[1].ValueChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                GlobalModel.SetScale(null, 
                                    ScaleControlSliders.Sliders[1].Value, null);
                                scaleChanging = false;
                            };
                            ScaleControlSliders.Sliders[2].ValueChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                GlobalModel.SetScale(null, null, 
                                    ScaleControlSliders.Sliders[2].Value);
                                scaleChanging = false;
                            };

                            GlobalModel.ScaleChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                ScaleControlSliders.Sliders[0].Value = GlobalModel.Scale.X;
                                ScaleControlSliders.Sliders[1].Value = GlobalModel.Scale.Y;
                                ScaleControlSliders.Sliders[2].Value = GlobalModel.Scale.Z;
                                scaleChanging = false;
                            };
                        }
                    }
                }

                var CameraControlPanel = new CollapsiblePanel("Управление камерой");
                SceneControlPanel.Add(CameraControlPanel);
                {
                    var ModelPositionControlPanel = new CollapsiblePanel("Положение камеры");
                    CameraControlPanel.Add(ModelPositionControlPanel);
                    {
                        var ModelPositionControlLabel = new SelfStretchingLabel();
                        ModelPositionControlPanel.Add(ModelPositionControlLabel);
                        {
                            SetupLabel(ModelPositionControlLabel);
                            ModelPositionControlLabel.Text =
                                "Слайдеры для перемещения вдоль глобальных осей X, Y и Z соотвественно";
                        }

                        var PositionControlSliders = new SliderGroup()
                        {
                            SetReset = 0,
                            Minimum = -50,
                            Maximum = 50,
                        };

                        PositionControlSliders.Sliders[2].SetReset = -5;
                        PositionControlSliders.Reset();
                        ModelPositionControlPanel.Add(PositionControlSliders);
                        {
                            bool positionChanging = false;
                            PositionControlSliders.Sliders[0].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalCamera.GlobalShift(
                                    PositionControlSliders.Sliders[0].Value -
                                    GlobalCamera.Position.X, 0, 0);
                                positionChanging = false;
                            };
                            PositionControlSliders.Sliders[1].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalCamera.GlobalShift(0,
                                    PositionControlSliders.Sliders[1].Value -
                                    GlobalCamera.Position.Y, 0);
                                positionChanging = false;
                            };
                            PositionControlSliders.Sliders[2].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalCamera.GlobalShift(0, 0,
                                    PositionControlSliders.Sliders[2].Value -
                                    GlobalCamera.Position.Z);
                                positionChanging = false;
                            };

                            GlobalCamera.PositionChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                PositionControlSliders.Sliders[0].Value = GlobalCamera.Position.X;
                                PositionControlSliders.Sliders[1].Value = GlobalCamera.Position.Y;
                                PositionControlSliders.Sliders[2].Value = GlobalCamera.Position.Z;
                                positionChanging = false;
                            };
                        }
                    }

                    var ModelRotationControlPanel = new CollapsiblePanel("Вращение камеры");
                    CameraControlPanel.Add(ModelRotationControlPanel);
                    {
                        var ModelRotationControlLabel = new SelfStretchingLabel();
                        ModelRotationControlPanel.Add(ModelRotationControlLabel);
                        {
                            SetupLabel(ModelRotationControlLabel);
                            ModelRotationControlLabel.Text =
                                "Слайдеры для вращения камеры в градусах " +
                                    "по тангажу, рысканью и крёну соотвественно";
                        }

                        var RotationControlSliders = new SliderGroup()
                        {
                            SetReset = 0,
                            Minimum = -180,
                            Maximum = 180,
                        };
                        RotationControlSliders.Sliders[0].Maximum =  GlobalConsts.PitchLimit / GlobalConsts.DegreesToRadians;
                        RotationControlSliders.Sliders[0].Minimum = -GlobalConsts.PitchLimit / GlobalConsts.DegreesToRadians;
                        ModelRotationControlPanel.Add(RotationControlSliders);
                        RotationControlSliders.Reset();
                        {

                            bool rotationChanging = false;
                            RotationControlSliders.Sliders[0].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalCamera.LocalRotate(
                                    RotationControlSliders.Sliders[0].Value * 
                                    GlobalConsts.DegreesToRadians -
                                    GlobalCamera.Rotation.X, 0 , 0);
                                rotationChanging = false;
                            };
                            RotationControlSliders.Sliders[1].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalCamera.LocalRotate(0,
                                    RotationControlSliders.Sliders[1].Value * 
                                    GlobalConsts.DegreesToRadians -
                                    GlobalCamera.Rotation.Y, 0);
                                rotationChanging = false;
                            };
                            RotationControlSliders.Sliders[2].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalCamera.LocalRotate(0, 0,
                                    RotationControlSliders.Sliders[2].Value * 
                                    GlobalConsts.DegreesToRadians -
                                    GlobalCamera.Rotation.Z);
                                rotationChanging = false;
                            };

                            GlobalCamera.RotationChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                RotationControlSliders.Sliders[0].Value = GlobalCamera.Rotation.X / GlobalConsts.DegreesToRadians;
                                RotationControlSliders.Sliders[1].Value = GlobalCamera.Rotation.Y / GlobalConsts.DegreesToRadians;
                                RotationControlSliders.Sliders[2].Value = GlobalCamera.Rotation.Z / GlobalConsts.DegreesToRadians;
                                rotationChanging = false;
                            };
                        }
                    }
                }
            }
            OnSizeChanged(null, EventArgs.Empty);
        }

        private void SetupLabel(Label label)
        {
            label.Font = GlobalConsts.BasicFont;
            label.ForeColor = Color.White;
            label.BackColor = Color.Black;
            label.Location = new Point(GlobalConsts.Padding, 0);
            label.BorderStyle = BorderStyle.None;
            label.Padding = new Padding(0, 0, 0, 50);
            label.Margin = Padding.Empty;
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

            // Updating state
            _scrollBar.Visible = true;
            _innerWidth = Width - _scrollBar.Width - GlobalConsts.LayerStepWidth;

            UpdateInnersWidth();
        }

        protected void DeactivateScrollBar()
        {
            if (!_scrollBar.Visible) return;

            // Updating state
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
