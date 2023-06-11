using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    public float visibilityRange = 5f;
    public float movementSpeed = 3f;
    public float patrolAreaRadius = 10f;

    private Transform mouse;
    private NavMeshAgent navMeshAgent;
    private bool isMouseVisible = false;
    private Vector3 randomPatrolDestination;

    private void Start()
    {
        mouse = GameObject.FindGameObjectWithTag("Mouse").transform;

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        SetRandomPatrolDestination();
    }

    private void Update()
    {
        // Check if the mouse is within visibility range
        isMouseVisible = false;
        float distanceToMouse = Vector3.Distance(transform.position, mouse.position);
        if (distanceToMouse <= visibilityRange)
        {
            isMouseVisible = true;
        }

        // Chase the mouse if it is visible
        if (isMouseVisible)
        {
            navMeshAgent.SetDestination(mouse.position);
        }
        else
        {
            // Continue patrolling if the mouse is not visible
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.1f)
            {
                SetRandomPatrolDestination();
            }
        }
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolAreaRadius;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, patrolAreaRadius, -1);
        randomPatrolDestination = navHit.position;

        navMeshAgent.SetDestination(randomPatrolDestination);
    }
}
