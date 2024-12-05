using UnityEngine;

namespace Custom.Pathfinding
{
    public interface PF_IMapper
    {
        bool IsPathable(Vector2Int position);

        float GetWalkMultiplier(Vector2Int position);
    }
}
