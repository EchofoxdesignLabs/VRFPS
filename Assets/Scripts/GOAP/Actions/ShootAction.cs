using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using VRDefender.GOAP;
using VRDefender.GOAP.Config;

namespace VRDefender.GOAP.Actions
{
    public class ShootAction : ActionBase<ShootData>, IInjectable
    {
        private AttackConfigSO attackConfigSO;
        public override void Created()
        {
            
        }

        public override void End(IMonoAgent agent, ShootData data)
        {
            data.animator.SetBool(ShootData.SHOOT, false);
        }

        public void Inject(DependencyInjector injector)
        {
            attackConfigSO = injector.attackConfigSO;
        }

        public override ActionRunState Perform(IMonoAgent agent, ShootData data, ActionContext context)
        {
            data.Timer -= context.DeltaTime;
            bool shouldAttack = data.Target != null;
            data.animator.SetBool(ShootData.SHOOT, shouldAttack);
            if (shouldAttack)
            {
                agent.transform.LookAt(data.Target.Position);
            }
            return data.Timer > 0 ? ActionRunState.Continue : ActionRunState.Stop;
        }

        public override void Start(IMonoAgent agent, ShootData data)
        {
            data.Timer = attackConfigSO.attackDelay;
        }
    }
}

