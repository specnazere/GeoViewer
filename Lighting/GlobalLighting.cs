using LayerSlicer.Forms;
using LayerSlicer.Lighting.Structs;
using SharpDX.Direct3D11;
using System;

namespace LayerSlicer.Lighting
{
    internal static class GlobalLighting
    {
        private static PointLightStruct[] _pointLightStructs = new PointLightStruct[GlobalConsts.MaxLightsPerType];
        private static PointLightObject[] _pointLightObjects = new PointLightObject[GlobalConsts.MaxLightsPerType];
        private static List<(uint, string)> _pointHandlersConnection = new();
        private static SharpDX.Direct3D11.Buffer _pointLightsBuffer;
        private static uint _activePointLightsCount = 0;

        private static DirectionalLightStruct[] _directionalLightStructs = new DirectionalLightStruct[GlobalConsts.MaxLightsPerType];
        private static DirectionalLightObject[] _directionalLightObjects = new DirectionalLightObject[GlobalConsts.MaxLightsPerType];
        private static List<(uint, string)> _directionalHandlersConnection = new();
        private static SharpDX.Direct3D11.Buffer _directionalLightsBuffer;
        private static uint _activeDirectionalLightsCount = 0;

        private static bool _isDirty = true;

        private static DeviceContext _deviceContext;

        public static event EventHandler LightingChanged;

        static GlobalLighting()
        {
            LightingChanged += (sender, e) => OnLightingChanged();
            _deviceContext = Program.GetDevice().ImmediateContext;

            _pointLightsBuffer = new SharpDX.Direct3D11.Buffer(
                Program.GetDevice(),
                new BufferDescription
                {
                    SizeInBytes = SharpDX.Utilities.SizeOf<PointLightStruct>() * GlobalConsts.MaxLightsPerType,
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = SharpDX.Utilities.SizeOf<PointLightStruct>()
                });

            for (int i = 0; i < GlobalConsts.MaxLightsPerType; i++)
            {
                _pointLightObjects[i] = new PointLightObject();
            }

            _directionalLightsBuffer = new SharpDX.Direct3D11.Buffer(
                Program.GetDevice(),
                new BufferDescription
                {
                    SizeInBytes = SharpDX.Utilities.SizeOf<DirectionalLightStruct>() * GlobalConsts.MaxLightsPerType,
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = SharpDX.Utilities.SizeOf<DirectionalLightStruct>()
                });

            for (int i = 0; i < GlobalConsts.MaxLightsPerType; i++)
            {
                _directionalLightObjects[i] = new DirectionalLightObject();
            }
        }

        public static bool AddPointLight(PointLightObject light, string name)
        {
            if (_activePointLightsCount >= GlobalConsts.MaxLightsPerType)
                return false;

            _pointLightObjects[_activePointLightsCount] = light;
            _pointHandlersConnection.Add((_activePointLightsCount, name));
            _activePointLightsCount++;

            light.DataChanged += (sender, e) => OnLightingChanged();

            MarkDirty();
            return true;
        }

        public static bool AddDirectionalLight(DirectionalLightObject light, string name)
        {
            if (_activeDirectionalLightsCount >= GlobalConsts.MaxLightsPerType)
                return false;

            _directionalLightObjects[_activeDirectionalLightsCount] = light;
            _directionalHandlersConnection.Add((_activeDirectionalLightsCount, name));
            _activeDirectionalLightsCount++;

            light.DataChanged += (sender, e) => OnLightingChanged();

            MarkDirty();
            return true;
        }

        public static bool RemovePointLight(string nameToDelete)
        {
            int index = 0;
            bool find = false;
            for (int i = 0; i < _activePointLightsCount; i++)
            {
                (var pointLightIndex, var name) = _pointHandlersConnection[i];
                if (name == nameToDelete)
                {
                    index = i;
                    find = true;
                    break;
                }
            }

            if (!find) 
                return false;

            if (index < 0 || index >= _activePointLightsCount)
                return false;

            // Сдвигаем все элементы после удаляемого
            for (int i = index; i < _activePointLightsCount - 1; i++)
            {
                _pointLightObjects[i] = _pointLightObjects[i + 1];

                (var pointLightIndex, var name) = _pointHandlersConnection[i];
                if (pointLightIndex > index)
                {
                    _pointHandlersConnection[i] = _pointHandlersConnection[i + 1];
                }
            }

            _pointLightObjects[_activePointLightsCount--] = new PointLightObject();
            MarkDirty();
            return true;
        }

