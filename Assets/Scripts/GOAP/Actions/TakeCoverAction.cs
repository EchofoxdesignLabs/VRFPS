using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using VRDefender.GOAP.Behaviors;
using VRDefender.GOAP.Config;

namespace VRDefender.GOAP.Actions
{
    public class TakeCoverAction : ActionBase<TakeCoverAction.Data>,IInjectable
    {
        private GetToSafetyConfigSO getToSafetyConfigSO;
        private static readonly int IS_CROUCHING = Animator.StringToHash("IsCrouching");
        private static readonly int IS_STANDING = Animator.StringToHash("IsStandingUp");
        public override void Created()
        {

        }

        public override void End(IMonoAgent agent, Data data)
        {
            // data.Animator.SetBool(IS_CROUCHING, false);
            // data.Animator.SetBool(IS_STANDING, true);
            data.TakeCoverBehaviour.enabled = true;
        }

        public void Inject(DependencyInjector injector)
        {
            getToSafetyConfigSO = injector.getToSafetyConfigSO;
        }

        public override ActionRunState Perform(IMonoAgent agent, Data data, ActionContext context)
        {
            data.Timer -= context.DeltaTime;
            Debug.Log(data.Animator+" data.Animator");
            data.Animator.SetBool(IS_CROUCHING, true);
            data.Animator.SetBool(IS_STANDING, false);
            data.TakeCoverBehaviour.Health += context.DeltaTime * 1f;
            if (data.Target == null || data.TakeCoverBehaviour.Health >= 80)
            {
                return ActionRunState.Stop;
            }
            return ActionRunState.Continue;
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.TakeCoverBehaviour.enabled = false;
            data.Timer = 1f;
        }

        public class Data : CommonData
        {
            [GetComponent] public Animator Animator { get; set; }
            [GetComponent] public TakeCoverBehaviour TakeCoverBehaviour { get; set; }
        }
    }
}

