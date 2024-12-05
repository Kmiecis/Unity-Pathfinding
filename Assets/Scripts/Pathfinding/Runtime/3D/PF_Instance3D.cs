using Common;
using Common.Extensions;
using Common.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Custom.Pathfinding
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(OnTransformChangedSender))]
    public class PF_Instance3D : MonoBehaviour, PF_IInstance, PF_IMapper3D
    {
        [SerializeField, FormerlySerializedAs("unit")]
        private Vector3 _unit = new Vector3(1.0f, 1.0f, 1.0f);
        [SerializeField, FormerlySerializedAs("size")]
        private Vector3 _size = new Vector3(16.0f, 16.0f, 16.0f);

        [SerializeField, HideInInspector]
        private bool[] _grid;

        public bool[] Grid
        {
            get => _grid;
        }

        public Vector3 GridPosition
        {
            get => transform.position - 0.5f * _unit;
        }

        public Vector3Int GridSize
        {
            get => Mathx.RoundToInt(Mathx.Div(_size, _unit));
        }

        public Vector3 WorldSize
        {
            get => _size;
        }

        public Vector3 ToWorldPosition(Vector3Int p)
        {
            return transform.position + Mathx.Mul(p, _unit);
        }

        public Vector3Int ToGridPosition(Vector3 p)
        {
            return Mathx.FloorToInt(Mathx.Div(p, _unit) - transform.position);
        }

        public Vector3 RoundToGrid(Vector3 position)
        {
            return ToWorldPosition(ToGridPosition(position));
        }

        public bool Contains(Vector3 p)
        {
            var gridPosition = ToGridPosition(p);
            return Contains(gridPosition);
        }

        public bool Contains(Vector3Int p)
        {
            var gsize = GridSize;
            return (
                _grid != null &&
                p.x > -1 && p.y > -1 && p.z > -1 &&
                p.x < gsize.x && p.y < gsize.y && p.z < gsize.z
            );
        }

        public bool IsWalkable(Vector3Int p, int s)
        {
            var gsize = GridSize;
            var index = Mathx.ToIndex(p.x, p.y, p.z, gsize.x, gsize.y);
            return (
                _grid != null &&
                p.x > -1 && p.y > -1 && p.z > -1 &&
                p.x < gsize.x && p.y < gsize.y && p.z < gsize.z &&
                _grid[index]
            );
        }

        public int GetWalkCost(Vector3Int p, int s)
        {
            return 0; // To override
        }

        public void Bake()
        {
            CheckGrid();

            var gsize = GridSize;
            var gposition = GridPosition;
            var halfGridSize = Mathx.Mul(gsize, 0.5f);
            var colliders = Physics.OverlapBox(gposition + halfGridSize, halfGridSize);

            for (int z = 0; z < gsize.z; z++)
            {
                for (int y = 0; y < gsize.y; ++y)
                {
                    for (int x = 0; x < gsize.x; ++x)
                    {
                        var gnode = new Vector3Int(x, y, z);

                        var walkable = true;
                        for (int c = 0; c < colliders.Length && walkable; ++c)
                        {
                            var collider = colliders[c];

                            for (int o = 0; o < Cubes.Vertices.Length && walkable; ++o)
                            {
                                var offset = Cubes.Vertices[o];

                                var position = ToWorldPosition(gnode) + Mathx.Mul(offset, _unit);
                                walkable = !collider.Contains(position);
                            }
                        }

                        int gi = Mathx.ToIndex(x, y, z, gsize.x, gsize.y);
                        _grid[gi] = walkable;
                    }
                }
            }
        }

        public bool TryFindPath(Vector3 start, Vector3 target, int size, out List<Vector3> path)
        {
            var startGridPosition = ToGridPosition(start);
            var targetGridPosition = ToGridPosition(target);

            if (PF_Core3D.TryFindPath(this, startGridPosition, targetGridPosition, size, out var gridPath))
            {
                gridPath = PF_Core3D.GetTrimmedPath(gridPath);

                path = new List<Vector3>() { start };
                foreach (var gridPosition in gridPath)
                    path.Add(ToWorldPosition(gridPosition));
                path.Add(target);
                return true;
            }

            path = null;
            return false;
        }

        private void CheckGrid()
        {
            var gsize = GridSize;
            if (_grid == null || _grid.Length != gsize.x * gsize.y * gsize.z)
            {
                _grid = new bool[gsize.x * gsize.y * gsize.z];
            }
        }

        private void OnTransformChanged()
        {
            if (autoBake)
            {
                Bake();
            }
        }

        private void Awake()
        {
            PF_IInstance.Instances.Add(this);
        }

        private void OnDestroy()
        {
            PF_IInstance.Instances.Remove(this);
        }

#if UNITY_EDITOR
        [Header("Editor")]
        public bool autoBake;
        public bool showGrid;
        public Color gridColor = Color.cyan.RGB_(0.5f);

        private void OnValidate()
        {
            if (autoBake)
            {
                Bake();
            }
        }

        private void OnDrawGizmosSelected()
        {
            CheckGrid();

            if (showGrid && _grid != null)
            {
                var gsize = GridSize;
                var gridPosition = GridPosition;

                var array = new Vector3Int[gsize.x * gsize.y * gsize.z];

                for (int z = 0; z < gsize.z; z++)
                {
                    for (int y = 0; y < gsize.y; ++y)
                    {
                        for (int x = 0; x < gsize.x; ++x)
                        {
                            int i = Mathx.ToIndex(x, y, z, gsize.x, gsize.y);
                            if (_grid[i])
                            {
                                array[i] = Vector3Int.one;
                            }
                        }
                    }
                }

                for (int y = 0; y < gsize.y; ++y)
                {
                    for (int z = 0; z < gsize.z; z++)
                    {
                        for (int x = 1; x < gsize.x; ++x)
                        {
                            int i = x + (y + z * gsize.y) * gsize.x;
                            int j = (x - 1) + (y + z * gsize.y) * gsize.x;

                            var vi = array[i];
                            var vj = array[j];

                            if (vi.y == vj.y && vi.z == vj.z)
                            {
                                vi.x += vj.x;
                                vj.x = 0;

                                array[i] = vi;
                                array[j] = vj;
                            }
                        }
                    }
                }

                for (int x = 0; x < gsize.x; ++x)
                {
                    for (int z = 0; z < gsize.z; z++)
                    {
                        for (int y = 1; y < gsize.y; ++y)
                        {
                            var i = x + (y + z * gsize.y) * gsize.x;
                            var j = x + ((y - 1) + z * gsize.y) * gsize.x;

                            var vi = array[i];
                            var vj = array[j];

                            if (vi.x == vj.x && vi.z == vj.z)
                            {
                                vi.y += vj.y;
                                vj.y = 0;

                                array[i] = vi;
                                array[j] = vj;
                            }
                        }
                    }
                }

                for (int x = 0; x < gsize.x; x++)
                {
                    for (int y = 0; y < gsize.y; y++)
                    {
                        for (int z = 1; z < gsize.z; z++)
                        {
                            var i = x + (y + z * gsize.y) * gsize.x;
                            var j = x + (y + (z - 1) * gsize.y) * gsize.x;

                            var vi = array[i];
                            var vj = array[j];

                            if (vi.x == vj.x && vi.y == vj.y)
                            {
                                vi.z += vj.z;
                                vj.z = 0;

                                array[i] = vi;
                                array[j] = vj;
                            }
                        }
                    }
                }

                Gizmos.color = gridColor;
                for (int x = 0; x < gsize.x; ++x)
                {
                    for (int y = 0; y < gsize.y; ++y)
                    {
                        for (int z = 0; z < gsize.z; z++)
                        {
                            var i = x + (y + z * gsize.y) * gsize.x;
                            var vi = array[i];

                            if (vi.x > 0 && vi.y > 0 && vi.z > 0)
                            {
                                var size = Mathx.Mul(vi, _unit);
                                var position = gridPosition + Mathx.Mul(new Vector3(x + 1, y + 1, z + 1), _unit) - size;

                                Gizmos.DrawCube(position + size * 0.5f, size);
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}