        public static bool RemoveDirectionalLight(string nameToDelete)
        {
            int index = 0;
            bool find = false;
            for (int i = 0; i < _activeDirectionalLightsCount; i++)
            {
                (var directionalLightIndex, var name) = _directionalHandlersConnection[i];
                if (name == nameToDelete)
                {
                    index = i;
                    find = true;
                    break;
                }
            }

            if (!find)
                return false;

            if (index < 0 || index >= _activeDirectionalLightsCount)
                return false;

            // Сдвигаем все элементы после удаляемого
            for (int i = index; i < _activeDirectionalLightsCount - 1; i++)
            {
                _directionalLightObjects[i] = _directionalLightObjects[i + 1];

                (var directionalLightIndex, var name) = _directionalHandlersConnection[i];
                if (directionalLightIndex > index)
                {
                    _directionalHandlersConnection[i] = _directionalHandlersConnection[i + 1];
                }
            }

            _directionalLightObjects[_activeDirectionalLightsCount--] = new DirectionalLightObject();
            MarkDirty();
            return true;
        }

        public static void ClearPointLights()
        {
            _activePointLightsCount = 0;
            MarkDirty();
        }

        public static void ClearDirectionalLights()
        {
            _activeDirectionalLightsCount = 0;
            MarkDirty();
        }

        public static uint GetActivePointLightsCount() => _activePointLightsCount;

        public static uint GetActiveDirectionalLightsCount() => _activeDirectionalLightsCount;

        public static bool IsDirty() => _isDirty;

        public static void MarkClean() => _isDirty = false;

        private static void MarkDirty()
        {
            _isDirty = true;
            LightingChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void OnLightingChanged()
        {
            var pointDataBox = _deviceContext.MapSubresource(
                _pointLightsBuffer,
                MapMode.WriteDiscard,
                MapFlags.None,
                out var stream1);

            for (int i = 0; i < _activePointLightsCount; i++)
            {
                _pointLightStructs[i] = _pointLightObjects[i].AsStruct();
            }

            SharpDX.Utilities.Write(pointDataBox.DataPointer, _pointLightStructs, 0, (int)_activePointLightsCount);
            _deviceContext.UnmapSubresource(_pointLightsBuffer, 0);

            var srvPointDescription = new ShaderResourceViewDescription
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource
                {
                    ElementWidth = sizeof(float) * 8, // Размер структуры PointLight (если она состоит из 8 float)
                    FirstElement = 0,
                    ElementCount = GlobalConsts.MaxLightsPerType
                }
            };
            var _pointLightsBufferShaderResourceView = new ShaderResourceView(Program.GetDevice(), _pointLightsBuffer, srvPointDescription);

            _deviceContext.PixelShader.SetShaderResource(0, _pointLightsBufferShaderResourceView);


            var directionalDataBox = _deviceContext.MapSubresource(
                _directionalLightsBuffer,
                MapMode.WriteDiscard,
                MapFlags.None,
                out var stream2);

            for (int i = 0; i < _activeDirectionalLightsCount; i++)
            {
                _directionalLightStructs[i] = _directionalLightObjects[i].AsStruct();
            }

            SharpDX.Utilities.Write(directionalDataBox.DataPointer, _directionalLightStructs, 0, (int)_activeDirectionalLightsCount);
            _deviceContext.UnmapSubresource(_directionalLightsBuffer, 0);

            var srvDirectionalDescription = new ShaderResourceViewDescription
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource
                {
                    ElementWidth = sizeof(float) * 8, // Размер структуры DirectionalLight (если она состоит из 8 float)
                    FirstElement = 0,
                    ElementCount = GlobalConsts.MaxLightsPerType
                }
            };
            var _directionalLightsBufferShaderResourceView = new ShaderResourceView(Program.GetDevice(), _directionalLightsBuffer, srvDirectionalDescription);

            _deviceContext.PixelShader.SetShaderResource(1, _directionalLightsBufferShaderResourceView);
        }
    }
}