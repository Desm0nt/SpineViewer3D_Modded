using System.IO;

namespace Volot.DescriptionOfGeometry.STL
{
    internal class StlWriter
    {
        private const string FloatFormat = "e6";

        public void Write(StlFile file, Stream stream, bool asAscii)
        {
            if (asAscii)
                WriteAscii(file, stream);
            else
                WriteBinary(file, stream);
        }

        private void WriteAscii(StlFile file, Stream stream)
        {
            var writer = new StreamWriter(stream);
            writer.WriteLine($"solid {file.SolidName}");
            foreach (var triangle in file.Triangles)
            {
                writer.WriteLine($"  facet normal {NormalToString(triangle.Normal).Replace(",", ".")}");
                writer.WriteLine("    outer loop");
                writer.WriteLine($"      vertex {VertexToString(triangle.Vertex1).Replace(",",".")}");
                writer.WriteLine($"      vertex {VertexToString(triangle.Vertex2).Replace(",", ".")}");
                writer.WriteLine($"      vertex {VertexToString(triangle.Vertex3).Replace(",", ".")}");
                writer.WriteLine("    endloop");
                writer.WriteLine("  endfacet");
            }

            writer.WriteLine($"endsolid {file.SolidName}");
            writer.Flush();
        }

        private void WriteBinary(StlFile file, Stream stream)
        {
            var writer = new BinaryWriter(stream);

            // write header
            var header = new byte[80]; // can be a garbage value
            writer.Write(header);

            // write vertex count
            writer.Write((uint)file.Triangles.Count);

            // write triangles
            foreach (var triangle in file.Triangles)
            {
                
                writer.Write(triangle.Normal.X);
                writer.Write(triangle.Normal.Y);
                writer.Write(triangle.Normal.Z);

                writer.Write(triangle.Vertex1.X);
                writer.Write(triangle.Vertex1.Y);
                writer.Write(triangle.Vertex1.Z);

                writer.Write(triangle.Vertex2.X);
                writer.Write(triangle.Vertex2.Y);
                writer.Write(triangle.Vertex2.Z);

                writer.Write(triangle.Vertex3.X);
                writer.Write(triangle.Vertex3.Y);
                writer.Write(triangle.Vertex3.Z);

                writer.Write((ushort)0); // garbage value
            }

            writer.Flush();
        }

        private static string NormalToString(StlNormal normal)
        {
            return
                $"{normal.X.ToString(FloatFormat)} {normal.Y.ToString(FloatFormat)} {normal.Z.ToString(FloatFormat)}";
        }

        private static string VertexToString(Vertex vertex)
        {
            return
                $"{(vertex.X - 500).ToString(FloatFormat)} {(vertex.Y-500).ToString(FloatFormat)} {vertex.Z.ToString(FloatFormat)}";
        }
    }
}