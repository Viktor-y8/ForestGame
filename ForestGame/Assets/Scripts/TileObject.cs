using UnityEngine;

public abstract class TileObject : MonoBehaviour
{
    protected Soil soil;

    public virtual void Initialize(Soil soil)
    {
        this.soil = soil;
    }

    public virtual string GetDisplayName()
    {
        return "Object";
    }
}