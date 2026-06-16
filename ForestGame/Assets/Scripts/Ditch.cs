using UnityEngine;

public class Ditch : TileObject
{
    public override void Initialize(Soil soil)
    {
        base.Initialize(soil);
    }

    public override string GetDisplayName()
    {
        return "Fire Ditch";
    }
}