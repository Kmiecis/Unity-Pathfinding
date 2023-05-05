using UnityEngine;

namespace Custom.Pathfinding
{
    public interface PF_IMapper3D
    {
        bool IsWalkable(Vector3Int position);

        int GetWalkCost(Vector3Int position);
    }
}
