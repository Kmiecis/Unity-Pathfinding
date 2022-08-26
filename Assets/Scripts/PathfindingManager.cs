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
            var agentPosition = agent.transform.position;

            var instances = PF_Instance.Instances;
            foreach (var instance in instances)
            {
                if (instance.Contains(agentPosition))
                {
                    var grid = instance.Grid;
                    var gridWidth = grid.GetWidth();
                    var gridHeight = grid.GetHeight();
                    var position = Vector2.zero;
                    do
                    {
                        position.x = _random.NextFloat(gridWidth);
                        position.y = _random.NextFloat(gridHeight);
                        position += instance.GridPosition;
                    }
                    while (!instance.Contains(position));

                    agent.Move(position);
                    break;
                }
            }
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
