using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAIController : MonoBehaviour
{
    public float speed = 50f;

    public Transform[] patrollingPoints;

    public float waitTime;
    int currentPositionIndex;

    public bool directionBasedOnVelocity = true;
    bool isWaiting;

    public Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void Update()
    {
        if (transform.position != patrollingPoints[currentPositionIndex].position)
        {
            transform.position = Vector3.MoveTowards(transform.position, patrollingPoints[currentPositionIndex].position, speed * Time.deltaTime);
        }
        else
        {
            if(!isWaiting)
            {
                StartCoroutine(WaitForNextPosition());
            }
        }
        
        Flip();
    }

    void Flip()
    {
        if(directionBasedOnVelocity)
        {
            if (rb.velocity.x > 0.1f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (rb.velocity.x < -0.1f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
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
        isWaiting = false;
    }
}
