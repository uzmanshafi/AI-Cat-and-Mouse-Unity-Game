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

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.SetDestination(cheeses[currentCheeseIndex].position);
    }

    private void Update()
    {
        // Check if cat is within visibility range
        isCatVisible = false;
        foreach (Transform cat in hidingSpots)
        {
            float distance = Vector3.Distance(transform.position, cat.position);
            if (distance <= visibilityRange)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, cat.position - transform.position, distance);
                if (hit.collider != null && hit.collider.transform.CompareTag("Cat"))
                {
                    isCatVisible = true;
                    break;
                }
            }
        }

        if(navMeshAgent.hasPath)
        {
            lineRenderer.positionCount = navMeshAgent.path.corners.Length;
            lineRenderer.SetPositions(navMeshAgent.path.corners);
        }
        else
        {
            lineRenderer.positionCount = 0;
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
            if (cheeses.Length > 0 && Vector3.Distance(transform.position, cheeses[currentCheeseIndex].position) < navMeshAgent.stoppingDistance)
            {
                // Remove the cheese from the scene
                Destroy(cheeses[currentCheeseIndex].gameObject);
                currentCheeseIndex++;

                if (currentCheeseIndex < cheeses.Length)
                {
                    navMeshAgent.SetDestination(cheeses[currentCheeseIndex].position);
                }
                else
                {
                    Debug.Log("All cheeses eaten. Mouse has won!");
                }
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
                RaycastHit2D hit = Physics2D.Raycast(transform.position, spot.position - transform.position, distance);
                if (hit.collider != null && hit.collider.transform == spot)
                {
                    closestDistance = distance;
                    closestSpot = spot;
                }
            }
        }

        return closestSpot;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Cat"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Cheese"))
        {
            Destroy(other.gameObject);
            currentCheeseIndex++;
            if (currentCheeseIndex < cheeses.Length)
            {
                navMeshAgent.SetDestination(cheeses[currentCheeseIndex].position);
            }
            else
            {
                Debug.Log("All cheeses eaten. Mouse has won!");
            }
        }
    }
}
