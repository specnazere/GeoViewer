using LayerSlicer.Lighting;
using LayerSlicer.Lighting.Structs;

namespace LayerSlicer.Forms
{
    internal class InteractForm : CollapsiblePanel
    {
        private PointF[] _stretchTriangle = new PointF[3];
        private bool _stretching = false;
        private Point _mouseLoc;

        private CollapsiblePanel _pointLightsContainer;

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
                            "поворот и масштаб модели)";
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
            _pointLightsContainer = LightingControlPanel;
            Add(LightingControlPanel);
            {
                var PointLightsCreationPanel = new CollapsiblePanel("Новый точечный источник");
                LightingControlPanel.Add(PointLightsCreationPanel);
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
