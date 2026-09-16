using UnityEngine;

public enum ToolType
{
    None,
    Plant,
    Remove,
    Fertilize,
    Water,
    Pesticide
}

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private TreeData selectedTree;
    private ToolType currTool;

    [SerializeField] private GameObject cursorPreview;
    private SpriteRenderer previewRenderer;


    [Header("Tool Sprites")]
    [SerializeField] private Sprite waterSprite;
    //[SerializeField] private Sprite ditchSprite;
    [SerializeField] private Sprite removeSprite;
    [SerializeField] private Sprite fertilizeSprite;
    [SerializeField] private Sprite pesticideSprite;

    [SerializeField] private TutorialStep firstPlantTutorial;

    private Grid grid;

    public int seedCount;

    public int waterBudget = 20;

    public int fertilizeBudget = 5;

    public static event System.Action OnBudgetChanged;
    public static event System.Action OnSeedChanged;

    public int treesPlanted = 0;
    public int treesDied = 0;
    public int waterToolsUsed = 0;
    public int ditchToolsUsed = 0;
    public int firesStarted = 0;

    [SerializeField] private float fireMinigameLossDamage = 0.25f;
    [SerializeField] private float fireSpreadBaseChance = 0.35f;
    [SerializeField] private float pestMinigameLossDamage = 0.08f;
    [SerializeField] private float pestSpreadBaseChance = 0.25f;

    [Header("Minigame Stats")]
    [SerializeField] private int weedAmount;
    [SerializeField] private float weedTime;
    [SerializeField] private int fireAmount;
    [SerializeField] private float fireTime;
    [SerializeField] private int diseaseAmount;
    [SerializeField] private float diseaseTime;
    [SerializeField] private int pestAmount;
    [SerializeField] private float pestTime;

    [Header("Cursor")]
    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private Vector2 defaultCursorHotspot = Vector2.zero;

    private float maxPlantableTiles;
    private int waterBudgetMax = 0;
    private int fertilizeBudgetMax = 0;

    [Header("Difficulty scaling")]
    [SerializeField] private float difficultyPerMatureTree = 0.05f;
    [SerializeField] private float maxDifficultyMultiplier = 2f;
    [SerializeField] private float minTimeMultiplier = 0.5f;

    public int getMaxWaterBudget() => waterBudgetMax;
    public int getMaxFertilizeBudget() => fertilizeBudgetMax;

    private bool wasTutorialActive = false;

    public Sprite GetToolSprite(ToolType tool) => tool switch
    {
        ToolType.Water => waterSprite,
        ToolType.Remove => removeSprite,
        ToolType.Pesticide => pesticideSprite,
        _ => null,
    };

    public bool hasSelectedTool() => currTool != ToolType.None;
    public void HideToolPreview() => cursorPreview.SetActive(false);

    private void Awake()
    {
        Instance = this;
        previewRenderer = cursorPreview.GetComponent<SpriteRenderer>();
        previewRenderer.color = new Color(1, 1, 1, 0.9f);
        cursorPreview.SetActive(false);
        currTool = ToolType.None;
    }
    private void Start()
    {
        ApplyCursorForCurrentTool();
    }

    public void InitializeBudgets()
    {
        maxPlantableTiles = 0;
        LevelData.TreeRequirement[] reqs = LevelManager.Instance.CurrentLevel.treeRequirements;

        foreach (LevelData.TreeRequirement req in reqs)
        {
            maxPlantableTiles += req.requiredMatureCount;
        }

        Debug.Log(maxPlantableTiles);

        int startingTrees = seedCount;

        waterBudget = Mathf.RoundToInt(startingTrees * 1.5f);
        fertilizeBudget = Mathf.RoundToInt(startingTrees * 0.75f);

        waterBudgetMax = Mathf.RoundToInt(maxPlantableTiles * 1.5f);
        fertilizeBudgetMax = Mathf.RoundToInt(maxPlantableTiles * 2.75f);

        OnBudgetChanged?.Invoke();
    }

    private void Update()
    {
        if (grid == null) return;   
        if (MinigameManager.Instance.IsMinigameActive) return;

        bool tutorialActive = TutorialManager.IsTutorialActive;

        if (tutorialActive)
        {
            GameCursor.Set(defaultCursorTexture, defaultCursorHotspot);
            cursorPreview.SetActive(false);
            wasTutorialActive = true;
            return;
        }

        if (wasTutorialActive)
        {
            wasTutorialActive = false;
            ApplyCursorForCurrentTool();
        }

        bool hasActivePreview = selectedTree != null ||
                                currTool == ToolType.Water ||
                                currTool == ToolType.Pesticide ||
                                currTool == ToolType.Remove ||
                                currTool == ToolType.Fertilize;


        if (hasActivePreview)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;

            int x, y;
            grid.GetXY(pos, out x, out y);
            Soil soil = grid.GetValue(x, y);
            Vector3 snapped = grid.GetSnappedPosition(pos);

            if (soil != null)
            {
                cursorPreview.transform.position = snapped;
                cursorPreview.SetActive(true);

                bool invalid = currTool switch
                {
                    ToolType.Plant => soil.HasObject || soil.isScarred,
                    ToolType.Remove => !soil.HasObject || soil.isLocked,
                    ToolType.Pesticide => (soil.CurrentObject is Tree tree && !tree.hasPest) || soil.CurrentObject is not Tree,
                    ToolType.Water => soil.isLocked || waterBudget <= 0 || (soil.CurrentObject is Tree tree && tree.hasDisease) || (soil.CurrentObject is not Tree && !soil.isOnFire) || soil.isWatered,
                    ToolType.Fertilize => soil.fertilized >= 2 || fertilizeBudget <= 0 || soil.isLocked || (soil.CurrentObject is Tree tree && tree.hasDisease) || !soil.HasObject || soil.CurrentObject is not Tree,
                    _ => false
                };

                previewRenderer.color = invalid
                    ? new Color(1, 0, 0, 0.75f)
                    : new Color(1, 1, 1, 0.75f);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            selectedTree = null;
            cursorPreview.SetActive(false);
            currTool = ToolType.None;
            InfoPanelUI.Instance.Hide();
            ApplyCursorForCurrentTool();
        }
    }

    public void SelectPlantTool(TreeData treeData)
    {
        currTool = ToolType.Plant;
        selectedTree = treeData;

        previewRenderer.sprite = treeData.previewSprite;
        cursorPreview.transform.position = Input.mousePosition;
        cursorPreview.SetActive(true);

        ApplyCursorForCurrentTool();
    }

    public void SelectTool(ToolType tool)
    {
        currTool = tool;
        selectedTree = null;

        Sprite toolSprite = tool switch
        {
            ToolType.Water => waterSprite,
            ToolType.Pesticide => pesticideSprite,
            ToolType.Remove => removeSprite,
            ToolType.Fertilize => fertilizeSprite,
            _ => null
        };

        if (toolSprite != null)
        {
            previewRenderer.sprite = toolSprite;
            previewRenderer.color = new Color(1, 1, 1, 0.75f);
            cursorPreview.SetActive(true);
        }
        else
        {
            cursorPreview.SetActive(false);
        }

        ApplyCursorForCurrentTool();
    }

    public void ApplyCursorForCurrentTool()
    {
        if (currTool == ToolType.None)
        {
            Cursor.visible = true;
            GameCursor.Set(defaultCursorTexture, defaultCursorHotspot);
        }
        else
        {
            GameCursor.Hide();
        }
    }

    public void Interact(Soil soil)
    {

        if (TutorialManager.IsTutorialActive) return;

        if (MinigameManager.Instance.IsMinigameActive) return;

        if (currTool == ToolType.Remove && soil.CurrentObject is Weed)
        {
            if (soil.isOnFire) return;

            Open(MinigameType.WeedClearing, soil, null, weedTime, weedAmount,
            "Click the weeds to clear them!");
            return;
        }

        if (currTool == ToolType.Water && soil.isOnFire)
        {

            waterBudget--;
            OnBudgetChanged?.Invoke();
            Open(MinigameType.FireSuppression, soil, null, fireTime, fireAmount,
                "Hold left-click on the ring to spray water!");
            return;
        }

        if (currTool == ToolType.Remove && soil.CurrentObject is Tree tree)
        {
            Open(MinigameType.Disease, soil, tree, diseaseTime, diseaseAmount,
                "Click the tree to chop it down!");
            return;
        }

        if (currTool == ToolType.Pesticide && soil.CurrentObject is Tree pestTree && pestTree.hasPest)
        {
            Open(MinigameType.PestControl, soil, pestTree, pestTime, pestAmount,
                "Click the bugs to squash them!");
            return;
        }

        switch (currTool)
        {
            case ToolType.None:
                InfoPanelUI.Instance.Show(soil);
                break;

            case ToolType.Plant:
                if (selectedTree != null)
                    TryPlant(soil);
                break;

            case ToolType.Remove:
                TryRemove(soil);
                break;

            case ToolType.Fertilize:
                TryFertilize(soil);
                break;

            case ToolType.Water:
                TryWater(soil);
                break;
        }

        SoilOverlay overlay = soil.GetComponent<SoilOverlay>();
        if (overlay != null)
            overlay.Refresh();
    }

    private void Open(MinigameType type, Soil soil, Tree tree, float baseTimeLimit, int baseRequired, string hint)
    {
        float multiplier = GetDifficultyMultiplier();

        int required = Mathf.Max(1, Mathf.RoundToInt(baseRequired * multiplier));

        float timeLimit = baseTimeLimit > 0f
            ? baseTimeLimit / Mathf.Max(minTimeMultiplier, Mathf.Sqrt(multiplier))
            : 0f;

        var context = new MinigameContext
        {
            type = type,
            targetSoil = soil,
            targetTree = tree,
            timeLimit = timeLimit,
            requiredSuccesses = required,
            hintText = hint
        };

        MinigameManager.Instance.Open(context, result => HandleMinigameResult(type, soil, tree, result));
    }
    private float GetDifficultyMultiplier()
    {
        int matureCount = 0;

        foreach (int val in LevelManager.Instance.GetMatureTreeCounts().Values)
        {
            matureCount += val;
        }
        Debug.Log("Mature Count: " + matureCount);
        return Mathf.Min(1f + matureCount * difficultyPerMatureTree, maxDifficultyMultiplier);
    }

    private void HandleMinigameResult(MinigameType type, Soil soil, Tree tree, MinigameResult result)
    {
        switch (type)
        {
            case MinigameType.WeedClearing:
                if (result.success) soil.RemoveObject();
                break;

            case MinigameType.FireSuppression:
                if (result.success)
                {
                    soil.Extinguish();
                }
                else
                {
                    if (soil.CurrentObject is Tree burningTree)
                        burningTree.TakeDamage(fireMinigameLossDamage);
                    TrySpreadFire(soil);
                }
                break;

            case MinigameType.Disease:
                tree.hasDisease = false;
                soil.RemoveObject();
                break;

            case MinigameType.PestControl:
                if (result.success)
                {
                    tree.hasPest = false;
                    tree.GetComponent<TreeOverlay>()?.Refresh();
                }
                else
                {
                    tree.TakeDamage(pestMinigameLossDamage);
                    TrySpreadPest(soil, tree);
                }
                break;
        }
    }

    private void TrySpreadFire(Soil source)
    {
        foreach (Soil neighbor in source.grid.Adjacent(source))
        {
            if (neighbor == null || neighbor.isOnFire) continue;
            if (neighbor.CurrentObject is Tree nt && (nt.hasPest || nt.hasDisease)) continue;
            if (Random.value < fireSpreadBaseChance)
            {
               
                neighbor.Ignite();

            }
        }
    }

    private void TrySpreadPest(Soil source, Tree sourceTree)
    {
        foreach (Soil neighbor in source.grid.Adjacent(source))
        {
            if (neighbor?.CurrentObject is not Tree neighborTree) continue;
            if (neighborTree.hasPest || neighborTree.hasDisease || neighbor.isOnFire || neighborTree.isImmune) continue;

            float chance = pestSpreadBaseChance;
            if (neighborTree.data == sourceTree.data) chance *= 1.6f;

            if (Random.value < chance)
            {

                neighborTree.hasPest = true;
                neighborTree.GetComponent<TreeOverlay>()?.Refresh();
            }
        }
    }

    public void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public void SelectTree(TreeData treeData)
    {
        selectedTree = treeData;

        previewRenderer.sprite = treeData.previewSprite;
        cursorPreview.transform.position = Input.mousePosition;
        cursorPreview.SetActive(true);

        ApplyCursorForCurrentTool();
    }

    public void TryPlant(Soil soil)
    {

        if (selectedTree == null || soil.HasObject || seedCount <= 0 || soil.isScarred) return;

        soil.PlantTree(selectedTree);
        seedCount--;
        OnSeedChanged?.Invoke();

        treesPlanted++;

        TutorialManager.Instance.TriggerTutorial(firstPlantTutorial);

        SoundManager.Instance.PlaySFX("plantSFX");
    }

    public void TryPlantWeed(Soil soil)
    {

        if (selectedTree == null || soil.HasObject || seedCount <= 0 || soil.isScarred) return;

        soil.PlantWeed();
    }

    public void TryFertilize(Soil soil)
    {
        if(fertilizeBudget <= 0 || soil.fertilized >= 2 || soil.isLocked || !soil.HasObject || soil.CurrentObject is not Tree || (soil.CurrentObject is Tree tree && tree.hasDisease)) return;

        soil.Fertilize();
        fertilizeBudget--;

        OnBudgetChanged?.Invoke();
    }

    public void TryWater(Soil soil)
    {
        if (waterBudget <= 0 || soil.isWatered || soil.CurrentObject is not Tree || (soil.CurrentObject is Tree tree && tree.hasDisease)) return;

        soil.Water();
        waterBudget--;

        waterToolsUsed++;

        OnBudgetChanged?.Invoke();

        SoundManager.Instance.PlaySFX("waterSFX");
    }

    /*public void TryDigDitch(Soil soil)
    {
        if (ditchBudget <= 0) return;
        if (soil.isOnFire || soil.recentlyOnFire) return;
        if (soil.CurrentObject is Ditch) return;

        if (soil.RemoveObject()) seedCount++;

        soil.PlantDitch();
        ditchBudget--;

        ditchToolsUsed++;

        OnBudgetChanged?.Invoke();

        SoundManager.Instance.PlaySFX("plantSFX");
    }
    */
    public void RefillToolBudget()
    {

        int treeCount = RoundManager.Instance.CountTrees();

        int waterRefill = Mathf.RoundToInt(waterBudgetMax * 0.15f)
                        + Mathf.RoundToInt(treeCount * 0.4f);

        if (WeatherManager.Instance.currentWeather == WeatherType.Rain)
            waterRefill = Mathf.RoundToInt(waterRefill * 1.75f);

        int fertilizeRefill = Mathf.RoundToInt(fertilizeBudgetMax * 0.15f)
                            + Mathf.RoundToInt(treeCount * 0.25f);

        waterBudget = Mathf.Min(waterBudget + waterRefill, waterBudgetMax);
        fertilizeBudget = Mathf.Min(fertilizeBudget + fertilizeRefill, fertilizeBudgetMax);

        Debug.Log(waterBudgetMax + " " + fertilizeBudgetMax);

        OnBudgetChanged?.Invoke();

    }

    public bool CanWaterAllTrees()
    {
        int needed = CountUnwateredTrees();
        return needed > 0 && waterBudget >= needed;
    }

    public bool CanFertilizeAllTrees()
    {
        int needed = CountUnfertilizedTrees();
        return needed > 0 && fertilizeBudget >= needed;
    }

    private int CountUnwateredTrees()
    {
        int count = 0;
        foreach (Soil s in GameManager.Instance.GetAllSoils())
            if (!s.isLocked && s.CurrentObject is Tree && !s.isWatered) count++;
        return count;
    }

    private int CountUnfertilizedTrees()
    {
        int count = 0;
        foreach (Soil s in GameManager.Instance.GetAllSoils())
            if (!s.isLocked && s.CurrentObject is Tree && s.fertilized < 2 && s.fertilized >= 0) count++;
        return count;
    }

    public void UseAllWater()
    {
        foreach (Soil s in GameManager.Instance.GetAllSoils())
        {
            if (waterBudget <= 0)
                break;

            if (s.isLocked || s.CurrentObject is not Tree || s.isWatered || s.isOnFire)
                continue;

            s.Water();
            waterBudget--;
        }

        OnBudgetChanged?.Invoke();
    }

    public void UseAllFertilize()
    {
        foreach (Soil s in GameManager.Instance.GetAllSoils())
        {
            if (fertilizeBudget <= 0)
                break;

            if (s.isLocked || s.CurrentObject is not Tree || s.fertilized >= 2)
                continue;

            s.Fertilize();
            fertilizeBudget--;
        }

        OnBudgetChanged?.Invoke();
    }

    public void TryRemove(Soil soil)
    {

        if (!soil.HasObject) return;

        if(soil.RemoveObject()) seedCount++;

        OnSeedChanged?.Invoke();

        SoundManager.Instance.PlaySFX("removeSFX");
    }

    public void SetInputLocked(bool input)
    {

    }

}
