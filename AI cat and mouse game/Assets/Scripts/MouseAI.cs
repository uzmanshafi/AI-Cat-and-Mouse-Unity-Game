using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MouseAI : MonoBehaviour
{
    public Transform[] cheeses;
    public Transform[] hidingSpots;
    public float visibilityRange = 5f;
    public float movementSpeed = 3f;

    private int currentCheeseIndex = 0;
    private NavMeshAgent navMeshAgent;
    private bool isCatVisible = false;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (cheeses.Length == 0)
        {
            Debug.Log("No cheeses available. Mouse has won!");
            return;
        }

        // Check if cat is within visibility range
        isCatVisible = false;
        foreach (Transform cat in hidingSpots)
        {
            float distance = Vector3.Distance(transform.position, cat.position);
            if (distance <= visibilityRange)
            {
                isCatVisible = true;
                break;
            }
        }

        // Find the closest hiding spot if the cat is visible
        if (isCatVisible)
        {
            Transform closestSpot = FindClosestHidingSpot();
            if (closestSpot != null)
            {
                navMeshAgent.SetDestination(closestSpot.position);
            }
        }
        else
        {
            // If the cat is not visible, move towards the current cheese
            navMeshAgent.SetDestination(cheeses[currentCheeseIndex].position);
        }

        // Check if reached the current cheese
        if (Vector3.Distance(transform.position, cheeses[currentCheeseIndex].position) < navMeshAgent.stoppingDistance)
        {
            // Remove the cheese from the scene
            Destroy(cheeses[currentCheeseIndex].gameObject);
            currentCheeseIndex++;

            if (currentCheeseIndex >= cheeses.Length)
            {
                Debug.Log("All cheeses eaten. Mouse has won!");
            }
        }
    }

    private Transform FindClosestHidingSpot()
    {
        Transform closestSpot = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform spot in hidingSpots)
        {
            float distance = Vector3.Distance(transform.position, spot.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpot = spot;
            }
        }

        return closestSpot;
    }
}
