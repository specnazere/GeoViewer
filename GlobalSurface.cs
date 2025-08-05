using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace LayerSlicer
{
    internal static class GlobalSurface
    {
        private static DeviceContext _deviceContext;
        private static Vector4[] Circle;

        public static Vector2[] SurfaceCorners = new Vector2[2];

        public static Vector4[] RenderSurface;
        public static Vector4[] RenderMesh;

        public static Vector4[] Well;
        public static Vector4[] MeshedWell;

        //public static Buffer MeshBuffer;
        public static Buffer SurfaceBuffer;
        public static VertexBufferBinding SurfaceVerticesBuffer;

        public static Buffer WellLinesBuffer;
        public static VertexBufferBinding WellLinesVerticesBuffer;

        public static Buffer MeshedWellBuffer;
        public static VertexBufferBinding MeshedWellVerticesBuffer;

        public static int[] Size = new int[2];
        public static Vector4 NormalisedSurfaceOffset;

        public static Vector4 TopColor = new(0.9f, 0.9f, 0.9f, 1f);
        public static float TopHeight = 2000;

        public static Vector4 BotColor = new(0.1f, 0.95f, 0.1f, 1f);
        public static float BotHeight = 0;

        public static Vector4 MidColor = new(0.6f, 0.3f, 0f, 1f);
        public static float MidHeight = 1600;

        public static Matrix ToLocalTransformation;
        public static float Scale = float.MinValue;

        static GlobalSurface()
        {
            _deviceContext = Program.GetDevice().ImmediateContext;

            Circle = new Vector4[6];

            float angleDiff = float.Tau / 6;
            for (int i = 0; i < 6; i += 1)
            {
                Circle[i] = new Vector4(
                    (float)Math.Cos(angleDiff * i) * 30f,
                    0, 
                    (float)Math.Sin(angleDiff * i) * 30f, 
                    1);
            }
        }

        public static void ReadSurfFile(string path)
        {
            using (var reader = new StreamReader("Surf"))
            {
                string buffer = reader.ReadLine();

                while (!buffer.Contains("Grid_size:"))
                {
                    buffer = reader.ReadLine();
                }
                var match = Regex.Matches(buffer, GlobalConsts.SizeRegex);

                string firstNumber = match[0].Groups[1].Value;
                string secondNumber = match[0].Groups[2].Value;

                Size[0] = int.Parse(firstNumber);
                Size[1] = int.Parse(secondNumber);

                var SurfacePoints = new Vector4[Size[0], Size[1]];

                while (!buffer.Contains("Grid_space:"))
                {
                    buffer = reader.ReadLine();
                }

                match = Regex.Matches(buffer, GlobalConsts.FloatRegex);

                SurfaceCorners[0] = new Vector2(float.Parse(match[0].Groups[0].Value), float.Parse(match[2].Groups[0].Value));
                SurfaceCorners[1] = new Vector2(float.Parse(match[1].Groups[0].Value), float.Parse(match[3].Groups[0].Value));

                Vector2 SurfaceOffset = (SurfaceCorners[0] + SurfaceCorners[1]) / 2;
                NormalisedSurfaceOffset = new Vector4(SurfaceOffset.X, 0, SurfaceOffset.Y, 0);


                while (buffer.Contains('#'))
                {
                    buffer = reader.ReadLine();
                }

                for (int x = 0; x < Size[0]; x++)
                {
                    for (int y = 0; y < Size[1]; y++)
                    {
                        match = Regex.Matches(buffer, GlobalConsts.FloatRegex);

                        SurfacePoints[x, y] = new Vector4(
                            float.Parse(match[0].Groups[0].Value), 
                            float.Parse(match[2].Groups[0].Value) * -1, 
                            float.Parse(match[1].Groups[0].Value),
                            1) - NormalisedSurfaceOffset;
                        buffer = reader.ReadLine();
                    }
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

                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;

                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;

                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;

                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x + 1, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, SurfacePoints[x, y + 1]);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                        AddHeightColoredVertex(ref RenderSurface, trianglesIndex, QuadCenter);
                        trianglesIndex += GlobalConsts.PerVertexVectors;
                    }
                }

                //MeshBuffer = Buffer.Create(Program.GetDevice(), BindFlags.VertexBuffer, RenderMesh);
                SurfaceBuffer = Buffer.Create(Program.GetDevice(), BindFlags.VertexBuffer, RenderSurface);
                SurfaceVerticesBuffer = new VertexBufferBinding(SurfaceBuffer, Utilities.SizeOf<Vector4>() * GlobalConsts.PerVertexVectors, 0);

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Scale = float.Max(float.Abs(SurfaceCorners[i][j]), Scale);
                    }
                }
                ToLocalTransformation = Matrix.Scaling(1 / Scale);
            }
        }

        public static void AddWell(string path)
        {
            var pointsBuffer = new List<Vector4>();
            using (var reader = new StreamReader("well"))
            {
                string readedPoint = reader.ReadLine();
                MatchCollection match;

                while (!reader.EndOfStream)
                {
                    readedPoint = reader.ReadLine();
                    match = Regex.Matches(readedPoint, GlobalConsts.FloatRegex);

                    pointsBuffer.Add(new Vector4(
                        float.Parse(match[0].Groups[0].Value),
                        float.Parse(match[2].Groups[0].Value),
                        float.Parse(match[1].Groups[0].Value),
                        1) - NormalisedSurfaceOffset);
                }

                Well = new Vector4[GlobalConsts.PerVertexVectors * GlobalConsts.PerLineVertices * (pointsBuffer.Count - 1)];

                for (int i = 0; i < pointsBuffer.Count - 1; i += 1)
                {
                    AddVertex(ref Well, i * GlobalConsts.PerLineVertices,     pointsBuffer[i]);
                    AddVertex(ref Well, i * GlobalConsts.PerLineVertices + 1, pointsBuffer[i + 1]);
                }
            }

            WellLinesBuffer = Buffer.Create(Program.GetDevice(), BindFlags.VertexBuffer, Well);
            WellLinesVerticesBuffer = new VertexBufferBinding(WellLinesBuffer, Utilities.SizeOf<Vector4>() * GlobalConsts.PerVertexVectors, 0);

            MeshedWell = new Vector4[GlobalConsts.PerVertexVectors * GlobalConsts.PerTriangleVertices * 2 * 6 * (pointsBuffer.Count - 1)];

            var Axises = new Vector3[pointsBuffer.Count];
            var Angles = new float[pointsBuffer.Count];

            for (int i = 1; i < (pointsBuffer.Count - 1); i++)
            {
                Vector4 backVector4 = pointsBuffer[i] - pointsBuffer[i - 1];
                Vector3 backVector3 = new Vector3(backVector4.X, backVector4.Y, backVector4.Z);
                Vector4 frontVector4 = pointsBuffer[i] - pointsBuffer[i + 1];
                Vector3 frontVector3 = new Vector3(frontVector4.X, frontVector4.Y, frontVector4.Z);
                Axises[i] = Vector3.Cross(backVector3, frontVector3);
                Axises[i].Normalize();

                Vector3 circleSurfaceVector = Vector3.Normalize(backVector3) + Vector3.Normalize(frontVector3);
                circleSurfaceVector.Normalize();
                Angles[i] = (float.Pi / 2 - (float)Math.Acos(Vector3.Dot(circleSurfaceVector, Vector3.UnitY))) * -1;
            }
            Axises[0] = Axises[1];
            Axises[pointsBuffer.Count - 1] = Axises[pointsBuffer.Count - 2];

            Vector4 firstVector4 = pointsBuffer[0] - pointsBuffer[1];
            Vector3 firstVector3 = new Vector3(firstVector4.X, firstVector4.Y, firstVector4.Z);
            firstVector3.Normalize();
            Angles[0] = (float)Math.Acos(Vector3.Dot(firstVector3, Vector3.UnitY));

            Vector4 lastVector4 = pointsBuffer[pointsBuffer.Count - 2] - pointsBuffer[pointsBuffer.Count - 1];
            Vector3 lastVector3 = new Vector3(lastVector4.X, lastVector4.Y, lastVector4.Z);
            lastVector3.Normalize();
            Angles[pointsBuffer.Count - 1] = (float)Math.Acos(Vector3.Dot(lastVector3, Vector3.UnitY));

            var cuts = new Vector4[pointsBuffer.Count, 6];
            for (int i = 0; i < pointsBuffer.Count; i++)
            {
                var rotationMatrix = Matrix.RotationAxis(Axises[i], Angles[i]);

                for (int j = 0; j < 6; j++)
                {
                    cuts[i, j] = Vector4.Transform(Circle[j], rotationMatrix) + pointsBuffer[i];
                }
            }

            int addedVectors = 0;
            for (int i = 0; i < pointsBuffer.Count - 1; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    int nextIndex = (j + 1) % 6;

                    AddVertex(ref MeshedWell, addedVectors, cuts[i + 1, j]);
                    addedVectors += GlobalConsts.PerVertexVectors;
                    AddVertex(ref MeshedWell, addedVectors, cuts[i, j]);
                    addedVectors += GlobalConsts.PerVertexVectors;
                    AddVertex(ref MeshedWell, addedVectors, cuts[i + 1, nextIndex]);
                    addedVectors += GlobalConsts.PerVertexVectors;

                    AddVertex(ref MeshedWell, addedVectors, cuts[i + 1, nextIndex]);
                    addedVectors += GlobalConsts.PerVertexVectors;
                    AddVertex(ref MeshedWell, addedVectors, cuts[i, j]);
                    addedVectors += GlobalConsts.PerVertexVectors;
                    AddVertex(ref MeshedWell, addedVectors, cuts[i, nextIndex]);
                    addedVectors += GlobalConsts.PerVertexVectors;
                }
            }
            MeshedWellBuffer = Buffer.Create(Program.GetDevice(), BindFlags.VertexBuffer, MeshedWell);
            MeshedWellVerticesBuffer = new VertexBufferBinding(MeshedWellBuffer, Utilities.SizeOf<Vector4>() * GlobalConsts.PerVertexVectors, 0);

            Scale = float.Max(float.Abs(pointsBuffer[0].Y), float.Abs(pointsBuffer[pointsBuffer.Count - 1].Y));
            ToLocalTransformation = Matrix.Scaling(1 / Scale);
        }

        public static void DrawSurface(GlobalConsts.PixelShaderType shaderType)
        {
            switch (shaderType)
            {
            case GlobalConsts.PixelShaderType.Toon:
            case GlobalConsts.PixelShaderType.Strict:
            case GlobalConsts.PixelShaderType.Phong:
                {
                    _deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    _deviceContext.InputAssembler.SetVertexBuffers(0, SurfaceVerticesBuffer);
                    if (GlobalGrid.ShowBorders)
                    {
                        //SwitchToBound();
                        //_deviceContext.Draw(GlobalSurface.Size[0] * GlobalSurface.Size[1], 0);
                        //SwitchBack();
                    }

                    _deviceContext.Draw(Size[0] * Size[1] * 4 * 3, 0);
                    break;
                }
            }
        }

        private static void AddVertex(ref Vector4[] array, int index, Vector4 toAdd)
        {
            array[index + GlobalConsts.VertexPositionOffset] = toAdd;
            array[index + GlobalConsts.VertexColorOffset] = Vector4.One;
            array[index + GlobalConsts.VertexNormalOffset] = Vector4.UnitY;
        }

        private static void AddHeightColoredVertex(ref Vector4[] array, int index, Vector4 toAdd)
        {
            float height = (float)Math.Clamp(toAdd.Y, BotHeight, TopHeight);

            Vector4 color;
            if (height > MidHeight)
            {
                float diff = TopHeight - MidHeight;
                height -= MidHeight;
                color = MidColor * (1 - height / diff) + TopColor * (height / diff);
            }
            else
            {
                color = BotColor * (height / MidHeight) + MidColor * (1 - height / MidHeight);
            }

            array[index + GlobalConsts.VertexPositionOffset] = toAdd;
            array[index + GlobalConsts.VertexColorOffset] = color;
            array[index + GlobalConsts.VertexNormalOffset] = Vector4.UnitY;
        }
    }
}
