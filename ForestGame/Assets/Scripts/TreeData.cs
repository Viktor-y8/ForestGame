using UnityEngine;

[CreateAssetMenu(fileName = "TreeData", menuName = "Scriptable Objects/TreeData")]
public class TreeData : ScriptableObject
{
    [Header("Basic")]
    public string treeName;

    [Header("Spread")]
    public float spreadChance;

    [Header("Old age")]
    public int oldAgeStartYears;
    public int maxAgeYears;

    [Header("Maturity")]
    public int minMaturityAgeYears;
    public int maxMaturityAgeYears;

    [Header("Growth Stages")]
    public int seedlingAge;
    public int saplingAge;
    public int youngTreeAge;

    [Header("Ecology")]
    public SoilType preferredSoil;

    [Range(0f, 1f)]
    public float droughtResistance;

    [Range(0f, 1f)]
    public float shadeTolerance;

    [Range(0f, 1f)]
    public float biodiversityValue;

    [Range(0f, 1f)]
    public float moistureUsage;

    [Range(0f, 1f)]
    public float minimumMoisture;

    [Header("Gameplay")]
    public bool improvesSoil;
    public bool fireResistant;
    public bool diseaseResistant;

    [Header("Visuals")]
    public Sprite[] growthStages;
    public Sprite previewSprite;
}