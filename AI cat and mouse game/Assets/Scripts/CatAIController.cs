using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CatAIController : MonoBehaviour
{
    public float speed = 2f;
    public Transform[] patrollingPoints;
    public float waitTime = 2f;
    public Rigidbody2D rb;
    public bool isPatrolling = true;
    public CatLineOfSight lineOfSight;

    Path path;
    int currentWaypoint = 0;
    Seeker seeker;
    Vector2 originalPosition;
    List<Transform> unvisitedPoints;
    bool isCalculatingPath = false;

    bool waiting = false;

    private Transform mouse = null;
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        unvisitedPoints = new List<Transform>(patrollingPoints);
        StartCoroutine(InitialWaitCoroutine());
        lineOfSight = GetComponent<CatLineOfSight>();
    }

    IEnumerator InitialWaitCoroutine()
    {
        yield return new WaitForSeconds(4f);
        StartPatrolToClosestPoint();
    }

    void StartPatrolToClosestPoint()
    {
        // This ensures that there is always a destination point
        if (unvisitedPoints.Count == 0)
        {
            unvisitedPoints = new List<Transform>(patrollingPoints);
        }

        Transform closestPoint = GetClosestPoint();
        isCalculatingPath = true;

        // Clear the path here before calculating a new one
        path = null;
        currentWaypoint = 0;

        seeker.StartPath(rb.position, closestPoint.position, OnPathComplete);
    }


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

    void Update()
    {
        if (lineOfSight.IsCatLookingAtMouse())
        {
            // If the mouse is in sight, initiate or continue chasing
            mouse = lineOfSight.GetMouseTransform();
            ChaseMouse();
        }
        else
        {
            // If the mouse is no longer in sight, go back to patrolling
            mouse = null;
            StartCoroutine(ReturnToOriginalPosition());
        }
    }


    void ChaseMouse()
{
    if (mouse != null)
    {
        // If the mouse is not null and a path is not currently being calculated, start a new path to the mouse
        if (!isCalculatingPath)
        {
            isPatrolling = false;
            path = null;
            seeker.StartPath(rb.position, mouse.position, OnPathComplete);
        }
    }
}

    IEnumerator ReturnToOriginalPosition()
    {
        yield return new WaitForSeconds(waitTime);
        isPatrolling = true;
        seeker.StartPath(rb.position, originalPosition, OnPathComplete);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mouse"))
        {
            Destroy(other.gameObject);
            mouse = null;
            StartCoroutine(ReturnToOriginalPosition());
        }
    }

    void FixedUpdate()
    {
        Debug.Log("path: " + path);
        Debug.Log("rb: " + rb);

        if (isPatrolling)
        {
            if (path == null)
            {
                // If we're not already calculating a path, then wait for next position
                if (!isCalculatingPath && !waiting)
                {
                    StartCoroutine(WaitForNextPosition());
                }
                return;
            }

            if (currentWaypoint >= path.vectorPath.Count)
            {
                // If the current waypoint is at the end of the path, start the coroutine to calculate a new path
                if (!isCalculatingPath && !waiting)
                {
                    StartCoroutine(WaitForNextPosition());
                }
                return;
            }

            Vector2 currentPosition = rb.position;
            Vector2 targetPosition = path.vectorPath[currentWaypoint];
            Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            // Calculate distance after moving
            float distanceAfterMoving = Vector2.Distance(newPosition, targetPosition);

            // Flip only if moved a little towards the new waypoint
            if (Vector2.Distance(currentPosition, targetPosition) - distanceAfterMoving > 0.01f)
            {
                Flip();
            }

            if (distanceAfterMoving < 0.1f)
            {
                currentWaypoint++;
            }
        }
    }




    void MoveAlongPath()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            if (!isCalculatingPath)
            {
                StartCoroutine(WaitForNextPosition());
            }
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = path.vectorPath[currentWaypoint];
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);

        if (Vector2.Distance(newPosition, targetPosition) < 0.1f)
        {
            currentWaypoint++;
        }


        Flip();
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
        if (!waiting)
        {
            waiting = true;
            yield return new WaitForSeconds(waitTime);
            if (!isCalculatingPath)
            {
                StartPatrolToClosestPoint();
            }
            waiting = false;
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
        isCalculatingPath = false;
    }

}
