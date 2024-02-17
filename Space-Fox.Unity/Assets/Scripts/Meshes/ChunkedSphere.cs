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
        private readonly ObservableValue<PrimitiveType> PrimitiveType = new();

        private bool IsDirty = false;

        [Range(0, 7)]
        [SerializeField] private int CurrentRecursiveDepth = default;

        [Range(0.1f, 100f)]
        [SerializeField] private float CurrentRadius = default;

        [SerializeField] private PrimitiveType CurrentPrimitiveType = default;

        protected override void AwakeBeforeDestroy()
        {
            RecursiveDepth.Subscribe(_ => IsDirty = true).While(this);
            Radius.Subscribe(_ => IsDirty = true).While(this);
            PrimitiveType.Subscribe(_ => IsDirty = true).While(this);

            UpdateProxy.Update.Subscribe(OnUpdate).While(this);
        }

        private void OnUpdate()
        {
            RecursiveDepth.Value = CurrentRecursiveDepth;
            Radius.Value = CurrentRadius;
            PrimitiveType.Value = CurrentPrimitiveType;

            if (IsDirty)
            {
                IsDirty = false;
                RegenerageMesh();
            }
        }

        private void RegenerageMesh()
        {
            var sphere = GetSphere(
                RecursiveDepth.Value,
                Center,
                Radius.Value,
                PrimitiveType.Value);

            var mesh = new Mesh();

            mesh.ApplyData(sphere);

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private static MeshPolygoned GetSphere(
            int recursiveDepth,
            Vector3 center,
            float radius,
            PrimitiveType primitiveType)
        {
            var primitive = MeshPolygoned.GetPrimitive(
                primitiveType, 
                center + (primitiveType.IsPlanar() ? 0.01f * Vector3.back : Vector3.zero));

            primitive.TransformVertices(v => GetLocalVertexPosition(v, center, radius));

            var (vertices, edges, polygons) = (primitive.Vertices, primitive.Edges, primitive.Polygons);

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
