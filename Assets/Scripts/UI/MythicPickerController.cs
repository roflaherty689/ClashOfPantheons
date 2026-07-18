using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MythicPickerController
{
    private sealed class ChoiceBinding
    {
        public BaseUnit Prefab;
        public Button Button;
    }

    private readonly RectTransform hostPanel;
    private readonly TMP_FontAsset font;
    private readonly Team playerTeam;
    private readonly Action purchaseCompleted;
    private readonly List<ChoiceBinding> choiceBindings = new List<ChoiceBinding>();

    private GameManager gameManager;
    private WorkerManager workerManager;
    private GameObject picker;

    public MythicPickerController(
        RectTransform hostPanel,
        TMP_FontAsset font,
        Team playerTeam,
        Action purchaseCompleted)
    {
        this.hostPanel = hostPanel;
        this.font = font;
        this.playerTeam = playerTeam;
        this.purchaseCompleted = purchaseCompleted;
    }

    public void Open(GameManager manager, WorkerManager economy)
    {
        gameManager = manager;
        workerManager = economy;

        if (hostPanel == null || gameManager?.MythicUnitRoster == null || picker != null)
        {
            return;
        }

        SetHostChildrenVisible(false);
        CreatePicker();
        Refresh(manager, economy);
    }

    public void Refresh(GameManager manager, WorkerManager economy)
    {
        gameManager = manager;
        workerManager = economy;

        if (picker == null || workerManager == null || gameManager == null) return;

        foreach (ChoiceBinding binding in choiceBindings)
        {
            bool valid = binding.Prefab != null && binding.Prefab.UnitData != null;
            binding.Button.interactable = valid && !gameManager.IsGameOver &&
                workerManager.CurrentGold >= binding.Prefab.UnitData.Cost;
        }
    }

    public void Close()
    {
        GameObject pickerToDestroy = picker;
        picker = null;
        if (pickerToDestroy != null)
        {
            pickerToDestroy.SetActive(false);
            UnityEngine.Object.Destroy(pickerToDestroy);
        }

        choiceBindings.Clear();
        SetHostChildrenVisible(true, pickerToDestroy);
    }

    private void CreatePicker()
    {
        picker = CreateUIObject("Mythic Picker", hostPanel).gameObject;
        RectTransform pickerRect = picker.GetComponent<RectTransform>();
        pickerRect.anchorMin = Vector2.zero;
        pickerRect.anchorMax = Vector2.one;
        pickerRect.offsetMin = new Vector2(10f, 10f);
        pickerRect.offsetMax = new Vector2(-10f, -10f);
        Image background = picker.AddComponent<Image>();
        background.color = new Color32(31, 36, 37, 252);

        RectTransform viewport = CreateUIObject("Viewport", pickerRect);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(8f, 8f);
        viewport.offsetMax = new Vector2(-8f, -8f);
        Image viewportImage = viewport.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.12f);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = CreateUIObject("Choices", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(190f, 58f);
        grid.spacing = new Vector2(8f, 7f);
        grid.padding = new RectOffset(3, 3, 3, 3);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = picker.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        List<BaseUnit> orderedMythics = GetOrderedMythics();
        foreach (BaseUnit prefab in orderedMythics)
        {
            CreateChoice(content, grid.cellSize, prefab);
        }
    }

    private List<BaseUnit> GetOrderedMythics()
    {
        List<BaseUnit> orderedMythics = new List<BaseUnit>();
        foreach (BaseUnit prefab in gameManager.MythicUnitRoster.Units)
        {
            if (prefab != null && prefab.UnitData != null)
            {
                orderedMythics.Add(prefab);
            }
        }

        orderedMythics.Sort((left, right) =>
        {
            int costComparison = left.UnitData.Cost.CompareTo(right.UnitData.Cost);
            return costComparison != 0
                ? costComparison
                : string.Compare(GetDisplayName(left), GetDisplayName(right), StringComparison.Ordinal);
        });
        return orderedMythics;
    }

    private void CreateChoice(Transform parent, Vector2 size, BaseUnit prefab)
    {
        string displayName = GetDisplayName(prefab);
        Button choice = CreateButton(parent, displayName, size);
        TextMeshProUGUI label = choice.GetComponentInChildren<TextMeshProUGUI>();
        label.text = $"{displayName}\n{prefab.UnitData.Cost} GOLD";
        label.fontSize = 14f;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchoredPosition = new Vector2(25f, 0f);
        labelRect.sizeDelta = new Vector2(128f, 52f);

        Sprite avatar = gameManager.MythicUnitRoster.GetAvatar(prefab) ?? GetUnitSprite(prefab);
        CreateAvatar(choice.transform, avatar);
        choice.onClick.AddListener(() => Purchase(prefab));
        choiceBindings.Add(new ChoiceBinding { Prefab = prefab, Button = choice });
    }

    private void Purchase(BaseUnit prefab)
    {
        if (gameManager == null ||
            !gameManager.TrySelectAndPurchaseMythic(playerTeam, prefab, workerManager))
        {
            return;
        }

        Close();
        purchaseCompleted?.Invoke();
    }

    private Button CreateButton(Transform parent, string objectName, Vector2 size)
    {
        RectTransform rect = CreateUIObject(objectName, parent);
        rect.sizeDelta = size;
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color32(43, 102, 127, 255);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateText(rect, objectName, size - new Vector2(8f, 4f));
        return button;
    }

    private void CreateText(Transform parent, string value, Vector2 size)
    {
        RectTransform rect = CreateUIObject(value + " Text", parent);
        rect.sizeDelta = size;
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = 16f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private static void CreateAvatar(Transform parent, Sprite avatar)
    {
        RectTransform avatarRect = CreateUIObject("Avatar", parent);
        avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
        avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
        avatarRect.sizeDelta = new Vector2(48f, 48f);
        avatarRect.anchoredPosition = new Vector2(-66f, 0f);
        Image avatarImage = avatarRect.gameObject.AddComponent<Image>();
        avatarImage.sprite = avatar;
        avatarImage.color = Color.white;
        avatarImage.preserveAspect = true;
        avatarImage.raycastTarget = false;
    }

    private void SetHostChildrenVisible(bool visible, GameObject excludedChild = null)
    {
        if (hostPanel == null) return;

        for (int i = 0; i < hostPanel.childCount; i++)
        {
            GameObject child = hostPanel.GetChild(i).gameObject;
            if (child != excludedChild)
            {
                child.SetActive(visible);
            }
        }
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        GameObject instance = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static Sprite GetUnitSprite(BaseUnit prefab)
    {
        SpriteRenderer renderer = prefab != null
            ? prefab.GetComponentInChildren<SpriteRenderer>(true)
            : null;
        return renderer != null ? renderer.sprite : null;
    }

    private static string GetDisplayName(BaseUnit prefab)
    {
        if (prefab == null) return "Unknown";

        return prefab.name
            .Replace("MythicUnit", string.Empty)
            .Replace("MeleeMythicAnimatedUnit", "Minotaur")
            .Replace("MonkUnit", " Monk")
            .Replace("Fish", " Fish")
            .Trim();
    }
}
