using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CatAIController : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] patrollingPoints;
    public float waitTime = 2f;
    int currentPositionIndex;
    public bool directionBasedOnVelocity = true;
    bool isWaiting;
    public Rigidbody2D rb;
    public bool isPatrolling = true;

    Path path;
    int currentWaypoint = 0;
    Seeker seeker;
    Vector2 originalPosition;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        StartCoroutine(InitialWaitCoroutine());
    }

    IEnumerator InitialWaitCoroutine()
    {
        yield return new WaitForSeconds(4f);
        seeker.StartPath(rb.position, patrollingPoints[currentPositionIndex].position, OnPathComplete);
    }

    void FixedUpdate()
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

            Vector2 currentPosition = rb.position;
            Vector2 targetPosition = path.vectorPath[currentWaypoint];
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            // Check if the cat has reached its destination
            if (newPosition == targetPosition)
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
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        if (currentPositionIndex + 1 < patrollingPoints.Length)
        {
            currentPositionIndex++;
        }
        else
        {
            currentPositionIndex = 0;
            seeker.StartPath(rb.position, originalPosition, OnPathComplete);
            yield break;
        }

        currentWaypoint = 0;
        path = null;
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
