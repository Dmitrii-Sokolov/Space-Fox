using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceFox
{
    public class MeshPolygoned : IMesh
    {
        public class Polygon : List<EdgeLink>
        {
            public Polygon(params (int index, bool reversed)[] edges)
                : this(edges.Select(edge => new EdgeLink(edge.index, edge.reversed)))
            {
            }

            public Polygon(params EdgeLink[] edges)
                : base(edges)
            {
            }

            public Polygon(IEnumerable<EdgeLink> edges)
                : base(edges)
            {
            }

            public bool Match(Polygon other)
            {
                if (other.Count != Count)
                    return false;

                for (var i = 0; i < Count; i++)
                {
                    if (this[i] != other[i])
                        return false;
                }

                return true;

            }

            public Vector3 GetCenter(List<Vector3> vertices, List<Edge> edges)
            {
                var sum = Vector3.zero;
                foreach (var link in this)
                    sum += vertices[link.GetFirstVertexIndex(edges)];
                sum /= Count;

                return sum;
            }

            public Vector3 GetCenter(MeshPolygoned mesh)
            {
                var sum = Vector3.zero;
                foreach (var link in this)
                    sum += mesh.Vertices[link.GetFirstVertexIndex(mesh.Edges)];
                sum /= Count;

                return sum;
            }

            public IEnumerable<Vector3> GetVertices(MeshPolygoned mesh)
                => this.Select(edge => mesh.GetFirstVertexVector(edge));

            public QuadVector3 GetQuad(MeshPolygoned mesh)
                => new(GetVertices(mesh));

            public float GetMinSideSize(MeshPolygoned mesh)
                => GetExtremum(mesh, Mathf.Min);

            public float GetMaxSideSize(MeshPolygoned mesh)
                => GetExtremum(mesh, Mathf.Max);

            private float GetExtremum(MeshPolygoned mesh, Func<float, float, float> extremum)
            {
                var result = GetSqrDistance(0, 1);

                for (var n = 0; n < Count - 1; n++)
                {
                    for (var i = Mathf.Max(n + 1, 2); i < Count; i++)
                        result = extremum(GetSqrDistance(n, (n + i) % Count), result);
                }

                return Mathf.Sqrt(result);

                float GetSqrDistance(int i, int n)
                    => Vector3.SqrMagnitude(mesh.GetFirstVertexVector(this[i]) - mesh.GetFirstVertexVector(this[n]));
            }
        }

        private readonly List<Vector3> Vertices;
        private readonly List<Edge> Edges;
        private readonly List<Polygon> Polygons;

        public MeshPolygoned() : this(
            new Vector3[0],
            new Edge[0],
            new Polygon[0])
        {
        }

        public MeshPolygoned(
            IEnumerable<Vector3> vertices,
            IEnumerable<Edge> edges,
            IEnumerable<Polygon> polygons)
        {
            Vertices = vertices.ToList();
            Edges = edges.ToList();
            Polygons = polygons.ToList();
        }

        public MeshPolygoned(
            IEnumerable<Vector3> vertices,
            IEnumerable<Edge> edges,
            IEnumerable<Polygon> polygons,
            Vector3 offset,
            float scale) : this(vertices, edges, polygons)
            => MoveAndScale(offset, scale);

        public (Vector3[], int[]) GetVerticesAndTrianglesAsPlainArray()
        {
            var trianglesCount = Polygons.Sum(p => Mathf.Max(p.Count - 2, 0));
            var trianglesClassic = new List<int>(3 * trianglesCount);
            foreach (var polygon in Polygons)
            {
                for (var i = 0; i < polygon.Count - 2; i++)
                {
                    trianglesClassic.Add(GetFirstVertexIndex(polygon[i]));
                    trianglesClassic.Add(GetFirstVertexIndex(polygon[i + 1]));
                    trianglesClassic.Add(GetFirstVertexIndex(polygon[^1]));
                }
            }

            return (Vertices.ToArray(), trianglesClassic.ToArray());
        }

        public MeshPolygoned MakeRibbed()
        {
            var verticesCount = Polygons.Sum(p => p.Count);

            var newVertices = new List<Vector3>(verticesCount);
            var newEdges = new List<Edge>(verticesCount);

            foreach (var polygon in Polygons)
            {
                var firstIndex = Vertices.Count;
                var newElementsCount = polygon.Count;

                foreach (var edge in polygon)
                    newVertices.Add(Vertices[edge.GetFirstVertexIndex(Edges)]);

                polygon.Clear();
                for (var i = 0; i < newElementsCount; i++)
                {
                    var index = newEdges.AddAndReturnIndex(new(firstIndex + i, firstIndex + (i + 1) % newElementsCount));
                    polygon.Add(new(index, false));
                }
            }

            Vertices.Clear();
            Vertices.AddRange(newVertices);

            Edges.Clear();
            Edges.AddRange(newEdges);

            return this;
        }

        public int GetNearestPolygonIndex(Vector3 point)
        {
            var polygonIndex = 0;
            var minDistance = GetDistanceToPolygonPlane(polygonIndex, point);

            for (var i = 1; i < Polygons.Count; i++)
            {
                var newDistance = GetDistanceToPolygonPlane(i, point);
                if (newDistance < minDistance)
                {
                    polygonIndex = i;
                    minDistance = newDistance;
                }
            }

            return polygonIndex;
        }

        //TODO Optimise that by storing every edge owners, my by half-edge structure
        public bool TryGetAdjacentPolygonIndex(
            int polygonIndex,
            int edgeIndex,
            out int adjacentPolygonIndex,
            out int edgeIndexShift)
        {
            adjacentPolygonIndex = default;
            edgeIndexShift = default;
            var edge = Polygons[polygonIndex][edgeIndex].Index;

            for (var p = 0; p < Polygons.Count; p++)
            {
                if (polygonIndex == p)
                    continue;

                var polygon = Polygons[p];
                for (var e = 0; e < polygon.Count; e++)
                {
                    if (polygon[e].Index == edge)
                    {
                        adjacentPolygonIndex = p;
                        edgeIndexShift = edgeIndex - e;

                        return true;
                    }
                }
            }

            return false;
        }

        public float GetDistanceToPolygonPlane(int polygonIndex, Vector3 point)
            => Vector3.SqrMagnitude(GetPolygonCenter(polygonIndex) - point);

        public Vector3 GetPolygonCenter(int polygonIndex)
            => Polygons[polygonIndex].GetCenter(this);

        public QuadVector3 GetQuad(int polygonIndex)
            => Polygons[polygonIndex].GetQuad(this);

        public float GetMinSideSize(int polygonIndex)
            => Polygons[polygonIndex].GetMinSideSize(this);

        public float GetMaxSideSize(int polygonIndex)
            => Polygons[polygonIndex].GetMaxSideSize(this);

        public MeshPolygoned MakeCopy()
            => new(Vertices, Edges, Polygons);

        public MeshPolygoned TransformVertices(Func<Vector3, Vector3> transformer)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = transformer(Vertices[i]);

            return this;
        }

        public MeshPolygoned MoveAndScale(Vector3 offset, float scale = 1f)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = scale * Vertices[i] + offset;

            return this;
        }

        public MeshPolygoned Subdivide(int iterations, Func<Vector3, Vector3> vertexTransform = null)
        {
            for (var i = 0; i < iterations; i++)
                Subdivide(vertexTransform);

            return this;
        }

        public MeshPolygoned Subdivide(Func<Vector3, Vector3> vertexTransform = null)
        {
            var newPolygonsCount = Polygons.Sum(p => p.Count);
            var newEdgesCount = 2 * Edges.Count + newPolygonsCount;

            var newEdges = new List<Edge>(newEdgesCount);
            var newPolygons = new List<Polygon>(newPolygonsCount);

            var edgesRemap = new (int FirstEdge, int LastEdge, int CenterVertex)[Edges.Count];

            for (var l = 0; l < Edges.Count; l++)
            {
                var edge = Edges[l];
                var edgeCenter = edge.GetCenter(Vertices);

                if (vertexTransform != null)
                    edgeCenter = vertexTransform(edgeCenter);

                var edgeCenterIndex = Vertices.AddAndReturnIndex(edgeCenter);

                edgesRemap[l] = (
                    newEdges.AddAndReturnIndex(new(edge.Vertex0, edgeCenterIndex)),
                    newEdges.AddAndReturnIndex(new(edgeCenterIndex, edge.Vertex1)),
                    edgeCenterIndex);
            }

            foreach (var polygon in Polygons)
            {
                var polygonCenter = polygon.GetCenter(Vertices, Edges);
                if (vertexTransform != null)
                    polygonCenter = vertexTransform(polygonCenter);

                var centerIndex = Vertices.AddAndReturnIndex(polygonCenter);

                var polygonInnerEdges = new int[polygon.Count];
                for (var m = 0; m < polygon.Count; m++)
                {
                    var edgeLink = polygon[m];
                    var edgeCentre = edgesRemap[edgeLink.Index].CenterVertex;
                    polygonInnerEdges[m] = newEdges.AddAndReturnIndex(new(edgeCentre, centerIndex));
                }

                for (var m = 0; m < polygon.Count; m++)
                {
                    var nextEdgeNumber = (m + 1) % polygon.Count;
                    newPolygons.Add(new(
                        GetEdgeHalf(polygon[m], false),
                        GetEdgeHalf(polygon[nextEdgeNumber], true),
                        new(polygonInnerEdges[nextEdgeNumber], false),
                        new(polygonInnerEdges[m], true)));
                }

                EdgeLink GetEdgeHalf(EdgeLink oldLink, bool firstHalf)
                {
                    return new(
                        oldLink.Reversed ^ firstHalf
                            ? edgesRemap[oldLink.Index].FirstEdge
                            : edgesRemap[oldLink.Index].LastEdge,
                        oldLink.Reversed);
                }
            }

            Edges.Clear();
            Edges.AddRange(newEdges);

            Polygons.Clear();
            Polygons.AddRange(newPolygons);

            return this;
        }

        private int GetFirstVertexIndex(EdgeLink edge)
            => edge.GetFirstVertexIndex(Edges);

        private Vector3 GetFirstVertexVector(EdgeLink edge)
            => Vertices[GetFirstVertexIndex(edge)];

        public static MeshPolygoned GetPolygon(IEnumerable<Vector3> vertices)
        {
            var length = vertices.Count();
            var edges = new Edge[length];
            for (var i = 0; i < length; i++)
                edges[i] = new(i, (i + 1) % length);

            var edgeLinks = new EdgeLink[length];
            for (var i = 0; i < length; i++)
                edgeLinks[i] = new(i, false);

            var polygons = new Polygon[] { new(edgeLinks) };

            return new(vertices, edges, polygons);
        }

        public static MeshPolygoned GetPrimitive(PrimitiveType primitiveType, float size)
            => GetPrimitive(primitiveType, Vector3.zero, size);

        public static MeshPolygoned GetPrimitive(PrimitiveType primitiveType, Vector3 center = default, float size = 1f)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Triangle:
                    return GetTriangle(center, size);

                case PrimitiveType.Quad:
                    return GetQuad(center, size);

                case PrimitiveType.Tetrahedron:
                    return GetTetrahedron(center, size);

                case PrimitiveType.Cube:
                    return GetCube(center, size);

                default:
                    throw new NotImplementedException();
            }
        }

        public static MeshPolygoned GetTriangle(float side)
            => GetTriangle(Vector3.zero, side);

        public static MeshPolygoned GetTriangle(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(        Mathf.Sqrt(1f / 3f),    0f, 0f),
                new(-0.5f * Mathf.Sqrt(1f / 3f), -0.5f, 0f),
                new(-0.5f * Mathf.Sqrt(1f / 3f),  0.5f, 0f),
            };

            var edges = new List<Edge>()
            {
                new(0, 1),
                new(1, 2),
                new(2, 0),
            };

            var polygons = new List<Polygon>()
            {
                new((0, false), (1, false), (2, false))
            };

            return new MeshPolygoned(vertices, edges, polygons, center, side);
        }

        public static MeshPolygoned GetQuad(float side)
            => GetQuad(Vector3.zero, side);

        public static MeshPolygoned GetQuad(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(-0.5f, -0.5f, 0f),
                new(-0.5f,  0.5f, 0f),
                new( 0.5f, -0.5f, 0f),
                new( 0.5f,  0.5f, 0f),
            };

            var edges = new List<Edge>()
            {
                new(0, 1),
                new(0, 2),
                new(2, 3),
                new(1, 3),
            };

            var polygons = new List<Polygon>()
            {
                new((0, false), (3, false), (2, true), (1, true))
            };

            return new MeshPolygoned(vertices, edges, polygons, center, side);
        }

        public static MeshPolygoned GetTetrahedron(float side)
            => GetTetrahedron(Vector3.zero, side);

        public static MeshPolygoned GetTetrahedron(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new( Mathf.Sqrt(1f / 3f),                   0f, -Mathf.Sqrt(1f / 24f)),
                new(-Mathf.Sqrt(1f / 12f), -Mathf.Sqrt(1f / 4f), -Mathf.Sqrt(1f / 24f)),
                new(-Mathf.Sqrt(1f / 12f),  Mathf.Sqrt(1f / 4f), -Mathf.Sqrt(1f / 24f)),
                new(                   0,                   0f,  Mathf.Sqrt(3f / 8f)  ),
            };

            var edges = new List<Edge>()
            {
                new(0, 1),
                new(0, 2),
                new(0, 3),
                new(1, 2),
                new(1, 3),
                new(2, 3),
            };

            var polygons = new List<Polygon>()
            {
                new((0, false), (3, false), (1, true)),
                new((0,  true), (2, false), (4, true)),
                new((1, false), (5, false), (2, true)),
                new((3,  true), (4, false), (5, true)),
            };

            return new(vertices, edges, polygons, center, side);
        }

        public static MeshPolygoned GetCube(float side)
            => GetCube(Vector3.zero, side);

        public static MeshPolygoned GetCube(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(-0.5f, -0.5f, -0.5f),
                new(-0.5f, -0.5f,  0.5f),
                new(-0.5f,  0.5f, -0.5f),
                new(-0.5f,  0.5f,  0.5f),
                new( 0.5f, -0.5f, -0.5f),
                new( 0.5f, -0.5f,  0.5f),
                new( 0.5f,  0.5f, -0.5f),
                new( 0.5f,  0.5f,  0.5f),
            };

            var edges = new List<Edge>()
            {
                new(0, 1),
                new(0, 2),
                new(2, 3),
                new(1, 3),

                new(0, 4),
                new(1, 5),
                new(2, 6),
                new(3, 7),

                new(4, 5),
                new(4, 6),
                new(6, 7),
                new(5, 7),
            };

            var polygons = new List<Polygon>()
            {
                new((0, false), (3, false),   (2, true), (1, true)),
                new( (0, true), (4, false),  (8, false), (5, true)),
                new((1, false), (6, false),   (9, true), (4, true)),
                new((2, false), (7, false),  (10, true), (6, true)),
                new( (3, true), (5, false), (11, false), (7, true)),
                new( (8, true), (9, false), (10, false), (11, true)),
            };

            return new(vertices, edges, polygons, center, side);
        }
    }
}
