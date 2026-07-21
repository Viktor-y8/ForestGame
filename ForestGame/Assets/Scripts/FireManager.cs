using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public static FireManager Instance;

    [SerializeField] private Grid grid;

    // Base chance fire spreads to a neighbor per day
    private const float baseSpreadChance = 0.12f;

    private void Awake() => Instance = this;

    private void Start()
    {
        TimeManager.Instance.OnDayPassed += UpdateFires;
    }

    public void SetGrid(Grid grid)
    {

        this.grid = grid;
    }

    private void UpdateFires()
    {
        if (TimeManager.IsFastForwarding) return;

        List<Soil> allSoils = grid.GetAllSoils();
        List<Soil> burning = new List<Soil>();

        // Collect currently burning tiles first to avoid
        // spreading from tiles ignited this same tick
        foreach (Soil s in allSoils)
        {
            if (s.isOnFire) burning.Add(s);
        }

        foreach (Soil fire in burning)
        {
            fire.UpdateFire();
            TrySpread(fire);
        }
    }

    private void TrySpread(Soil source)
    {
        foreach (Soil neighbor in grid.Adjacent(source))
        {
            if (neighbor.isOnFire || (neighbor.CurrentObject is Tree treeTemp && treeTemp.isImmune)) continue;

            float spreadChance = baseSpreadChance;

            // Dry neighbors catch more easily
            spreadChance *= (1f - neighbor.moisture);

            // Dense canopy (high shade) spreads fire faster — crown fire
            spreadChance *= (1f + neighbor.shade);

            // Fire-resistant trees are harder to ignite
            if (neighbor.CurrentObject is Tree tree)
            {
                if (tree.data.fireResistant)
                    spreadChance *= 0.2f;

                // Stressed trees are more flammable
                spreadChance *= (1f + tree.stress * 0.5f);
            }

            if (WeatherManager.Instance.currentWeather == WeatherType.Rain) spreadChance *= 0.5f;
            if (WeatherManager.Instance.currentWeather == WeatherType.Heatwave) spreadChance *= 1.5f;

            if (neighbor.CurrentObject == null) spreadChance *= 0.2f;

            spreadChance = Mathf.Clamp(spreadChance, 0f, 0.6f);

            if (Random.value < spreadChance)
                neighbor.Ignite();
        }
    }

    // Call this from UI or a game event to start a fire
    public bool StartFire(Soil soil)
    {
        return soil.Ignite();
    }

    // Rain extinguishes fires
    public void OnRain()
    {
        foreach (Soil s in grid.GetAllSoils())
        {
            if (s.isOnFire && Random.value < 0.8f)
                s.Extinguish();
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayPassed -= UpdateFires;
    }
}