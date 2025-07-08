using System;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using VRDefender.GOAP.Behaviors;
using VRDefender.GOAP.Goals;

namespace VRDefender.GOAP.Behaviours
{
    [RequireComponent(typeof(AgentBehaviour))]
    public class EnemyBrain : MonoBehaviour
    {
        private AgentBehaviour agentBehaviour;
        private TakeCoverBehaviour takeCoverBehaviour;


        private void Awake()
        {
            agentBehaviour = GetComponent<AgentBehaviour>();
            takeCoverBehaviour = GetComponent<TakeCoverBehaviour>();

        }
        private void Start()
        {
            //agentBehaviour.SetGoal<WanderGoal>(false);
            agentBehaviour.SetGoal<KillPlayer>(true);
        }

        private void OnEnable()
        {
            this.agentBehaviour.Events.OnActionStop += this.OnActionStop;
            this.agentBehaviour.Events.OnNoActionFound += this.OnNoActionFound;
            this.agentBehaviour.Events.OnGoalCompleted += this.OnGoalCompleted;
        }

        private void OnGoalCompleted(IGoalBase goal)
        {
            Debug.Log("Goal Completed "+goal);
        }

        private void OnNoActionFound(IGoalBase goal)
        {
            
        }

        private void OnActionStop(IActionBase action)
        {
            
        }

        private void OnDisable()
        {
            this.agentBehaviour.Events.OnActionStop -= this.OnActionStop;
            this.agentBehaviour.Events.OnNoActionFound -= this.OnNoActionFound;
            this.agentBehaviour.Events.OnGoalCompleted -= this.OnGoalCompleted;
        }

        private void Update()
        {
            if (takeCoverBehaviour.Health < 80)
            {
                agentBehaviour.SetGoal<TakeCoverGoal>(endAction: true);
            }
            else if (takeCoverBehaviour.Health >= 80 && agentBehaviour.CurrentGoal is TakeCoverGoal)
            {
                agentBehaviour.SetGoal<KillPlayer>(false);
            }
            else
            {
                //agentBehaviour.SetGoal<KillPlayer>(false);
            }
        }
    }
}
