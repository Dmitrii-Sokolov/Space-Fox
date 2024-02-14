using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ChunkedSphere : DisposableMonoBehaviour
    {
        private static readonly Vector3 Center = Vector3.zero;

        [Inject] private readonly UpdateProxy UpdateProxy = default;

        private readonly ObservableValue<int> RecursiveDepth = new();
        private readonly ObservableValue<float> Radius = new();

        private bool IsDirty = false;

        [SerializeField]
        [Range(0, 7)] private int CurrentRecursiveDepth = 3;

        [SerializeField]
        [Range(0.1f, 100f)] private float CurrentRadius = 10f;

        protected override void AwakeBeforeDestroy()
        {
            //TODO Add Primitive choosing

            RecursiveDepth.Subscribe(_ => IsDirty = true).While(this);
            Radius.Subscribe(_ => IsDirty = true).While(this);
            UpdateProxy.Update.Subscribe(OnUpdate).While(this);
        }

        private void OnUpdate()
        {
            RecursiveDepth.Value = CurrentRecursiveDepth;
            Radius.Value = CurrentRadius;

            if (IsDirty)
            {
                IsDirty = false;
                RegenerageMesh();
            }
        }

        private void RegenerageMesh()
        {
            var sphere = GetSphereFromCube(RecursiveDepth.Value, Center, Radius.Value);

            var mesh = new Mesh();

            mesh.ApplyData(sphere);

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private static MeshPolygoned GetSphereFromCube(int recursiveDepth, Vector3 center, float radius)
        {
            var cube = MeshPolygoned.GetTetrahedron(center, radius);

            var (vertices, edges, polygons) = (cube.Vertices, cube.Edges, cube.Polygons);

            for (var i = 0; i < vertices.Count; i++)
                vertices[i] = GetLocalVertexPosition(vertices[i], center, radius);

            for (var i = 0; i < recursiveDepth; i++)
            {
                var newEdges = new List<Edge>(2 * edges.Count + 4 * polygons.Count);
                var newPolygons = new List<MeshPolygoned.Polygon>(4 * polygons.Count);
                var edgesRemap = new (int FirstEdge, int LastEdge, int CenterVertex)[edges.Count];

                for (var l = 0; l < edges.Count; l++)
                {
                    var edge = edges[l];
                    var edgeCenter = GetLocalVertexPosition(0.5f * (vertices[edge.Vertex0] + vertices[edge.Vertex1]), center, radius);
                    var centerNumber = vertices.AddAndReturnIndex(edgeCenter);

                    edgesRemap[l] = (
                        newEdges.AddAndReturnIndex(new(edge.Vertex0, centerNumber)),
                        newEdges.AddAndReturnIndex(new(centerNumber, edge.Vertex1)),
                        centerNumber);
                }

                foreach (var polygon in polygons)
                {
                    var polygonCenter = vertices.AddAndReturnIndex(
                        GetLocalVertexPosition(polygon.GetCenter(vertices, edges), center, radius));

                    var polygonInnerEdges = new int[polygon.Count];
                    for (var m = 0; m < polygon.Count; m++)
                    {
                        var edgeLink = polygon[m];
                        var edgeCentre = edgesRemap[edgeLink.Index].CenterVertex;
                        polygonInnerEdges[m] = newEdges.AddAndReturnIndex(new(edgeCentre, polygonCenter));
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
                        return new (
                            oldLink.Reversed ^ firstHalf
                                ? edgesRemap[oldLink.Index].FirstEdge
                                : edgesRemap[oldLink.Index].LastEdge,
                            oldLink.Reversed);
                    }
                }

                edges = newEdges;
                polygons = newPolygons;
            }

            return new(vertices, edges, polygons);
        }

        private static Vector3 GetLocalVertexPosition(Vector3 position, Vector3 center, float radius)
            => Center + radius * (position - center).normalized;
    }
}
