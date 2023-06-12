using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CatAIController : MonoBehaviour
{
    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public Transform[] patrollingPoints;
    public float waitTime;
    int currentPositionIndex;
    public bool directionBasedOnVelocity = true;
    bool isWaiting;
    public Rigidbody2D rb;
    public bool isPatrolling = true;

    Path path;
    int currentWaypoint = 0;
    Seeker seeker;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        seeker.StartPath(rb.position, patrollingPoints[currentPositionIndex].position, OnPathComplete);
    }

    void Update()
    {
        if (isPatrolling)
        {
            if (path == null)
            {
                return;
            }

            if (currentWaypoint >= path.vectorPath.Count)
            {
                if (!isWaiting)
                {
                    StartCoroutine(WaitForNextPosition());
                }
                return;
            }

            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = direction * speed * Time.deltaTime;

            rb.AddForce(force);

            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }

            Flip();
        }
    }

    void Flip()
    {
        if (directionBasedOnVelocity)
        {
            if (rb.velocity.x > 0.1f)
            {
                transform.localScale = new Vector3(.8f, .8f, .8f);
            }
            else if (rb.velocity.x < -0.1f)
            {
                transform.localScale = new Vector3(.8f, .8f, .8f);
            }
        }
    }

    IEnumerator WaitForNextPosition()
    {
        yield return new WaitForSeconds(waitTime);
        if (currentPositionIndex + 1 < patrollingPoints.Length)
        {
            currentPositionIndex++;
        }
        else
        {
            currentPositionIndex = 0;
        }

        currentWaypoint = 0;
        path = null;
        // Check if seeker is done with the last path before starting a new one
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, patrollingPoints[currentPositionIndex].position, OnPathComplete);
        }
        isWaiting = false;
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
