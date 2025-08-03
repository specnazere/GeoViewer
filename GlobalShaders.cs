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
            public Vector4 OverlapColor;
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

        private static VertexShaderInput _lastVSInput;

        private static ShaderType _selectedShader;
        private static OverlapType _selectedOverlap;
        private static DataType _selectedDataType;

        private static VertexBufferBinding[] _bufferBindings;

        public static float DiffusalComponent = 1;
        public static float SpecularComponent = 1;
        public static float SpecularDispersion = 2;

        public static Vector4 ExternalOverlapingColor = Vector4.Zero;

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
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, Float4BytesSize * 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, Float4BytesSize * 1, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Float4BytesSize * 2, 0)
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
                new VertexBufferBinding(GlobalModel.VerticesByTrianglesBuffer, Utilities.SizeOf<Vector4>() * PerVertexVectors, 0),
                new VertexBufferBinding(GlobalModel.VerticesByLinesBuffer, Utilities.SizeOf<Vector4>() * PerVertexVectors, 0),
                new VertexBufferBinding(GlobalModel.BoundBuffer, Utilities.SizeOf<Vector4>() * PerVertexVectors, 0),

                new VertexBufferBinding(GlobalSurface.SurfaceBuffer, Utilities.SizeOf<Vector4>() * PerVertexVectors, 0),
                //new VertexBufferBinding(GlobalSurface.MeshBuffer, Utilities.SizeOf<Vector4>() * PerVertexVectors, 0),
            };

            // Prepare All the stages
            _deviceContext.InputAssembler.InputLayout = layout;
            _deviceContext.VertexShader.SetConstantBuffer(1, _vertexConstsBuffer);
            _deviceContext.VertexShader.Set(vertexShader);
            _deviceContext.PixelShader.SetConstantBuffer(0, _pixelConstsBuffer);
        }

        public static void SelectToonShader()
        {
            _selectedShader = ShaderType.Toon;
            _deviceContext.PixelShader.Set(_toonShader);
        }

        public static void SelectPhongShader()
        {
            _selectedShader = ShaderType.Phong;
            _deviceContext.PixelShader.Set(_phongShader);
        }

        public static void SelectStrictShader()
        {
            _selectedShader = ShaderType.Strict;
            _deviceContext.PixelShader.Set(_meshShader);
        }

        public static void SelectMeshShader()
        {
            _selectedShader = ShaderType.Mesh;
            _deviceContext.PixelShader.Set(_meshShader);
        }

        public static void SelectNoneOverlap()
        {
            _selectedOverlap = OverlapType.None;
        }
        public static void SelectFullOverlap()
        {
            _selectedOverlap = OverlapType.Full;
        }
        public static void SelectExternalOverlap()
        {
            _selectedOverlap = OverlapType.External;
        }

        public static void SelectGridData()
        {
            _selectedDataType = DataType.Grid;
            SwitchBack();
        }
        public static void SelectSurfaceData()
        {
            _selectedDataType = DataType.Surface;
            SwitchBack();
        }

        public static void Draw()
        {
            switch (_selectedDataType)
            {
            case DataType.Grid:
                {
                    for (int x = 0; x < GlobalGrid.SizesXYZ[0]; x += 1)
                    {
                        for (int y = 0; y < GlobalGrid.SizesXYZ[1]; y += 1)
                        {
                            for (int z = 0; z < GlobalGrid.SizesXYZ[2]; z += 1)
                            {
                                if (GlobalGrid.DrawSlices)
                                {
                                    if (x != GlobalGrid.DrawableSlices[0] &&
                                        y != GlobalGrid.DrawableSlices[1] &&
                                        z != GlobalGrid.DrawableSlices[2])
                                    {
                                        continue;
                                    }
                                }

                                if (GlobalGrid.ShowBorders)
                                {
                                    SwitchToBound();
                                    _deviceContext.Draw(54, 0);
                                    SwitchBack();
                                }

                                ExternalOverlapingColor = GlobalGrid.GetCellColor(x, y, z);
                                UpdateForSubModel(GlobalGrid.GridRenderMatrices[x, y, z]);
                                _deviceContext.Draw(54, 0);
                            }
                        }
                    }
                }
                break;
            case DataType.Surface:
                GlobalSurface.Draw(_selectedShader);
                break;
            }
        }

        public static void UpdateBuffers()
        {
            var _lastVSInput = new VertexShaderInput()
            {
                WorldViewProj = GlobalModel.Matrix * GlobalCamera.GetMatrix(),
                ModelRotationTransformation = GlobalModel.GetRotationMatrix(),
                ModelFullTransformation = GlobalModel.Matrix,
            };

            _lastVSInput.WorldViewProj.Transpose();
            _lastVSInput.ModelRotationTransformation.Transpose();
            _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);

            switch (_selectedShader)
            {
            case ShaderType.Toon:
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

            case ShaderType.Phong:
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

        public static void UpdateForSubModel(Matrix subTransformationMatrix)
        {
            _lastVSInput.WorldViewProj = (subTransformationMatrix * GlobalModel.Matrix * GlobalCamera.GetMatrix());
            _lastVSInput.WorldViewProj.Transpose();
            _lastVSInput.ModelFullTransformation = subTransformationMatrix * GlobalModel.Matrix;
            _lastVSInput.ModelFullTransformation.Transpose();

            switch (_selectedOverlap)
            {
            case OverlapType.None:
                _lastVSInput.OverlapColor = Vector4.Zero;
                break;

            case OverlapType.Full:
                _lastVSInput.OverlapColor = Vector4.One;
                break;

            case OverlapType.External:
                ExternalOverlapingColor.W = 1f;
                _lastVSInput.OverlapColor = ExternalOverlapingColor;
                break;
            }

            _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);
        }

        public static void SwitchToBound()
        {
            switch (_selectedDataType)
            {
            case DataType.Grid:
                _lastVSInput.OverlapColor = Vector4.Zero;
                _lastVSInput.OverlapColor.W = 1;
                _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);
                _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[2]);
                _deviceContext.PixelShader.Set(_meshShader);
                break;

            case DataType.Surface:
                _lastVSInput.OverlapColor = Vector4.Zero;
                _lastVSInput.OverlapColor.W = 1;
                _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);
                _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[2]);
                _deviceContext.PixelShader.Set(_meshShader);
                break;
            }
        }

        public static void SwitchBack()
        {
            switch (_selectedShader)
            {
                case ShaderType.Toon: SelectToonShader(); break;
                case ShaderType.Phong: SelectPhongShader(); break;
                case ShaderType.Mesh: SelectMeshShader(); break;
                case ShaderType.Strict: SelectStrictShader(); break;
            }
        }
    }
}
