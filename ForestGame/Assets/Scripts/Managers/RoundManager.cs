using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    [Header("Round / season")]
    [SerializeField] private int currentRound = 0;
    public int CurrentRound => currentRound;

    private static readonly Season[] seasonCycle =
        { Season.Summer, Season.Autumn, Season.Winter, Season.Spring };
    public Season CurrentSeason => seasonCycle[currentRound % seasonCycle.Length];

    public GamePhase Phase { get; private set; } = GamePhase.Planning;
    public event Action<GamePhase> OnPhaseChanged;

    [Header("Round summary")]
    public int treesLostThisRound;
    public int treesSavedThisRound;

    [Header("Weeds")]
    [SerializeField] private float initialWeedChance = 0.12f;
    [SerializeField] private float weedRegrowChancePerRound = 0.06f;

    [Header("Disease")]
    [SerializeField] private float diseaseChance = 0.2f;
    [SerializeField] private float diseaseHealthThreshold = 0.3f;
    [SerializeField] private float diseaseSpreadChance = 0.15f;

    [Header("Pest")]
    public float pestRoundEndDamage = 0.2f;

    [SerializeField] private TMP_Text timeText;

    private void Awake()
    {
        Instance = this;
        timeText.text = "Year : " + currentRound;
    }

    public void SeedInitialWeeds()
    {
        foreach (Soil soil in GetPlayAreaSoils())
        {
            if (!soil.HasObject && UnityEngine.Random.value < initialWeedChance)
            {
                soil.PlantWeed();
            }
        }
    }

    private void SetPhase(GamePhase phase)
    {
        Phase = phase;
        OnPhaseChanged?.Invoke(phase);
    }

    public void EndPlanningPhase()
    {
        if (Phase != GamePhase.Planning) return;
        StartCoroutine(ResolveRoundRoutine());
    }

    private IEnumerator ResolveRoundRoutine()
    {
        SetPhase(GamePhase.Resolving);

        List<Soil> playAreaSoils = GetPlayAreaSoils();

        foreach (Soil soil in playAreaSoils) soil.ClearScar();

        ResolveUnextinguishedFires(playAreaSoils);

        List<(Soil soil, Tree tree)> treesToResolve = new();
        foreach (Soil soil in playAreaSoils)
            if (soil.CurrentObject is Tree tree) treesToResolve.Add((soil, tree));

        int treesBefore = treesToResolve.Count;

        foreach (var (soil, tree) in treesToResolve)
        {
            if (tree == null) continue;

            if (tree.hasPest) tree.TakeDamage(pestRoundEndDamage);
            if (tree == null) continue;

            if (tree.hasDisease) TrySpreadDisease(soil, tree);

            tree.ResolveRound();
        }

        foreach (Soil soil in playAreaSoils) soil.ResolveSoilState();

        foreach (Soil soil in playAreaSoils)
            if (!soil.HasObject && UnityEngine.Random.value < weedRegrowChancePerRound)
                soil.PlantWeed();

        WeatherType weather = WeatherManager.Instance.RollForSeason(CurrentSeason);

        bool gameEnded = LevelManager.Instance.CheckEndConditions();
        if (gameEnded) { SetPhase(GamePhase.GameOver); yield break; }

        EventManager.Instance.RollFireEvents(playAreaSoils, CurrentSeason, weather);
        EventManager.Instance.RollPestEvents(playAreaSoils, CurrentSeason);

        foreach (Soil soil in playAreaSoils)
        {
            if (soil.CurrentObject is not Tree tree || tree.dead || tree.hasDisease || tree.hasPest || soil.isOnFire) continue;

            if (tree.health < diseaseHealthThreshold && UnityEngine.Random.value < diseaseChance)
            {
                tree.hasDisease = true;
                tree.GetComponent<TreeOverlay>()?.Refresh();
            }
        }

        yield return null;

        int treesAfter = CountTrees(playAreaSoils);
        treesLostThisRound = Mathf.Max(0, treesBefore - treesAfter);
        treesSavedThisRound = treesAfter;

        InteractionManager.Instance.RefillToolBudget();

        currentRound++;
        timeText.text = "Year : " + currentRound * 10;

        SetPhase(GamePhase.RoundSummary);
    }
    private void TrySpreadDisease(Soil source, Tree sourceTree)
    {
        foreach (Soil neighbor in source.grid.Adjacent(source))
        {
            if (neighbor?.CurrentObject is not Tree neighborTree) continue;
            if (neighborTree.hasDisease || neighborTree.hasPest || neighbor.isOnFire || neighborTree.dead || neighborTree.isImmune) continue;

            if (UnityEngine.Random.value < diseaseSpreadChance)
            {
                neighborTree.hasDisease = true;
                neighborTree.GetComponent<TreeOverlay>()?.Refresh();
            }
        }
    }

    public void AcknowledgeRoundSummary()
    {
        if (Phase != GamePhase.RoundSummary) return;
        SetPhase(GamePhase.Planning);
    }

    private List<Soil> GetPlayAreaSoils()
    {
        List<Soil> result = new List<Soil>();
        foreach (Soil soil in GameManager.Instance.GetAllSoils())
        {
            if (!soil.isLocked) result.Add(soil);
        }
        return result;
    }

    private int CountTrees(List<Soil> soils)
    {
        int count = 0;
        foreach (Soil s in soils)
            if (s.CurrentObject is Tree) count++;
        return count;
    }

    public int CountTrees()
    {
        return CountTrees(GetPlayAreaSoils());
    }

    public HashSet<Soil> PredictBurnSpread() => ComputeFireSpreadSet(GetPlayAreaSoils());

    private HashSet<Soil> ComputeFireSpreadSet(List<Soil> playAreaSoils)
    {
        HashSet<Soil> result = new();
        Queue<Soil> frontier = new();

        foreach (Soil s in playAreaSoils.FindAll(s => s.isOnFire))
        {
            result.Add(s);
            frontier.Enqueue(s);
        }

        while (frontier.Count > 0)
        {
            Soil current = frontier.Dequeue();
            foreach (Soil n in current.grid.Adjacent(current))
            {
                if (n == null || n.isLocked) continue;
                if (result.Contains(n)) continue;
                if (n.CurrentObject is not Tree && n.CurrentObject is not Weed) continue;
                if (n.CurrentObject is Tree nt && nt.isImmune) continue;

                result.Add(n);
                frontier.Enqueue(n); 
            }
        }

        return result;
    }

    private void ResolveUnextinguishedFires(List<Soil> playAreaSoils)
    {
        HashSet<Soil> toBurnDown = ComputeFireSpreadSet(playAreaSoils);
        if (toBurnDown.Count == 0) return;

        foreach (Soil s in toBurnDown)
        {
            if (s.CurrentObject is Tree t && !t.dead) t.Die();
            else if (s.CurrentObject is Weed) s.RemoveObject();

            s.Extinguish();
            s.MarkScarred();
        }
    }
}