using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace VRDefender.GOAP.Actions
{
    public class WanderAction : GoapActionBase<CommonData>
    {
        public override void Created()
        {
            base.Created();
        }
        public override void Start(IMonoAgent agent, CommonData data)
        {
            // base.Start(agent, data);
            data.Timer = Random.Range(0, 2);
        }
        public override IActionRunState Perform(IMonoAgent agent, CommonData data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;
            if (data.Timer > 0)
            {
                return ActionRunState.Continue;
            }
            return ActionRunState.Stop;
        }
        public override void End(IMonoAgent agent, CommonData data)
        {
            //base.End(agent, data);
        }
    }
}

