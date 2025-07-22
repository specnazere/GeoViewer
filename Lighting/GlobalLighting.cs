using LayerSlicer.Lighting.Structs;
using SharpDX.Direct3D11;
using System;
using System.Linq;

namespace LayerSlicer.Lighting
{
    internal static class GlobalLighting
    {
        private static PointLight[] _pointLights = new PointLight[GlobalConsts.MaxLightsPerType];
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
                    SizeInBytes = SharpDX.Utilities.SizeOf<PointLight>() * GlobalConsts.MaxLightsPerType,
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = SharpDX.Utilities.SizeOf<PointLight>()
                });
            _deviceContext = Program.GetDevice().ImmediateContext;
        }

        public static bool AddPointLight(PointLight light)
        {
            if (_activePointLightsCount >= GlobalConsts.MaxLightsPerType)
                return false;

            _pointLights[_activePointLightsCount] = light;
            _activePointLightsCount++;
            MarkDirty();
            return true;
        }

        public static bool UpdatePointLight(int index, PointLight light)
        {
            if (index < 0 || index >= _activePointLightsCount)
                return false;

            _pointLights[index] = light;
            MarkDirty();
            return true;
        }

        public static bool RemovePointLight(int index)
        {
            if (index < 0 || index >= _activePointLightsCount)
                return false;

            // Сдвигаем все элементы после удаляемого
            for (int i = index; i < _activePointLightsCount - 1; i++)
            {
                _pointLights[i] = _pointLights[i + 1];
            }

            _activePointLightsCount--;
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

            SharpDX.Utilities.Write(dataBox.DataPointer, _pointLights, 0, (int)_activePointLightsCount);
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