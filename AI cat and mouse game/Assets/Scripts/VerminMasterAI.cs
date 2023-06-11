using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class VerminMasterAI : MonoBehaviour
{
    [Header("Pathfinding")]
    public List<Transform> CheeseTargets;
    public float activateDistance = 10f;
    public float pathUpdateSeconds = 0.5f;

    [Header("Physics")]
    public float speed = 200f;
    public float nextWaypointDistance = 3f;

    [Header("Custom Behavior")]
    public bool findClosestCheese = false;
    public bool directionBasedOnVelocity = true;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Transform targetCheese;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
    }

    void UpdatePath()
    {
        if(findClosestCheese && seeker.IsDone())
        {
            targetCheese = FindClosestCheese();
            if(targetCheese != null)
            {
                seeker.StartPath(rb.position, targetCheese.position, OnPathComplete);
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
        if(path == null)
        {
            return;
        }

        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (directionBasedOnVelocity)
        {
            if(rb.velocity.x >= 0.01f)
            {
                transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            } 
            else if(rb.velocity.x <= -0.01f)
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
        if(!p.error)
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

        foreach(Transform cheeseTransform in CheeseTargets)
        {
            Vector3 directionToCheese = cheeseTransform.position - currentPosition;
            float dSqrToTarget = directionToCheese.sqrMagnitude;
            if(dSqrToTarget < closestDistance)
            {
                closestDistance = dSqrToTarget;
                closestCheese = cheeseTransform;
            }
        }

        return closestCheese;
    }

    private void OnCollisionTrigger2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Cheese")
        {
            Destroy(collision.gameObject);
        }
    }
}