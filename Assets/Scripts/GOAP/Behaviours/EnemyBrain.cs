using CrashKonijn.Goap.Behaviours;
using UnityEngine;
using VRDefender.GOAP.Goals;

namespace VRDefender.GOAP.Behaviours
{
    [RequireComponent(typeof(AgentBehaviour))]
    public class EnemyBrain : MonoBehaviour
    {
        private AgentBehaviour agentBehaviour;
        

        private void Awake()
        {
            agentBehaviour = GetComponent<AgentBehaviour>();
            
        }
        private void Start()
        {
            //agentBehaviour.SetGoal<WanderGoal>(false);
            agentBehaviour.SetGoal<KillPlayer>(true);
        }
    }
}
