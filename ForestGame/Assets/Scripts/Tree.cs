using UnityEngine;

public class Tree : MonoBehaviour
{

    private Soil soil;
    TreeData data;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Soil soil, TreeData data)
    {
        this.soil = soil;
        this.data = data;

        spriteRenderer.sprite = data.growthStages[0];

        soil.OnSoilChanged += HandleSoilChanged;

        ApplySoilEffect(soil.type);
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
                GrowNormal();
                break;
            case SoilType.Fertile:
                GrowFaster();
                break;
        }
    }

    public void GrowNormal()
    {
        Debug.Log("Normal growth");
    }

    public void GrowFaster()
    {
        Debug.Log("Faster growth");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
