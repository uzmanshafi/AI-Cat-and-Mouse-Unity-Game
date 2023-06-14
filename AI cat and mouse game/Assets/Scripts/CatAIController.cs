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
    public CatLineOfSight lineOfSight;
    public float pathRecalculationCooldown = 0.5f;

    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Vector2 originalPosition;
    private List<Transform> unvisitedPoints;
    private bool isCalculatingPath = false;
    private float pathRecalculationTime = 0f;
    private Transform mouse = null;

    public enum CatState
    {
        Patrolling,
        ChasingMouse
    }

    public CatState CurrentCatState { get; private set; }


    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        unvisitedPoints = new List<Transform>(patrollingPoints);
        StartPatrolToClosestPoint();
        lineOfSight = transform.Find("LineOfSightCheck").GetComponent<CatLineOfSight>();
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
        if (lineOfSight != null && Time.time > pathRecalculationTime)
        {
            if (lineOfSight.IsCatLookingAtMouse() && mouse == null)
            {
                mouse = lineOfSight.GetMouseTransform();
                if (!isCalculatingPath)
                {
                    ChaseMouse();
                    CurrentCatState = CatState.ChasingMouse;
                }
            }
            else if (!lineOfSight.IsCatLookingAtMouse() && mouse != null)
            {
                mouse = null;
                if (!isCalculatingPath)
                {
                    StartPatrolToClosestPoint();
                    CurrentCatState = CatState.Patrolling;
                }
            }
        }
    }


    void ChaseMouse()
    {
        if (mouse != null)
        {
            path = null;
            currentWaypoint = 0;
            isCalculatingPath = true;
            seeker.StartPath(rb.position, mouse.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
            isCalculatingPath = false;
            pathRecalculationTime = Time.time + pathRecalculationCooldown;
        }
        else
        {
            Debug.LogError("Path error: " + p.error);
        }
    }

    void FixedUpdate()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            path = null;
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = path.vectorPath[currentWaypoint];
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);

        rb.MovePosition(newPosition);

        if (Vector2.Distance(newPosition, targetPosition) < 0.1f)
        {
            currentWaypoint++;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            path = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mouse"))
        {
            Destroy(other.gameObject);
            mouse = null;
            StartPatrolToClosestPoint();
        }
    }
}
