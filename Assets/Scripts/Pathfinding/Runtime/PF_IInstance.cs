using System.Collections.Generic;
using UnityEngine;

namespace Custom.Pathfinding
{
    public interface PF_IInstance
    {
        public static List<PF_IInstance> Instances = new List<PF_IInstance>();

        bool TryFindPath(Vector3 start, Vector3 target, int size, out List<Vector3> path);
    }
}
