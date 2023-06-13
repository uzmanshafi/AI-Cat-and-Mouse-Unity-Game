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

    private Transform mouse = null;
    private bool isRelaxing = false;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        unvisitedPoints = new List<Transform>(patrollingPoints);
        StartCoroutine(InitialWaitCoroutine());
        lineOfSight = transform.Find("LineOfSightCheck").GetComponent<CatLineOfSight>();
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
            unvisitedPoints = new List<Transform>(patrollingPoints);
        }

        Transform closestPoint = GetClosestPoint();
        isCalculatingPath = true;

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
        if (lineOfSight != null)
        {
            if (lineOfSight.IsCatLookingAtMouse())
            {
                mouse = lineOfSight.GetMouseTransform();
                isPatrolling = false;
                if (!isCalculatingPath)
                {
                    // Stop all coroutines to immediately chase the mouse
                    StopAllCoroutines();
                    isRelaxing = false;
                    ChaseMouse();
                }
            }
            else if (mouse != null)
            {
                mouse = null;
                isPatrolling = true;
                if (!isCalculatingPath && !isRelaxing)
                {
                    StartCoroutine(RelaxAtCurrentPosition());
                }
            }
        }
    }


    void ChaseMouse()
    {
        if (mouse != null)
        {
            if (!isCalculatingPath)
            {
                Debug.Log("Starting to chase mouse...");
                path = null;
                seeker.StartPath(rb.position, mouse.position, OnPathComplete);
            }
        }
    }

    IEnumerator RelaxAtCurrentPosition()
    {
        isRelaxing = true;
        yield return new WaitForSeconds(1f);  // Relax for 1 second
        isRelaxing = false;
        if (!isCalculatingPath)
        {
            StartPatrolToClosestPoint();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mouse"))
        {
            Destroy(other.gameObject);
            mouse = null;
            StartCoroutine(RelaxAtCurrentPosition());
        }
    }

    void FixedUpdate()
    {
        if (mouse != null)
        {
            // If the mouse is still visible, continue to chase it
            ChaseMouse();
        }

        MoveAlongPath();

        if (path == null || isRelaxing)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            path = null; // Reset the path
            return;
        }

        // Move along the path
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = path.vectorPath[currentWaypoint];
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);

        // Calculate distance after moving
        float distanceAfterMoving = Vector2.Distance(newPosition, targetPosition);

        if (distanceAfterMoving < 0.1f && currentWaypoint < path.vectorPath.Count - 1)
        {
            currentWaypoint++;
        }

        // Flip only if moved a little towards the new waypoint
        if (Vector2.Distance(currentPosition, targetPosition) - distanceAfterMoving > 0.01f)
        {
            Flip();
        }
    }

    void MoveAlongPath()
    {
        if (path == null || isRelaxing || currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        // Move along the path
        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = path.vectorPath[currentWaypoint];
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPosition);

        // Calculate distance after moving
        float distanceAfterMoving = Vector2.Distance(newPosition, targetPosition);

        if (distanceAfterMoving < 0.1f && currentWaypoint < path.vectorPath.Count - 1)
        {
            currentWaypoint++;
        }

        // Flip only if moved a little towards the new waypoint
        if (Vector2.Distance(currentPosition, targetPosition) - distanceAfterMoving > 0.01f)
        {
            Flip();
        }
    }

    void Flip()
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

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            if (mouse != null)
            {
                // Start chasing the mouse immediately
                ChaseMouse();
            }
        }
        isCalculatingPath = false;
        // Immediately move the cat along the new path
        MoveAlongPath();
    }


}
