using System;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes.Builders;
using CrashKonijn.Goap.Configs.Interfaces;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Resolver;
using UnityEngine;
using VRDefender.GOAP.Actions;
using VRDefender.GOAP.Goals;
using VRDefender.GOAP.Sensors;
using VRDefender.GOAP.Targets;
using VRDefender.GOAP.WorldKeys;

namespace VRDefender.GOAP.Factories
{
    [RequireComponent(typeof(DependencyInjector))]
    public class GoapSetConfigFactory : GoapSetFactoryBase
    {
        private DependencyInjector injector;
        public override IGoapSetConfig Create()
        {
            injector = GetComponent<DependencyInjector>();
            GoapSetBuilder builder = new("EnemySet");

            BuildGoals(builder);
            BuildActions(builder);
            BuildSensors(builder);

            return builder.Build();
        }

        private void BuildSensors(GoapSetBuilder builder)
        {
            builder.AddTargetSensor<WanderTargetSensor>()
                .SetTarget<WanderTarget>();
            builder.AddTargetSensor<PlayerTargetSensor>()
                .SetTarget<PlayerTarget>();
            builder.AddTargetSensor<CoverTargetSensor>()
                .SetTarget<CoverTarget>();
            builder.AddWorldSensor<HealthSensor>()
                .SetKey<Health>();
        }

        private void BuildActions(GoapSetBuilder builder)
        {
            builder.AddAction<WanderAction>()
                .SetTarget<WanderTarget>()
                .AddEffect<IsWandering>(EffectType.Increase)
                .SetBaseCost(5)
                .SetInRange(10);
            builder.AddAction<ShootAction>()
                .SetTarget<PlayerTarget>()
                .AddEffect<PlayerHealth>(EffectType.Decrease)
                .SetBaseCost(injector.attackConfigSO.rangedAttackCost)
                .SetInRange(injector.attackConfigSO.sensorRadius);
            builder.AddAction<TakeCoverAction>()
                .SetTarget<CoverTarget>()
                .AddEffect<Health>(EffectType.Increase)
                .SetBaseCost(8)
                .SetInRange(2);
        }

        private void BuildGoals(GoapSetBuilder builder)
        {
            builder.AddGoal<WanderGoal>()
                .AddCondition<IsWandering>(Comparison.GreaterThanOrEqual, 1);
            builder.AddGoal<KillPlayer>()
                .AddCondition<PlayerHealth>(Comparison.SmallerThanOrEqual, 0);
            builder.AddGoal<TakeCoverGoal>()
                .AddCondition<Health>(Comparison.GreaterThanOrEqual, 80);
        }
    }
}

