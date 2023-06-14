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
    public float speed = 2f;
    public float maxSpeed = 8f;
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

    public bool isBeingChased = false;
    public Transform catTransform;

    private Vector2 escapeDirection;

    private float baseSpeed;

    bool isMouseAlive = true;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        targetCheese = FindClosestCheese();
        originalPosition = transform.position;
        catTransform = GameObject.FindGameObjectWithTag("Cat").transform;
        baseSpeed = speed;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            if (isBeingChased)
            {
                speed = baseSpeed + 1;  // Increases speed when being chased

                // Update the escape direction
                escapeDirection = (transform.position - catTransform.position).normalized;
                // Move in the opposite direction of the cat
                seeker.StartPath(rb.position, rb.position + escapeDirection * activateDistance, OnPathComplete);
            }
            else
            {
                speed = baseSpeed;  // Revert back to base speed when not being chased

                // Find new cheese target only when not being chased
                if (findClosestCheese && CheeseTargets.Count > 0)
                {
                    targetCheese = FindClosestCheese();
                    if (targetCheese != null)
                    {
                        seeker.StartPath(rb.position, targetCheese.position, OnPathComplete);
                    }
                }
                else
                {
                    // if all cheese has been eaten, set target back to original position
                    seeker.StartPath(rb.position, originalPosition, OnPathComplete);
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

        // Ensures speed does not exceed maxSpeed
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
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

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = path.vectorPath[currentWaypoint];
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);

        // Checks if the mouse has reached its destination
        if (newPosition == targetPosition)
        {
            currentWaypoint++;
        }

        if (directionBasedOnVelocity && currentWaypoint < path.vectorPath.Count)
        {
            if (rb.position.x < path.vectorPath[currentWaypoint].x)
            {
                transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            }
            else if (rb.position.x > path.vectorPath[currentWaypoint].x)
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
        currentWaypoint = 0;  // Resets currentWaypoint whenever a new cheese target is set

        return closestCheese;
    }

    public int GetCheeseCount()
    {
        return CheeseTargets.Count;
    }

    public bool HasMouseEatenALLCheese()
    {
        return CheeseTargets.Count == 0;
    }

    public void OnCatSighting()
    {
        isBeingChased = true;
    }

    public void OnCatLost()
    {
        isBeingChased = false;
    }

    public bool IsMouseAlive()
    {
        return isMouseAlive;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered with " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Cat"))
        {
            // Cat has caught the mouse
            Destroy(gameObject);
            isMouseAlive = false;
        }
        else if(collision.gameObject.CompareTag("Cheese"))
        {
            CheeseTargets.Remove(collision.transform);
            Destroy(collision.gameObject);

            // Finds next closest cheese
            targetCheese = FindClosestCheese();
            // If there is a new target, starts path
            if (targetCheese != null)
            {
                seeker.StartPath(rb.position, targetCheese.position, OnPathComplete);
            }
        }
    }
}
