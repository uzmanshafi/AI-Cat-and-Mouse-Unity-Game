using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class VerminMasterAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public List<Transform> CheeseTargets;
    public float activateDistance = 20f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float speed = 4f;
    public float nextWaypointDistance = 3f;

    [Header("Custom Behavior")]
    public bool findClosestCheese = true;
    public bool directionBasedOnVelocity = true;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Transform targetCheese;

    private Vector2 originalPosition;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        targetCheese = FindClosestCheese();
        originalPosition = transform.position;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    void UpdatePath()
    {
        if (findClosestCheese && seeker.IsDone())
        {
            if (CheeseTargets.Count == 0)
            {
                // if all cheese has been eaten, set target back to original position
                seeker.StartPath(rb.position, originalPosition, OnPathComplete);
            }
            else
            {
                targetCheese = FindClosestCheese();
                if (targetCheese != null)
                {
                    seeker.StartPath(rb.position, targetCheese.position, OnPathComplete);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (TargetInDistance() && findClosestCheese)
        {
            PathFollow();
        }
    }

    void PathFollow()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        rb.velocity = direction * speed;

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (directionBasedOnVelocity)
        {
            if (rb.velocity.x >= 0.01f)
            {
                transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            }
            else if (rb.velocity.x <= -0.01f)
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }

    private bool TargetInDistance()
    {
        if (targetCheese != null)
        {
            return Vector2.Distance(transform.position, targetCheese.position) < activateDistance;
        }
        else
        {
            return false;
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private Transform FindClosestCheese()
    {
        Transform closestCheese = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Transform cheeseTransform in CheeseTargets)
        {
            Vector3 directionToCheese = cheeseTransform.position - currentPosition;
            float dSqrToTarget = directionToCheese.sqrMagnitude;
            if (dSqrToTarget < closestDistance)
            {
                closestDistance = dSqrToTarget;
                closestCheese = cheeseTransform;
            }
        }

        return closestCheese;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered with " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Cheese"))
        {
            CheeseTargets.Remove(collision.transform);
            Destroy(collision.gameObject);

            // Find next closest cheese
            targetCheese = FindClosestCheese();
            // If there is a new target, start path
            if (targetCheese != null)
            {
                seeker.StartPath(rb.position, targetCheese.position, OnPathComplete);
            }
            else if (CheeseTargets.Count == 0)
            {
                // if all cheese has been eaten, set target back to original position
                seeker.StartPath(rb.position, originalPosition, OnPathComplete);
            }
        }
    }
}
