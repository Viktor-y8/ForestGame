using UnityEngine;

[CreateAssetMenu(fileName = "TreeData", menuName = "Scriptable Objects/TreeData")]
public class TreeData : ScriptableObject
{
    public string treeName;
    public float growthSpeed;
    public SoilType preferredSoil;

    public Sprite[] growthStages;


}
