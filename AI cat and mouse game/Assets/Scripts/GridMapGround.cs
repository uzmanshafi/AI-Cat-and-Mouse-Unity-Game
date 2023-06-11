using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMapGround : MonoBehaviour
{
    public Tilemap tilemap; // Assign your tilemap in the inspector

    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int size = bounds.size;

        int width = size.x;
        int height = size.y;

        Vector3 cellSizeVector = tilemap.cellSize;
        float cellSize = Mathf.Max(cellSizeVector.x, cellSizeVector.y); // use the larger one in case they aren't equal

        Vector3 originPosition = transform.position - new Vector3(width * cellSize, height * cellSize) / 2f;
        
        GridMap gridMap = new GridMap(width, height, cellSize, originPosition);     
    }
}
