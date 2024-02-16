﻿using System;
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

            public Polygon(IEnumerable<EdgeLink> edges) : base(edges)
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
        }

        public List<Vector3> Vertices { get; }
        public List<Edge> Edges { get; }
        public List<Polygon> Polygons { get; }

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

        public int[] GetTrianglesAsPlainArray()
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

                int GetFirstVertexIndex(EdgeLink edge)
                    => edge.GetFirstVertexIndex(Edges);
            }

            return trianglesClassic.ToArray();
        }

        public void MakeRibbed()
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
        }

        public MeshPolygoned MoveAndScale(Vector3 offset, float scale)
        {
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] = scale * Vertices[i] + offset;

            return this;
        }

        public static MeshPolygoned GetPrimitive(PrimitiveType primitiveType, float size)
            => GetPrimitive(primitiveType, Vector3.zero, size);

        public static MeshPolygoned GetPrimitive(PrimitiveType primitiveType, Vector3 center = default, float size = 1f)
        {
            switch (primitiveType)
            {
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

        public static MeshPolygoned GetQuad(float side)
            => GetQuad(Vector3.zero, side);

        public static MeshPolygoned GetQuad(Vector3 center = default, float side = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(-0.5f, 0f, -0.5f),
                new(-0.5f, 0f,  0.5f),
                new(0.5f, 0f, -0.5f),
                new(0.5f, 0f,  0.5f),
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

        public static MeshPolygoned GetTetrahedron(float radius)
            => GetTetrahedron(Vector3.zero, radius);

        public static MeshPolygoned GetTetrahedron(Vector3 center = default, float radius = 1f)
        {
            var vertices = new List<Vector3>()
            {
                new(Mathf.Sqrt(8f / 9f), 0, -1f / 3f),
                new(-Mathf.Sqrt(2f / 9f), -Mathf.Sqrt(2f / 3f), -1f / 3f),
                new(-Mathf.Sqrt(2f / 9f), Mathf.Sqrt(2f / 3f), -1f / 3f),
                new(0, 0, 1),
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
                new((0, true), (2, false), (4, true)),
                new((1, false), (5, false), (2, true)),
                new((3, true), (4, false), (5, true)),
            };

            return new(vertices, edges, polygons, center, radius);
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