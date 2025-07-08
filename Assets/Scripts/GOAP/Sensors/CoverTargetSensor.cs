using System.Linq;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;
using VRDefender.GOAP;
using VRDefender.GOAP.Config;

namespace VRDefender.GOAP.Sensors
{
    public class CoverTargetSensor : LocalTargetSensorBase, IInjectable
    {
        private GetToSafetyConfigSO getToSafetyConfigSO;
        private Collider[] colliders = new Collider[5];
        public override void Created()
        {

        }

        public void Inject(DependencyInjector injector)
        {
            getToSafetyConfigSO = injector.getToSafetyConfigSO;
        }

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {
            Vector3 agentPosition = agent.transform.position;
            int hits = Physics.OverlapSphereNonAlloc(agentPosition, getToSafetyConfigSO.coverSearchRadius, colliders, getToSafetyConfigSO.coverLayer);
            if (hits == 0)
            {
                return null;
            }
            for (int i = colliders.Length - 1; i > hits; i--)
            {
                colliders[i] = null;
            }
            colliders = colliders.OrderBy(collider => collider == null ? float.MaxValue : (collider.transform.position - agent.transform.position).sqrMagnitude).ToArray();
            return new PositionTarget(colliders[0].transform.position);
        }

        public override void Update()
        {
            
        }
    }
}

