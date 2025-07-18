using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This script controls the movement of an animal GameObject.
/// It makes the animal wander to random points on a NavMesh, waits for a bit, 
/// and plays animations for running and idling.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AnimalMovement : MonoBehaviour
{
    // Inspector-assigned variables for core components
    [Header("Component References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Wander Settings")]
    [Tooltip("The radius within which the animal will search for a new random destination.")]
    [SerializeField] private float wanderRadius = 20f;
    [Tooltip("The minimum time the animal will wait at a destination before moving again.")]
    [SerializeField] private float minIdleTime = 2f;
    [Tooltip("The maximum time the animal will wait at a destination before moving again.")]
    [SerializeField] private float maxIdleTime = 5f;


    // Animator parameter hash for performance
    private static readonly int RUN_ANIMATION_HASH = Animator.StringToHash("Run");

    // Flag to ensure the wait coroutine is only started once.
    private bool isWaiting = false;

    /// <summary>
    /// Called once when the script instance is being loaded.
    /// Used for initialization.
    /// </summary>
    void Start()
    {
        // Good practice: ensure components are assigned. If not, try to get them from the GameObject.
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Start the wandering behavior as soon as the game begins.
        MoveToNewRandomDestination();
    }

    /// <summary>
    /// Update is called once per frame.
    /// We use this to check the agent's state and update animations.
    /// </summary>
    void Update()
    {
        // Update the animator based on whether the agent is moving.
        // When the agent stops, its velocity will be zero, setting "Run" to false and allowing the Idle animation to play.
        animator.SetBool(RUN_ANIMATION_HASH, navMeshAgent.velocity.sqrMagnitude > 0.1f);

        // Check if the agent has reached its destination and is not already in the waiting state.
        if (!isWaiting && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            // If we've arrived, start the coroutine to wait and then find a new destination.
            StartCoroutine(WaitAndFindNewDestination());
        }
    }

    /// <summary>
    /// A coroutine that waits for a random duration and then triggers the move to a new destination.
    /// </summary>
    private IEnumerator WaitAndFindNewDestination()
    {
        // Set the flag to true so this coroutine doesn't get called again while it's running.
        isWaiting = true;

        // Calculate a random wait time based on the min and max idle times.
        float waitTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(waitTime);

        // Once the wait is over, find a new place to go.
        MoveToNewRandomDestination();

        // Reset the flag so the agent can trigger the wait again at the next destination.
        isWaiting = false;
    }


    /// <summary>
    /// Finds a random point on the NavMesh within the wanderRadius and sets it as the agent's new destination.
    /// </summary>
    private void MoveToNewRandomDestination()
    {
        // Find a random direction and multiply by the wander radius.
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        
        // Add the random direction to the agent's current position to get a target point.
        randomDirection += transform.position;

        NavMeshHit navHit;
        // Use NavMesh.SamplePosition to find the closest valid point on the NavMesh to our random target point.
        // The '-1' is a layer mask for all NavMesh areas.
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, -1))
        {
            // If a valid point was found, set it as the new destination.
            navMeshAgent.SetDestination(navHit.position);
        }
    }
}
