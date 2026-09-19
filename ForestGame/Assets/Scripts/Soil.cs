using UnityEngine;
using UnityEngine.EventSystems;

public class Soil : MonoBehaviour
{
    private TileObject currentObject;
    public GameObject treePrefab;
    public GameObject ditchPrefab;
    public GameObject weedPrefab;

    public int x;
    public int y;
    public Grid grid;

    //0 - not fertilized, 1 - medium fertlized, 2 - fully fertilized
    public int fertilized = 0;
    public bool isFertilized = false;
    public bool isWatered = false;

    [Range(0f, 1f)]
    public float shade = 0f;

    public bool isOnFire = false;
    public bool isScarred = false;

    public bool isLocked = false;

    [SerializeField]
    private Sprite soilSprite;
    [SerializeField]
    private Sprite burntSprite;

    [SerializeField] private TutorialStep firstFireTutorial;

    public AudioClip fireLoopClip;

    public event System.Action OnStateChanged;
    private void NotifyChanged() => OnStateChanged?.Invoke();

    public bool HasObject => currentObject != null;
    public void Water() {
        isWatered = true;
        NotifyChanged();
    }

    public void Fertilize() { 
        fertilized = Mathf.Clamp(fertilized + 1, 0, 2);
        isFertilized = true;
        NotifyChanged();
    }

    public TileObject CurrentObject => currentObject;

    public void ResolveTreeGrowth()
    {
        if (currentObject is Tree tree)
        {
            tree.ResolveRound();
        }
    }

    public void MarkScarred()
    {
        isScarred = true;
        GetComponent<SpriteRenderer>().sprite = burntSprite;
        NotifyChanged();
    }

    public void ClearScar()
    {
        if (!isScarred) return;
        isScarred = false;
        GetComponent<SpriteRenderer>().sprite = soilSprite;
        NotifyChanged();
    }

    public void ResolveSoilState()
    {
        fertilized = Mathf.Clamp(fertilized - 1, 0, 2);
        isFertilized = false;
        isWatered = false;

        NotifyChanged();
    }


    public void PlantTree(TreeData treeData)
    {

        if (HasObject) return;

        GameObject treeObj = Instantiate(treePrefab, transform.position, Quaternion.identity);

        Tree tree = treeObj.GetComponent<Tree>();

        tree.Initialize(this, treeData);

        currentObject = tree;
    }
    
    public void PlantDitch()
    {
        if (HasObject) return;

        GameObject obj = Instantiate(ditchPrefab, transform.position, Quaternion.identity);
        Ditch ditch = obj.GetComponent<Ditch>();
        ditch.Initialize(this);
        currentObject = ditch;
    }

    public void PlantWeed()
    {
        if (HasObject) return;

        GameObject obj = Instantiate(weedPrefab, transform.position, Quaternion.identity);
        Weed weed = obj.GetComponent<Weed>();
        weed.Initialize(this);
        currentObject = weed;
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (isLocked) return;

        InteractionManager.Instance.Interact(this);
    }

    public bool RemoveObject()
    {
        if (currentObject == null) return false;

        bool wasTree = currentObject is Tree;
        if (wasTree) InteractionManager.Instance.treesDied++;

        bool shouldReturnSeed = currentObject is Tree tree && tree.justPlanted;

        Destroy(currentObject.gameObject);

        currentObject = null;

        if (wasTree) LevelManager.Instance.CheckLossImmediate();

        return shouldReturnSeed;
    }

    public bool Ignite()
    {
        if (isOnFire) return false;
        if (CurrentObject is Ditch) return false;
        if (CurrentObject is Tree t && t.isImmune) return false;

        isOnFire = true;
        NotifyChanged();

        InteractionManager.Instance.firesStarted++;
        SoundManager.Instance.PlaySFX("fireSFX", 1f, 0.1f);
        SoundManager.Instance.PlayLoopingSFX(this, fireLoopClip, transform.position);

        return true;
    }

    private void OnMouseEnter()
    {
        if (RoundManager.Instance.Phase != GamePhase.Planning) return;
        //!InteractionManager.Instance.hasSelectedTool() && 
        if (CurrentObject is Tree tree && !tree.isImmune && !isOnFire && !tree.hasPest && !tree.hasDisease)
        {
            tree.GetComponent<TreeForecastLabel>()?.Show(
                tree.health * 100f, tree.PredictedHealthDeltaPercent(), tree.WillDieNextRound(), false);
        }

        ToolType? hintTool = GetHintTool();
        if (hintTool.HasValue && CurrentObject is Tree hintTree)
            hintTree.GetComponent<ToolHintIcon>()?.Show(hintTool.Value);
        if (hintTool.HasValue && CurrentObject is Weed weed)
            weed.GetComponent<ToolHintIcon>()?.Show(hintTool.Value);
    }

    private void OnMouseExit()
    {
        if (CurrentObject is Tree tree)
        {
            tree.GetComponent<TreeForecastLabel>()?.Hide();
            tree.GetComponent<ToolHintIcon>()?.Hide();
        }
        if (CurrentObject is Weed weed)
        {
            weed.GetComponent<ToolHintIcon>()?.Hide();
        }
    }

    public ToolType? GetHintTool()
    {
        if (isOnFire) return ToolType.Water;
        if (CurrentObject is Tree tree)
        {
            if(tree.hasDisease)return ToolType.Remove;
            if (tree.hasPest)return ToolType.Pesticide;
        }
        if (CurrentObject is Weed) return ToolType.Remove;

        return null;
    }

    public void Extinguish()
    {
        isOnFire = false;

        SoundManager.Instance.StopLoopingSFX(this);

        NotifyChanged();
    }
}
