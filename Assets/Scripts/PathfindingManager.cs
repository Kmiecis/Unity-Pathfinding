using Common.Extensions;
using Custom.Pathfinding;
using UnityEngine;
using Random = System.Random;

namespace Common
{
    public class PathfindingManager : MonoBehaviour
    {
        public PF_Agent[] agents;

        private readonly Random _random = new Random(0);

        private void SetAgentPath(PF_Agent agent)
        {
            var agentPosition = agent.transform.position;

            var instances = PF_IInstance.Instances;
            foreach (var instance in instances)
            {
                if (
                    instance is PF_Instance instance2D &&
                    instance2D.Contains(agentPosition)
                )
                {
                    var min = instance2D.GridPosition;
                    var max = min + instance2D.GridSize;

                    var position = _random.NextVector2(min, max);

                    agent.TryMove(position);
                    break;
                }
                else if (
                    instance is PF_Instance3D instance3D &&
                    instance3D.Contains(agentPosition)
                )
                {
                    var min = instance3D.GridPosition;
                    var max = min + instance3D.GridSize;

                    var position = _random.NextVector3(min, max);

                    agent.TryMove(position);
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
