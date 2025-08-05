using LayerSlicer.Lighting;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static LayerSlicer.GlobalConsts;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace LayerSlicer
{
    internal static class GlobalShaders
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexColorOverlapShaderInput()
        {
            public Matrix WorldViewProj;
            public Matrix ModelRotationTransformation;
            public Matrix ModelFullTransformation;
            public Vector4 OverlapColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexColorByHeightShaderInput()
        {
            public Matrix WorldViewProj;
            public Matrix ModelRotationTransformation;
            public Matrix ModelFullTransformation;
            public Vector4 OverlapColor;
            public Vector3 TopColor;
            public float TopHeight;
            public Vector3 BotColor;
            public float BotHeight;
            public Vector3 MidColor;
            public float MidRelaiveHeight;
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
        private static PixelShader _strictShader;

        private static VertexColorOverlapShaderInput _lastVSInput = new();

        private static PixelShaderType _selectedShader;
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
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/Vertex/colorOverlapShader.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(_device, vertexShaderByteCode);

            var toonPixelShaderToonByteCode = ShaderBytecode.CompileFromFile("Shaders/Pixel/toonShader.fx", "PS_Toon", "ps_4_0");
            _toonShader = new PixelShader(_device, toonPixelShaderToonByteCode);

            var phongPixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/Pixel/phongShader.fx", "PS_Phong", "ps_4_0");
            _phongShader = new PixelShader(_device, phongPixelShaderByteCode);

            var meshPixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/Pixel/meshShader.fx", "PS_Mesh", "ps_4_0");
            _strictShader = new PixelShader(_device, meshPixelShaderByteCode);

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

        public static void SelectToonPixelShader()
        {
            _selectedShader = PixelShaderType.Toon;
            _deviceContext.PixelShader.Set(_toonShader);
        }

        public static void SelectPhongPixelShader()
        {
            _selectedShader = PixelShaderType.Phong;
            _deviceContext.PixelShader.Set(_phongShader);
        }

        public static void SelectStrictPixelShader()
        {
            _selectedShader = PixelShaderType.Strict;
            _deviceContext.PixelShader.Set(_strictShader);
        }

        public static void SelectMeshPixelShader()
        {
            _selectedShader = PixelShaderType.Mesh;
            _deviceContext.PixelShader.Set(_strictShader);
        }

        public static void SelectNoneOverlap()
        {
            _selectedOverlap = OverlapType.None;
            UpdateOverlap();
        }
        public static void SelectFullOverlap()
        {
            _selectedOverlap = OverlapType.Full;
            UpdateOverlap();
        }
        public static void SelectExternalOverlap()
        {
            _selectedOverlap = OverlapType.External;
            UpdateOverlap();
        }

        public static void SelectGridData()
        {
            _selectedDataType = DataType.Grid;
        }
        public static void SelectSurfaceData()
        {
            _selectedDataType = DataType.Surface;
        }

        public static void Draw()
        {
            switch (_selectedDataType)
            {
            case DataType.Grid:
                    {
                        UdpateShader();
                        switch (_selectedShader)
                        {
                            case PixelShaderType.Toon:
                            case PixelShaderType.Strict:
                            case PixelShaderType.Phong:

                                _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                                _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[0]);

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

                                            ExternalOverlapingColor = GlobalGrid.GetCellColor(x, y, z);
                                            UpdateOverlap();
                                            UpdateForSubModel(GlobalGrid.GridRenderMatrices[x, y, z]);
                                            _deviceContext.Draw(54, 0);
                                        }
                                    }
                                }

                                if (GlobalGrid.ShowBorders)
                                {
                                    _lastVSInput.OverlapColor = Vector4.Zero;
                                    _lastVSInput.OverlapColor.W = 1;
                                    _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);
                                    _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                                    _deviceContext.InputAssembler.SetVertexBuffers(0, _bufferBindings[2]);
                                    _deviceContext.PixelShader.Set(_strictShader);

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
                                                UpdateForSubModel(GlobalGrid.GridRenderMatrices[x, y, z]);
                                                _deviceContext.Draw(54, 0);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                break;
            case DataType.Surface:
                    SelectFullOverlap();
                    UpdateForSubModel(GlobalSurface.ToLocalTransformation);
                    if (true)
                    {
                        switch (_selectedShader)
                        {
                            case PixelShaderType.Toon:
                            case PixelShaderType.Strict:
                            case PixelShaderType.Phong:
                                {
                                    _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                                    _deviceContext.InputAssembler.SetVertexBuffers(0, GlobalSurface.MeshedWellVerticesBuffer);
                                    if (GlobalGrid.ShowBorders)
                                    {
                                        //SwitchToBound();
                                        //_deviceContext.Draw(GlobalSurface.Size[0] * GlobalSurface.Size[1], 0);
                                        //SwitchBack();
                                    }

                                    _deviceContext.Draw(GlobalSurface.MeshedWell.Length, 0);
                                    break;
                                }
                        }
                        /*
                        GlobalShaders.SelectFullOverlap();
                        _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                        _deviceContext.InputAssembler.SetVertexBuffers(0, WellLinesVerticesBuffer);

                        if (GlobalGrid.ShowBorders)
                        {
                            //SwitchToBound();
                            //_deviceContext.Draw(GlobalSurface.Size[0] * GlobalSurface.Size[1], 0);
                            //SwitchBack();
                        }

                        _deviceContext.Draw(Well.Length, 0);
                        */
                    }
                    SelectNoneOverlap();
                    UpdateForSubModel(GlobalSurface.ToLocalTransformation);
                    if (true)
                    {
                        switch (_selectedShader)
                        {
                            case PixelShaderType.Toon:
                            case PixelShaderType.Strict:
                            case PixelShaderType.Phong:
                                {
                                    _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                                    _deviceContext.InputAssembler.SetVertexBuffers(0, GlobalSurface.SurfaceVerticesBuffer);
                                    if (GlobalGrid.ShowBorders)
                                    {
                                        //SwitchToBound();
                                        //_deviceContext.Draw(GlobalSurface.Size[0] * GlobalSurface.Size[1], 0);
                                        //SwitchBack();
                                    }

                                    _deviceContext.Draw(GlobalSurface.RenderSurface.Length, 0);
                                    break;
                                }
                        }
                    }
                    break;
            }
        }

        public static void UpdateBuffers()
        {
            _lastVSInput.WorldViewProj = GlobalModel.Matrix * GlobalCamera.GetMatrix();
            _lastVSInput.ModelRotationTransformation = GlobalModel.GetRotationMatrix();
            _lastVSInput.ModelFullTransformation = GlobalModel.Matrix;

            _lastVSInput.WorldViewProj.Transpose();
            _lastVSInput.ModelRotationTransformation.Transpose();
            _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);

            switch (_selectedShader)
            {
            case PixelShaderType.Toon:
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

            case PixelShaderType.Phong:
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

            _deviceContext.UpdateSubresource(ref _lastVSInput, _vertexConstsBuffer);
        }

        public static void UpdateOverlap()
        {
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
        }

        public static void UdpateShader()
        {
            switch (_selectedShader)
            {
                case PixelShaderType.Toon: _deviceContext.PixelShader.Set(_toonShader); break;
                case PixelShaderType.Phong: _deviceContext.PixelShader.Set(_phongShader); break;
                case PixelShaderType.Mesh: _deviceContext.PixelShader.Set(_strictShader); break;
                case PixelShaderType.Strict: _deviceContext.PixelShader.Set(_strictShader); break;
            }
        }
    }
}
