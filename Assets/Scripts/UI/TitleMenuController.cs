using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public sealed class TitleMenuController : MonoBehaviour
{
    [SerializeField] private GameObject titleView;
    [SerializeField] private GameObject factionSelectionView;
    [SerializeField] private GameObject difficultySelectionView;
    [SerializeField] private FactionCatalog factionCatalog;
    [SerializeField] private RectTransform factionButtonContainer;
    [SerializeField] private Button factionButtonTemplate;
    [SerializeField, Min(0)] private int battleSceneBuildIndex = 1;

    private void Awake()
    {
        EnsureDifficultySelectionView();
        ConfigureExistingDifficultyView();
        PopulateFactionButtons();
        ShowTitle();
    }

    private void ConfigureExistingDifficultyView()
    {
        if (difficultySelectionView == null) return;

        Button easy = FindDifficultyButton("Easy Difficulty Button");
        Button medium = FindDifficultyButton("Medium Difficulty Button");
        Button hard = FindDifficultyButton("Hard Difficulty Button");
        Button back = FindDifficultyButton("Difficulty Back Button");
        Button start = FindDifficultyButton("Start Battle Button");

        RectTransform card = FindDifficultyRect("Difficulty Card") ?? FindDifficultyRect("Difficulty Panel");
        if (card != null)
        {
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(760f, 800f);
        }

        RectTransform banner = FindDifficultyRect("Difficulty Banner");
        if (banner != null)
        {
            banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 0.5f);
            banner.anchoredPosition = new Vector2(0f, 245f);
            banner.sizeDelta = new Vector2(650f, 120f);
        }

        ConfigureDifficultyButton(easy, "EASY", new Vector2(0f, 95f), BeginEasyBattle);
        ConfigureDifficultyButton(medium, "MEDIUM", new Vector2(0f, -10f), BeginMediumBattle);
        ConfigureDifficultyButton(hard, "HARD", new Vector2(0f, -115f), BeginHardBattle);
        ConfigureDifficultyButton(back, "BACK", new Vector2(0f, -245f), ShowFactionSelection);

        if (start != null)
        {
            start.gameObject.SetActive(false);
        }

        foreach (TMP_Text text in difficultySelectionView.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null) continue;
            string value = text.text.ToUpperInvariant();
            if (value.Contains("CHOOSE DIFFICULTY") && banner != null)
            {
                RectTransform heading = text.rectTransform;
                heading.SetParent(banner, false);
                heading.anchorMin = Vector2.zero;
                heading.anchorMax = Vector2.one;
                heading.offsetMin = new Vector2(18f, 12f);
                heading.offsetMax = new Vector2(-18f, -12f);
                heading.anchoredPosition = Vector2.zero;
            }
            if (value.Contains("GOLD") || value.Contains("ENEMY BONUS") || value.Contains("START BATTLE"))
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    private Button FindDifficultyButton(string objectName)
    {
        foreach (Button button in difficultySelectionView.GetComponentsInChildren<Button>(true))
        {
            if (button != null && button.name == objectName) return button;
        }
        return null;
    }

    private RectTransform FindDifficultyRect(string objectName)
    {
        foreach (RectTransform rect in difficultySelectionView.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect != null && rect.name == objectName) return rect;
        }
        return null;
    }

    private static void ConfigureDifficultyButton(Button button, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(430f, 82f);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.gameObject.SetActive(true);
            text.text = label;
        }
    }

    private void EnsureDifficultySelectionView()
    {
        if (difficultySelectionView != null) return;

        Transform parent = titleView != null ? titleView.transform.parent : transform;
        GameObject view = new GameObject("Difficulty Selection View", typeof(RectTransform));
        RectTransform viewRect = view.GetComponent<RectTransform>();
        viewRect.SetParent(parent, false);
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = Vector2.zero;
        viewRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("Difficulty Panel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(viewRect, false);
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 760f);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.sprite = FindMenuSprite("Selection Card");
        panelImage.type = panelImage.sprite != null && panelImage.sprite.border.sqrMagnitude > 0
            ? Image.Type.Sliced
            : Image.Type.Simple;
        panelImage.color = panelImage.sprite == null ? new Color32(55, 48, 42, 248) : Color.white;

        GameObject banner = new GameObject("Difficulty Banner", typeof(RectTransform), typeof(Image));
        RectTransform bannerRect = banner.GetComponent<RectTransform>();
        bannerRect.SetParent(panelRect, false);
        bannerRect.anchorMin = bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRect.anchoredPosition = new Vector2(0f, 245f);
        bannerRect.sizeDelta = new Vector2(650f, 120f);
        Image bannerImage = banner.GetComponent<Image>();
        bannerImage.sprite = FindMenuSprite("Selection Banner");
        bannerImage.type = bannerImage.sprite != null && bannerImage.sprite.border.sqrMagnitude > 0
            ? Image.Type.Sliced
            : Image.Type.Simple;
        bannerImage.color = bannerImage.sprite == null ? new Color32(203, 157, 70, 255) : Color.white;
        CreateRuntimeLabel(bannerRect, "CHOOSE DIFFICULTY", 42f);

        Sprite buttonSprite = (factionButtonTemplate?.targetGraphic as Image)?.sprite;
        CreateRuntimeButton(panelRect, "EASY", new Vector2(0f, 95f), buttonSprite, BeginEasyBattle);
        CreateRuntimeButton(panelRect, "MEDIUM", new Vector2(0f, -10f), buttonSprite, BeginMediumBattle);
        CreateRuntimeButton(panelRect, "HARD", new Vector2(0f, -115f), buttonSprite, BeginHardBattle);
        CreateRuntimeButton(panelRect, "BACK", new Vector2(0f, -245f), buttonSprite, ShowFactionSelection);
        difficultySelectionView = view;
        view.SetActive(false);
    }

    private Sprite FindMenuSprite(string objectName)
    {
        Transform match = factionSelectionView?.transform.Find(objectName);
        if (match == null && factionSelectionView != null)
        {
            foreach (Image image in factionSelectionView.GetComponentsInChildren<Image>(true))
            {
                if (image.name == objectName) return image.sprite;
            }
        }
        return match?.GetComponent<Image>()?.sprite;
    }

    private static TMP_Text CreateRuntimeLabel(RectTransform parent, string value, float fontSize)
    {
        GameObject labelObject = new GameObject(value, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    private static void CreateRuntimeButton(RectTransform parent, string label, Vector2 position, Sprite sprite, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(430f, 82f);
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = sprite != null && sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
        image.color = sprite == null ? new Color32(42, 111, 138, 255) : Color.white;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        CreateRuntimeLabel(buttonRect, label, 28f);
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

        FactionSelectionSession.SelectPlayerFaction(faction);
        ShowDifficultySelection();
    }

    public void BeginEasyBattle() => SelectDifficulty(GameDifficulty.Easy);
    public void BeginMediumBattle() => SelectDifficulty(GameDifficulty.Medium);
    public void BeginHardBattle() => SelectDifficulty(GameDifficulty.Hard);

    private void SelectDifficulty(GameDifficulty difficulty)
    {
        FactionData playerFaction = FactionSelectionSession.PlayerFaction;
        List<FactionData> opponents = new List<FactionData>();
        if (factionCatalog?.Factions != null)
        {
            foreach (FactionData faction in factionCatalog.Factions)
            {
                if (faction != null && faction != playerFaction && !opponents.Contains(faction))
                {
                    opponents.Add(faction);
                }
            }
        }

        if (playerFaction == null || opponents.Count == 0)
        {
            Debug.LogError($"{name}: A player faction and at least one different enemy faction are required.", this);
            return;
        }

        if (battleSceneBuildIndex < 0 || battleSceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"{name}: Battle scene build index {battleSceneBuildIndex} is invalid.", this);
            return;
        }

        FactionData enemyFaction = opponents[Random.Range(0, opponents.Count)];
        FactionSelectionSession.ConfigureMatch(playerFaction, enemyFaction, difficulty);
        SceneManager.LoadScene(battleSceneBuildIndex);
    }

    private void ShowDifficultySelection()
    {
        factionSelectionView?.SetActive(false);
        difficultySelectionView?.SetActive(true);
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
        difficultySelectionView?.SetActive(false);
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

        difficultySelectionView?.SetActive(false);
    }

    public void ShowFactionSelection()
    {
        titleView?.SetActive(false);
        difficultySelectionView?.SetActive(false);
        factionSelectionView?.SetActive(true);
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
