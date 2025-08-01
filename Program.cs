using LayerSlicer.Lighting;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Windows;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace LayerSlicer
{
    internal static class Program
    {
        private static Device _device;
        private static SwapChain _swapChain;

        private static RenderTargetView _renderView;

        //      [STAThread]
        private static void Main()
        {
            var format = Format.R8G8B8A8_UNorm;
            var sampledesc = new SampleDescription(4, 0);

            // SwapChain description
            var SWdesc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(
                        MainWindow.GetRenderForm().ClientSize.Width, 
                        MainWindow.GetRenderForm().ClientSize.Height,
                        new Rational(60, 1),
                        format),
                IsWindowed = true,
                OutputHandle = MainWindow.GetRenderForm().Handle,
                SampleDescription = sampledesc,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };
            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, SWdesc, out _device, out _swapChain);
            var context = _device.ImmediateContext;

            var bufferDesc = new BufferDescription
            {
                SizeInBytes = GlobalConsts.PerSideVectors * Utilities.SizeOf<Vector4>() * 6, // Размер в байтах
                Usage = ResourceUsage.Dynamic,  // Динамический буфер
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write, // Разрешить запись с CPU
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            GlobalModel.VerticesByTrianglesBuffer = new Buffer(
                GetDevice(),
                DataStream.Create(GlobalModel.CubeVerticesByTriangles, false, false),
                bufferDesc
            );
            GlobalModel.VerticesByLinesBuffer = Buffer.Create(GetDevice(), BindFlags.VertexBuffer, GlobalModel.CubeVerticesByLines);

            // Ignore all windows events
            var factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(MainWindow.GetRenderForm().Handle, WindowAssociationFlags.IgnoreAll);

            GlobalShaders.SelectStrictShader();
            GlobalShaders.SelectExternalOverlap();
            MainWindow.InitInteractForm();
            GlobalGrid.ReadGridFile("grid.bin");
            GlobalGrid.ReadPropertiesFile("grid.binprops.txt");

            var startingPointLight1 = new Lighting.Structs.PointLightObject()
            {
                Position = new Vector4(5, 5, 5, 1),
                Color = new Vector4(1, 0.5f, 0.5f, 100)
            };
            GlobalLighting.AddPointLight(startingPointLight1, "starting1");
            MainWindow.GetInteractForm().ConnectPointLight(startingPointLight1, "starting1");

            var startingPointLight2 = new Lighting.Structs.PointLightObject()
            {
                Position = new Vector4(-5, -5, -5, 1),
                Color = new Vector4(0.5f, 0.5f, 1, 100)
            };
            GlobalLighting.AddPointLight(startingPointLight2, "starting2");
            MainWindow.GetInteractForm().ConnectPointLight(startingPointLight2, "starting2");

            var startingDirectionalLight1 = new Lighting.Structs.DirectionalLightObject()
            {
                Direction = new Vector4(0, 0, 5, 1),
                Color = new Vector4(0.1f, 1f, 0.1f, 1)
            };
            GlobalLighting.AddDirectionalLight(startingDirectionalLight1, "starting1");
            MainWindow.GetInteractForm().ConnectDirectionalLight(startingDirectionalLight1, "starting1");

            // Use clock
            GlobalUpdater.Update();

            // Resize handling
            bool userResized = true;
            MainWindow.GetRenderForm().Resize += (sender, args) => userResized = true;

            // Declare texture for rendering
            Texture2D backBuffer = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;

            // Prepare global values
            GlobalCamera.Position = GlobalConsts.StandartCameraPostion;

            // Main loop
            RenderLoop.Run(MainWindow.GetRenderForm(), () =>
            {
                // If Form resized
                if (userResized)
                {
                    // Dispose all previous allocated resources
                    Utilities.Dispose(ref backBuffer);
                    Utilities.Dispose(ref _renderView);
                    Utilities.Dispose(ref depthBuffer);
                    Utilities.Dispose(ref depthView);

                    // Resize the backbuffer
                    _swapChain.ResizeBuffers(
                        SWdesc.BufferCount, 
                        MainWindow.GetRenderForm().ClientSize.Width, 
                        MainWindow.GetRenderForm().ClientSize.Height, 
                        Format.Unknown, SwapChainFlags.None);

                    // Get the backbuffer from the swapchain
                    backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);

                    // Renderview on the backbuffer
                    _renderView = new RenderTargetView(_device, backBuffer);

                    // Create the depth buffer
                    depthBuffer = new Texture2D(_device, new Texture2DDescription()
                    {
                        Format = Format.D32_Float_S8X24_UInt,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = MainWindow.GetRenderForm().ClientSize.Width,
                        Height = MainWindow.GetRenderForm().ClientSize.Height,
                        SampleDescription = sampledesc,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    // Create the depth buffer view
                    depthView = new DepthStencilView(_device, depthBuffer);

                    // Setup targets and viewport for rendering
                    context.Rasterizer.SetViewport(new Viewport(
                        0, 
                        0, 
                        MainWindow.GetRenderForm().ClientSize.Width, 
                        MainWindow.GetRenderForm().ClientSize.Height, 
                        0.0f, 
                        1.0f));
                    context.OutputMerger.SetTargets(depthView, _renderView);

                    // We are done resizing
                    userResized = false;
                }

                // Clear views
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(_renderView, SharpDX.Color.Black);

                // Update before render
                GlobalUpdater.Update();

                // Draw the cubes
                for (int x = 0, i = 0; x < GlobalGrid.SizesXYZ[0]; x += 1)
                {
                    for (int y = 0; y < GlobalGrid.SizesXYZ[1]; y += 1)
                    {
                        for (int z = 0; z < GlobalGrid.SizesXYZ[2]; z += 1)
                        {
                            GlobalShaders.ExternalOverlapingColor = Vector4.One * GlobalGrid.Properties[GlobalGrid.SelectedProperty][x, y, z];
                            GlobalShaders.UpdateForSubModel(GlobalGrid.GridRenderMatrices[x, y, z]);
                            context.Draw(54, 0);
                        }
                    }
                }

                // Present!
                _swapChain.Present(0, PresentFlags.None);
            });

            // Release all resources
            GlobalModel.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
            _renderView.Dispose();
            backBuffer.Dispose();
            context.ClearState();
            context.Flush();
            _device.Dispose();
            context.Dispose();
            _swapChain.Dispose();
            factory.Dispose();
        }

        public static Device GetDevice()
        {
            if (_device != null)
            {
                return _device;
            }
            else
            {
                throw new InvalidOperationException("Can't get device before program initialisation");
            }
        }

        public static void Close()
        {
            MainWindow.GetRenderForm().Close();
        }

        public static Size GetRenderSize()
        {
            return MainWindow.GetRenderForm().ClientSize;
        }
    }
}