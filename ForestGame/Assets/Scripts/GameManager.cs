using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Grid grid;
    public GameObject cellPrefab;

    [SerializeField] private LevelData startingLevel;

    // Store zone bounds so camera can use them
    public Bounds playerZoneBounds { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (startingLevel != null)
            LoadLevel(startingLevel);
    }

    public void LoadLevel(LevelData level)
    {
        grid = new Grid(level.width, level.height, cellPrefab);

        InteractionManager.Instance.SetGrid(grid);
        FireManager.Instance.SetGrid(grid);

        SetupTiles(level);

        Camera.main.transform.position = new Vector3(
            playerZoneBounds.center.x,
            playerZoneBounds.center.y,
            -10
        );

        Bounds fullGridBounds = new Bounds(
            new Vector3(level.width / 2f, level.height / 2f),
            new Vector3(level.width, level.height)
        );

        FindObjectOfType<CameraController>()?.SetBounds(fullGridBounds);

        SpawnBorderPanels(level);

        LevelManager.Instance.SetLevel(level);
    }

    public List<Soil> GetAllSoils()
    {
        return grid.GetAllSoils();
    }

    private void SetupTiles(LevelData level)
    {
        // First find all player zone tiles to compute zone bounds
        Vector2 zoneMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 zoneMax = new Vector2(float.MinValue, float.MinValue);

        for (int x = 0; x < level.width; x++)
        {
            for (int y = 0; y < level.height; y++)
            {
                if (level.IsPlayerZone(x, y))
                {
                    zoneMin = Vector2.Min(zoneMin, new Vector2(x, y));
                    zoneMax = Vector2.Max(zoneMax, new Vector2(x, y));
                }
            }
        }

        playerZoneBounds = new Bounds(
            new Vector3((zoneMin.x + zoneMax.x) / 2f + 0.5f, (zoneMin.y + zoneMax.y) / 2f + 0.5f),
            new Vector3(zoneMax.x - zoneMin.x + 1f, zoneMax.y - zoneMin.y + 1f)
        );

        // Max possible distance for normalization — corner of grid to zone edge
        float maxDist = Vector2.Distance(Vector2.zero, new Vector2(level.width, level.height));

        for (int x = 0; x < level.width; x++)
        {
            for (int y = 0; y < level.height; y++)
            {
                Soil soil = grid.GetValue(x, y);
                bool inPlayerZone = level.IsPlayerZone(x, y);

                if (inPlayerZone)
                {
                    SetupPlayerTile(soil, level);
                    soil.GetComponent<SoilOverlay>()?.SetDarkness(0f);
                }
                else
                {
 
                    // Distance from this tile to the nearest player zone edge
                    float dist = DistanceToZone(x, y, (int)zoneMin.x, (int)zoneMin.y,
                                                (int)zoneMax.x, (int)zoneMax.y);

                    float darkness = Mathf.Clamp01(dist / (maxDist * 0.15f)); // much tighter falloff
                    darkness = Mathf.Pow(darkness, 0.4f); // stronger curve
                    darkness = Mathf.Max(darkness, 0.5f); // minimum darkness even at zone edge
                    SetupBorderTile(soil, level, darkness);
                    soil.GetComponent<SoilOverlay>()?.SetDarkness(darkness);
                }

                soil.isLocked = !inPlayerZone;
            }
        }
    }

    private void SetupPlayerTile(Soil soil, LevelData level)
    {
        soil.type = level.zoneSoilType;
        soil.moisture = level.zoneMoisture;
        soil.moistureRetention = level.zoneMoistureRetention;
    }

    private void SetupBorderTile(Soil soil, LevelData level, float darkness)
    {
        soil.type = level.borderSoilType;
        soil.moisture = level.borderMoisture;

        if (level.borderTreePool != null && level.borderTreePool.Length > 0)
        {
            TreeData randomTree = level.borderTreePool[
                Random.Range(0, level.borderTreePool.Length)
            ];

            soil.PlantTree(randomTree);

            if (soil.CurrentObject is Tree tree)
            {
                tree.ForceSetMature();
                tree.canPlant = false;
                tree.isImmune = true;
            }
        }

        // Apply after tree is planted so tree sprite gets darkened too
        soil.GetComponent<SoilOverlay>()?.SetDarkness(darkness);
    }

    private float DistanceToZone(int tx, int ty, int zx1, int zy1, int zx2, int zy2)
    {
        // Nearest point on the zone rectangle to this tile
        float cx = Mathf.Clamp(tx, zx1, zx2);
        float cy = Mathf.Clamp(ty, zy1, zy2);
        return Vector2.Distance(new Vector2(tx, ty), new Vector2(cx, cy));
    }

    private void SpawnBorderPanels(LevelData level)
    {
        float w = level.width;
        float h = level.height;
        float thickness = 50f; // how far the black extends beyond grid

        SpawnBlackPanel(new Vector3(w / 2f, h + thickness / 2f, 0f),
                        new Vector3(w + thickness * 2f, thickness, 1f));         // top

        SpawnBlackPanel(new Vector3(w / 2f, -thickness / 2f, 0f),
                        new Vector3(w + thickness * 2f, thickness, 1f));         // bottom

        SpawnBlackPanel(new Vector3(-thickness / 2f, h / 2f, 0f),
                        new Vector3(thickness, h + thickness * 2f, 1f));         // left

        SpawnBlackPanel(new Vector3(w + thickness / 2f, h / 2f, 0f),
                        new Vector3(thickness, h + thickness * 2f, 1f));         // right
    }

    private void SpawnBlackPanel(Vector3 position, Vector3 scale)
    {
        GameObject panel = new GameObject("BorderPanel");
        panel.transform.position = position;
        panel.transform.localScale = scale;

        SpriteRenderer sr = panel.AddComponent<SpriteRenderer>();
        sr.sprite = GetWhiteSprite();
        sr.color = Color.black;
        sr.sortingOrder = 99;
    }

    private Sprite GetWhiteSprite()
    {
        // Creates a 1x1 white texture to use as a solid black panel
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}