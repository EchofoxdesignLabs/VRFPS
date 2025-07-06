using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace VRDefender.GOAP.Sensors
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
            
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            Vector3 position = GetRandomPosition(agent);
            return new PositionTarget(position);
        }

        private Vector3 GetRandomPosition(IActionReceiver agent)
        {
            int count = 0;
            while (count < 5)
            {
                Vector2 random = Random.insideUnitCircle * 5;
                Vector3 position = agent.Transform.position + new Vector3(random.x, 0, random.y);
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1, NavMesh.AllAreas))
                {
                    return hit.position;
                }
                count++;
            }
            return agent.Transform.position;
        }

        public override void Update()
        {

        }
    }
}

