using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;
using VRDefender.GOAP.Config;

namespace VRDefender.GOAP.Sensors
{
    public class PlayerTargetSensor : LocalTargetSensorBase,IInjectable
    {
        private ITarget playerTarget;
        AttackConfigSO attackConfigSO;
        GameObject player;
        public override void Created()
        {
            // Find the player GameObject by its tag.
            player = GameObject.FindWithTag("Player");

            
        }

        public void Inject(DependencyInjector injector)
        {
            attackConfigSO = injector.attackConfigSO;
        }

        public override ITarget Sense(IMonoAgent agent, IComponentReference references)
        {
            // // If we found the player, create a Target object to store its transform.
            if (player != null)
            {
                Debug.Log(player.transform+" player.transform");
                return new TransformTarget(player.transform);
            }
            return null;
        }

        public override void Update()
        {
            
        }
    }
}

