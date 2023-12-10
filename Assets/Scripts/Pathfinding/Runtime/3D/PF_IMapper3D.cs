using UnityEngine;

namespace Custom.Pathfinding
{
    public interface PF_IMapper3D
    {
        bool IsWalkable(Vector3Int position, int size);

        int GetWalkCost(Vector3Int position, int size);
    }
}
