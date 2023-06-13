using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatLineOfSight : MonoBehaviour
{
    public float rotationDetectionSpeed = 50;
    public float rotationDetectionDistance = 4;

    private LineRenderer lineRenderer;
    private bool isCatLookingAtMouse = false;
    private Transform mouse = null;

    void Start()
    {
        lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.1f;

        CheckLineRenderer();
    }

    void Update()
    {
        lineRenderer.SetPosition(0, transform.position);

        // Only rotate around the Z-axis when the cat is not looking at the mouse
        if (!isCatLookingAtMouse)
        {
            transform.Rotate(0, 0, rotationDetectionSpeed * Time.deltaTime);
        }

        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.up, rotationDetectionDistance);
        if (hitInfo.collider != null)
        {
            Debug.Log("Raycast hit: " + hitInfo.collider.name);
            Debug.DrawLine(transform.position, hitInfo.point, Color.red);
            lineRenderer.SetPosition(1, hitInfo.point);
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;

            if (hitInfo.collider.CompareTag("Mouse"))
            {
                isCatLookingAtMouse = true;
                mouse = hitInfo.transform;
            }
            else
            {
                isCatLookingAtMouse = false;
            }
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + transform.up * rotationDetectionDistance, Color.green);
            lineRenderer.SetPosition(1, transform.position + transform.up * rotationDetectionDistance);
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
            isCatLookingAtMouse = false;
            mouse = null;  // reset the mouse transform if it leaves line of sight
        }
    }


    public bool IsCatLookingAtMouse()
    {
        return isCatLookingAtMouse;
    }

    public Transform GetMouseTransform()
    {
        return mouse;
    }

    void CheckLineRenderer()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not set in the inspector");
        }
    }
}
