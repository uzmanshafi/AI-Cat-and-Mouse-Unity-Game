using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    public Transform mouse;
    public float visibilityRange = 5f;
    public float movementSpeed = 3f;

    private Vector3 originalPosition;
    private NavMeshAgent navMeshAgent;
    private bool isMouseSpotted = false;

    private void Start()
    {
        originalPosition = transform.position;
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Check if mouse is within visibility range
        if (mouse != null && !isMouseSpotted)
        {
            float distance = Vector3.Distance(transform.position, mouse.position);
            if (distance <= visibilityRange)
            {
                isMouseSpotted = true;
                navMeshAgent.SetDestination(mouse.position);
            }
        }

        // Check if reached the mouse
        if (isMouseSpotted && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            // Remove the mouse game object from the scene
            Destroy(mouse.gameObject);
            isMouseSpotted = false;
            navMeshAgent.SetDestination(originalPosition);
        }
    }
}
