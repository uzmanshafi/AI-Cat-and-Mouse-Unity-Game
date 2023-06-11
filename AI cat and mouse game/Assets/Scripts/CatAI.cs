using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    public float visibilityRange = 5f;
    public float movementSpeed = 3f;
    public Vector3[] patrolWaypoints;
    private int currentWaypointIndex = 0;
    private Vector3 originalPosition;

    private Transform mouse;
    private NavMeshAgent navMeshAgent;
    private bool isMouseVisible = false;

    private Transform circleSprite;

    private void Start()
    {
        mouse = GameObject.FindGameObjectWithTag("Mouse").transform;

        if (mouse == null)
        {
            Debug.LogError("Mouse GameObject not found!");
            return;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        originalPosition = transform.position;
        patrolWaypoints = new Vector3[4];
        patrolWaypoints[0] = originalPosition + new Vector3(1, 1, 0);
        patrolWaypoints[1] = originalPosition + new Vector3(-1, 1, 0);
        patrolWaypoints[2] = originalPosition + new Vector3(-1, -1, 0);
        patrolWaypoints[3] = originalPosition + new Vector3(1, -1, 0);

        // Find circleSprite as a child of this GameObject
        circleSprite = transform.Find("CircleVisibility");
        if (circleSprite == null)
        {
            Debug.LogError("CircleSprite not found!");
            return;
        }
    }

    private void Update()
    {
        // Draw visibility range
        circleSprite.localScale = Vector3.one * visibilityRange * 2; // Multiply by 2 because visibilityRange is a radius, but localScale works with diameter

        // Check if the mouse is within visibility range and there is no obstacle
        isMouseVisible = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, mouse.position - transform.position, visibilityRange);
        if (hit.collider != null && hit.collider.transform == mouse)
        {
            isMouseVisible = true;
        }

        if(isMouseVisible)
        {
            // Chase the mouse if it is visible
            navMeshAgent.SetDestination(mouse.position);
        }
        else
        {
            if(navMeshAgent.remainingDistance < 0.1f) {
                // Patrol around original position if the mouse is not visible
                currentWaypointIndex = (currentWaypointIndex + 1) % 4;
                navMeshAgent.SetDestination(patrolWaypoints[currentWaypointIndex]);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mouse"))
        {
            Debug.Log("Mouse has been caught!");
            Destroy(collision.gameObject);
        }
    }
}
