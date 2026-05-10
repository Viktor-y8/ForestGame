using UnityEngine;

[CreateAssetMenu(fileName = "TreeData", menuName = "Scriptable Objects/TreeData")]
public class TreeData : ScriptableObject
{
    public string treeName;
    public float growthSpeed;
    public float growthTime;
    public SoilType preferredSoil;

    public Sprite[] growthStages;
    public Sprite previewSprite;


}
