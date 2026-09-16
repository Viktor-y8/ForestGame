using UnityEngine;

public class MenuCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private void Start()
    {
        Cursor.visible = true;
        GameCursor.Set(cursorTexture, hotspot);
    }
}