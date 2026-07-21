using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Grid")]
    public int width;
    public int height;

    [Header("Zone Mask")]
    [Tooltip("White pixels = player zone, black = border forest.")]
    public Texture2D zoneMask;

    [Header("Player Zone Soil")]
    public SoilType zoneSoilType;
    [Range(0f, 1f)] public float zoneMoisture = 0.5f;
    public float zoneMoistureRetention = 1f;

    [Header("Border Forest")]
    public TreeData[] borderTreePool;
    public SoilType borderSoilType;
    [Range(0f, 1f)] public float borderMoisture = 0.6f;

    public int seedCount;

    [System.Serializable]
    public class TreeRequirement
    {
        public TreeData treeType;
        public int requiredMatureCount; // can be 0 if not required
    }

    // Add to LevelData class:
    [Header("Win Condition")]
    public TreeRequirement[] treeRequirements;
    public int timeLimitYears; // 0 = no limit

    [Header("Context")]
    public string levelName;
    [TextArea] public string levelDescription;

    [Header("Audio")]
    public AudioClip backgroundMusic;

    // Samples the mask texture to determine if a tile is in the player zone
    public bool IsPlayerZone(int x, int y)
    {
        if (zoneMask == null) return true;

        // Map tile coords to texture pixel coords
        float u = (float)x / width;
        float v = (float)y / height;

        int px = Mathf.Clamp(Mathf.RoundToInt(u * (zoneMask.width - 1)), 0, zoneMask.width - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(v * (zoneMask.height - 1)), 0, zoneMask.height - 1);

        Color pixel = zoneMask.GetPixel(px, py);

        // White (or light) = player zone
        return pixel.r > 0.5f;
    }
}