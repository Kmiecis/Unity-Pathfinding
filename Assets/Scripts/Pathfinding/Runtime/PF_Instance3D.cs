using UnityEngine;
using Common.Extensions;
using Common.Mathematics;
using System.Collections.Generic;
using Common;

namespace Custom.Pathfinding
{
    [ExecuteInEditMode]
    public class PF_Instance3D : MonoBehaviour, PF_IInstance
    {
        private static readonly Vector3[] kCorners = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f)
        };

        public Vector3 unit = new Vector3(1.0f, 1.0f, 1.0f);
        public Vector3 size = new Vector3(16.0f, 16.0f, 16.0f);

        private TransformWatcher _transformWatcher;
        private bool[][][] _grid;

        public bool[][][] Grid
        {
            get => _grid;
        }

        public Vector3 GridPosition
        {
            get => transform.position - 0.5f * unit;
        }

        public Vector3Int GridSize
        {
            get => Mathx.RoundToInt(Mathx.Div(size, unit));
        }

        public Vector3 ToWorldPosition(Vector3Int p)
        {
            return transform.position + Mathx.Mul(p, unit);
        }

        public Vector3Int ToGridPosition(Vector3 p)
        {
            return Mathx.FloorToInt(Mathx.Div(p, unit) - transform.position);
        }

        public bool Contains(Vector3 p)
        {
            var gridPosition = ToGridPosition(p);
            return Contains(gridPosition);
        }

        public bool Contains(Vector3Int p)
        {
            return (
                _grid != null &&
                p.x > -1 && p.y > -1 && p.z > -1 &&
                p.x < _grid.GetWidth() && p.y < _grid.GetHeight() && p.z < _grid.GetDepth()
            );
        }

        public void Bake()
        {
            var gridSize = GridSize;
            if (
                _grid == null ||
                _grid.GetWidth() != gridSize.x ||
                _grid.GetHeight() != gridSize.y ||
                _grid.GetDepth() != gridSize.z
            )
            {
                _grid = Arrays.New<bool>(gridSize.x, gridSize.y, gridSize.z);
            }

            var gridPosition = GridPosition;
            var halfGridSize = Mathx.Mul(gridSize, 0.5f);
            var colliders = Physics.OverlapBox(gridPosition + halfGridSize, halfGridSize);

            for (int x = 0; x < gridSize.x; ++x)
            {
                for (int y = 0; y < gridSize.y; ++y)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        var gridNode = new Vector3Int(x, y, z);

                        var walkable = true;
                        for (int c = 0; c < colliders.Length && walkable; ++c)
                        {
                            var collider = colliders[c];

                            for (int o = 0; o < kCorners.Length && walkable; ++o)
                            {
                                var offset = kCorners[o];

                                var position = ToWorldPosition(gridNode) + Mathx.Mul(offset, unit);
                                walkable = !collider.Contains(position);
                            }
                        }
                        _grid[x][y][z] = walkable;
                    }
                }
            }
        }

        public bool TryFindPath(Vector3 start, Vector3 target, out List<Vector3> path)
        {
            var startGridPosition = ToGridPosition(start);
            var targetGridPosition = ToGridPosition(target);

            if (PF_Core3D.TryFindPath(Grid, startGridPosition, targetGridPosition, out var gridPath))
            {
                path = new List<Vector3>();
                path.Add(start);
                foreach (var gridPosition in gridPath)
                    path.Add(ToWorldPosition(gridPosition));
                path.Add(target);
                return true;
            }

            path = null;
            return false;
        }

        private void Awake()
        {
            PF_IInstance.Instances.Add(this);
        }

        private void Start()
        {
            _transformWatcher = new TransformWatcher(transform);
        }

        private void OnDestroy()
        {
            PF_IInstance.Instances.Remove(this);
        }

#if UNITY_EDITOR
        [Header("Editor")]
        public bool autoBake;
        public bool showGrid;
        public Color gridColor = Color.cyan.WithAlpha(0.5f);

        private void OnValidate()
        {
            if (autoBake)
            {
                Bake();
            }
        }

        private void Update()
        {
            if (autoBake && _transformWatcher != null && _transformWatcher.HasChanged)
            {
                Bake();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (showGrid && _grid != null)
            {
                var gridSize = GridSize;
                var gridPosition = GridPosition;

                var array = new Vector3Int[gridSize.x * gridSize.y * gridSize.z];

                for (int x = 0; x < gridSize.x; ++x)
                {
                    for (int y = 0; y < gridSize.y; ++y)
                    {
                        for (int z = 0; z < gridSize.z; z++)
                        {
                            if (_grid[x][y][z])
                            {
                                var i = x + y * gridSize.x + z * gridSize.x * gridSize.y;
                                array[i] = Vector3Int.one;
                            }
                        }
                    }
                }

                for (int y = 0; y < gridSize.y; ++y)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        for (int x = 1; x < gridSize.x; ++x)
                        {
                            int i = x + (y + z * gridSize.y) * gridSize.x;
                            int j = (x - 1) + (y + z * gridSize.y) * gridSize.x;

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

                for (int x = 0; x < gridSize.x; ++x)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        for (int y = 1; y < gridSize.y; ++y)
                        {
                            var i = x + (y + z * gridSize.y) * gridSize.x;
                            var j = x + ((y - 1) + z * gridSize.y) * gridSize.x;

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

                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        for (int z = 1; z < gridSize.z; z++)
                        {
                            var i = x + (y + z * gridSize.y) * gridSize.x;
                            var j = x + (y + (z - 1) * gridSize.y) * gridSize.x;

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
                for (int x = 0; x < gridSize.x; ++x)
                {
                    for (int y = 0; y < gridSize.y; ++y)
                    {
                        for (int z = 0; z < gridSize.z; z++)
                        {
                            var i = x + (y + z * gridSize.y) * gridSize.x;
                            var vi = array[i];

                            if (vi.x > 0 && vi.y > 0 && vi.z > 0)
                            {
                                var size = Mathx.Mul(vi, unit);
                                var position = gridPosition + Mathx.Mul(new Vector3(x + 1, y + 1, z + 1), unit) - size;

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
