using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace LayerSlicer
{
    internal static class GlobalShaders
    {
        private static Device _device;
        private static DeviceContext _deviceContext;
        private static Buffer _constsBuffer;

        private static PixelShader _ToonShader;
        private static PixelShader _PhongShader;

        static GlobalShaders()
        {
            _device = Program.GetDevice();
            _deviceContext = _device.ImmediateContext;

            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_device, vertexShaderByteCode);

            var toonPixelShaderToonByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "PS_Toon", "ps_4_0");
            _ToonShader = new PixelShader(_device, toonPixelShaderToonByteCode);

            var pixelShaderPhongByteCode = ShaderBytecode.CompileFromFile("shaders.fx", "PS_Phong", "ps_4_0");
            _PhongShader = new PixelShader(_device, pixelShaderPhongByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            var layout = new InputLayout(_device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 1, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 2, 0)
                    });

            // Create Constant Buffer
            _constsBuffer = new Buffer(
                _device,
                Utilities.SizeOf<ShaderConstDataInput>() + 8,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);

            // Prepare All the stages
            _deviceContext.InputAssembler.InputLayout = layout;
            _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            _deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(GlobalModel.VerticesBuffer, Utilities.SizeOf<Vector4>() * 3, 0));
            _deviceContext.VertexShader.SetConstantBuffer(0, _constsBuffer);
            _deviceContext.VertexShader.Set(vertexShader);
            _deviceContext.PixelShader.SetConstantBuffer(0, _constsBuffer);
        }

        public static void SelectToon()
        {
            _deviceContext.PixelShader.Set(_ToonShader);
        }

        public static void SelectPhong()
        {
            _deviceContext.PixelShader.Set(_PhongShader);
        }

        public static Buffer GetBuffer()
        {
            return _constsBuffer;
        }
    }
}
