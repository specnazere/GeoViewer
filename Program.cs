using LayerSlicer.Lighting;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
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
            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(
                        MainWindow.GetRenderForm().ClientSize.Width, 
                        MainWindow.GetRenderForm().ClientSize.Height,
                        new Rational(60, 1), 
                        Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = MainWindow.GetRenderForm().Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out _device, out _swapChain);
            var context = _device.ImmediateContext;

            GlobalModel.VerticesBuffer = Buffer.Create(GetDevice(), BindFlags.VertexBuffer, GlobalConsts.CubeVertices);

            // Ignore all windows events
            var factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(MainWindow.GetRenderForm().Handle, WindowAssociationFlags.IgnoreAll);

            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(_device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            var layout = new InputLayout(_device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 1, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 2, 0)
                    });

            // Create Constant Buffer
            var contantBuffer = new Buffer(
                _device,
                Utilities.SizeOf<ShaderMatricesInput>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);

            // Prepare All the stages
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(GlobalModel.VerticesBuffer, Utilities.SizeOf<Vector4>() * 3, 0));
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.SetConstantBuffer(0, contantBuffer);
            context.PixelShader.Set(pixelShader);

            MainWindow.InitInteractForm();

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
                        desc.BufferCount, 
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
                        SampleDescription = new SampleDescription(1, 0),
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

                // Update Matrices
                var ShaderInput = new ShaderMatricesInput()
                {
                    WorldViewProj = (GlobalModel.Matrix * GlobalCamera.GetMatrix()),
                    ModelRotationTransformation = GlobalModel.GetRotationMatrix(),
                    CameraRotationTransformation = GlobalCamera.GetRotationMatrix(),
                    ProjectionTransformation = GlobalCamera.GetProjMatrix(),
                    CameraPosition = GlobalCamera.Position,
                    PointLights = GlobalLighting.GetActivePointLightsCount(),
                };
                ShaderInput.WorldViewProj.Transpose();
                ShaderInput.ModelRotationTransformation.Transpose();
                ShaderInput.CameraRotationTransformation.Transpose();
                ShaderInput.ProjectionTransformation.Transpose();
                context.UpdateSubresource(ref ShaderInput, contantBuffer);

                // Draw the cube
                context.Draw(54, 0);

                // Present!
                _swapChain.Present(0, PresentFlags.None);
            });

            // Release all resources
            signature.Dispose();
            vertexShaderByteCode.Dispose();
            vertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            pixelShader.Dispose();
            GlobalModel.Dispose();
            layout.Dispose();
            contantBuffer.Dispose();
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