using UnityEngine;
using UnityEngine.AI;

public class CarAgent : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform[] targetPoints;
    private int currentTargetIndex = -1; // Track the current target
    private int spawnPointIndex; // Store the spawn point index
    private GameObject car;

    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    public void Initialize(NavMeshAgent navAgent, Transform[] targets)
    {
        agent = navAgent;
        targetPoints = targets;

        // Set the initial spawn point as the first target
        spawnPointIndex = Random.Range(0, targetPoints.Length);
        transform.position = targetPoints[spawnPointIndex].position;
        GoToNextPoint();
        agent.autoRepath = true;
    }

    void Update()
    {
        // Check if the car has reached its destination and move to the next point
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }

        // Rotate the car to align its forward direction with the velocity
        if (agent.velocity.magnitude > 0.1f) // Only rotate if moving
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Check for being stuck
        if (Vector3.Distance(lastPosition, transform.position) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 2f) // Adjust time threshold as needed
            {
                HandleStuckAgent();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f; // Reset timer if moving
        }

        lastPosition = transform.position;
    }

    void HandleStuckAgent()
    {
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            // Attempt to recalculate the path to the next destination
            agent.ResetPath();
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1000f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position); // Teleport to the nearest NavMesh point
            }
            GoToNextPoint(); // Try moving to the next point
        }
    }

    private void GoToNextPoint()
    {
        if (targetPoints.Length == 0) return;

        int newTargetIndex;
        // Ensure the new target index is different from the current and spawn indices
        do
        {
            newTargetIndex = Random.Range(0, targetPoints.Length);
        } while (newTargetIndex == currentTargetIndex);

        //Debug.Log("Nav Agent Index: " + newTargetIndex);
        currentTargetIndex = newTargetIndex;

        // Set the NavMeshAgent destination
        agent.destination = targetPoints[currentTargetIndex].position;
        
    }

    public void ToggleMovement(bool enable)
    {
        if (agent != null)
        {
            agent.enabled = enable;
        }
    }
}
