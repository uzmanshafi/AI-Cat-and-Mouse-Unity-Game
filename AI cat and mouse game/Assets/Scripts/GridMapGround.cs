using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CodeMonkey.Utils;
public class GridMapGround : MonoBehaviour
{
    public Tilemap tilemap; // Assign your tilemap in the inspector
    public GridMap gridMap;
    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int size = bounds.size;

        int width = size.x;
        int height = size.y;

        Vector3 cellSizeVector = tilemap.cellSize;
        float cellSize = Mathf.Max(cellSizeVector.x, cellSizeVector.y); // use the larger one in case they aren't equal

        Vector3 originPosition = transform.position - new Vector3(width * cellSize, height * cellSize) / 2f;
        
        GridMap(width, height, cellSize, originPosition);     
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gridMap.SetValue(UtilsClass.GetMouseWorldPosition(), 56);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(gridMap.GetValue(UtilsClass.GetMouseWorldPosition()));
        }
    }
}
