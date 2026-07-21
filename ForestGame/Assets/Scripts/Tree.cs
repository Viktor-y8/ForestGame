using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Tree : TileObject
{

    public TreeData data;
    private SpriteRenderer spriteRenderer;

    public float currGrowth;
    public bool justPlanted;

    public float health = 1f;
    public bool dead = false;

    public int ageMonths = 0;
    public float growthProgress = 0f;
    private TreeStage currentStage;
    private float requiredGrowthForMaturity;

    [Range(0f, 1f)]
    public float stress = 0f; // 0 = no stress, 1 = maximum stress
    private const float stressHealthThreshold = 0.4f;

    public int AgeYears => ageMonths / 12;
    public bool isMature => currentStage == TreeStage.Mature;

    public bool canPlant = true;
    public bool isImmune = false;

    private float accumulatedFireRisk = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Soil soil, TreeData data)
    {
        base.Initialize(soil);

        this.data = data;

        spriteRenderer.sprite = data.growthStages[0];

        soil.OnSoilChanged += HandleSoilChanged;

        justPlanted = true;
        currentStage = TreeStage.Seed;

        requiredGrowthForMaturity = CalculateMinGrowth();

        TimeManager.Instance.OnDayPassed += DailyUpdate;
        TimeManager.Instance.OnMonthPassed += MonthlyUpdate;
    }
    private void DailyUpdate()
    {
        ConsumeMoisture();
        CalculateStress();
        UpdateHealth();

        if (TimeManager.IsFastForwarding)
            AccumulateFireRisk();
    }

    private void AccumulateFireRisk()
    {
        // Same factors that would make a real fire likely to ignite/spread here
        float dryness = 1f - soil.moisture;
        float fireResistanceFactor = data.fireResistant ? 0.2f : 1f;
        float stressFactor = 1f + stress * 0.5f;

        float dailyRisk = 0.00015f * dryness * fireResistanceFactor * stressFactor;

        // Heatwave/drought seasons increase it — check current weather
        if (WeatherManager.Instance.currentWeather == WeatherType.Drought) dailyRisk *= 2f;
        if (WeatherManager.Instance.currentWeather == WeatherType.Heatwave) dailyRisk *= 3f;

        accumulatedFireRisk += dailyRisk;
    }

    public void ResolveFireRisk()
    {
        if (Random.value < accumulatedFireRisk)
        {
            InteractionManager.Instance.firesStarted++;
            Die();
        }

        accumulatedFireRisk = 0f;
    }

    private void ConsumeMoisture()
    {
        soil.moisture -= data.moistureUsage * 0.001f;

        soil.moisture = Mathf.Clamp01(soil.moisture);
    }

    private void MonthlyUpdate()
    {
        ageMonths++;

        Grow();

        UpdateGrowthStage();

        CheckNaturalDeath();

        if (currentStage == TreeStage.Mature && canPlant)
            PlantToAdjacent();
    }

    private float CalculateMinGrowth()
    {
        float avgSeasonalMultiplier = (1.4f + 1.1f + 0.7f + 0.15f) / 4f;

        float perfectGrowthRate = 1f
            * avgSeasonalMultiplier   // average season
            * 1.15f                   // preferred soil
            * 1f;                     // full health, no stress

        int monthsToMaturity = data.minMaturityAgeYears * 12;

        return perfectGrowthRate * monthsToMaturity;
    }

    private void HandleSoilChanged(SoilType newType)
    {
        
    }

    private void Grow()
    {
        if (currentStage == TreeStage.Mature) return;
        if (dead) return;

        float growthRate = 1f;

        switch (TimeManager.Instance.CurrentSeason)
        {
            case Season.Spring: growthRate *= 1.4f; break;
            case Season.Summer: growthRate *= 1.1f; break;
            case Season.Autumn: growthRate *= 0.7f; break;
            case Season.Winter: growthRate *= 0.15f; break;
        }

        if (AgeYears > data.oldAgeStartYears)
        {
            growthRate *= 0.7f;
        }

        growthRate *= (1f - stress);

        growthProgress += growthRate;
    }

    private void UpdateGrowthStage()
    {

        if (currentStage == TreeStage.Mature) return;

        TreeStage previousStage = currentStage;

        int age = AgeYears;

        bool mature =
            (
            growthProgress >= requiredGrowthForMaturity
            &&
            AgeYears >= data.minMaturityAgeYears
            );

        if (mature)
        {
            currentStage = TreeStage.Mature;
        }
        else if (age >= data.saplingAge)
        {
            currentStage = TreeStage.Young;
        }
        else if (age >= data.seedlingAge)
        {
            currentStage = TreeStage.Sapling;
        }
        else
        {
            currentStage = TreeStage.Seed;
        }

        spriteRenderer.sprite = data.growthStages[(int)currentStage];
        GetComponent<TreeOverlay>()?.RefreshSkullPosition();

        bool gainedShade =
            (previousStage == TreeStage.Sapling && currentStage == TreeStage.Young)
            ||
            (previousStage == TreeStage.Young && currentStage == TreeStage.Mature);

        if (gainedShade)
        {
            soil.grid.RefreshNeighbors(soil);
        }

    }

    public void ForceSetMature()
    {
        ageMonths = data.minMaturityAgeYears * 12;
        growthProgress = requiredGrowthForMaturity;
        currentStage = TreeStage.Mature;
        spriteRenderer.sprite = data.growthStages[(int)TreeStage.Mature];
        health = 1f;
        stress = 0f;
    }

    private void CheckNaturalDeath()
    {

        if (isImmune) return;

        int age = AgeYears;

        if (age >= data.maxAgeYears)
        {
            Die();
            return;
        }

        if (age >= data.oldAgeStartYears)
        {
            float ageRange =
                data.maxAgeYears - data.oldAgeStartYears;

            float currentAge =
                age - data.oldAgeStartYears;

            float deathChance =
                currentAge / ageRange;

            if (Random.value < deathChance * 0.01f)
            {
                Die();
            }
        }
    }

    public void PlantToAdjacent() {

        if (Random.value >= data.spreadChance) return;

        Soil[] neighbors = soil.grid.Adjacent(soil);

        List<Soil> validSoils = new List<Soil>();

        foreach (Soil s in neighbors)
        {
            if (!s.HasObject)
            {
                validSoils.Add(s);
            }
        }

        if (validSoils.Count == 0) return;

        Soil chosen = validSoils[Random.Range(0, validSoils.Count)];

        chosen.PlantTree(data);

        InteractionManager.Instance.treesPlanted++;
    }

    private void CalculateStress()
    {
        float targetStress = 0f;

        if (soil.moisture < data.minimumMoisture)
        {
            float moistureStress = 1f - (soil.moisture / data.minimumMoisture);
            targetStress += Mathf.Clamp01(moistureStress * (1f - data.droughtResistance));
        }

        if (soil.shade > data.shadeTolerance)
        {
            targetStress += Mathf.Clamp01(soil.shade - data.shadeTolerance);
        }

        if (soil.type != data.preferredSoil)
        {
            targetStress += 0.15f;
        }

        targetStress = Mathf.Clamp01(targetStress);

        float driftRate = targetStress > stress ? 0.06f : 0.02f;
        stress = Mathf.MoveTowards(stress, targetStress, driftRate);
    }

    private void UpdateHealth()
    {
        if (dead || isImmune) return;

        float healthChange = 0f;

        if (stress > stressHealthThreshold)
        {
            // Damage scales with how far above the threshold stress is
            float stressOverflow = stress - stressHealthThreshold;
            healthChange -= stressOverflow * 0.01f;
        }
        else
        {
            // Below threshold: recover, faster the closer to ideal (stress == 0)
            float recoveryRate = Mathf.Lerp(0.003f, 0.001f, stress / stressHealthThreshold);
            healthChange += recoveryRate;
        }

        health = Mathf.Clamp01(health + healthChange);

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {

        if (isImmune) return;

        dead = true;

        CancelInvoke();

        soil.RemoveObject();

        InteractionManager.Instance.treesDied++;

        Destroy(gameObject);

        //TODO: Spawn dead tree
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayPassed -= DailyUpdate;
            TimeManager.Instance.OnMonthPassed -= MonthlyUpdate;
        }

        if (soil != null)
        {
            soil.OnSoilChanged -= HandleSoilChanged;
        }
    }
}
