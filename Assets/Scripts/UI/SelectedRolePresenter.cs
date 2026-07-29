using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SelectedRolePresenter
{
    private const string SelectedRolePath = "Safe Area/Bottom HUD/Selected Role/";

    private readonly Team playerTeam;
    private readonly ProductionCardPresenter cardPresenter;
    private readonly Action<ProductionSlotId> purchaseRequested;
    private readonly Image roleIcon;
    private readonly TextMeshProUGUI titleText;
    private readonly TextMeshProUGUI statusText;
    private readonly TextMeshProUGUI tierText;
    private readonly TextMeshProUGUI descriptionText;
    private readonly TextMeshProUGUI actionText;
    private readonly Button actionButton;

    private ProductionSlotId selectedSlot = ProductionSlotId.Standard0;

    public SelectedRolePresenter(
        Transform root,
        Team playerTeam,
        ProductionCardPresenter cardPresenter,
        Action<ProductionSlotId> purchaseRequested)
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
            SoundManager.SuppressGenericClick(actionButton);
        }
    }

    public RectTransform Panel { get; }
    public TMP_FontAsset Font => titleText != null ? titleText.font : null;
    public ProductionSlotId SelectedSlot => selectedSlot;

    public void Select(
        ProductionSlotId slotId,
        GameManager gameManager,
        WorkerManager workerManager)
    {
        selectedSlot = slotId;
        Refresh(gameManager, workerManager);
    }

    public void Refresh(GameManager gameManager, WorkerManager workerManager)
    {
        if (gameManager == null || workerManager == null) return;

        int tier = gameManager.GetProductionTier(playerTeam, selectedSlot);
        bool hasData = gameManager.TryGetProductionData(playerTeam, selectedSlot, out UnitData data);
        int cost = hasData ? data.Cost : 0;
        BaseUnit selectedMythic = selectedSlot == ProductionSlotId.Mythic
            ? gameManager.GetSelectedMythicUnit(playerTeam)
            : null;
        gameManager.TryGetProductionRole(playerTeam, selectedSlot, out UnitRole role);
        BaseUnit standardPrefab = null;
        if (selectedSlot != ProductionSlotId.Mythic)
        {
            gameManager.TryGetProductionPrefab(playerTeam, selectedSlot, out standardPrefab);
        }

        string roleName = selectedMythic != null
            ? MythicArtworkPresenter.GetDisplayName(selectedMythic).ToUpperInvariant()
            : selectedSlot == ProductionSlotId.Standard4
                ? "MONK"
                : role == UnitRole.Mythic && standardPrefab != null
                    ? MythicArtworkPresenter.GetDisplayName(standardPrefab).ToUpperInvariant()
                : role.ToString().ToUpperInvariant();

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
            descriptionText.text = selectedSlot == ProductionSlotId.Mythic && tier == 0
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
                : selectedSlot == ProductionSlotId.Mythic && tier == 0 ? "SELECT UNIT"
                : tier == 0 ? $"UNLOCK   {cost} GOLD" : $"UPGRADE   {cost} GOLD";
        }

        if (actionButton != null)
        {
            actionButton.interactable = selectedSlot == ProductionSlotId.Mythic && tier == 0
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
        purchaseRequested?.Invoke(selectedSlot);
    }

    private void RefreshIcon(GameManager gameManager, BaseUnit selectedMythic)
    {
        if (roleIcon == null) return;

        if (selectedSlot == ProductionSlotId.Mythic)
        {
            MythicArtworkPresenter.Update(
                roleIcon,
                selectedMythic,
                gameManager.MythicUnitRoster,
                GetMythicParchmentSprite());
            roleIcon.color = selectedMythic == null ? Color.clear : Color.white;
            MythicArtworkPresenter.SetCrossedSwordsColour(
                roleIcon,
                selectedMythic == null ? new Color(0.35f, 0.35f, 0.35f, 1f) : Color.white);
            return;
        }

        MythicArtworkPresenter.HideCrossedSwords(roleIcon);
        Image selectedArt = cardPresenter?.GetArt(selectedSlot);
        if (selectedArt == null) return;

        roleIcon.sprite = selectedArt.sprite;
        roleIcon.color = selectedArt.color;
        roleIcon.preserveAspect = true;
    }

    private Sprite GetMythicParchmentSprite()
    {
        Image mythicArt = cardPresenter?.GetArt(ProductionSlotId.Mythic);
        Transform authoredParchment = mythicArt != null && mythicArt.transform.parent != null
            ? mythicArt.transform.parent.Find("Portrait Paper")
            : null;
        return authoredParchment != null && authoredParchment.TryGetComponent(out Image image)
            ? image.sprite
            : null;
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

}
