using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Scriptable Objects/TutorialStep")]
public class TutorialStep : ScriptableObject
{
    public string stepId;
    public string title;
    [TextArea(3, 8)] public string body;

    //optional
    public Sprite image;
}