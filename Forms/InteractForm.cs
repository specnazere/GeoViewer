using LayerSlicer.Lighting;
using LayerSlicer.Lighting.Structs;
using SharpDX.DirectInput;

namespace LayerSlicer.Forms
{
    internal class InteractForm : CollapsiblePanel
    {
        private PointF[] _stretchTriangle = new PointF[3];
        private bool _stretching = false;
        private Point _mouseLoc;

        private CollapsiblePanel _pointLightsContainer;
        private CollapsiblePanel _directionalLightsContainer;

        private ComboBox _propertyComboBox;

        private ScrollBar _scrollBar;

        protected int _innerWidth;

        public InteractForm() : base("Интерактивная панель", true)
        {
            // Base setup
            Text = "Interact";
            CalculateSretchTriangle();

            _header.Paint += (sender, e) => OnPaint(e);
            _header.MouseDown += OnMouseDown;
            _header.MouseUp += OnMouseUp;
            _header.MouseMove += OnMouseMove;

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
            CollapsiblePanel preActivated;

            var ControlInfoPanel = new CollapsiblePanel("Справка по интерфейсу", true);
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
                            "поворот и масштаб модели)" +
                        Environment.NewLine +
                        "\"ESC\" для завершения работы программы " +
                        Environment.NewLine +
                        "\"ESC/ENTER\" при наборе значений в текстовые поля " +
                            "для завершения редактирования ";
                HotkeysInfoPanel.Add(ControlInfoLabel);
                }

