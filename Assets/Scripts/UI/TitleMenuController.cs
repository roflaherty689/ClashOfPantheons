using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public sealed class TitleMenuController : MonoBehaviour
{
    [SerializeField] private GameObject titleView;
    [SerializeField] private GameObject factionSelectionView;
    [SerializeField] private FactionCatalog factionCatalog;
    [SerializeField] private RectTransform factionButtonContainer;
    [SerializeField] private Button factionButtonTemplate;
    [SerializeField, Min(0)] private int battleSceneBuildIndex = 1;

    private void Awake()
    {
        PopulateFactionButtons();
        ShowTitle();
    }

    private void PopulateFactionButtons()
    {
        if (factionCatalog == null || factionButtonContainer == null || factionButtonTemplate == null)
        {
            Debug.LogError($"{name}: Faction selection configuration is incomplete.", this);
            return;
        }

        factionButtonTemplate.gameObject.SetActive(false);
        HashSet<FactionData> createdFactions = new HashSet<FactionData>();

        if (factionCatalog.Factions == null)
        {
            Debug.LogError($"{name}: Faction catalog has no faction list.", factionCatalog);
            return;
        }

        foreach (FactionData faction in factionCatalog.Factions)
        {
            if (faction == null)
            {
                Debug.LogWarning($"{name}: Faction catalog contains a null entry.", factionCatalog);
                continue;
            }

            if (!createdFactions.Add(faction))
            {
                Debug.LogWarning($"{name}: Ignoring duplicate faction '{faction.FactionName}'.", factionCatalog);
                continue;
            }

            Button option = Instantiate(factionButtonTemplate, factionButtonContainer);
            option.name = $"{faction.FactionName} Faction Button";
            option.interactable = true;
            if (option.targetGraphic != null)
            {
                option.targetGraphic.raycastTarget = true;
            }
            option.gameObject.SetActive(true);

            TMP_Text label = option.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = faction.FactionName;
            }

            Image icon = option.transform.Find("Faction Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = faction.CastleSprite;
                icon.enabled = faction.CastleSprite != null;
            }

            FactionData selectedFaction = faction;
            option.onClick.AddListener(() => SelectFaction(selectedFaction));
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(factionButtonContainer);
    }

    private void SelectFaction(FactionData faction)
    {
        if (faction == null)
        {
            Debug.LogError($"{name}: Cannot select a null faction.", this);
            return;
        }

        if (battleSceneBuildIndex < 0 || battleSceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"{name}: Battle scene build index {battleSceneBuildIndex} is invalid.", this);
            return;
        }

        FactionSelectionSession.SelectPlayerFaction(faction);
        SceneManager.LoadScene(battleSceneBuildIndex);
    }

    public void Play()
    {
        if (titleView == null || factionSelectionView == null)
        {
            Debug.LogError($"{name}: Title menu views are not assigned.", this);
            return;
        }

        titleView.SetActive(false);
        factionSelectionView.SetActive(true);
    }

    public void ShowTitle()
    {
        if (titleView != null)
        {
            titleView.SetActive(true);
        }

        if (factionSelectionView != null)
        {
            factionSelectionView.SetActive(false);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
