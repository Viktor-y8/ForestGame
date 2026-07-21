#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelData level = (LevelData)target;

        if (level.zoneMask == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Zone Preview", EditorStyles.boldLabel);

        Rect rect = GUILayoutUtility.GetRect(level.width * 10, level.height * 10);

        for (int x = 0; x < level.width; x++)
        {
            for (int y = 0; y < level.height; y++)
            {
                bool isZone = level.IsPlayerZone(x, y);
                EditorGUI.DrawRect(new Rect(
                    rect.x + x * 10,
                    rect.y + (level.height - 1 - y) * 10,
                    9, 9
                ), isZone ? Color.green : new Color(0.2f, 0.4f, 0.2f));
            }
        }
    }
}
#endif