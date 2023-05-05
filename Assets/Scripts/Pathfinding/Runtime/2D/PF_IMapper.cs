using UnityEngine;

namespace Custom.Pathfinding
{
    public interface PF_IMapper
    {
        bool IsWalkable(Vector2Int position);

        int GetWalkCost(Vector2Int position);
    }
}
