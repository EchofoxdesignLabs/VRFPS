using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using VRDefender.GOAP.Config;

namespace VRDefender.GOAP
{
    public class DependencyInjector : GoapConfigInitializerBase, IGoapInjector
    {
        public AttackConfigSO attackConfigSO;
        public override void InitConfig(GoapConfig config)
        {
            config.GoapInjector = this;
        }

        public void Inject(IActionBase action)
        {
            if (action is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }

        public void Inject(IGoalBase goal)
        {
            if (goal is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }

        public void Inject(IWorldSensor worldSensor)
        {
            if (worldSensor is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }

        public void Inject(ITargetSensor targetSensor)
        {
            if (targetSensor is IInjectable injectable)
            {
                injectable.Inject(this);
            }
        }
    }
}

