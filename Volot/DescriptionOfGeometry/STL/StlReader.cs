using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Volot.DescriptionOfGeometry.STL
{
    internal class StlReader
    {
        private readonly Stream _baseStream;
        private readonly BinaryReader _binReader;
        private IEnumerator<string> _tokenEnumerator;
        private bool _isAscii;
        private uint _triangleCount;
        private readonly Regex _headerReg = new Regex("^solid (.*)$");

        public StlReader(Stream stream)
        {
            _baseStream = stream;
            _binReader = new BinaryReader(stream);
        }

        public string ReadSolidName()
        {
            int i;
            bool headerComplete = false;
            var sb = new StringBuilder();
            for (i = 0; i < 80 && !headerComplete; i++)
            {
                var b = _binReader.ReadByte();
                if (b == '\n')
                {
                    _isAscii = true;
                    headerComplete = true;
                }
                else if (b == 0)
                {
                    _isAscii = false;
                    headerComplete = true;
                }
                else
                {
                    sb.Append((char)b);
                }
            }

            var header = sb.ToString().Trim();
            var match = _headerReg.Match(header);
            if (match.Success)
            {
                header = match.Groups[1].Value;
            }
            else
            {
                header = null;
            }

            if (_isAscii)
            {
                _tokenEnumerator = new StlTokenStream(_baseStream);
                _tokenEnumerator.MoveNext();
            }
            else
            {
                // swallow the remainder of the header
                for (; i < 80; i++)
                    _binReader.ReadByte();

                // get count
                _triangleCount = _binReader.ReadUInt32();
            }

            return header;
        }

        public List<StlTriangle> ReadTriangles()
        {
            var triangles = new List<StlTriangle>();
            if (_isAscii)
            {
                var t = ReadTriangle();
                while (t != null)
                {
                    EnsureCorrectNormal(t);
                    triangles.Add(t);
                    t = ReadTriangle();
                }
            }
            else
            {
                for (uint i = 0; i < _triangleCount; i++)
                {
                    // normal should equal (v3-v2)x(v1-v1)
                    var normal = new StlNormal(ReadFloatBinary(), ReadFloatBinary(), ReadFloatBinary());
                    var v1 = ReadVertexBinary();
                    var v2 = ReadVertexBinary();
                    var v3 = ReadVertexBinary();
                    _binReader.ReadUInt16(); // attribute byte count; garbage value
                    var t = new StlTriangle(normal, v1, v2, v3);
                    EnsureCorrectNormal(t);
                    triangles.Add(t);
                }
            }

            return triangles;
        }

        private static void EnsureCorrectNormal(StlTriangle stlTriangle)
        {
            stlTriangle.Normal = stlTriangle.GetValidNormal();
        }

        private float ReadFloatBinary()
        {
            return _binReader.ReadSingle();
        }

        private Vertex ReadVertexBinary()
        {
            return new Vertex(ReadFloatBinary(), ReadFloatBinary(), ReadFloatBinary());
        }

        private StlTriangle ReadTriangle()
        {
            StlTriangle stlTriangle = null;
            if (_isAscii)
            {
                switch (PeekToken())
                {
                    case "facet":
                        AdvanceToken();
                        SwallowToken("normal");
                        var normal = new StlNormal(ConsumeNumberToken(), ConsumeNumberToken(), ConsumeNumberToken());
                        SwallowToken("outer");
                        SwallowToken("loop");
                        SwallowToken("vertex");
                        var v1 = ConsumeVertexToken();
                        SwallowToken("vertex");
                        var v2 = ConsumeVertexToken();
                        SwallowToken("vertex");
                        var v3 = ConsumeVertexToken();
                        SwallowToken("endloop");
                        SwallowToken("endfacet");
                        stlTriangle = new StlTriangle(normal, v1, v2, v3);
                        break;
                    case "endsolid":
                        return null;
                    default:
                        throw new StlReadException("Unexpected token " + PeekToken());
                }
            }

            return stlTriangle;
        }

        private void SwallowToken(string token)
        {
            if (PeekToken() == token)
            {
                AdvanceToken();
            }
            else
            {
                throw new StlReadException("Expected token " + token);
            }
        }

        private float ConsumeNumberToken()
        {
            var text = PeekToken();
            AdvanceToken();
            float value;
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                throw new StlReadException("Expected number");
            return value;
        }

        private Vertex ConsumeVertexToken()
        {
            return new Vertex(ConsumeNumberToken(), ConsumeNumberToken(), ConsumeNumberToken());
        }

        private string PeekToken()
        {
            return _tokenEnumerator.Current;
        }

        private void AdvanceToken()
        {
            _tokenEnumerator.MoveNext();
        }
    }
}