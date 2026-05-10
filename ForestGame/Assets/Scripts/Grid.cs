using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid 
{
    private int width;
    private int height;
    private float cellSize = 1f;
    private Soil[,] gridObjects;

    public Grid(int width, int height, GameObject cellPrefab)
    {
        this.width = width;
        this.height = height;

        gridObjects = new Soil[width, height];

        for (int x = 0; x < gridObjects.GetLength(0); x++)
        {
            for (int y = 0; y < gridObjects.GetLength(1); y++)
            {
                Vector3 spawnPosition = GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f;

                GameObject obj = Object.Instantiate(cellPrefab, spawnPosition, Quaternion.identity);

                obj.transform.localScale = new Vector3(1f, 1f, 1f);

                Soil soil = obj.GetComponent<Soil>();
                soil.x = x;
                soil.y = y;
                soil.grid = this;
                gridObjects[x, y] = soil;

            }
        }
    }

    public Soil[] Adjacent(Soil soil)
    {
        List<Soil> neighbors = new List<Soil>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = soil.x + dx;
                int ny = soil.y + dy;

                if (nx >= 0 && ny >= 0 &&
                    nx < width && ny < height)
                {
                    neighbors.Add(gridObjects[nx, ny]);
                }
            }
        }

        return neighbors.ToArray();
    }

    public Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);

        return GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f;
    }
    public void SetValue(int x, int y, TreeData treeData)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridObjects[x, y].PlantTree(treeData);
        }
    }

    public void SetValue(Vector3 worldPos, TreeData treeData)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        SetValue(x, y, treeData);
    }

    public Soil GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridObjects[x, y];
        }
        else
        {
            return null;
        }
    }

    public Soil GetValue(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return GetValue(x, y);
    }

    public void GetXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.RoundToInt(worldPos.x / cellSize - 0.5f);
        y = Mathf.RoundToInt(worldPos.y / cellSize - 0.5f);
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

}
