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
    private int[,] gridArray;
    private Vector3 originPos;
    private Soil[,] gridObjects;

    public Grid(int width, int height, GameObject myPrefab, Vector3 originPos)
    {
        this.width = width;
        this.height = height;
        this.originPos = originPos;

        gridArray = new int[width, height];
        gridObjects = new Soil[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Vector3 spawnPosition = GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f;

                GameObject obj = Object.Instantiate(myPrefab, spawnPosition, Quaternion.identity);

                obj.transform.localScale = new Vector3(1f, 1f, 1f);

                gridObjects[x, y] = obj.GetComponent<Soil>();

            }
        }
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

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return 0;
        }
    }

    public int GetValue(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return GetValue(x, y);
    }

    private void GetXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - originPos).x / cellSize);
        y = Mathf.FloorToInt((worldPos - originPos).y / cellSize);
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPos;
    }

}
