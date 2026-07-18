using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SelectedRolePresenter
{
    private const string SelectedRolePath = "Safe Area/Bottom HUD/Selected Role/";

    private readonly Team playerTeam;
    private readonly ProductionCardPresenter cardPresenter;
    private readonly Action<UnitRole> purchaseRequested;
    private readonly Image roleIcon;
    private readonly TextMeshProUGUI titleText;
    private readonly TextMeshProUGUI statusText;
    private readonly TextMeshProUGUI tierText;
    private readonly TextMeshProUGUI descriptionText;
    private readonly TextMeshProUGUI actionText;
    private readonly Button actionButton;

    private UnitRole selectedRole = UnitRole.Melee;

    public SelectedRolePresenter(
        Transform root,
        Team playerTeam,
        ProductionCardPresenter cardPresenter,
        Action<UnitRole> purchaseRequested)
    {
        this.playerTeam = playerTeam;
        this.cardPresenter = cardPresenter;
        this.purchaseRequested = purchaseRequested;

        Transform panelTransform = root.Find(SelectedRolePath.TrimEnd('/'));
        Panel = panelTransform as RectTransform;
        roleIcon = FindImage(root, SelectedRolePath + "Role Icon", SelectedRolePath + "Melee Icon");
        titleText = FindText(root, SelectedRolePath + "MELEE Text");
        statusText = FindDirectTextContaining(panelTransform, "LOCKED");
        tierText = FindDirectTextContaining(panelTransform, "STARS");
        descriptionText = FindText(
            root,
            SelectedRolePath + "Unlock to begin recurring melee production. Upgrades affect future spawns only. Text");

        Transform actionTransform = root.Find(SelectedRolePath + "Role Action");
        actionButton = actionTransform != null ? actionTransform.GetComponent<Button>() : null;
        actionText = actionTransform != null
            ? actionTransform.GetComponentInChildren<TextMeshProUGUI>(true)
            : null;

        if (actionButton != null)
        {
            if (actionButton.targetGraphic != null)
            {
                actionButton.targetGraphic.raycastTarget = true;
            }

            actionButton.onClick.AddListener(PurchaseSelectedRole);
        }
    }

    public RectTransform Panel { get; }
    public TMP_FontAsset Font => titleText != null ? titleText.font : null;
    public UnitRole SelectedRole => selectedRole;

    public void Select(UnitRole role, GameManager gameManager, WorkerManager workerManager)
    {
        selectedRole = role;
        Refresh(gameManager, workerManager);
    }

    public void Refresh(GameManager gameManager, WorkerManager workerManager)
    {
        if (gameManager == null || workerManager == null) return;

        int tier = gameManager.GetProductionTier(playerTeam, selectedRole);
        bool hasData = gameManager.TryGetProductionData(playerTeam, selectedRole, out UnitData data);
        int cost = hasData ? data.Cost : 0;
        BaseUnit selectedMythic = selectedRole == UnitRole.Mythic
            ? gameManager.GetSelectedMythicUnit(playerTeam)
            : null;
        string roleName = selectedMythic != null
            ? GetDisplayName(selectedMythic).ToUpperInvariant()
            : selectedRole.ToString().ToUpperInvariant();

        RefreshIcon(gameManager, selectedMythic);

        if (titleText != null)
        {
            titleText.text = roleName;
        }

        if (statusText != null)
        {
            statusText.text = tier == 0 ? "LOCKED" : "PRODUCING";
        }

        if (tierText != null)
        {
            tierText.text = $"{tier} / {GameManager.MaximumProductionTier} STARS";
        }

        if (descriptionText != null)
        {
            descriptionText.text = selectedRole == UnitRole.Mythic && tier == 0
                ? "Choose a mythic unit before spending gold. Its own cost and production cadence will apply for this match."
                : tier == 0
                ? $"Unlock to begin recurring {roleName.ToLowerInvariant()} production. Upgrades affect future spawns only."
                : tier < GameManager.MaximumProductionTier
                    ? $"Producing {tier}-star {roleName.ToLowerInvariant()} units. Upgrade affects future spawns only."
                    : $"Producing maximum-tier {roleName.ToLowerInvariant()} units.";
        }

        if (actionText != null)
        {
            actionText.text = tier >= GameManager.MaximumProductionTier
                ? "MAXIMUM TIER"
                : selectedRole == UnitRole.Mythic && tier == 0 ? "SELECT UNIT"
                : tier == 0 ? $"UNLOCK   {cost} GOLD" : $"UPGRADE   {cost} GOLD";
        }

        if (actionButton != null)
        {
            actionButton.interactable = selectedRole == UnitRole.Mythic && tier == 0
                ? gameManager.HasMythicChoices(playerTeam)
                : hasData && tier < GameManager.MaximumProductionTier &&
                    workerManager.CurrentGold >= cost && !gameManager.IsGameOver;
        }
    }

    public void Dispose()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(PurchaseSelectedRole);
        }
    }

    private void PurchaseSelectedRole()
    {
        purchaseRequested?.Invoke(selectedRole);
    }

    private void RefreshIcon(GameManager gameManager, BaseUnit selectedMythic)
    {
        if (roleIcon == null) return;

        if (selectedRole == UnitRole.Mythic)
        {
            UpdateMythicArt(roleIcon, selectedMythic, gameManager);
            roleIcon.color = selectedMythic == null ? Color.clear : Color.white;
            SetCrossedSwordsColour(
                roleIcon,
                selectedMythic == null ? new Color(0.35f, 0.35f, 0.35f, 1f) : Color.white);
            return;
        }

        HideCrossedSwords(roleIcon);
        Image selectedArt = cardPresenter?.GetArt(selectedRole);
        if (selectedArt == null) return;

        roleIcon.sprite = selectedArt.sprite;
        roleIcon.color = selectedArt.color;
        roleIcon.preserveAspect = true;
    }

    private void UpdateMythicArt(Image art, BaseUnit selectedMythic, GameManager gameManager)
    {
        if (gameManager?.MythicUnitRoster == null) return;

        Transform crossedSwords = art.transform.Find("Crossed Swords");
        if (selectedMythic != null)
        {
            if (crossedSwords != null)
            {
                crossedSwords.gameObject.SetActive(false);
            }

            art.sprite = gameManager.MythicUnitRoster.GetAvatar(selectedMythic) ??
                GetUnitSprite(selectedMythic);
            art.preserveAspect = true;
            return;
        }

        art.sprite = null;
        if (crossedSwords == null)
        {
            crossedSwords = CreateUIObject("Crossed Swords", art.transform);
            RectTransform crossedRect = (RectTransform)crossedSwords;
            crossedRect.anchorMin = Vector2.zero;
            crossedRect.anchorMax = Vector2.one;
            crossedRect.offsetMin = Vector2.zero;
            crossedRect.offsetMax = Vector2.zero;

            CreateParchmentImage(crossedSwords);
            CreateSwordImage(crossedSwords, "Sword A", 0f, gameManager);
            CreateSwordImage(crossedSwords, "Sword B", 90f, gameManager);
        }

        crossedSwords.gameObject.SetActive(true);
    }

    private void CreateParchmentImage(Transform parent)
    {
        RectTransform parchmentRect = CreateUIObject("Parchment", parent);
        parchmentRect.anchorMin = Vector2.zero;
        parchmentRect.anchorMax = Vector2.one;
        parchmentRect.offsetMin = Vector2.zero;
        parchmentRect.offsetMax = Vector2.zero;
        Image parchment = parchmentRect.gameObject.AddComponent<Image>();
        Image mythicArt = cardPresenter?.GetArt(UnitRole.Mythic);
        Transform authoredParchment = mythicArt != null && mythicArt.transform.parent != null
            ? mythicArt.transform.parent.Find("Portrait Paper")
            : null;
        parchment.sprite = authoredParchment != null &&
            authoredParchment.TryGetComponent(out Image image)
                ? image.sprite
                : null;
        parchment.color = Color.white;
        parchment.preserveAspect = true;
        parchment.raycastTarget = false;
    }

    private static void CreateSwordImage(
        Transform parent,
        string objectName,
        float rotation,
        GameManager gameManager)
    {
        RectTransform swordRect = CreateUIObject(objectName, parent);
        swordRect.anchorMin = new Vector2(0.5f, 0.5f);
        swordRect.anchorMax = new Vector2(0.5f, 0.5f);
        swordRect.sizeDelta = new Vector2(78f, 78f);
        swordRect.anchoredPosition = Vector2.zero;
        swordRect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        Image sword = swordRect.gameObject.AddComponent<Image>();
        sword.sprite = gameManager.MythicUnitRoster.DefaultIcon;
        sword.preserveAspect = true;
        sword.raycastTarget = false;
    }

    private static Image FindImage(Transform root, params string[] paths)
    {
        foreach (string path in paths)
        {
            Transform target = root.Find(path);
            if (target != null && target.TryGetComponent(out Image image))
            {
                return image;
            }
        }

        return null;
    }

    private static TextMeshProUGUI FindText(Transform root, params string[] paths)
    {
        foreach (string path in paths)
        {
            Transform target = root.Find(path);
            if (target != null && target.TryGetComponent(out TextMeshProUGUI text))
            {
                return text;
            }
        }

        return null;
    }

    private static TextMeshProUGUI FindDirectTextContaining(Transform parent, string content)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).TryGetComponent(out TextMeshProUGUI text) &&
                text.text.Contains(content))
            {
                return text;
            }
        }

        return null;
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

    private static void SetCrossedSwordsColour(Image art, Color colour)
    {
        Transform crossedSwords = art.transform.Find("Crossed Swords");
        if (crossedSwords == null) return;

        for (int i = 0; i < crossedSwords.childCount; i++)
        {
            Transform child = crossedSwords.GetChild(i);
            if (child.name.StartsWith("Sword") && child.TryGetComponent(out Image sword))
            {
                sword.color = colour;
            }
        }
    }

    private static void HideCrossedSwords(Image art)
    {
        Transform crossedSwords = art.transform.Find("Crossed Swords");
        if (crossedSwords != null)
        {
            crossedSwords.gameObject.SetActive(false);
        }
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