                var StretchInfoPanel = new CollapsiblePanel("Растягивание панели");
                ControlInfoPanel.Add(StretchInfoPanel);
                {
                    var ControlInfoLabel = new SelfStretchingLabel();
                    SetupLabel(ControlInfoLabel);
                    ControlInfoLabel.Text =
                        "Ширина интерактивной панели может быть изменена " +
                            "путём перемещения белого уголка справа сверху";
                    StretchInfoPanel.Add(ControlInfoLabel);
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
                                "соответственно. QE для перемещения вверх и вниз соответсвенно" +
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

                var HeadersInfoPanel = new CollapsiblePanel("Справка по заголовкам", true);
                ControlInfoPanel.Add(HeadersInfoPanel);
                {
                    var HeadersInfoText = new SelfStretchingLabel();
                    SetupLabel(HeadersInfoText);
                    HeadersInfoText.Text =
                        "ЛКМ на кнопку слева от заголовка, что бы развернуть/свернуть панель";
                    HeadersInfoPanel.Add(HeadersInfoText);
                }
                preActivated = HeadersInfoPanel;
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
                            PositionControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalModel.SetPosition(
                                    PositionControlSliders.SliderWraps[0].Value, null, null);
                                positionChanging = false;
                            };
                            PositionControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalModel.SetPosition(null, 
                                    PositionControlSliders.SliderWraps[1].Value, null);
                                positionChanging = false;
                            };
                            PositionControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalModel.SetPosition(null, null, 
                                    PositionControlSliders.SliderWraps[2].Value);
                                positionChanging = false;
                            };

                            GlobalModel.PositionChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                PositionControlSliders.SliderWraps[0].Value = GlobalModel.Position.X;
                                PositionControlSliders.SliderWraps[1].Value = GlobalModel.Position.Y;
                                PositionControlSliders.SliderWraps[2].Value = GlobalModel.Position.Z;
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
                            RotationControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalModel.SetRotation(
                                    RotationControlSliders.SliderWraps[0].Value
                                        * GlobalConsts.DegreesToRadians, null, null);
                                rotationChanging = false;
                            };
                            RotationControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalModel.SetRotation(null, null,
                                    RotationControlSliders.SliderWraps[1].Value
                                        * GlobalConsts.DegreesToRadians);
                                rotationChanging = false;
                            };
                            RotationControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalModel.SetRotation(null,
                                    RotationControlSliders.SliderWraps[2].Value
                                        * GlobalConsts.DegreesToRadians, null);
                                rotationChanging = false;
                            };

                            GlobalModel.RotationChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                RotationControlSliders.SliderWraps[0].Value = GlobalModel.Rotation.X;
                                RotationControlSliders.SliderWraps[1].Value = GlobalModel.Rotation.Y;
                                RotationControlSliders.SliderWraps[2].Value = GlobalModel.Rotation.Z;
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
                            ScaleControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                GlobalModel.SetScale(
                                    ScaleControlSliders.SliderWraps[0].Value, null, null);
                                scaleChanging = false;
                            };
                            ScaleControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                GlobalModel.SetScale(null, 
                                    ScaleControlSliders.SliderWraps[1].Value, null);
                                scaleChanging = false;
                            };
                            ScaleControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                GlobalModel.SetScale(null, null, 
                                    ScaleControlSliders.SliderWraps[2].Value);
                                scaleChanging = false;
                            };

                            GlobalModel.ScaleChanged += (sender, e) =>
                            {
                                if (scaleChanging) return;
                                scaleChanging = true;
                                ScaleControlSliders.SliderWraps[0].Value = GlobalModel.Scale.X;
                                ScaleControlSliders.SliderWraps[1].Value = GlobalModel.Scale.Y;
                                ScaleControlSliders.SliderWraps[2].Value = GlobalModel.Scale.Z;
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

                        PositionControlSliders.SliderWraps[2].SetReset = -5;
                        PositionControlSliders.Reset();
                        ModelPositionControlPanel.Add(PositionControlSliders);
                        {
                            bool positionChanging = false;
                            PositionControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalCamera.GlobalShift(
                                    PositionControlSliders.SliderWraps[0].Value -
                                    GlobalCamera.Position.X, 0, 0);
                                positionChanging = false;
                            };
                            PositionControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalCamera.GlobalShift(0,
                                    PositionControlSliders.SliderWraps[1].Value -
                                    GlobalCamera.Position.Y, 0);
                                positionChanging = false;
                            };
                            PositionControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                GlobalCamera.GlobalShift(0, 0,
                                    PositionControlSliders.SliderWraps[2].Value -
                                    GlobalCamera.Position.Z);
                                positionChanging = false;
                            };

                            GlobalCamera.PositionChanged += (sender, e) =>
                            {
                                if (positionChanging) return;
                                positionChanging = true;
                                PositionControlSliders.SliderWraps[0].Value = GlobalCamera.Position.X;
                                PositionControlSliders.SliderWraps[1].Value = GlobalCamera.Position.Y;
                                PositionControlSliders.SliderWraps[2].Value = GlobalCamera.Position.Z;
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
                        RotationControlSliders.SliderWraps[0].Maximum =
                            GlobalConsts.PitchLimit / GlobalConsts.DegreesToRadians;
                        RotationControlSliders.SliderWraps[0].Minimum = 
                           -GlobalConsts.PitchLimit / GlobalConsts.DegreesToRadians;
                        ModelRotationControlPanel.Add(RotationControlSliders);
                        RotationControlSliders.Reset();
                        {

                            bool rotationChanging = false;
                            RotationControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalCamera.LocalRotate(
                                    RotationControlSliders.SliderWraps[0].Value * 
                                    GlobalConsts.DegreesToRadians -
                                    GlobalCamera.Rotation.X, 0 , 0);
                                rotationChanging = false;
                            };
                            RotationControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalCamera.LocalRotate(0,
                                    RotationControlSliders.SliderWraps[1].Value * 
                                    GlobalConsts.DegreesToRadians -
                                    GlobalCamera.Rotation.Y, 0);
                                rotationChanging = false;
                            };
                            RotationControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                GlobalCamera.LocalRotate(0, 0,
                                    RotationControlSliders.SliderWraps[2].Value * 
                                    GlobalConsts.DegreesToRadians -
                                    GlobalCamera.Rotation.Z);
                                rotationChanging = false;
                            };

                            GlobalCamera.RotationChanged += (sender, e) =>
                            {
                                if (rotationChanging) return;
                                rotationChanging = true;
                                RotationControlSliders.SliderWraps[0].Value = 
                                    GlobalCamera.Rotation.X / GlobalConsts.DegreesToRadians;
                                RotationControlSliders.SliderWraps[1].Value = 
                                    GlobalCamera.Rotation.Y / GlobalConsts.DegreesToRadians;
                                RotationControlSliders.SliderWraps[2].Value = 
                                    GlobalCamera.Rotation.Z / GlobalConsts.DegreesToRadians;
                                rotationChanging = false;
                            };
                        }
                    }

                    var FovControlPanel = new CollapsiblePanel("Поле зрения");
                    CameraControlPanel.Add(FovControlPanel);
                    {
                        var FovControlLabel = new SelfStretchingLabel();
                        FovControlPanel.Add(FovControlLabel);
                        {
                            SetupLabel(FovControlLabel);
                            FovControlLabel.Text =
                                "Слайдер для регулирования поля зрения камеры в градусах";
                        }

                        var FovControlSlider = new SliderWrap()
                        {
                            IsVerticalLayout = false,
                            SetReset = (float)Math.Round(GlobalConsts.FovBaseValue / GlobalConsts.DegreesToRadians),
                            Minimum = (float)Math.Round(GlobalConsts.FovDownLimit / GlobalConsts.DegreesToRadians),
                            Maximum = (float)Math.Round(GlobalConsts.FovUpperLimit / GlobalConsts.DegreesToRadians),
                        };
                        bool fovChanging = false;
                        FovControlPanel.Add(FovControlSlider);
                        {
                            FovControlSlider.ValueChanged += (sender, e) =>
                            {
                                if (fovChanging) return;
                                fovChanging = true;
                                GlobalCamera.Fov = FovControlSlider.Value * GlobalConsts.DegreesToRadians;
                                fovChanging = false;
                            };

                            GlobalCamera.FovChanged += (sender, e) =>
                            {
                                if (fovChanging) return;
                                fovChanging = true;
                                FovControlSlider.Value = GlobalCamera.Fov / GlobalConsts.DegreesToRadians;
                                fovChanging = false;
                            };
                        }
                    }
                }
            }

            var LightingControlPanel = new CollapsiblePanel("Контроль освещения");
            Add(LightingControlPanel);
            {
                var PointLightsControlPanel = new CollapsiblePanel("Точечные источники");
                _pointLightsContainer = PointLightsControlPanel;
                LightingControlPanel.Add(PointLightsControlPanel);
                {
                    var PointLightsCreationPanel = new CollapsiblePanel("Новый точечный источник");
                    PointLightsControlPanel.Add(PointLightsCreationPanel);
                    {
                        var AddableLight = new PointLightObject
                        {
                            Position = new SharpDX.Vector4(0, 0, 0, 1),
                            Color = new SharpDX.Vector4(1, 1, 1, 100) // Белый цвет по умолчанию
                        };
                        GlobalLighting.AddPointLight(AddableLight, "Demonstrative");

                        SharpDX.Vector4 savedColor = AddableLight.Color;
                        PointLightsCreationPanel.Toggled += (sender, e) =>
                        {
                            if (PointLightsCreationPanel.IsCollapsed())
                            {
                                savedColor = AddableLight.Color;
                                AddableLight.Color = SharpDX.Vector4.Zero;
                            }
                            else
                            {
                                AddableLight.Color = savedColor;
                            }
                        };

                        var PositionControlLabel = new SelfStretchingLabel();
                        PointLightsCreationPanel.Add(PositionControlLabel);
                        {
                            SetupLabel(PositionControlLabel);
                            PositionControlLabel.Text =
                                "Слайдеры для размещения вдоль осей X, Y и Z соотвественно";
                        }

                        var PositionControlSliders = new SliderGroup()
                        {
                            SetReset = 0,
                            Minimum = -50,
                            Maximum = 50,
                            Height = 200,
                        };
                        PointLightsCreationPanel.Add(PositionControlSliders);
                        {
                            PositionControlSliders.Reset();

                            SharpDX.Vector4 bufferPosition = SharpDX.Vector4.Zero;
                            PositionControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferPosition = AddableLight.Position;
                                bufferPosition.X = PositionControlSliders.SliderWraps[0].Value;
                                AddableLight.Position = bufferPosition;
                            };
                            PositionControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferPosition = AddableLight.Position;
                                bufferPosition.Y = PositionControlSliders.SliderWraps[1].Value;
                                AddableLight.Position = bufferPosition;
                            };
                            PositionControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferPosition = AddableLight.Position;
                                bufferPosition.Z = PositionControlSliders.SliderWraps[2].Value;
                                AddableLight.Position = bufferPosition;
                            };
                        }

                        var ColorControlLabel = new SelfStretchingLabel();
                        PointLightsCreationPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета света по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        PointLightsCreationPanel.Add(ColorControlSliders);
                        ColorControlSliders.Reset();
                        {
                            SharpDX.Vector4 bufferColor = SharpDX.Vector4.Zero;
                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                AddableLight.Color = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                AddableLight.Color = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                AddableLight.Color = bufferColor;
                            };
                        }

                        var IntencyControlLabel = new SelfStretchingLabel();
                        PointLightsCreationPanel.Add(IntencyControlLabel);
                        {
                            SetupLabel(IntencyControlLabel);
                            IntencyControlLabel.Text =
                                "Слайдер для изменения интенсивности освещения";
                        }

                        var IntencyControlSlider = new SliderWrap()
                        {
                            SetReset = 10,
                            Minimum = 0,
                            Maximum = 1000,
                            IsVerticalLayout = false,
                        };
                        PointLightsCreationPanel.Add(IntencyControlSlider);
                        IntencyControlSlider.Reset();
                        {
                            SharpDX.Vector4 bufferColor = SharpDX.Vector4.Zero;
                            IntencyControlSlider.ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.W = IntencyControlSlider.Value;
                                AddableLight.Color = bufferColor;
                            };
                        }

                        var LightName = new TextBox()
                        {
                            TextAlign = HorizontalAlignment.Left,
                            Font = GlobalConsts.BasicFont,
                            Text = "New point light name",
                            Dock = DockStyle.None,
                        };
                        LightName.GotFocus += (s, e) =>
                        {
                            UserEventRouter.IsTexting = true;
                        };
                        LightName.LostFocus += (s, e) =>
                        {
                            UserEventRouter.IsTexting = false;
                        };
                        LightName.KeyUp += (s, e) =>
                        {
                            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                            {
                                MainWindow.GetRenderForm().ActiveControl = null;
                            }
                        };
                        PointLightsCreationPanel.Add(LightName);

                        var NewLightButton = new Button()
                        {
                            FlatStyle = FlatStyle.Flat,
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = Color.White,
                            ForeColor = Color.Black,
                            Text = "NewLight",
                            Font = GlobalConsts.BasicFont,
                            Dock = DockStyle.None,
                            Height = 26,
                        };
                        PointLightsCreationPanel.Add(NewLightButton);
                        NewLightButton.Click += (sedner, e) =>
                        {
                            var coppiedPointLight = new PointLightObject
                            {
                                Color = AddableLight.Color,
                                Position = AddableLight.Position,
                            };
                            GlobalLighting.AddPointLight(coppiedPointLight, LightName.Text);
                            ConnectPointLight(coppiedPointLight, LightName.Text);
                        };
                    }
                }

                var DirectionalLightsControlPanel = new CollapsiblePanel("Направленные источники");
                _directionalLightsContainer = DirectionalLightsControlPanel;
                LightingControlPanel.Add(DirectionalLightsControlPanel);
                {
                    var DirectionalLightsCreationPanel = new CollapsiblePanel("Новый направленный источник");
                    DirectionalLightsControlPanel.Add(DirectionalLightsCreationPanel);
                    {
                        var AddableLight = new DirectionalLightObject
                        {
                            Direction = new SharpDX.Vector4(0, 0, 0, 1),
                            Color = new SharpDX.Vector4(1, 1, 1, 100) // Белый цвет по умолчанию
                        };
                        GlobalLighting.AddDirectionalLight(AddableLight, "Demonstrative");

                        SharpDX.Vector4 savedColor = AddableLight.Color;
                        DirectionalLightsCreationPanel.Toggled += (sender, e) =>
                        {
                            if (DirectionalLightsCreationPanel.IsCollapsed())
                            {
                                savedColor = AddableLight.Color;
                                AddableLight.Color = SharpDX.Vector4.Zero;
                            }
                            else
                            {
                                AddableLight.Color = savedColor;
                            }
                        };

                        var PositionControlLabel = new SelfStretchingLabel();
                        DirectionalLightsCreationPanel.Add(PositionControlLabel);
                        {
                            SetupLabel(PositionControlLabel);
                            PositionControlLabel.Text =
                                "Слайдеры для управление вектором направления по осям X, Y и Z соотвественно";
                        }

                        var DirectionControlSliders = new SliderGroup()
                        {
                            SetReset = 0,
                            Minimum = -5,
                            Maximum = 5 ,
                            Height = 200,
                        };
                        DirectionalLightsCreationPanel.Add(DirectionControlSliders);
                        {
                            DirectionControlSliders.Reset();

                            SharpDX.Vector4 bufferDirection = SharpDX.Vector4.Zero;
                            DirectionControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferDirection = AddableLight.Direction;
                                bufferDirection.X = DirectionControlSliders.SliderWraps[0].Value;
                                AddableLight.Direction = bufferDirection;
                            };
                            DirectionControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferDirection = AddableLight.Direction;
                                bufferDirection.Y = DirectionControlSliders.SliderWraps[1].Value;
                                AddableLight.Direction = bufferDirection;
                            };
                            DirectionControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferDirection = AddableLight.Direction;
                                bufferDirection.Z = DirectionControlSliders.SliderWraps[2].Value;
                                AddableLight.Direction = bufferDirection;
                            };
                        }

                        var ColorControlLabel = new SelfStretchingLabel();
                        DirectionalLightsCreationPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета света по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        DirectionalLightsCreationPanel.Add(ColorControlSliders);
                        ColorControlSliders.Reset();
                        {
                            SharpDX.Vector4 bufferColor = SharpDX.Vector4.Zero;
                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                AddableLight.Color = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                AddableLight.Color = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                AddableLight.Color = bufferColor;
                            };
                        }

                        var IntencyControlLabel = new SelfStretchingLabel();
                        DirectionalLightsCreationPanel.Add(IntencyControlLabel);
                        {
                            SetupLabel(IntencyControlLabel);
                            IntencyControlLabel.Text =
                                "Слайдер для изменения интенсивности освещения";
                        }

                        var IntencyControlSlider = new SliderWrap()
                        {
                            SetReset = 1,
                            Minimum = 0,
                            Maximum = 10,
                            IsVerticalLayout = false,
                        };
                        DirectionalLightsCreationPanel.Add(IntencyControlSlider);
                        IntencyControlSlider.Reset();
                        {
                            SharpDX.Vector4 bufferColor = SharpDX.Vector4.Zero;
                            IntencyControlSlider.ValueChanged += (sender, e) =>
                            {
                                bufferColor = AddableLight.Color;
                                bufferColor.W = IntencyControlSlider.Value;
                                AddableLight.Color = bufferColor;
                            };
                        }

                        var LightName = new TextBox()
                        {
                            TextAlign = HorizontalAlignment.Left,
                            Font = GlobalConsts.BasicFont,
                            Text = "New directional light name",
                            Dock = DockStyle.None,
                        };
                        LightName.GotFocus += (s, e) =>
                        {
                            UserEventRouter.IsTexting = true;
                        };
                        LightName.LostFocus += (s, e) =>
                        {
                            UserEventRouter.IsTexting = false;
                        };
                        LightName.KeyUp += (s, e) =>
                        {
                            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                            {
                                MainWindow.GetRenderForm().ActiveControl = null;
                            }
                        };
                        DirectionalLightsCreationPanel.Add(LightName);

                        var NewLightButton = new Button()
                        {
                            FlatStyle = FlatStyle.Flat,
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = Color.White,
                            ForeColor = Color.Black,
                            Text = "NewLight",
                            Font = GlobalConsts.BasicFont,
                            Dock = DockStyle.None,
                            Height = 26,
                        };
                        DirectionalLightsCreationPanel.Add(NewLightButton);
                        NewLightButton.Click += (sedner, e) =>
                        {
                            var coppiedDirectionalLight = new DirectionalLightObject
                            {
                                Color = AddableLight.Color,
                                Direction = AddableLight.Direction,
                            };
                            GlobalLighting.AddDirectionalLight(coppiedDirectionalLight, LightName.Text);
                            ConnectDirectionalLight(coppiedDirectionalLight, LightName.Text);
                        };
                    }
                }

                var AmbiendLightControlPanel = new CollapsiblePanel("Рассеянный свет");
                LightingControlPanel.Add(AmbiendLightControlPanel);
                {
                    var ColorControlLabel = new SelfStretchingLabel();
                    AmbiendLightControlPanel.Add(ColorControlLabel);
                    {
                        SetupLabel(ColorControlLabel);
                        ColorControlLabel.Text =
                            "Слайдеры для изменения цвета рассеянного света по компонентам RGB";
                    }

                    var ColorControlSliders = new SliderGroup()
                    {
                        SetReset = 256,
                        Minimum = 0,
                        Maximum = 256,
                        Height = 200,
                    };
                    ColorControlSliders.SliderWraps[0].SetReset = GlobalLighting.AmbiendLighting.X * 256;
                    ColorControlSliders.SliderWraps[1].SetReset = GlobalLighting.AmbiendLighting.Y * 256;
                    ColorControlSliders.SliderWraps[2].SetReset = GlobalLighting.AmbiendLighting.Z * 256;
                    AmbiendLightControlPanel.Add(ColorControlSliders);
                    ColorControlSliders.Reset();
                    {
                        SharpDX.Vector4 bufferLight = SharpDX.Vector4.Zero;
                        ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                        {
                            bufferLight = GlobalLighting.AmbiendLighting;
                            bufferLight.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                            GlobalLighting.AmbiendLighting = bufferLight;
                        };
                        ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                        {
                            bufferLight = GlobalLighting.AmbiendLighting;
                            bufferLight.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                            GlobalLighting.AmbiendLighting = bufferLight;
                        };
                        ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                        {
                            bufferLight = GlobalLighting.AmbiendLighting;
                            bufferLight.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                            GlobalLighting.AmbiendLighting = bufferLight;
                        };
                    }


                    var IntencyControlLabel = new SelfStretchingLabel();
                    AmbiendLightControlPanel.Add(IntencyControlLabel);
                    {
                        SetupLabel(IntencyControlLabel);
                        IntencyControlLabel.Text =
                            "Слайдер для изменения интенсивности рассеянного освещения";
                    }

                    var IntencyControlSlider = new SliderWrap()
                    {
                        SetReset = 1,
                        Minimum = 0,
                        Maximum = 10,
                        IsVerticalLayout = false,
                    };
                    IntencyControlSlider.SetReset = GlobalLighting.AmbiendLighting.W;
                    AmbiendLightControlPanel.Add(IntencyControlSlider);
                    IntencyControlSlider.Reset();
                    {
                        SharpDX.Vector4 bufferLight = SharpDX.Vector4.Zero;
                        IntencyControlSlider.ValueChanged += (sender, e) =>
                        {
                            bufferLight = GlobalLighting.AmbiendLighting;
                            bufferLight.W = IntencyControlSlider.Value;
                            GlobalLighting.AmbiendLighting = bufferLight;
                        };
                    }
                }
            }

            var DisplayControlPanel = new CollapsiblePanel("Управление отображением");
            Add(DisplayControlPanel);
            {
                var ShaderControlPanel = new CollapsiblePanel("Настройки отрисовки");
                DisplayControlPanel.Add(ShaderControlPanel);
                {
                    var ShaderSelectLabel = new SelfStretchingLabel();
                    ShaderControlPanel.Add(ShaderSelectLabel);
                    {
                        SetupLabel(ShaderSelectLabel);
                        ShaderSelectLabel.Text =
                            "Выбор типа отрисовки";
                    }
                    var ShaderSelectComboBox = new ComboBox()
                    {
                        Font = GlobalConsts.BasicFont,
                        FlatStyle = FlatStyle.Flat,
                    };
                    ShaderControlPanel.Add(ShaderSelectComboBox);
                    {
                        ShaderSelectComboBox.Items.Add("Мультяшный вид");
                        ShaderSelectComboBox.Items.Add("Стандартный вид");
                        ShaderSelectComboBox.Items.Add("Строгий вид");
                        ShaderSelectComboBox.Items.Add("Вид сетки");
                        ShaderSelectComboBox.SelectedIndex = 2;

                        ShaderSelectComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            switch (ShaderSelectComboBox.SelectedIndex)
                            {
                            case 0:
                                GlobalShaders.SelectToonShader();
                                break;
                            case 1:
                                GlobalShaders.SelectPhongShader();
                                break;
                            case 2:
                                GlobalShaders.SelectStrictShader();
                                break;
                            case 3:
                                GlobalShaders.SelectMeshShader();
                                break;
                            }
                        };
                    }

                    var ColorSelectLabel = new SelfStretchingLabel();
                    ShaderControlPanel.Add(ColorSelectLabel);
                    {
                        SetupLabel(ColorSelectLabel);
                        ColorSelectLabel.Text =
                            "Выбор цвета";
                    }
                    var ColorSelectComboBox = new ComboBox()
                    {
                        Font = GlobalConsts.BasicFont,
                        FlatStyle = FlatStyle.Flat,
                    };
                    ShaderControlPanel.Add(ColorSelectComboBox);
                    {
                        ColorSelectComboBox.Items.Add("Цвета сторон");
                        ColorSelectComboBox.Items.Add("Полный цвет");
                        ColorSelectComboBox.Items.Add("Цвет свойства");
                        ColorSelectComboBox.SelectedIndex = 2;

                        ColorSelectComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            switch (ColorSelectComboBox.SelectedIndex)
                            {
                            case 0:
                                GlobalShaders.SelectNoneOverlap();
                                break;
                            case 1:
                                GlobalShaders.SelectFullOverlap();
                                break;
                            case 2:
                                GlobalShaders.SelectExternalOverlap();
                                break;
                            }
                        };
                    }

                    var ToonShadingControlPanel = new CollapsiblePanel("Настройки мультяшного вида");
                    ShaderControlPanel.Add(ToonShadingControlPanel);
                    {
                        var IntencyControlLabel = new SelfStretchingLabel();
                        ToonShadingControlPanel.Add(IntencyControlLabel);
                        {
                            SetupLabel(IntencyControlLabel);
                            IntencyControlLabel.Text =
                                "Слайдер для управления количеством шагов в мультяшном шейдере";
                        }

                        var IntencyControlSlider = new SliderWrap()
                        {
                            SetReset = GlobalLighting.ToonShadingSteps,
                            Minimum = 0,
                            Maximum = 32,
                            IsVerticalLayout = false,
                            Integer = true,
                        };
                        ToonShadingControlPanel.Add(IntencyControlSlider);
                        IntencyControlSlider.Reset();
                        {
                            int BufferSteps = 0;
                            IntencyControlSlider.ValueChanged += (sender, e) =>
                            {
                                BufferSteps = GlobalLighting.ToonShadingSteps;
                                BufferSteps = (int)IntencyControlSlider.Value;
                                GlobalLighting.ToonShadingSteps = BufferSteps;
                            };
                        }
                    }

                    var PropertyShadingControlPanel = new CollapsiblePanel("Настройки вида свойств");
                    ShaderControlPanel.Add(PropertyShadingControlPanel);
                    {
                        var PropertySelectLabel = new SelfStretchingLabel();
                        PropertyShadingControlPanel.Add(PropertySelectLabel);
                        {
                            SetupLabel(ShaderSelectLabel);
                            ShaderSelectLabel.Text =
                                "Выбор визуализируемого свойства";
                        }
                        _propertyComboBox = new ComboBox()
                        {
                            Font = GlobalConsts.BasicFont,
                            FlatStyle = FlatStyle.Flat,
                        };
                        PropertyShadingControlPanel.Add(_propertyComboBox);
                        {
                            _propertyComboBox.SelectedIndexChanged += (sender, e) =>
                            {
                                GlobalGrid.SelectedProperty = _propertyComboBox.Text;
                            };
                        }
                    }
                }

                var ColorControlPanel = new CollapsiblePanel("Настройки цвета");
                DisplayControlPanel.Add(ColorControlPanel);
                {
                    var FrontControlPanel = new CollapsiblePanel("Передняя сторона");
                    ColorControlPanel.Add(FrontControlPanel);
                    {
                        var ColorControlLabel = new SelfStretchingLabel();
                        FrontControlPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета передней стороны по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        FrontControlPanel.Add(ColorControlSliders);
                        {
                            SharpDX.Vector4 bufferColor = GlobalModel.FrontColor;

                            ColorControlSliders.SliderWraps[0].SetReset = bufferColor.X * 256;
                            ColorControlSliders.SliderWraps[1].SetReset = bufferColor.Y * 256;
                            ColorControlSliders.SliderWraps[2].SetReset = bufferColor.Z * 256;
                            ColorControlSliders.Reset();

                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.FrontColor;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                GlobalModel.FrontColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.FrontColor;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                GlobalModel.FrontColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.FrontColor;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                GlobalModel.FrontColor = bufferColor;
                            };
                        }
                    }

                    var BackControlPanel = new CollapsiblePanel("Задняя сторона");
                    ColorControlPanel.Add(BackControlPanel);
                    {
                        var ColorControlLabel = new SelfStretchingLabel();
                        BackControlPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета передней стороны по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        BackControlPanel.Add(ColorControlSliders);
                        {
                            SharpDX.Vector4 bufferColor = GlobalModel.BackColor;

                            ColorControlSliders.SliderWraps[0].SetReset = bufferColor.X * 256;
                            ColorControlSliders.SliderWraps[1].SetReset = bufferColor.Y * 256;
                            ColorControlSliders.SliderWraps[2].SetReset = bufferColor.Z * 256;
                            ColorControlSliders.Reset();

                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.BackColor;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                GlobalModel.BackColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.BackColor;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                GlobalModel.BackColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.BackColor;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                GlobalModel.BackColor = bufferColor;
                            };
                        }
                    }

                    var TopControlPanel = new CollapsiblePanel("Верхняя сторона");
                    ColorControlPanel.Add(TopControlPanel);
                    {
                        var ColorControlLabel = new SelfStretchingLabel();
                        TopControlPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета передней стороны по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        TopControlPanel.Add(ColorControlSliders);
                        {
                            SharpDX.Vector4 bufferColor = GlobalModel.TopColor;

                            ColorControlSliders.SliderWraps[0].SetReset = bufferColor.X * 256;
                            ColorControlSliders.SliderWraps[1].SetReset = bufferColor.Y * 256;
                            ColorControlSliders.SliderWraps[2].SetReset = bufferColor.Z * 256;
                            ColorControlSliders.Reset();

                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.TopColor;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                GlobalModel.TopColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.TopColor;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                GlobalModel.TopColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.TopColor;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                GlobalModel.TopColor = bufferColor;
                            };
                        }
                    }

                    var BottomControlPanel = new CollapsiblePanel("Нижняя сторона");
                    ColorControlPanel.Add(BottomControlPanel);
                    {
                        var ColorControlLabel = new SelfStretchingLabel();
                        BottomControlPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета передней стороны по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        BottomControlPanel.Add(ColorControlSliders);
                        {
                            SharpDX.Vector4 bufferColor = GlobalModel.BottomColor;

                            ColorControlSliders.SliderWraps[0].SetReset = bufferColor.X * 256;
                            ColorControlSliders.SliderWraps[1].SetReset = bufferColor.Y * 256;
                            ColorControlSliders.SliderWraps[2].SetReset = bufferColor.Z * 256;
                            ColorControlSliders.Reset();

                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.BottomColor;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                GlobalModel.BottomColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.BottomColor;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                GlobalModel.BottomColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.BottomColor;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                GlobalModel.BottomColor = bufferColor;
                            };
                        }
                    }

                    var LeftControlPanel = new CollapsiblePanel("Левая сторона");
                    ColorControlPanel.Add(LeftControlPanel);
                    {
                        var ColorControlLabel = new SelfStretchingLabel();
                        LeftControlPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета передней стороны по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        LeftControlPanel.Add(ColorControlSliders);
                        {
                            SharpDX.Vector4 bufferColor = GlobalModel.LeftColor;

                            ColorControlSliders.SliderWraps[0].SetReset = bufferColor.X * 256;
                            ColorControlSliders.SliderWraps[1].SetReset = bufferColor.Y * 256;
                            ColorControlSliders.SliderWraps[2].SetReset = bufferColor.Z * 256;
                            ColorControlSliders.Reset();

                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.LeftColor;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                GlobalModel.LeftColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.LeftColor;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                GlobalModel.LeftColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.LeftColor;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                GlobalModel.LeftColor = bufferColor;
                            };
                        }
                    }

                    var RightControlPanel = new CollapsiblePanel("Правая сторона");
                    ColorControlPanel.Add(RightControlPanel);
                    {
                        var ColorControlLabel = new SelfStretchingLabel();
                        RightControlPanel.Add(ColorControlLabel);
                        {
                            SetupLabel(ColorControlLabel);
                            ColorControlLabel.Text =
                                "Слайдеры для изменения цвета передней стороны по компонентам RGB";
                        }

                        var ColorControlSliders = new SliderGroup()
                        {
                            SetReset = 256,
                            Minimum = 0,
                            Maximum = 256,
                            Height = 200,
                        };
                        RightControlPanel.Add(ColorControlSliders);
                        {
                            SharpDX.Vector4 bufferColor = GlobalModel.RightColor;

                            ColorControlSliders.SliderWraps[0].SetReset = bufferColor.X * 256;
                            ColorControlSliders.SliderWraps[1].SetReset = bufferColor.Y * 256;
                            ColorControlSliders.SliderWraps[2].SetReset = bufferColor.Z * 256;
                            ColorControlSliders.Reset();

                            ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.RightColor;
                                bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                                GlobalModel.RightColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.RightColor;
                                bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                                GlobalModel.RightColor = bufferColor;
                            };
                            ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                            {
                                bufferColor = GlobalModel.RightColor;
                                bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                                GlobalModel.RightColor = bufferColor;
                            };
                        }
                    }
                }

                var MaterialControlPanel = new CollapsiblePanel("Настройки материала");
                DisplayControlPanel.Add(MaterialControlPanel);
                {
                    var DiffusalControlLabel = new SelfStretchingLabel();
                    MaterialControlPanel.Add(DiffusalControlLabel);
                    {
                        SetupLabel(DiffusalControlLabel);
                        DiffusalControlLabel.Text =
                            "Сила рассеянного света";
                    }

                    var DiffusalControlSlider = new SliderWrap()
                    {
                        SetReset = GlobalShaders.DiffusalComponent,
                        Minimum = 0,
                        Maximum = 5,
                        IsVerticalLayout = false,
                    };
                    MaterialControlPanel.Add(DiffusalControlSlider);
                    DiffusalControlSlider.Reset();
                    {
                        float bufferDiffusal = GlobalShaders.DiffusalComponent;
                        DiffusalControlSlider.ValueChanged += (sender, e) =>
                        {
                            bufferDiffusal = GlobalShaders.DiffusalComponent;
                            bufferDiffusal = DiffusalControlSlider.Value;
                            GlobalShaders.DiffusalComponent = bufferDiffusal;
                        };
                    }

                    var SpecularControlLabel = new SelfStretchingLabel();
                    MaterialControlPanel.Add(SpecularControlLabel);
                    {
                        SetupLabel(SpecularControlLabel);
                        SpecularControlLabel.Text =
                            "Сила отражённого света";
                    }

                    var SpecularControlSlider = new SliderWrap()
                    {
                        SetReset = GlobalShaders.SpecularComponent,
                        Minimum = 0,
                        Maximum = 5,
                        IsVerticalLayout = false,
                    };
                    MaterialControlPanel.Add(SpecularControlSlider);
                    SpecularControlSlider.Reset();
                    {
                        float bufferSpecular = GlobalShaders.SpecularComponent;
                        SpecularControlSlider.ValueChanged += (sender, e) =>
                        {
                            bufferSpecular = GlobalShaders.SpecularComponent;
                            bufferSpecular = SpecularControlSlider.Value;
                            GlobalShaders.SpecularComponent = bufferSpecular;
                        };
                    }

                    var SpecularDispersionControlLabel = new SelfStretchingLabel();
                    MaterialControlPanel.Add(SpecularDispersionControlLabel);
                    {
                        SetupLabel(SpecularDispersionControlLabel);
                        SpecularDispersionControlLabel.Text =
                            "Степень размера блика";
                    }

                    var SpecularDispersionControlSlider = new SliderWrap()
                    {
                        SetReset = 5 - GlobalShaders.SpecularDispersion,
                        Minimum = 0,
                        Maximum = 5,
                        IsVerticalLayout = false,
                    };
                    MaterialControlPanel.Add(SpecularDispersionControlSlider);
                    SpecularDispersionControlSlider.Reset();
                    {
                        float bufferDispersion = GlobalShaders.SpecularDispersion;
                        SpecularDispersionControlSlider.ValueChanged += (sender, e) =>
                        {
                            bufferDispersion = GlobalShaders.SpecularDispersion;
                            bufferDispersion = 5 - SpecularDispersionControlSlider.Value;
                            GlobalShaders.SpecularDispersion = bufferDispersion;
                        };
                    }
                }
            }

            OnSizeChanged(null, EventArgs.Empty);
            preActivated.RecalibrateHeight();
        }

        public void ConnectPointLight(PointLightObject newPointLight, string name)
        {
            var pointLightAccessPanel = new CollapsiblePanel(name);
            _pointLightsContainer.Add(pointLightAccessPanel);

            var PositionControlLabel = new SelfStretchingLabel();
            pointLightAccessPanel.Add(PositionControlLabel);
            {
                SetupLabel(PositionControlLabel);
                PositionControlLabel.Text =
                    "Слайдеры для перемещения вдоль осей X, Y и Z соотвественно";
            }

            var PositionControlSliders = new SliderGroup()
            {
                SetReset = 0,
                Minimum = -50,
                Maximum = 50,
                Height = 200,
            };
            PositionControlSliders.SliderWraps[0].SetReset = newPointLight.Position.X;
            PositionControlSliders.SliderWraps[1].SetReset = newPointLight.Position.Y;
            PositionControlSliders.SliderWraps[2].SetReset = newPointLight.Position.Z;
            pointLightAccessPanel.Add(PositionControlSliders);
            {
                PositionControlSliders.Reset();

                SharpDX.Vector4 bufferPosition = SharpDX.Vector4.Zero;
                PositionControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                {
                    bufferPosition = newPointLight.Position;
                    bufferPosition.X = PositionControlSliders.SliderWraps[0].Value;
                    newPointLight.Position = bufferPosition;
                };
                PositionControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                {
                    bufferPosition = newPointLight.Position;
                    bufferPosition.Y = PositionControlSliders.SliderWraps[1].Value;
                    newPointLight.Position = bufferPosition;
                };
                PositionControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                {
                    bufferPosition = newPointLight.Position;
                    bufferPosition.Z = PositionControlSliders.SliderWraps[2].Value;
                    newPointLight.Position = bufferPosition;
                };
            }

            var ColorControlLabel = new SelfStretchingLabel();
            pointLightAccessPanel.Add(ColorControlLabel);
            {
                SetupLabel(ColorControlLabel);
                ColorControlLabel.Text =
                    "Слайдеры для изменения цвета света по компонентам RGB";
            }

            var ColorControlSliders = new SliderGroup()
            {
                SetReset = 256,
                Minimum = 0,
                Maximum = 256,
                Height = 200,
            };
            ColorControlSliders.SliderWraps[0].SetReset = newPointLight.Color.X * 256;
            ColorControlSliders.SliderWraps[1].SetReset = newPointLight.Color.Y * 256;
            ColorControlSliders.SliderWraps[2].SetReset = newPointLight.Color.Z * 256;
            SharpDX.Vector4 bufferColor = SharpDX.Vector4.Zero;
            pointLightAccessPanel.Add(ColorControlSliders);
            ColorControlSliders.Reset();
            {
                ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                {
                    bufferColor = newPointLight.Color;
                    bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                    newPointLight.Color = bufferColor;
                };
                ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                {
                    bufferColor = newPointLight.Color;
                    bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                    newPointLight.Color = bufferColor;
                };
                ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                {
                    bufferColor = newPointLight.Color;
                    bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                    newPointLight.Color = bufferColor;
                };
            }

            var IntencyControlLabel = new SelfStretchingLabel();
            pointLightAccessPanel.Add(IntencyControlLabel);
            {
                SetupLabel(IntencyControlLabel);
                IntencyControlLabel.Text =
                    "Слайдер для изменения интенсивности освещения";
            }

            var IntencyControlSlider = new SliderWrap()
            {
                SetReset = newPointLight.Color.W,
                Minimum = 0,
                Maximum = 1000,
                IsVerticalLayout = false,
            };
            pointLightAccessPanel.Add(IntencyControlSlider);
            IntencyControlSlider.Reset();
            {
                IntencyControlSlider.ValueChanged += (sender, e) =>
                {
                    bufferColor = newPointLight.Color;
                    bufferColor.W = IntencyControlSlider.Value;
                    newPointLight.Color = bufferColor;
                };
            }

            var lightDeleteButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Text = "DELETE",
                Font = GlobalConsts.BasicFont,
                Dock = DockStyle.None,
                Height = 26,
            };
            pointLightAccessPanel.Add(lightDeleteButton);

            lightDeleteButton.Click += (sender, e) =>
            {
                _pointLightsContainer.Remove(pointLightAccessPanel);
                GlobalLighting.RemovePointLight(name);
            };
        }

        public void ConnectDirectionalLight(DirectionalLightObject newDirectionalLight, string name)
        {
            var directionalLightAccessPanel = new CollapsiblePanel(name);
            _directionalLightsContainer.Add(directionalLightAccessPanel);

            var PositionControlLabel = new SelfStretchingLabel();
            directionalLightAccessPanel.Add(PositionControlLabel);
            {
                SetupLabel(PositionControlLabel);
                PositionControlLabel.Text =
                    "Слайдеры для перемещения вдоль осей X, Y и Z соотвественно";
            }

            var PositionControlSliders = new SliderGroup()
            {
                SetReset = 0,
                Minimum = -50,
                Maximum = 50,
                Height = 200,
            };
            PositionControlSliders.SliderWraps[0].SetReset = newDirectionalLight.Direction.X;
            PositionControlSliders.SliderWraps[1].SetReset = newDirectionalLight.Direction.Y;
            PositionControlSliders.SliderWraps[2].SetReset = newDirectionalLight.Direction.Z;
            directionalLightAccessPanel.Add(PositionControlSliders);
            {
                PositionControlSliders.Reset();

                SharpDX.Vector4 bufferDirection = SharpDX.Vector4.Zero;
                PositionControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                {
                    bufferDirection = newDirectionalLight.Direction;
                    bufferDirection.X = PositionControlSliders.SliderWraps[0].Value;
                    newDirectionalLight.Direction = bufferDirection;
                };
                PositionControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                {
                    bufferDirection = newDirectionalLight.Direction;
                    bufferDirection.Y = PositionControlSliders.SliderWraps[1].Value;
                    newDirectionalLight.Direction = bufferDirection;
                };
                PositionControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                {
                    bufferDirection = newDirectionalLight.Direction;
                    bufferDirection.Z = PositionControlSliders.SliderWraps[2].Value;
                    newDirectionalLight.Direction = bufferDirection;
                };
            }

            var ColorControlLabel = new SelfStretchingLabel();
            directionalLightAccessPanel.Add(ColorControlLabel);
            {
                SetupLabel(ColorControlLabel);
                ColorControlLabel.Text =
                    "Слайдеры для изменения цвета света по компонентам RGB";
            }

            var ColorControlSliders = new SliderGroup()
            {
                SetReset = 256,
                Minimum = 0,
                Maximum = 256,
                Height = 200,
            };
            ColorControlSliders.SliderWraps[0].SetReset = newDirectionalLight.Color.X * 256;
            ColorControlSliders.SliderWraps[1].SetReset = newDirectionalLight.Color.Y * 256;
            ColorControlSliders.SliderWraps[2].SetReset = newDirectionalLight.Color.Z * 256;
            SharpDX.Vector4 bufferColor = SharpDX.Vector4.Zero;
            directionalLightAccessPanel.Add(ColorControlSliders);
            ColorControlSliders.Reset();
            {
                ColorControlSliders.SliderWraps[0].ValueChanged += (sender, e) =>
                {
                    bufferColor = newDirectionalLight.Color;
                    bufferColor.X = ColorControlSliders.SliderWraps[0].Value / 256f;
                    newDirectionalLight.Color = bufferColor;
                };
                ColorControlSliders.SliderWraps[1].ValueChanged += (sender, e) =>
                {
                    bufferColor = newDirectionalLight.Color;
                    bufferColor.Y = ColorControlSliders.SliderWraps[1].Value / 256f;
                    newDirectionalLight.Color = bufferColor;
                };
                ColorControlSliders.SliderWraps[2].ValueChanged += (sender, e) =>
                {
                    bufferColor = newDirectionalLight.Color;
                    bufferColor.Z = ColorControlSliders.SliderWraps[2].Value / 256f;
                    newDirectionalLight.Color = bufferColor;
                };
            }

            var IntencyControlLabel = new SelfStretchingLabel();
            directionalLightAccessPanel.Add(IntencyControlLabel);
            {
                SetupLabel(IntencyControlLabel);
                IntencyControlLabel.Text =
                    "Слайдер для изменения интенсивности освещения";
            }

            var IntencyControlSlider = new SliderWrap()
            {
                SetReset = newDirectionalLight.Color.W,
                Minimum = 0,
                Maximum = 10,
                IsVerticalLayout = false,
            };
            directionalLightAccessPanel.Add(IntencyControlSlider);
            IntencyControlSlider.Reset();
            {
                IntencyControlSlider.ValueChanged += (sender, e) =>
                {
                    bufferColor = newDirectionalLight.Color;
                    bufferColor.W = IntencyControlSlider.Value;
                    newDirectionalLight.Color = bufferColor;
                };
            }

            var lightDeleteButton = new Button()
            {
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Text = "DELETE",
                Font = GlobalConsts.BasicFont,
                Dock = DockStyle.None,
                Height = 26,
            };
            directionalLightAccessPanel.Add(lightDeleteButton);

            lightDeleteButton.Click += (sender, e) =>
            {
                _directionalLightsContainer.Remove(directionalLightAccessPanel);
                GlobalLighting.RemoveDirectionalLight(name);
            };
        }

        public void ConnectGridProperty(string name)
        {
            _propertyComboBox.Items.Add(name);
            if (_propertyComboBox.Items.Count == 1)
            {
                _propertyComboBox.SelectedIndex = 0;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillPolygon(Brushes.White, _stretchTriangle);
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

        internal override void RecalibrateHeight()
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

        protected void UpdateInnerWidth()
        {
            if (_scrollBar.Visible)
            {
                _innerWidth = Width - _scrollBar.Width - GlobalConsts.LayerStepWidth;
            }
            else
            {
                _innerWidth = Width - GlobalConsts.LayerStepWidth;
            }
        }

        protected override int GetInnerWidth()
        {
            return _innerWidth;
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if ((e.Location.X + _header.Location.X) - (e.Location.Y + _header.Location.Y) 
                >= Width - _header.Height / 2 && e.Button == MouseButtons.Left)
            {
                _mouseLoc = e.Location;
                _stretching = true;
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _stretching = false;
                UpdateInnerWidth();
                OnSizeChanged(null, EventArgs.Empty);
            }
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_stretching)
            {
                Width = Math.Clamp(Width - (_mouseLoc.X - e.Location.X), GlobalConsts.InteractFormWidth / 2, MainWindow.GetRenderForm().Width);
            }
            _mouseLoc = e.Location;
        }

        private void CalculateSretchTriangle()
        {
            _stretchTriangle[0] = new PointF(_header.Width - GlobalConsts.BorderWidth, 0);
            _stretchTriangle[1] = new PointF(_header.Width - GlobalConsts.BorderWidth, _header.Height / 2);
            _stretchTriangle[2] = new PointF(_header.Width - GlobalConsts.BorderWidth - _header.Height / 2, 0);
        }

        protected override void OnSizeChanged(object? sender, EventArgs e)
        {
            if (!_stretching)
            {
                base.OnSizeChanged(sender, e);
                CalculateSretchTriangle();
            }
        }
    }
}
