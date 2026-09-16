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

    [Range(0f, 1f)]
    public float droughtResistance;

    [Range(0f, 1f)]
    public float shadeTolerance;

    [Range(0f, 1f)]
    public float biodiversityValue;

    [Header("Visuals")]
    public Sprite[] growthStages;
    public Sprite previewSprite;
}