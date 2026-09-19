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

    public int AgeYears => ageMonths / 12;
    public bool isMature => currentStage == TreeStage.Mature;

    public bool canPlant = true;
    public bool isImmune = false;

    public bool hasDisease = false;
    public bool hasPest = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Soil soil, TreeData data)
    {
        base.Initialize(soil);

        this.data = data;

        spriteRenderer.sprite = data.growthStages[0];

        justPlanted = true;
        currentStage = TreeStage.Seed;
    }

    public void ResolveRound()
    {
        float stress = CalculateStress();
        UpdateHealth(stress);

        ageMonths += 120;
        UpdateGrowthStage();
        CheckNaturalDeath();

        if (!dead && isMature) PlantToAdjacent();

        justPlanted = false;
    }

    private float CalculateStress()
    {
        float stress = 0;

        if (!isMature)
        {
            stress += soil.fertilized switch
            {
                0 => 0.2f,
                1 => 0.05f,
                _ => -0.1f,
            };


            stress += soil.isWatered ? -0.1f : 0.3f;
        }
        else
        {
            stress += soil.fertilized switch
            {
                0 => 0.15f,
                1 => 0.05f,
                _ => -0.1f,
            };

            stress += soil.isWatered ? -0.1f : 0.2f;
        }

        return stress;
    }

    public void TakeDamage(float amount)
    {
        if (dead || isImmune) return;
        health = Mathf.Clamp01(health - amount);
        GetComponent<TreeOverlay>()?.Refresh();
        if (health <= healthEpsilon) Die();
    }

    public float PredictedHealthDeltaPercent()
    {
        float stress = CalculateStress();
        if (hasPest) stress += RoundManager.Instance.pestRoundEndDamage;

        float predictedHealth = Mathf.Clamp01(health - stress);
        return (predictedHealth - health) * 100f;
    }

    public bool WillDieNextRound()
    {
        float stress = CalculateStress();
        if (hasPest) stress += RoundManager.Instance.pestRoundEndDamage;

        float predictedHealth = Mathf.Clamp01(health - stress);
        if (predictedHealth <= healthEpsilon) return true;

        int predictedAgeYears = (ageMonths + 120) / 12;
        if (predictedAgeYears >= data.maxAgeYears) return true;

        return false;
    }

    private void UpdateGrowthStage()
    {

        if (currentStage == TreeStage.Mature) return;

        TreeStage previousStage = currentStage;

        int age = AgeYears;

        bool mature = false;

        if (age >= data.minMaturityAgeYears && age <= data.maxMaturityAgeYears)
        {
            mature = Random.value < 0.5;    
        }
        else if (age > data.maxMaturityAgeYears)
        {
            mature = true;
        }

        mature = mature && AgeYears >= data.minMaturityAgeYears;

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
        GetComponent<TreeOverlay>()?.RefreshPosition();
        GetComponent<TreeForecastLabel>()?.RefreshPosition();
        GetComponent<ToolHintIcon>()?.RefreshPosition();
    }

    public void ForceSetMature()
    {
        ageMonths = data.minMaturityAgeYears * 12;
        currentStage = TreeStage.Mature;
        spriteRenderer.sprite = data.growthStages[(int)TreeStage.Mature];
        health = 1f;
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

            if (Random.value < deathChance * 0.35f)
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

    private const float healthEpsilon = 0.001f;
    private void UpdateHealth(float stress)
    {
        if (dead || isImmune) return;

        health = Mathf.Clamp01(health - stress);

        GetComponent<TreeOverlay>()?.Refresh();

        if (health <= healthEpsilon)
        {
            Die();
        }
    }

    public void Die()
    {

        if (isImmune) return;

        dead = true;

        soil.RemoveObject();
    }
}
