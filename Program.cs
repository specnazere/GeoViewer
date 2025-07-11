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
        private static Form _form;

        private static RenderTargetView _renderView;

        //      [STAThread]
        private static void Main()
        {
            _form = new Form()
            {
                Text = "SharpDX Color Transition",
                ClientSize = new Size(800, 600),
                StartPosition = FormStartPosition.CenterScreen
            };

            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(_form.ClientSize.Width, _form.ClientSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = _form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out _device, out _swapChain);
            var context = _device.ImmediateContext;

            // Ignore all windows events
            var factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(_form.Handle, WindowAssociationFlags.IgnoreAll);

            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(_device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            var layout = new InputLayout(_device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                    });

            // Create Constant Buffer
            var contantBuffer = new Buffer(_device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            // Prepare All the stages
            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(GlobalModel.CubeVertices, Utilities.SizeOf<Vector4>() * 2, 0));
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            // Use clock
            GlobalUpdater.Update();

            // Declare texture for rendering
            bool userResized = true;
            Texture2D backBuffer = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;

            // Setup handlers
            _form.Resize += (sender, args) => userResized = true;
            _form.KeyUp += EventHandler.KeyUp;
            _form.KeyDown += EventHandler.KeyDown;
            _form.MouseDown += EventHandler.MouseDown;
            _form.MouseUp += EventHandler.MouseUp;
            _form.MouseMove += EventHandler.MouseMove;
            _form.MouseWheel += EventHandler.WheelScroll;

            // Prepare global values
            GlobalCamera.Position = new Vector3(0, 0, -5f);

            // Main loop
            RenderLoop.Run(_form, () =>
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
                    _swapChain.ResizeBuffers(desc.BufferCount, _form.ClientSize.Width, _form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

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
                        Width = _form.ClientSize.Width,
                        Height = _form.ClientSize.Height,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    // Create the depth buffer view
                    depthView = new DepthStencilView(_device, depthBuffer);

                    // Setup targets and viewport for rendering
                    GlobalCamera.UpdatePerspectiveMatrix();
                    context.Rasterizer.SetViewport(new Viewport(0, 0, _form.ClientSize.Width, _form.ClientSize.Height, 0.0f, 1.0f));
                    context.OutputMerger.SetTargets(depthView, _renderView);

                    // We are done resizing
                    userResized = false;
                }

                // Clear views
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(_renderView, SharpDX.Color.Black);

                // Update before render
                GlobalUpdater.Update();

                // Update WorldViewProj Matrix
                var worldViewProj = GlobalCamera.GetMatrix();
                worldViewProj.Transpose();
                context.UpdateSubresource(ref worldViewProj, contantBuffer);

                // Draw the cube
                context.Draw(36, 0);

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
            _form.Close();
        }

        public static Size GetRenderSize()
        {
            return _form.ClientSize;
        }
    }
}