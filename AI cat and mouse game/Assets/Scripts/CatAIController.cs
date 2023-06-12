using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CatAIController : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] patrollingPoints;
    public float waitTime = 2f;
    bool isWaiting;
    public Rigidbody2D rb;
    public bool isPatrolling = true;

    Path path;
    int currentWaypoint = 0;
    Seeker seeker;
    Vector2 originalPosition;
    List<Transform> unvisitedPoints;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        unvisitedPoints = new List<Transform>(patrollingPoints);  // Initialize list of unvisited points
        StartCoroutine(InitialWaitCoroutine());
    }

    IEnumerator InitialWaitCoroutine()
    {
        yield return new WaitForSeconds(4f);
        StartPatrolToClosestPoint();
    }

    void StartPatrolToClosestPoint()
    {
        if (unvisitedPoints.Count == 0)
        {
            seeker.StartPath(rb.position, originalPosition, OnPathComplete);
            return;
        }
        
        Transform closestPoint = GetClosestPoint();
        seeker.StartPath(rb.position, closestPoint.position, OnPathComplete);
    }

    // Returns the transform of the closest unvisited point and removes it from the list
    Transform GetClosestPoint()
    {
        Transform closestPoint = unvisitedPoints[0];
        float closestDistance = Vector2.Distance(rb.position, closestPoint.position);

        foreach (Transform point in unvisitedPoints)
        {
            float distance = Vector2.Distance(rb.position, point.position);
            if (distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
            }
        }

        unvisitedPoints.Remove(closestPoint);
        return closestPoint;
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

            if (newPosition == targetPosition)
            {
                currentWaypoint++;
            }

            Flip();
        }
    }

    void Flip()
    {
        if (path != null && currentWaypoint < path.vectorPath.Count)
        {
            if (rb.position.x < path.vectorPath[currentWaypoint].x)
            {
                transform.localScale = new Vector3(-.8f, .8f, .8f);
            }
            else if (rb.position.x > path.vectorPath[currentWaypoint].x)
            {
                transform.localScale = new Vector3(.8f, .8f, .8f);
            }
        }
    }

    IEnumerator WaitForNextPosition()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        currentWaypoint = 0;
        path = null;
        if (seeker.IsDone())
        {
            StartPatrolToClosestPoint();
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
