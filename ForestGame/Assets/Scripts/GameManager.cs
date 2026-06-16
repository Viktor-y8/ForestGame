using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Grid grid;
    public GameObject cellPrefab;
    public int gridWidth = 4, gridHeight = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Grid(gridWidth, gridHeight, cellPrefab);
        Camera.main.transform.position = new Vector3(gridWidth  / 2f, gridHeight / 2f, -10);

        InteractionManager.Instance.SetGrid(grid);
        FireManager.Instance.SetGrid(grid);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
