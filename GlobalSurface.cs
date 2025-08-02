using SharpDX;
using SharpDX.Direct3D11;
using System.Text.RegularExpressions;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace LayerSlicer
{
    internal static class GlobalSurface
    {
        public static Vector2[] SurfaceCorners = new Vector2[2];

        public static Vector4[] RenderSurface;
        public static Vector4[] RenderMesh;

        //public static Buffer MeshBuffer;
        public static Buffer SurfaceBuffer;

        public static int[] Size = new int[2];

        public static void ReadSurfFile(string path)
        {
            using (var reader = new StreamReader("Surf"))
            {
                string buffer = reader.ReadLine();

                string floatRegex = @"-?[0-9]+.-?[0-9]+";
                string sizeRegex = @"([0-9]+) x ([0-9]+)";

                while (!buffer.Contains("Grid_size:"))
                {
                    buffer = reader.ReadLine();
                }
                var match = Regex.Matches(buffer, sizeRegex);

                string firstNumber = match[0].Groups[1].Value;
                string secondNumber = match[0].Groups[2].Value;

                Size[0] = int.Parse(firstNumber);
                Size[1] = int.Parse(secondNumber);

                var SurfacePoints = new Vector4[Size[0], Size[1]];

                while (!buffer.Contains("Grid_space:"))
                {
                    buffer = reader.ReadLine();
                }

                match = Regex.Matches(buffer, floatRegex);

                SurfaceCorners[0] = new Vector2(float.Parse(match[0].Groups[0].Value), float.Parse(match[2].Groups[0].Value));
                SurfaceCorners[1] = new Vector2(float.Parse(match[1].Groups[0].Value), float.Parse(match[3].Groups[0].Value));

                Vector2 SurfaceOffset = (SurfaceCorners[0] + SurfaceCorners[1]) / 2;
                Vector4 NormalisedSurfaceOffset = new Vector4(SurfaceOffset.X, 0, SurfaceOffset.Y, 0);


                while (buffer.Contains('#'))
                {
                    buffer = reader.ReadLine();
                }

                for (int x = 0; x < Size[0]; x++)
                {
                    for (int y = 0; y < Size[1]; y++)
                    {
                        match = Regex.Matches(buffer, floatRegex);

                        SurfacePoints[x, y] = new Vector4(
                            float.Parse(match[0].Groups[0].Value), 
                            float.Parse(match[2].Groups[0].Value) * -1, 
                            float.Parse(match[1].Groups[0].Value),
                            1) - NormalisedSurfaceOffset;
                        buffer = reader.ReadLine();
                    }
                }

                void AddVertex(ref Vector4[] array, int index, Vector4 toAdd)
                {
                    array[index + GlobalConsts.VertexPositionOffset] = toAdd;
                    array[index + GlobalConsts.VertexColorOffset] = Vector4.One;
                    array[index + GlobalConsts.VertexNormalOffset] = Vector4.UnitZ * -1;
                }

                // WritingLines
                RenderMesh = new Vector4[
                    GlobalConsts.PerVertexVectors *
                    GlobalConsts.PerLineVertices *
                    (
                        ((Size[0] - 1) * (Size[1] - 1)) * 2 + 
                        (Size[0] - 1) +
                        (Size[1] - 1)
                    )];
                int linesIndex = 0;

                for (int x = 0; x < Size[0] - 1; x++)
                {
                    AddVertex(ref RenderMesh, linesIndex, SurfacePoints[x, 0]);
                    linesIndex += GlobalConsts.PerVertexVectors;
                    AddVertex(ref RenderMesh, linesIndex, SurfacePoints[x + 1, 0]);
                    linesIndex += GlobalConsts.PerVertexVectors;
                }

                for (int y = 0; y < Size[1] - 1; y++)
                {
                    AddVertex(ref RenderMesh, linesIndex, SurfacePoints[0, y]);
                    linesIndex += GlobalConsts.PerVertexVectors;
                    AddVertex(ref RenderMesh, linesIndex, SurfacePoints[0, y + 1]);
                    linesIndex += GlobalConsts.PerVertexVectors;
                }

                for (int x = 1; x < Size[0]; x++)
                {
                    for (int y = 1; y < Size[1]; y++)
                    {
                        AddVertex(ref RenderMesh, linesIndex, SurfacePoints[x, y]);
                        linesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderMesh, linesIndex, SurfacePoints[x - 1, y]);
                        linesIndex += GlobalConsts.PerVertexVectors;

                        AddVertex(ref RenderMesh, linesIndex, SurfacePoints[x, y]);
                        linesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderMesh, linesIndex, SurfacePoints[x, y - 1]);
                        linesIndex += GlobalConsts.PerVertexVectors;
                    }
                }

                // Writing triangles
                RenderSurface = new Vector4[
                    GlobalConsts.PerVertexVectors *
                    GlobalConsts.PerTriangleVertices *
                    (Size[0] * Size[1]) * 4];
                int trianglesIndex = 0;

                for (int x = 0; x < Size[0] - 1; x++)
                {
                    for (int y = 0; y < Size[1] - 1; y++)
                    {
                        Vector4 QuadCenter =
                            (
                                SurfacePoints[x, y] +
                                SurfacePoints[x + 1, y] +
                                SurfacePoints[x, y + 1] +
                                SurfacePoints[x + 1, y + 1]
                            ) / 4 ;

                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;

                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;

                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;

                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                    }
                }

                //MeshBuffer = Buffer.Create(Program.GetDevice(), BindFlags.VertexBuffer, RenderMesh);
                SurfaceBuffer = Buffer.Create(Program.GetDevice(), BindFlags.VertexBuffer, RenderSurface);
            }
        }
    }
}
