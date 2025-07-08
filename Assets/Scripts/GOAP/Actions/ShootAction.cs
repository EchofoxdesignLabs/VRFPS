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
        private static readonly int IS_CROUCHING = Animator.StringToHash("IsCrouching");
        private static readonly int IS_STANDING = Animator.StringToHash("IsStandingUp");
        public override void Created()
        {

        }

        public override void End(IMonoAgent agent, ShootData data)
        {
            data.Animator.SetBool(ShootData.SHOOT, false);
        }

        public void Inject(DependencyInjector injector)
        {
            attackConfigSO = injector.attackConfigSO;
        }

        public override ActionRunState Perform(IMonoAgent agent, ShootData data, ActionContext context)
        {
            data.Timer -= context.DeltaTime;
            bool shouldAttack = data.Target != null;
            data.Animator.SetBool(ShootData.SHOOT, shouldAttack);
            if (shouldAttack)
            {
                agent.transform.LookAt(data.Target.Position);
                data.Animator.SetBool(IS_CROUCHING, false);
                data.Animator.SetBool(IS_STANDING, true);
            }
            return data.Timer > 0 ? ActionRunState.Continue : ActionRunState.Stop;
        }

        public override void Start(IMonoAgent agent, ShootData data)
        {
            data.Timer = attackConfigSO.attackDelay;
        }
    }
}

