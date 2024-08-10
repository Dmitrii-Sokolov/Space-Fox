using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

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

            public IEnumerable<Region> GetSelfAndNeighbours(MeshPolygoned referenceMesh)
            {
                yield return this;

                for (var nIndex = 0; nIndex < QuadVector3.Neighbours.Length; nIndex++)
                {
                    var neighbourOffset = QuadVector3.Neighbours[nIndex];

                    if (IsValidNeighbourOffset(neighbourOffset))
                    {
                        yield return CreateNeighbour(neighbourOffset);
                    }
                    //!!Warning nIndex refers to Neighbours array order
                    else if (referenceMesh.TryGetAdjacentPolygonIndex(
                            PolygonIndex,
                            nIndex,
                            out var adjacentPolygonIndex,
                            out var edgeIndexShift))
                    {
                        yield return CreateNeighbour(neighbourOffset, adjacentPolygonIndex, edgeIndexShift);
                    }
                }

                for (var nIndex0 = 0; nIndex0 < QuadVector3.Neighbours.Length; nIndex0++)
                {
                    var nIndex1 = (nIndex0 + 1) % QuadVector3.Neighbours.Length;
                    var neighbourOffset0 = QuadVector3.Neighbours[nIndex0];
                    var neighbourOffset1 = QuadVector3.Neighbours[nIndex1];

                    if (IsValidNeighbourOffset(neighbourOffset0) && IsValidNeighbourOffset(neighbourOffset1))
                    {
                        //n0pol == this == n1pol  => inner case
                        yield return CreateNeighbour(neighbourOffset0 + neighbourOffset1);
                    }
                    else if (!IsValidNeighbourOffset(neighbourOffset0) && IsValidNeighbourOffset(neighbourOffset1))
                    {
                        //n0pol != this == n1pol  => edge case
                        if (referenceMesh.TryGetAdjacentPolygonIndex(
                            PolygonIndex,
                            nIndex0,
                            out var adjacentPolygonIndex,
                            out var edgeIndexShift))
                        {
                            yield return CreateNeighbour(neighbourOffset0 + neighbourOffset1, adjacentPolygonIndex, edgeIndexShift);
                        }

                        yield return CreateNeighbour(neighbourOffset0 + neighbourOffset1);
                    }
                    else if (IsValidNeighbourOffset(neighbourOffset0) && !IsValidNeighbourOffset(neighbourOffset1))
                    {
                        //n0pol == this != n1pol  => edge case
                        if (referenceMesh.TryGetAdjacentPolygonIndex(
                            PolygonIndex,
                            nIndex1,
                            out var adjacentPolygonIndex,
                            out var edgeIndexShift))
                        {
                            yield return CreateNeighbour(neighbourOffset0 + neighbourOffset1, adjacentPolygonIndex, edgeIndexShift);
                        }
                    }
                    else
                    {
                        if (!referenceMesh.TryGetAdjacentPolygonIndex(PolygonIndex, nIndex0, out var neighbour0, out var _))
                            neighbour0 = -1;

                        if (!referenceMesh.TryGetAdjacentPolygonIndex(PolygonIndex, nIndex1, out var neighbour1, out var _))
                            neighbour1 = -1;

                        //TODO Compare size of PolygonedMesh and HalfEdge
                        foreach (var (index, edgeIndexShift) in referenceMesh.GetAllPolygonsByVertex(PolygonIndex, nIndex1))
                        {
                            if (index != PolygonIndex && index != neighbour0 && index != neighbour1)
                                yield return CreateNeighbour(neighbourOffset0 + neighbourOffset1, index, edgeIndexShift);
                        }
                    }
                }

                yield break;
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

            public override string ToString()
                => $"({SubregionX}, {SubregionY}), Divider : {Divider},  Subdivider : {Subdivider}, PolygonIndex : {PolygonIndex}";

            public static bool operator ==(Region a, Region b)
                => a.Equals(b);

            public static bool operator !=(Region a, Region b)
                => !(a == b);

            private bool IsValidNeighbourOffset(Vector2Int offest)
                => IsValidNeighbourOffset(offest.x, offest.y);

            private bool IsValidNeighbourOffset(int x, int y)
                => IsValidIndex(SubregionX + x) && IsValidIndex(SubregionY + y);

            private bool IsValidIndex(int index)
                => 0 <= index && index < Divider;

            private Region CreateNeighbour(Vector2Int offest)
                => new(PolygonIndex, Divider, SubregionX + offest.x, SubregionY + offest.y, Subdivider);

            private Region CreateNeighbour(Vector2Int neighbourOffset, int adjacentPolygonIndex, int edgeIndexShift)
            {
                //!!Warning this formulas refers to Quad inner structure
                var x = (SubregionX + neighbourOffset.x + Divider) % Divider;
                var y = (SubregionY + neighbourOffset.y + Divider) % Divider;

                return ((edgeIndexShift + 6) % 4) switch
                {
                    0 => AnotherPolygonRegion(this, x, y),
                    1 => AnotherPolygonRegion(this, Divider - 1 - y, x),
                    2 => AnotherPolygonRegion(this, Divider - 1 - x, Divider - 1 - y),
                    3 => AnotherPolygonRegion(this, y, Divider - 1 - x),
                    _ => throw new ArgumentException(),
                };

                Region AnotherPolygonRegion(Region region, int x, int y)
                    => new(adjacentPolygonIndex, region.Divider, x, y, region.Subdivider);
            }
        }

        [Inject] private readonly IUpdateProxy UpdateProxy = default;

        [SerializeField] private ObservableValue<Vector3> Center = default;

        [Slider(0.1f, 10f)]
        [SerializeField] private ObservableValue<float> Radius = default;

        [Slider(0.1f, 2f)]
        [SerializeField] private ObservableValue<float> AreaSize = default;

        [Slider(0.02f, 1f)]
        [SerializeField] private ObservableValue<float> TriangleSize = default;

        [SerializeField] private ObservableTransform Observer = default;
        [SerializeField] private ObservableTransform Self = default;
        [SerializeField] private MeshFilter ViewPrefab = default;

        private bool IsDirty = false;
        private bool RedrawIsNeeded = false;

        private readonly ObservableValue<Region> CurrentRegion = new();
        private readonly MeshPolygoned ReferenceMesh = MeshPolygoned.GetCube();

        //TODO Invalidate cache with time (for memory saving)
        private readonly Dictionary<Region, Mesh> MeshesPool = new();
        private readonly List<MeshFilter> CurrentViews = new();

        private SimplePool<MeshFilter, Mesh> ViewsPool;

        protected override void AwakeBeforeDestroy()
        {
            //This can be calculated with node tree, root -- view affected value
            //Its children -- values that affecting it
            //When value changed, its node marked as 'dirty'
            //Then, all its parents must be recalculated
            //Also, it's necessary to check that there are no any cycles

            //Center affects cache and all
            //Radius affects  cache and all

            //AreaSize affects Divider
            //TriangleSize affects Subdivider
            //Observer.Position affects PolygonIndex
            //Self.Position affects PolygonIndex
            //Self.Rotation affects PolygonIndex

            //Region.PolygonIndex affects Divider, Subdivider
            //Region.Divider affects RegionXY
            //Region.RegionXY affects meshes
            //Region.Subdivider affects meshes

            ViewsPool = new(CreateView, OnGetView, OnReturnView);

            Center.Subscribe(InvalidateCache).While(this);
            Radius.Subscribe(InvalidateCache).While(this);

            AreaSize.Subscribe(SetForceRedraw).While(this);
            TriangleSize.Subscribe(SetForceRedraw).While(this);

            Observer.Position.Subscribe(OnSpatialChange).While(this);
            Self.Position.Subscribe(OnSpatialChange).While(this);
            Self.Rotation.Subscribe(OnSpatialChange).While(this);

            CurrentRegion.Subscribe(RefillMeshFilters, false).While(this);

            Observer.SetUpdateProvider(UpdateProxy.Update).While(this);
            Self.SetUpdateProvider(UpdateProxy.Update).While(this);

            UpdateProxy.LateUpdate.Subscribe(OnLateUpdate).While(this);
        }

        private MeshFilter CreateView()
        {
            var view = Instantiate(ViewPrefab, transform);
            view.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            return view;
        }

        private void OnGetView(MeshFilter meshFilter, Mesh mesh)
        {
            meshFilter.gameObject.SetActive(true);
            meshFilter.mesh = mesh;
        }

        private void OnReturnView(MeshFilter meshFilter)
        {
            meshFilter.gameObject.SetActive(false);
            meshFilter.mesh = null;
        }

        private void InvalidateCache()
        {
            MeshesPool.Clear();
            SetForceRedraw();
        }

        private void OnSpatialChange()
        {
            //TODO This can be optimised (this brokes caching)

            var distance2 = GetVectorToCenter().sqrMagnitude;
            AreaSize.Value = Radius.Value * Mathf.Sqrt(1 - (Radius.Value * Radius.Value / distance2));

            var desiredAngularSize = 0.1f;
            var distanceToSurface = Mathf.Sqrt(distance2) - Radius.Value;
            TriangleSize.Value = distanceToSurface * desiredAngularSize;

            IsDirty = true;
        }

        private void SetForceRedraw()
            => RedrawIsNeeded = true;

        private void OnLateUpdate()
        {
            //TODO Generate whole sphere when observer is far

            if (RedrawIsNeeded)
            {
                RedrawIsNeeded = false;
                IsDirty = false;
                ForceRedraw();
            }

            if (IsDirty)
            {
                IsDirty = false;
                CheckRegion();
            }
        }

        private void CheckRegion()
        {
            var vectorToCenter = GetVectorToCenter();
            var sector = GetSector(CurrentRegion.Value, ReferenceMesh);

            if (!IsInside(vectorToCenter, sector))
                CurrentRegion.Value = GetRegion(vectorToCenter);
        }

        private void ForceRedraw()
        {
            var vectorToCenter = GetVectorToCenter();
            CurrentRegion.Value = GetRegion(vectorToCenter);
        }

        private Vector3 GetVectorToCenter()
        {
            var observerPositionInLocalSpace = Self.Value.InverseTransformPoint(Observer.Position.Value);
            var vectorToCenter = observerPositionInLocalSpace - Center.Value;
            return vectorToCenter;
        }

        //This works only if vectors aren't complanar
        //TODO Make a solution for planar case
        private Region GetRegion(Vector3 vectorToCenter)
        {
            var polygonIndex = ReferenceMesh.GetNearestPolygonIndex(vectorToCenter);
            var minSize = ReferenceMesh.GetMinSideSize(polygonIndex) * Radius.Value;
            var dividerPower = Mathf.Max(Mathf.RoundToInt(Mathf.Log(minSize / AreaSize.Value, 2)), 0);

            //TODO Remove this
            dividerPower = Mathf.Clamp(dividerPower, 0, 5);

            var divider = 1 << dividerPower;
            var sector = ReferenceMesh.GetQuad(polygonIndex);

            var xInt = 0;
            var yInt = 0;

            if (IsInside(vectorToCenter, sector))
            {
                for (var d = 0; d < dividerPower; d++)
                {
                    var rightMedian = Vector3.Slerp(sector.RightTop, sector.RightBottom, 0.5f);
                    var bottomMedian = Vector3.Slerp(sector.RightBottom, sector.LeftBottom, 0.5f);
                    var leftMedian = Vector3.Slerp(sector.LeftBottom, sector.LeftTop, 0.5f);
                    var topMedian = Vector3.Slerp(sector.LeftTop, sector.RightTop, 0.5f);

                    var verticalNormal = Vector3.Cross(bottomMedian, topMedian);
                    var horizontalNormal = Vector3.Cross(rightMedian, leftMedian);

                    var isRight = Vector3.Dot(vectorToCenter, verticalNormal) > 0;
                    var isTop = Vector3.Dot(vectorToCenter, horizontalNormal) > 0;

                    var xBit = isRight ? 1 : 0;
                    var yBit = isTop ? 1 : 0;

                    sector.Cut(xBit, yBit, 2, Vector3.Slerp);

                    xInt = (2 * xInt) + xBit;
                    yInt = (2 * yInt) + yBit;
                }
            }
            else
            {
                //TODO This must be implemented
                Debug.LogWarning("Vector is outside the polygon");
            }

            var maxSize = ReferenceMesh.GetMaxSideSize(polygonIndex) * Radius.Value;
            var currentTrianlgeSide = maxSize / divider;
            var subdivider = Mathf.RoundToInt(Mathf.Log(currentTrianlgeSide / TriangleSize.Value, 2));

            return new Region(polygonIndex, divider, xInt, yInt, subdivider);
        }

        //TODO Optimise that (collection intersection and substract)
        private void RefillMeshFilters(Region region)
        {
            var regions = region.GetSelfAndNeighbours(ReferenceMesh);
            var meshes = regions.Select(GetMesh).ToList();
            var newViews = new List<MeshFilter>(meshes.Count);

            foreach (var view in CurrentViews)
            {
                if (meshes.Contains(view.mesh))
                {
                    newViews.Add(view);
                    meshes.Remove(view.mesh);
                }
                else
                {
                    ViewsPool.Return(view);
                }
            }

            foreach (var mesh in meshes)
                newViews.Add(ViewsPool.Get(mesh));

            CurrentViews.Clear();
            CurrentViews.AddRange(newViews);
        }

        private Mesh GetMesh(Region region)
        {
            if (!MeshesPool.TryGetValue(region, out var mesh))
            {
                mesh = GenerateMesh(region, ReferenceMesh, Center.Value, Radius.Value);
                MeshesPool.Add(region, mesh);
            }

            return mesh;
        }

        private static bool IsInside(Vector3 vectorToCenter, QuadVector3 sector)
            => Vector3.Dot(vectorToCenter, sector.LeftNormal) > 0 &&
            Vector3.Dot(vectorToCenter, sector.RightNormal) < 0 &&
            Vector3.Dot(vectorToCenter, sector.BottomNormal) > 0 &&
            Vector3.Dot(vectorToCenter, sector.TopNormal) < 0;

        private static Mesh GenerateMesh(Region region, MeshPolygoned reference, Vector3 center, float radius)
        {
            var sector = GetSector(region, reference);
            var mesh = MeshPolygoned.GetPolygon(sector);
            mesh.TransformVertices(GetRelativeHeight);
            mesh.Subdivide(region.Subdivider, GetRelativeHeight);
            mesh.MoveAndScale(center, radius);

            var result = new Mesh();
            result.ApplyData(mesh);

            return result;
        }

        private static QuadVector3 GetSector(Region region, MeshPolygoned reference)
        {
            var sector = reference.GetQuad(region.PolygonIndex);
            sector.Cut(region.SubregionX, region.SubregionY, region.Divider, Vector3.Slerp);
            return sector;
        }

        private static Vector3 GetRelativeHeight(Vector3 direction)
            => direction.normalized * ((PerlinNoise.GetRandomValue((Vector2)direction) * 0.1f) + 1f);
    }
}
