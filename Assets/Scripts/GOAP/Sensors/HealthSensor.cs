using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;
using VRDefender.GOAP.Behaviors;

namespace VRDefender.GOAP.Sensors
{
    public class HealthSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
            
        }

        public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
        {
            return new SenseValue(Mathf.CeilToInt(references.GetCachedComponent<TakeCoverBehaviour>().Health));
        }

        public override void Update()
        {
            
        }
    }
}

