using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{

    private Soil soil;
    public TreeData data;
    private SpriteRenderer spriteRenderer;
    private float growthSpeed;
    private float growthFactor;
    public float currGrowth;
    public bool justPlanted;
    public bool mature = false;
    private bool spreadingStarted = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Soil soil, TreeData data)
    {
        this.soil = soil;
        this.data = data;
        growthSpeed = data.growthSpeed;

        spriteRenderer.sprite = data.growthStages[0];

        soil.OnSoilChanged += HandleSoilChanged;

        ApplySoilEffect(soil.type);

        justPlanted = true;
    }
    private void HandleSoilChanged(SoilType newType)
    {
        ApplySoilEffect(newType);
    }

    private void ApplySoilEffect(SoilType type)
    {
        switch (type)
        {
            case SoilType.Normal:
                growthFactor = 1;
                break;
            case SoilType.Fertile:
                growthFactor = 1.2f;
                break;
        }
    }

    // Update is called once per frame

    void Update()
    {
        currGrowth += (growthSpeed * growthFactor) * Time.deltaTime;

        if (currGrowth >= 1)
        {
            spriteRenderer.sprite = data.growthStages[3];
            mature = true;
        }
        else if (currGrowth >= 0.5f)
            spriteRenderer.sprite = data.growthStages[2];
        else if (currGrowth >= 0.25f)
        {
            spriteRenderer.sprite = data.growthStages[1];
            justPlanted = false;
        }

        if (mature && !spreadingStarted)
        {
            spreadingStarted = true;
            InvokeRepeating(nameof(PlantToAdjacent), 2f, 10f);
        }
    }

    public void PlantToAdjacent() {

        if (Random.value >= 0.5) return;

        Soil[] neighbors = soil.grid.Adjacent(soil);

        List<Soil> validSoils = new List<Soil>();

        foreach (Soil s in neighbors)
        {
            if (!s.HasTree)
            {
                validSoils.Add(s);
            }
        }

        if (validSoils.Count == 0) return;

        Soil chosen = validSoils[Random.Range(0, validSoils.Count)];

        chosen.PlantTree(data);
    }
}
