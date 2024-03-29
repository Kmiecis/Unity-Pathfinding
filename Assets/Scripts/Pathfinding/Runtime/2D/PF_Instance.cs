﻿using Common;
using Common.Extensions;
using Common.Mathematics;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    [ExecuteInEditMode]
    public class PF_Instance : MonoBehaviour, PF_IInstance, PF_IMapper
    {
        private static readonly Vector2[] kCorners = new Vector2[]
        {
            new Vector2(-0.5f, -0.5f),
            new Vector2(-0.5f, +0.5f),
            new Vector2(+0.5f, +0.5f),
            new Vector2(+0.5f, -0.5f)
        };

        public Vector2 unit = new Vector2(1.0f, 1.0f);
        public Vector2 size = new Vector2(16.0f, 16.0f);

        [SerializeField, HideInInspector]
        private bool[] _grid;

        private TransformWatcher _transformWatcher;

        public bool[] Grid
        {
            get => _grid;
        }

        public Vector2 GridPosition
        {
            get => (Vector2)transform.position - 0.5f * unit;
        }

        public Vector2Int GridSize
        {
            get => Mathx.RoundToInt(size / unit);
        }

        public Vector2 ToWorldPosition(Vector2Int p)
        {
            return (Vector2)transform.position + p * unit;
        }
        
        public Vector2Int ToGridPosition(Vector2 p)
        {
            return Mathx.FloorToInt(p / unit - (Vector2)transform.position);
        }

        public bool Contains(Vector2 p)
        {
            var gridPosition = ToGridPosition(p);
            return Contains(gridPosition);
        }

        public bool Contains(Vector2Int p)
        {
            var gsize = GridSize;
            return (
                _grid != null &&
                p.x > -1 && p.y > -1 &&
                p.x < gsize.x && p.y < gsize.y
            );
        }

        public bool IsWalkable(Vector2Int p, int s)
        {
            var gsize = GridSize;
            for (int dy = 0; dy < s; ++dy)
            {
                for (int dx = 0; dx < s; ++dx)
                {
                    var x = p.x + dx;
                    var y = p.y + dy;

                    var index = x + y * gsize.x;
                    var walkable = (
                        x > -1 && y > -1 &&
                        x < gsize.x && y < gsize.y &&
                        _grid[index]
                    );

                    if (!walkable)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int GetWalkCost(Vector2Int p, int s)
        {
            return 0; // To override
        }

        public void Bake()
        {
            CheckGrid();

            var gsize = GridSize;
            var gposition = GridPosition;
            var colliders = Physics2D.OverlapAreaAll(gposition, gposition + gsize);

            for (int y = 0; y < gsize.y; ++y)
            {
                for (int x = 0; x < gsize.x; ++x)
                {
                    var gnode = new Vector2Int(x, y);

                    var walkable = true;
                    for (int c = 0; c < colliders.Length && walkable; ++c)
                    {
                        var collider = colliders[c];

                        for (int o = 0; o < kCorners.Length && walkable; ++o)
                        {
                            var offset = kCorners[o];

                            var position = ToWorldPosition(gnode) + offset * unit;
                            walkable = !collider.Contains(position);
                        }
                    }

                    var gi = Mathx.ToIndex(x, y, gsize.x);
                    _grid[gi] = walkable;
                }
            }
        }
        
        public bool TryFindPath(Vector3 start, Vector3 target, int size, out List<Vector3> path)
        {
            var startGridPosition = ToGridPosition(start);
            var targetGridPosition = ToGridPosition(target);

            if (PF_Core.TryFindPath(this, startGridPosition, targetGridPosition, size, out var gridPath))
            {
                gridPath = PF_Core.GetTrimmedPath(gridPath);

                path = new List<Vector3> { start };
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
            if (_grid == null || _grid.Length != gsize.x * gsize.y)
            {
                _grid = new bool[gsize.x * gsize.y];
            }
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
        private static readonly Color kWallColor = Color.yellow.WithA(0.25f);
        private static readonly Color kRoomColor = Color.cyan.WithA(0.5f);

        [Header("Editor")]
        public bool autoBake;
        public bool showGrid;

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
            CheckGrid();

            if (showGrid && _grid != null)
            {
                var gridSize = GridSize;
                var gridPosition = GridPosition;
                
                var array = new Vector2Int[gridSize.x * gridSize.y];

                for (int y = 0; y < gridSize.y; ++y)
                {
                    for (int x = 0; x < gridSize.x; ++x)
                    {
                        int gi = Mathx.ToIndex(x, y, gridSize.x);
                        if (_grid[gi])
                        {
                            array[gi] = Vector2Int.one;
                        }
                    }
                }

                for (int y = 0; y < gridSize.y; ++y)
                {
                    for (int x = 1; x < gridSize.x; ++x)
                    {
                        int i = x + y * gridSize.x;
                        int j = (x - 1) + y * gridSize.x;

                        var vi = array[i];
                        var vj = array[j];

                        if (vi.y == vj.y)
                        {
                            vi.x += vj.x;
                            vj.x = 0;

                            array[i] = vi;
                            array[j] = vj;
                        }
                    }
                }

                for (int x = 0; x < gridSize.x; ++x)
                {
                    for (int y = 1; y < gridSize.y; ++y)
                    {
                        var i = x + y * gridSize.x;
                        var j = x + (y - 1) * gridSize.x;

                        var vi = array[i];
                        var vj = array[j];

                        if (vi.x == vj.x)
                        {
                            vi.y += vj.y;
                            vj.y = 0;

                            array[i] = vi;
                            array[j] = vj;
                        }
                    }
                }

                var rect = new Vector3[Rects.Vertices.Length];
                Rects.GetVertices(rect, gridPosition + size * 0.5f, size, Quaternion.identity);
                UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, kWallColor, kWallColor);

                for (int y = 0; y < gridSize.y; ++y)
                {
                    for (int x = 0; x < gridSize.x; ++x)
                    {
                        var i = x + y * gridSize.x;
                        var vi = array[i];

                        if (vi.x > 0 && vi.y > 0)
                        {
                            var size = Mathx.Mul(vi, unit);
                            var position = gridPosition + new Vector2(x + 1, y + 1) * unit - size * 0.5f;

                            Rects.GetVertices(rect, position, size, Quaternion.identity);
                            UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, kRoomColor, kRoomColor);
                        }
                    }
                }
            }
        }
#endif
    }
}
