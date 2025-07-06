using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using VRDefender.GOAP.Goals;

namespace VRDefender.GOAP.Behaviours
{
    [RequireComponent(typeof(AgentBehaviour)),RequireComponent(typeof(GoapActionProvider))]
    public class EnemyBrain : MonoBehaviour
    {
        private AgentBehaviour agentBehaviour;
        private GoapActionProvider goapActionProvider;

        private void Awake()
        {
            agentBehaviour = GetComponent<AgentBehaviour>();
            goapActionProvider = GetComponent<GoapActionProvider>();
        }
        private void Start()
        {
            goapActionProvider.RequestGoal<WanderGoal>(false);
        }
    }
}

