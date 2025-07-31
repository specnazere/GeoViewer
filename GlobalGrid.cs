using SharpDX;
using System.Runtime.InteropServices;

namespace LayerSlicer
{
    internal static class GlobalGrid
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RawCell
        {
            public bool IsActive;
            public Vector3 Angle0;
            public Vector3 Angle1;
            public Vector3 Angle2;
            public Vector3 Angle3;
            public Vector3 Angle4;
            public Vector3 Angle5;
            public Vector3 Angle6;
            public Vector3 Angle7;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderInfo
        {
            public Vector3 Position;
            public Vector3 Scale;

            public RenderInfo(RawCell rawData)
            {
                Position = (rawData.Angle0 + rawData.Angle5) / 2;
                Position.Y *= -1;

                // Crutches to remove
                Scale.X = (rawData.Angle2.X - rawData.Angle0.X) / 1.3f;
                Scale.Z = (rawData.Angle6.Z - rawData.Angle0.Z) / 2.1f;
                Scale.Y = (rawData.Angle1.Y - rawData.Angle0.Y) / 2f;
            }
        }

        public static Dictionary<string, float[,,]> Properties = new();
        public static string SelectedProperty;
        public static int[] SizesXYZ = new int[3];
        public static Vector3 AveragePosition;
        public static Vector3 SummScale;
        public static Matrix Rotation;
        public static Matrix[,,] GridRenderMatrices;
        public static int CellsNum;

        public static unsafe void ReadGridFile(string path)
        {
            var fileBytes = File.ReadAllBytes(path);

            fixed (byte* bytesPtr = &fileBytes[0])
            {
                int* size = (int*)bytesPtr;
                SizesXYZ = [size[0], size[2], size[1]];
                GridRenderMatrices = new Matrix[SizesXYZ[0], SizesXYZ[1], SizesXYZ[2]];

                RawCell* data = (RawCell*)(bytesPtr + 12);

                RenderInfo[,,] generalInfo = new RenderInfo[SizesXYZ[0], SizesXYZ[1], SizesXYZ[2]];
                CellsNum = SizesXYZ[0] * SizesXYZ[1] * SizesXYZ[2];

                Vector3 diff = data[0].Angle0 - data[SizesXYZ[2] - 1].Angle0;
                diff.Normalize();
                Rotation = Matrix.RotationYawPitchRoll(
                    (float)Math.Acos(Vector3.Dot(diff, Vector3.UnitZ)) + float.Pi / 2, 0, 0);

                for (int y = 0, i = 0; y < SizesXYZ[1]; y += 1)
                {
                    for (int x = 0; x < SizesXYZ[0]; x += 1)
                    {
                        for (int z = 0; z < SizesXYZ[2]; z += 1)
                        {
                            generalInfo[x, y, z] = new RenderInfo(data[i++]);
                        }
                    }
                }

                AveragePosition = (generalInfo[0, 0, 0].Position + generalInfo[SizesXYZ[0] - 1, SizesXYZ[1] - 1, SizesXYZ[2] - 1].Position) / 2;

                for (int x = 0; x < SizesXYZ[0]; x += 1)
                {
                    for (int y = 0; y < SizesXYZ[1]; y += 1)
                    {
                        for (int z = 0; z < SizesXYZ[2]; z += 1)
                        {
                            generalInfo[x, y, z].Position -= AveragePosition;
                            Vector3.TransformCoordinate(ref generalInfo[x, y, z].Position, ref Rotation, out generalInfo[x, y, z].Position);

                            GridRenderMatrices[x, y, z] =
                                Matrix.Transformation(
                                    Vector3.Zero,
                                    Quaternion.Identity,
                                    generalInfo[x, y, z].Scale,
                                    Vector3.Zero,
                                    Quaternion.Identity,
                                    generalInfo[x, y, z].Position);
                        }
                    }
                }
            }
            return;
        }

        public static void ReadPropertiesFile(string path)
        {
            using (var reader = new StreamReader("grid.binprops.txt"))
            {
                int PropertiesCount = int.Parse(reader.ReadLine());

                string line;

                for (int propertyNum = 0; propertyNum < PropertiesCount; propertyNum++)
                {
                    string name = reader.ReadLine();
                    MainWindow.GetInteractForm().ConnectGridProperty(name);
                    Properties.Add(name, new float[SizesXYZ[0], SizesXYZ[1], SizesXYZ[2]]);

                    float min = float.MaxValue;
                    float max = float.MinValue;

                    for (int x = 0; x < SizesXYZ[0]; x += 1)
                    {
                        for (int z = 0; z < SizesXYZ[2]; z += 1)
                        {
                            for (int y = 0; y < SizesXYZ[1]; y += 1)
                            {
                                float value = float.Parse(reader.ReadLine());
                                Properties[name][x, y, z] = value;
                                min = float.Min(value, min);
                                max = float.Max(value, max);
                            }
                        }
                    }
                    float amplitude = max - min;

                    for (int x = 0, i = 0; x < SizesXYZ[0]; x += 1)
                    {
                        for (int y = 0; y < SizesXYZ[1]; y += 1)
                        {
                            for (int z = 0; z < SizesXYZ[2]; z += 1)
                            {
                                Properties[name][x, y, z] = 1 - (Properties[name][x, y, z] - min) / amplitude;
                            }
                        }
                    }
                }
            }
        }
    }
}
