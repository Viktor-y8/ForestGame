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
        public int requiredMatureCount;
    }

    [Header("Win Condition")]
    public TreeRequirement[] treeRequirements;
    
    //0 for no limit
    public int timeLimitYears;

    [Header("Context")]
    public string levelName;
    [TextArea] public string levelDescription;

    [Header("Audio")]
    public AudioClip backgroundMusic;

    public bool IsPlayerZone(int x, int y)
    {
        if (zoneMask == null) return true;

        float u = (float)x / width;
        float v = (float)y / height;

        int px = Mathf.Clamp(Mathf.RoundToInt(u * (zoneMask.width - 1)), 0, zoneMask.width - 1);
        int py = Mathf.Clamp(Mathf.RoundToInt(v * (zoneMask.height - 1)), 0, zoneMask.height - 1);

        Color pixel = zoneMask.GetPixel(px, py);

        return pixel.r > 0.5f;
    }
}