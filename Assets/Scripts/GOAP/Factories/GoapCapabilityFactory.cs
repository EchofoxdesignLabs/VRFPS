using System;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using VRDefender.GOAP.Actions;
using VRDefender.GOAP.Goals;
using VRDefender.GOAP.Sensors;
using VRDefender.GOAP.Targets;
using VRDefender.GOAP.WorldKeys;

namespace VRDefender.GOAP.Factories
{
    public class GoapCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            CapabilityBuilder builder = new("WanderCapability");
            BuildGoals(builder);
            BuildActions(builder);
            BuildSensores(builder);
            return builder.Build();
        }

        private void BuildSensores(CapabilityBuilder builder)
        {
            builder.AddGoal<WanderGoal>().AddCondition<IsWandering>(Comparison.GreaterThanOrEqual, 1);
        }

        private void BuildActions(CapabilityBuilder builder)
        {
            builder.AddAction<WanderAction>().SetTarget<WanderTarget>().AddEffect<IsWandering>(EffectType.Increase)
            .SetBaseCost(5).SetStoppingDistance(10);
        }

        private void BuildGoals(CapabilityBuilder builder)
        {
            builder.AddTargetSensor<WanderTargetSensor>().SetTarget<WanderTarget>();
        }
    }
}

