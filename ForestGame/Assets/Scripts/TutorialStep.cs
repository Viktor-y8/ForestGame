using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Scriptable Objects/TutorialStep")]
public class TutorialStep : ScriptableObject
{
    public string stepId; //unique key
    public string title;
    [TextArea(3, 8)] public string body;
    public Sprite image; //optional
}