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
        private static List<(uint, string)> _handlersConnection = new();
        private static SharpDX.Direct3D11.Buffer _pointLightsBuffer;
        private static uint _activePointLightsCount = 0;
        private static bool _isDirty = true;

        private static DeviceContext _deviceContext;

        public static event EventHandler LightingChanged;

        static GlobalLighting()
        {
            LightingChanged += (sender, e) => OnLightingChanged();

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
            _deviceContext = Program.GetDevice().ImmediateContext;

            for (int i = 0; i < GlobalConsts.MaxLightsPerType; i++)
            {
                _pointLightObjects[i] = new PointLightObject();
            }
        }

        public static bool AddPointLight(PointLightObject light, string name)
        {
            if (_activePointLightsCount >= GlobalConsts.MaxLightsPerType)
                return false;

            _pointLightObjects[_activePointLightsCount] = light;
            _handlersConnection.Add((_activePointLightsCount, name));
            _activePointLightsCount++;

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
                (var pointLightIndex, var name) = _handlersConnection[i];
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

                (var pointLightIndex, var name) = _handlersConnection[i];
                if (pointLightIndex > index)
                {
                    _handlersConnection[i] = _handlersConnection[i + 1];
                }
            }

            _pointLightObjects[_activePointLightsCount--] = new PointLightObject();
            MarkDirty();
            return true;
        }

        public static void ClearPointLights()
        {
            _activePointLightsCount = 0;
            MarkDirty();
        }

        public static uint GetActivePointLightsCount() => _activePointLightsCount;

        public static bool IsDirty() => _isDirty;

        public static void MarkClean() => _isDirty = false;

        private static void MarkDirty()
        {
            _isDirty = true;
            LightingChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void OnLightingChanged()
        {
            var dataBox = _deviceContext.MapSubresource(
                _pointLightsBuffer,
                MapMode.WriteDiscard,
                MapFlags.None,
                out var stream);

            for (int i = 0; i < _activePointLightsCount; i++)
            {
                _pointLightStructs[i] = _pointLightObjects[i].AsStruct();
            }

            SharpDX.Utilities.Write(dataBox.DataPointer, _pointLightStructs, 0, (int)_activePointLightsCount);
            _deviceContext.UnmapSubresource(_pointLightsBuffer, 0);

            var srvDescription = new ShaderResourceViewDescription
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
            var _pointLightsBufferShaderResourceView = new ShaderResourceView(Program.GetDevice(), _pointLightsBuffer, srvDescription);

            _deviceContext.PixelShader.SetShaderResource(0, _pointLightsBufferShaderResourceView);
        }
    }
}