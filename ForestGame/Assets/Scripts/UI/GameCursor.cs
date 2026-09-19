using UnityEngine;

public static class GameCursor
{
    public static void Set(Texture2D texture, Vector2 hotspot)
    {
        Cursor.visible = true;
        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
    }

    public static void Hide()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.visible = false;
    }
}