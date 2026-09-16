using UnityEngine;

public class Weed : TileObject
{
    public override void Initialize(Soil soil)
    {
        base.Initialize(soil);
    }

    public override string GetDisplayName()
    {
        return "Weed";
    }
}