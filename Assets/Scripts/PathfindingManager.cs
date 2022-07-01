using Common.Extensions;
using Custom.Pathfinding;
using UnityEngine;
using Random = System.Random;

namespace Custom
{
    public class PathfindingManager : MonoBehaviour
    {
        public PF_Agent[] agents;

        private readonly Random _random = new Random(0);

        private void SetAgentPath(PF_Agent agent)
        {
            var instances = PF_Instance.Instances;
            var instance = _random.NextItem(instances);
            var grid = instance.Grid;
            var gridWidth = grid.GetWidth();
            var gridHeight = grid.GetHeight();
            var gridPosition = Vector2Int.zero;
            do
            {
                gridPosition.x = _random.Next(gridWidth);
                gridPosition.y = _random.Next(gridHeight);
            }
            while (!grid[gridPosition.x, gridPosition.y]);
            var position = instance.FromGridPosition(gridPosition);

            agent.Move(position);
        }

        private void Update()
        {
            if (agents != null)
            {
                foreach (var agent in agents)
                {
                    if (!agent.HasPath)
                    {
                        SetAgentPath(agent);
                    }
                }
            }    
        }
    }
}
