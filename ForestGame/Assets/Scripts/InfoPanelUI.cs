using System;
using TMPro;
using UnityEngine;

public class InfoPanelUI : MonoBehaviour
{
    public static InfoPanelUI Instance;

    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text soilText;
    [SerializeField] private TMP_Text treeText;

    [SerializeField] private Vector3 offset;

    private Soil currentSoil;

    private Camera cam;

    public bool SoilExists => currentSoil != null;

    private void Awake()
    {
        Instance = this;

        cam = Camera.main;

        Hide();
    }

    private void Update()
    {
        if (currentSoil == null || !panel.activeSelf)
            return;

        UpdatePosition();

        Refresh();
    }

    public void Show(Soil soil)
    {
        currentSoil = soil;

        panel.SetActive(true);

        UpdatePosition();

        Refresh();
    }

    private void UpdatePosition()
    {
        Vector3 screenPos =
            cam.WorldToScreenPoint(currentSoil.transform.position);

        // hide if behind camera
        if (screenPos.z < 0)
        {
            panel.SetActive(false);
            return;
        }

        // check if outside screen
        bool offscreen =
            screenPos.x < 0 ||
            screenPos.x > Screen.width ||
            screenPos.y < 0 ||
            screenPos.y > Screen.height;

        panel.SetActive(!offscreen);

        if (!offscreen)
        {
            panel.transform.position = screenPos + offset;
        }
    }

    private void Refresh()
    {
        if (currentSoil == null)
            return;

        soilText.text =
            $"Soil Type: {currentSoil.type}\n" +
            $"Moisture: {(currentSoil.moisture * 100f):0}%\n" +
            $"Fertility: {(currentSoil.fertility * 100f):0}%\n" +
            $"Shade: {(currentSoil.shade * 100f):0}%\n" +
            $"OnFire: {currentSoil.isOnFire}";

        Tree tree = currentSoil.CurrentObject is Tree treeObj ? treeObj : null;

        if (tree != null)
        {

            treeText.text =
                $"Tree: {tree.data.treeName}\n" +
                $"Health: {(tree.health * 100f):0}%\n" +
                $"Mature: {tree.isMature}\n" +
                $"Age: {tree.AgeYears} months";
        }
        else
        {
            treeText.text = "No tree planted";
        }
    }

    public void Hide()
    {
        currentSoil = null;

        panel.SetActive(false);
    }
}