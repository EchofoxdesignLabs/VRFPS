using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using UnityEngine.AI;
using VRDefender.GOAP.Targets;

namespace VRDefender.GOAP.Behaviours
{
    [RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator)), RequireComponent(typeof(AgentBehaviour))]
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private AgentBehaviour agentBehaviour;
        private ITarget currentTarget;
        [SerializeField] private float minMoveDistance = 0.25f;
        private Vector3 lastPosition;
        private static readonly int WALK = Animator.StringToHash("Walk");

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            agentBehaviour = GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {

            agentBehaviour.Events.OnTargetChanged += EventsOnTargetChanged;
            agentBehaviour.Events.OnTargetOutOfRange += EventsOnTargetNotInRange;
        }



        private void OnDisable()
        {

            agentBehaviour.Events.OnTargetChanged -= EventsOnTargetChanged;
            agentBehaviour.Events.OnTargetOutOfRange -= EventsOnTargetNotInRange;
        }

        private void EventsOnTargetNotInRange(ITarget target)
        {
            animator.SetBool(WALK, value: false);
        }

        private void EventsOnTargetChanged(ITarget target, bool inRange)
        {
            currentTarget = target;            
            Debug.Log(currentTarget+" currentTarget");
            Debug.Log(target is CoverTarget);
            lastPosition = currentTarget.Position;
            
                navMeshAgent.SetDestination(target.Position);
                animator.SetBool(WALK, value: true);
            
        }

        private void Update()
        {
            if (currentTarget == null)
            {
                return;
            }
            if (minMoveDistance <= Vector3.Distance(currentTarget.Position, lastPosition))
            {
                lastPosition = currentTarget.Position;
                navMeshAgent.SetDestination(currentTarget.Position);

            }
            
            animator.SetBool(WALK, navMeshAgent.velocity.magnitude > 0.1f);
        }
    }
}

