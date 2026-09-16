using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    [Header("Fire")]
    [SerializeField] private float fireBaseChance = 0.05f;
    [SerializeField] private float fireHeatwaveMultiplier = 4f;
    [SerializeField] private float fireDroughtMultiplier = 2.5f;
    [SerializeField] private float fireUnwateredRoundWeight = 0.08f;
    [SerializeField] private float grassFireMultiplier = 0.15f;

    [Header("Pest")]
    [SerializeField] private float pestChanceSummer = 0.12f;
    [SerializeField] private float pestChanceSpring = 0.08f;
    [SerializeField] private float pestChanceAutumn = 0.04f;
    [SerializeField] private float pestChanceWinter = 0.01f;
    [SerializeField] private float pestMonocultureWeight = 0.5f;



    private void Awake() => Instance = this;

    public List<Soil> RollFireEvents(List<Soil> allSoils, Season season, WeatherType weather)
    {
        var ignited = new List<Soil>();
        if (weather == WeatherType.Rain) return ignited;

        float weatherMultiplier = weather switch
        {
            WeatherType.Heatwave => fireHeatwaveMultiplier,
            WeatherType.Drought => fireDroughtMultiplier,
            _ => 1f,
        };

        foreach (Soil s in allSoils)
        {
            if (s.isLocked || s.isOnFire || s.CurrentObject is Ditch) continue;
            if (s.CurrentObject is Tree existingTree && (existingTree.hasPest || existingTree.hasDisease)) continue;

            float chance = fireBaseChance * weatherMultiplier;
            //chance *= 0.5f + s.DrynessFactor();
            //chance += s.roundsUnwatered * fireUnwateredRoundWeight;

            if (s.CurrentObject is not Tree) chance *= grassFireMultiplier;

            if (Random.value < chance && s.Ignite())
                ignited.Add(s);
        }

        return ignited;
    }

    public List<Soil> RollPestEvents(List<Soil> allSoils, Season season)
    {
        var infested = new List<Soil>();

        float baseChance = season switch
        {
            Season.Summer => pestChanceSummer,
            Season.Spring => pestChanceSpring,
            Season.Autumn => pestChanceAutumn,
            _ => pestChanceWinter,
        };

        foreach (Soil s in allSoils)
        {
            if (s.isLocked || s.CurrentObject is not Tree tree) continue;
            if (tree.hasPest || tree.hasDisease || tree.dead || s.isOnFire) continue;

            float chance = baseChance;
            chance *= 1.5f - tree.health;
            chance *= 1f - tree.data.biodiversityValue * 0.4f;

            int sameSpecies = 0, total = 0;
            foreach (Soil n in s.grid.Adjacent(s))
            {
                if (n?.CurrentObject is Tree nt) { total++; if (nt.data == tree.data) sameSpecies++; }
            }
            if (total > 0) chance *= 1f + (float)sameSpecies / total * pestMonocultureWeight;

            if (Random.value < chance)
            {
                tree.hasPest = true;
                tree.GetComponent<TreeOverlay>()?.Refresh();
                infested.Add(s);
            }
        }

        return infested;
    }
}