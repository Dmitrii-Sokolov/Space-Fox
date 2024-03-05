using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SpaceFox
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SphereChunk : DisposableMonoBehaviour
    {
        private readonly struct Region : IEquatable<Region>
        {
            public readonly int PolygonIndex { get; }
            public readonly int Divider { get; }
            public readonly int SubregionX { get; }
            public readonly int SubregionY { get; }
            public readonly int Subdivider { get; }
            
            public Region(int polygonIndex, int divider, int subregionX, int subregionY, int subdivider)
            {
                PolygonIndex = polygonIndex;
                Divider = divider;
                SubregionX = subregionX;
                SubregionY = subregionY;
                Subdivider = subdivider;
            }

            public override int GetHashCode()
                => HashCode.Combine(PolygonIndex, Divider, SubregionX, SubregionY, Subdivider);

            public override bool Equals(object other)
                => other is Region region && Equals(region);

            public bool Equals(Region other)
                => PolygonIndex == other.PolygonIndex &&
                Divider == other.Divider &&
                SubregionX == other.SubregionX &&
                SubregionY == other.SubregionY &&
                Subdivider == other.Subdivider;
        }

        [Inject] private readonly UpdateProxy UpdateProxy = default;

        [SerializeField] private ObservableValue<Vector3> Center = new();

        [Slider(0.1f, 10f)]
        [SerializeField] private ObservableValue<float> Radius = new();

        [Slider(0.1f, 2f)]
        [SerializeField] private ObservableValue<float> AreaSize = new();

        [Slider(0.02f, 1f)]
        [SerializeField] private ObservableValue<float> TriangleSize = new();

        [SerializeField] private ObservableTransform Observer = new();
        [SerializeField] private ObservableTransform Self = new();

        private bool IsDirty = false;

        private MeshPolygoned ReferenceMesh = default;

        protected override void AwakeBeforeDestroy()
        {
            Center.Subscribe(SetDirty).While(this);
            Radius.Subscribe(SetDirty).While(this);
            AreaSize.Subscribe(SetDirty).While(this);
            TriangleSize.Subscribe(SetDirty).While(this);
            Observer.Position.Subscribe(SetDirty).While(this);
            Self.Position.Subscribe(SetDirty).While(this);
            Self.Rotation.Subscribe(SetDirty).While(this);

            Observer.SetUpdateProvider(UpdateProxy.Update).While(this);
            Self.SetUpdateProvider(UpdateProxy.Update).While(this);

            UpdateProxy.LateUpdate.Subscribe(OnLateUpdate).While(this);
        }

        private void SetDirty()
            => IsDirty = true;

        private void OnLateUpdate()
        {
            if (IsDirty)
            {
                IsDirty = false;
                RegenerageMesh();
            }
        }

        private void RegenerageMesh()
              => GetComponent<MeshFilter>().mesh = GenerateMesh(CalculateRegion(), ReferenceMesh, Center.Value, Radius.Value);

        private Region CalculateRegion()
        {
            ReferenceMesh ??= MeshPolygoned.GetCube();

            var observerPositionInLocalSpace = Self.Value.InverseTransformPoint(Observer.Position.Value);
            var vectorToCenter = observerPositionInLocalSpace - Center.Value;
            var polygonIndex = ReferenceMesh.GetNearestPolygonIndex(vectorToCenter);

            var minSize = ReferenceMesh.GetMinSideSize(polygonIndex) * Radius.Value;
            var divider = 1 << Mathf.Max(Mathf.RoundToInt(Mathf.Log(minSize / AreaSize.Value, 2)), 0);

            var sector = ReferenceMesh.GetQuad(polygonIndex);

            //Will try to find barycentric coordinates of vectorToSurface in this orthant
            var x = DecompositeByPlanes(vectorToCenter, sector.LeftNormal, sector.RightNormal);
            var xInt = Mathf.FloorToInt(x * divider);

            var y = DecompositeByPlanes(vectorToCenter, sector.BottomNormal, sector.TopNormal);
            var yInt = Mathf.FloorToInt(y * divider);

            var maxSize = ReferenceMesh.GetMaxSideSize(polygonIndex) * Radius.Value;
            var currentTrianlgeSide = maxSize / divider;
            var subdivider = Mathf.RoundToInt(Mathf.Log(currentTrianlgeSide / TriangleSize.Value, 2));

            return new Region(polygonIndex, divider, xInt, yInt, subdivider);
        }

        //This works only if vectors normal0 and normal1 aren't complanar
        //TODO Make a solution for planar case
        private static float DecompositeByPlanes(Vector3 vector, Vector3 normal0, Vector3 normal1)
        {
            //Slice is the plane, that is perpendicular to two basis planes
            var sliceNormal = Vector3.Cross(normal0, normal1).normalized;

            //Directions of the intersections of planes with slice
            var direction0 = Vector3.Cross(normal0, sliceNormal);
            var direction1 = Vector3.Cross(normal1, sliceNormal);
            var vectorProjectionToSlice = vector - sliceNormal * Vector3.Dot(sliceNormal, vector);
            var (a, b) = DecompositeByBasis(vectorProjectionToSlice, direction0, direction1);

            return Mathf.Abs(b) / (Mathf.Abs(a) + Mathf.Abs(b));
        }

        private static (float X, float Y) DecompositeByBasis(Vector3 vector, Vector3 baseA, Vector3 baseB)
        {
            //Using Gaussian elimination method
            var ab = Vector3.Dot(baseA, baseB);
            var a2 = Vector3.Dot(baseA, baseA);
            var b2 = Vector3.Dot(baseB, baseB);
            var va = Vector3.Dot(vector, baseA);
            var vb = Vector3.Dot(vector, baseB);
            var y = (vb - va * ab / a2) / (b2 - ab * ab / a2);
            var x = (va - y * ab) / a2;
            return (x, y);
        }

        private static Mesh GenerateMesh(Region region, MeshPolygoned reference, Vector3 center, float radius)
        {
            //TODO Add neighbours quad
            //TODO Generate whole sphere when far
            //TODO Add height noise

            var divider = (float)region.Divider;
            var sector = reference.GetQuad(region.PolygonIndex);

            sector.CutByX(region.SubregionX, divider, Vector3.Slerp);
            sector.CutByY(region.SubregionY, divider, Vector3.Slerp);

            var mesh = MeshPolygoned.GetPolygon(sector);
            mesh.TransformVertices(GetRelativeHeight);
            mesh.Subdivide(region.Subdivider, GetRelativeHeight);
            mesh.MoveAndScale(center, radius);

            var result = new Mesh();
            result.ApplyData(mesh);

            return result;
        }

        private static Vector3 GetRelativeHeight(Vector3 direction)
            => direction.normalized;
    }
}
