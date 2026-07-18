using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ProductionCardPresenter
{
    private static readonly UnitRole[] Roles =
    {
        UnitRole.Melee,
        UnitRole.Archer,
        UnitRole.Cavalry,
        UnitRole.Siege,
        UnitRole.Mythic
    };

    private sealed class CardBinding
    {
        public UnitRole Role;
        public ProductionCardView View;
        public Button Button;
        public Image Art;
        public TextMeshProUGUI StatusText;
        public TextMeshProUGUI TierText;
        public TextMeshProUGUI ActionText;
        public Color UnlockedColour;
        public UnityAction PurchaseAction;
        public EventTrigger InteractionTrigger;
        public EventTrigger.Entry HoverEntry;
        public EventTrigger.Entry PointerDownEntry;
    }

    private readonly Transform root;
    private readonly Team playerTeam;
    private readonly Action<UnitRole> purchaseRequested;
    private readonly Action<UnitRole> roleSelected;
    private readonly CardBinding[] bindings = new CardBinding[Roles.Length];

    public ProductionCardPresenter(
        Transform root,
        Team playerTeam,
        Action<UnitRole> purchaseRequested,
        Action<UnitRole> roleSelected)
    {
        this.root = root;
        this.playerTeam = playerTeam;
        this.purchaseRequested = purchaseRequested;
        this.roleSelected = roleSelected;

        ResolveBindings();
        BindInteractions();
    }

    public Image GetArt(UnitRole role)
    {
        CardBinding binding = GetBinding(role);
        return binding?.Art;
    }

    public void Refresh(GameManager gameManager, WorkerManager workerManager)
    {
        if (gameManager == null || workerManager == null) return;

        foreach (CardBinding binding in bindings)
        {
            if (binding == null) continue;

            int tier = gameManager.GetProductionTier(playerTeam, binding.Role);
            bool hasData = gameManager.TryGetProductionData(playerTeam, binding.Role, out UnitData data);
            int cost = hasData ? data.Cost : 0;
            bool needsMythicChoice = binding.Role == UnitRole.Mythic && tier == 0;
            bool canPurchase = needsMythicChoice
                ? gameManager.HasMythicChoices(playerTeam)
                : hasData && tier < GameManager.MaximumProductionTier &&
                    workerManager.CurrentGold >= cost && !gameManager.IsGameOver;

            RefreshArt(binding, tier, gameManager);

            if (binding.StatusText != null)
            {
                binding.StatusText.text = tier == 0 ? "LOCKED" : "PRODUCING";
            }

            if (binding.TierText != null)
            {
                binding.TierText.text = $"{tier} / {GameManager.MaximumProductionTier} STARS";
            }

            if (binding.ActionText != null)
            {
                binding.ActionText.enableAutoSizing = true;
                binding.ActionText.fontSizeMin = 11f;
                binding.ActionText.fontSizeMax = 18f;
                binding.ActionText.alignment = TextAlignmentOptions.Center;
                binding.ActionText.text = tier >= GameManager.MaximumProductionTier
                    ? "MAX"
                    : needsMythicChoice ? "CHOOSE"
                    : tier == 0 ? $"UNLOCK {cost}" : $"UPGRADE {cost}";
            }

            if (binding.Button != null)
            {
                binding.Button.interactable = canPurchase;
            }
        }
    }

    public void Dispose()
    {
        foreach (CardBinding binding in bindings)
        {
            if (binding?.Button != null && binding.PurchaseAction != null)
            {
                binding.Button.onClick.RemoveListener(binding.PurchaseAction);
            }

            if (binding?.InteractionTrigger == null) continue;

            if (binding.HoverEntry != null)
            {
                binding.InteractionTrigger.triggers.Remove(binding.HoverEntry);
            }

            if (binding.PointerDownEntry != null)
            {
                binding.InteractionTrigger.triggers.Remove(binding.PointerDownEntry);
            }
        }
    }

    private void ResolveBindings()
    {
        ProductionCardView[] views = root.GetComponentsInChildren<ProductionCardView>(true);
        foreach (ProductionCardView view in views)
        {
            int index = Array.IndexOf(Roles, view.Role);
            if (index < 0 || bindings[index] != null || !view.HasCompleteBindings) continue;
            Image art = view.Artwork;
            bindings[index] = new CardBinding
            {
                Role = view.Role,
                View = view,
                Button = view.PurchaseButton,
                Art = art,
                StatusText = view.StatusText,
                TierText = view.TierText,
                ActionText = view.ActionText,
                UnlockedColour = art != null ? art.color : Color.white
            };
        }
    }

    private void BindInteractions()
    {
        foreach (CardBinding binding in bindings)
        {
            if (binding == null) continue;

            BindCardSelection(binding);
            if (binding.Button == null) continue;

            if (binding.Button.targetGraphic != null)
            {
                binding.Button.targetGraphic.raycastTarget = true;
            }

            UnitRole role = binding.Role;
            binding.PurchaseAction = () => purchaseRequested?.Invoke(role);
            binding.Button.onClick.AddListener(binding.PurchaseAction);
        }
    }

    private void BindCardSelection(CardBinding binding)
    {
        if (binding.View == null) return;
        Graphic cardGraphic = binding.View.InteractionGraphic;
        cardGraphic.raycastTarget = true;

        binding.InteractionTrigger = binding.View.GetComponent<EventTrigger>();
        if (binding.InteractionTrigger == null)
        {
            binding.InteractionTrigger = binding.View.gameObject.AddComponent<EventTrigger>();
        }

        UnitRole role = binding.Role;
        binding.HoverEntry = CreateInteractionEntry(
            EventTriggerType.PointerEnter,
            _ => roleSelected?.Invoke(role));
        binding.PointerDownEntry = CreateInteractionEntry(
            EventTriggerType.PointerDown,
            _ => roleSelected?.Invoke(role));
        binding.InteractionTrigger.triggers.Add(binding.HoverEntry);
        binding.InteractionTrigger.triggers.Add(binding.PointerDownEntry);
    }

    private void RefreshArt(CardBinding binding, int tier, GameManager gameManager)
    {
        if (binding.Art == null) return;

        if (binding.Role == UnitRole.Mythic && gameManager.MythicUnitRoster != null)
        {
            MythicArtworkPresenter.Update(
                binding.Art,
                gameManager.GetSelectedMythicUnit(playerTeam),
                gameManager.MythicUnitRoster,
                GetMythicParchmentSprite());
        }

        bool lockedMythic = binding.Role == UnitRole.Mythic && tier == 0;
        binding.Art.color = lockedMythic
            ? Color.clear
            : tier == 0
                ? new Color(0.35f, 0.35f, 0.35f, binding.UnlockedColour.a)
                : binding.UnlockedColour;

        MythicArtworkPresenter.SetCrossedSwordsColour(
            binding.Art,
            lockedMythic ? new Color(0.35f, 0.35f, 0.35f, 1f) : binding.Art.color);
    }

    private Sprite GetMythicParchmentSprite()
    {
        CardBinding mythic = GetBinding(UnitRole.Mythic);
        return mythic?.View != null ? mythic.View.PortraitPaperSprite : null;
    }

    private static EventTrigger.Entry CreateInteractionEntry(
        EventTriggerType eventType,
        UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        return entry;
    }

    private CardBinding GetBinding(UnitRole role)
    {
        foreach (CardBinding binding in bindings)
        {
            if (binding != null && binding.Role == role)
            {
                return binding;
            }
        }

        return null;
    }

}
