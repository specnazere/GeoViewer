using LayerSlicer.Lighting;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;
using static LayerSlicer.GlobalConsts;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace LayerSlicer
{
    internal static class GlobalShaders
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexShaderInput()
        {
            public Matrix WorldViewProj;
            public Matrix ModelRotationTransformation;
            public Matrix ModelFullTransformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ToonShaderInput()
        {
            public Vector4 AmbiendLighting;
            public Vector3 CameraPosition;
            public uint PointLights;
            public uint DirectionalLights;
            public float ToonShadingSteps;
            public float DiffusalComponent;
            public float SpecularComponent;
            public float SpecularDispersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PhongShaderInput()
        {
            public Vector4 AmbiendLighting;
            public Vector3 CameraPosition;
            public uint PointLights;
            public uint DirectionalLights;
            public float DiffusalComponent;
            public float SpecularComponent;
            public float SpecularDispersion;
        }

        private static Device _device;
        private static DeviceContext _deviceContext;
        private static Buffer _pixelConstsBuffer;
        private static Buffer _vertexConstsBuffer;

        private static PixelShader _toonShader;
        private static PixelShader _phongShader;
        private static PixelShader _meshShader;

        private static GlobalConsts.selectedShader _selectedShader;

        private static VertexBufferBinding[] _bufferBindings;

        public static float DiffusalComponent = 1;
        public static float SpecularComponent = 1;
        public static float SpecularDispersion = 2;

        static GlobalShaders()
        {
            _device = Program.GetDevice();
            _deviceContext = _device.ImmediateContext;

            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/vertexShader.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_device, vertexShaderByteCode);

            var toonPixelShaderToonByteCode = ShaderBytecode.CompileFromFile("Shaders/toonShader.fx", "PS_Toon", "ps_4_0");
            _toonShader = new PixelShader(_device, toonPixelShaderToonByteCode);

            var phongPixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/phongShader.fx", "PS_Phong", "ps_4_0");
            _phongShader = new PixelShader(_device, phongPixelShaderByteCode);

            var meshPixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/meshShader.fx", "PS_Mesh", "ps_4_0");
            _meshShader = new PixelShader(_device, meshPixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            // Layout from VertexShader input signature
            var layout = new InputLayout(_device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 1, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, GlobalConsts.Float4BytesSize * 2, 0)
                    });

            // Create Constant Buffer
            _vertexConstsBuffer = new Buffer(
                _device,
                256,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0); 
            _pixelConstsBuffer = new Buffer(
                _device,
                256,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);

            //
            _bufferBindings = new[]
            {
                new VertexBufferBinding(GlobalModel.VerticesByTrianglesBuffer, Utilities.SizeOf<Vector4>() * 3, 0),
                new VertexBufferBinding(GlobalModel.VerticesByLinesBuffer, Utilities.SizeOf<Vector4>() * 3, 0),
            };

            // Prepare All the stages
            _deviceContext.InputAssembler.InputLayout = layout;
            _deviceContext.VertexShader.SetConstantBuffer(1, _vertexConstsBuffer);
            _deviceContext.VertexShader.Set(vertexShader);
            _deviceContext.PixelShader.SetConstantBuffer(0, _pixelConstsBuffer);
        }

        public static void SelectToon()
        {
            _selectedShader = selectedShader.Toon;
            _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[0]);
            _deviceContext.PixelShader.Set(_toonShader);
        }

        public static void SelectPhong()
        {
            _selectedShader = selectedShader.Phong;
            _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[0]);
            _deviceContext.PixelShader.Set(_phongShader);
        }

        public static void SelectMesh()
        {
            _selectedShader = selectedShader.Mesh;
            _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[1]);
            _deviceContext.PixelShader.Set(_meshShader);
        }

        public static void UpdateBuffers()
        {
            var ShaderInput = new VertexShaderInput()
            {
                WorldViewProj = (GlobalModel.Matrix * GlobalCamera.GetMatrix()),
                ModelRotationTransformation = GlobalModel.GetRotationMatrix(),
                ModelFullTransformation = GlobalModel.Matrix,
            };
            ShaderInput.WorldViewProj.Transpose();
            ShaderInput.ModelRotationTransformation.Transpose();
            _deviceContext.UpdateSubresource(ref ShaderInput, _vertexConstsBuffer);

            switch (_selectedShader)
            {
            case GlobalConsts.selectedShader.Toon:
                var toonShaderInput = new ToonShaderInput()
                {
                    AmbiendLighting = GlobalLighting.AmbiendLighting,
                    CameraPosition = GlobalCamera.Position,
                    PointLights = GlobalLighting.GetActivePointLightsCount(),
                    DirectionalLights = GlobalLighting.GetActiveDirectionalLightsCount(),
                    ToonShadingSteps = GlobalLighting.ToonShadingSteps,
                    DiffusalComponent = DiffusalComponent,
                    SpecularComponent = SpecularComponent,
                    SpecularDispersion = SpecularDispersion,
                };
                _deviceContext.UpdateSubresource(ref toonShaderInput, _pixelConstsBuffer);
                break;

            case GlobalConsts.selectedShader.Phong:
                var phongShaderInput = new PhongShaderInput()
                {
                    AmbiendLighting = GlobalLighting.AmbiendLighting,
                    CameraPosition = GlobalCamera.Position,
                    PointLights = GlobalLighting.GetActivePointLightsCount(),
                    DirectionalLights = GlobalLighting.GetActiveDirectionalLightsCount(),
                    DiffusalComponent = DiffusalComponent,
                    SpecularComponent = SpecularComponent,
                    SpecularDispersion = SpecularDispersion,
                };
                _deviceContext.UpdateSubresource(ref phongShaderInput, _pixelConstsBuffer);
                break;

            default:
                break;
            }
        }
    }
}
