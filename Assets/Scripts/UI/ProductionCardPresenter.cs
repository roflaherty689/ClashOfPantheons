using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ProductionCardPresenter
{
    private static readonly ProductionSlotId[] Slots =
    {
        ProductionSlotId.Standard0,
        ProductionSlotId.Standard1,
        ProductionSlotId.Standard2,
        ProductionSlotId.Standard3,
        ProductionSlotId.Standard4,
        ProductionSlotId.Mythic
    };

    private sealed class CardBinding
    {
        public ProductionSlotId SlotId;
        public ProductionCardView View;
        public Button Button;
        public Image Art;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI StatusText;
        public TextMeshProUGUI TierText;
        public TextMeshProUGUI ActionText;
        public BaseUnit PresentedPrefab;
        public UnitRole PresentedRole;
        public bool HasPresentedRole;
        public Color UnlockedColour;
        public UnityAction PurchaseAction;
        public EventTrigger InteractionTrigger;
        public EventTrigger.Entry HoverEntry;
        public EventTrigger.Entry PointerDownEntry;
    }

    private readonly Transform root;
    private readonly Team playerTeam;
    private readonly Action<ProductionSlotId> purchaseRequested;
    private readonly Action<ProductionSlotId> slotSelected;
    private readonly CardBinding[] bindings = new CardBinding[Slots.Length];

    public ProductionCardPresenter(
        Transform root,
        Team playerTeam,
        Action<ProductionSlotId> purchaseRequested,
        Action<ProductionSlotId> slotSelected)
    {
        this.root = root;
        this.playerTeam = playerTeam;
        this.purchaseRequested = purchaseRequested;
        this.slotSelected = slotSelected;

        ResolveBindings();
        BindInteractions();
    }

    public Image GetArt(ProductionSlotId slotId)
    {
        CardBinding binding = GetBinding(slotId);
        return binding?.Art;
    }

    public void Refresh(GameManager gameManager, WorkerManager workerManager)
    {
        if (gameManager == null || workerManager == null) return;

        foreach (CardBinding binding in bindings)
        {
            if (binding == null) continue;

            int tier = gameManager.GetProductionTier(playerTeam, binding.SlotId);
            bool hasData = gameManager.TryGetProductionData(playerTeam, binding.SlotId, out UnitData data);
            int cost = hasData ? data.Cost : 0;
            bool needsMythicChoice = binding.SlotId == ProductionSlotId.Mythic && tier == 0;
            bool canPurchase = needsMythicChoice
                ? gameManager.HasMythicChoices(playerTeam)
                : hasData && tier < GameManager.MaximumProductionTier &&
                    workerManager.CurrentGold >= cost && !gameManager.IsGameOver;

            RefreshArt(binding, tier, gameManager);

            if (binding.TitleText != null &&
                binding.SlotId == ProductionSlotId.Standard4)
            {
                binding.TitleText.text = "MONK";
            }
            else if (binding.TitleText != null &&
                     gameManager.TryGetProductionRole(
                         playerTeam,
                         binding.SlotId,
                         out UnitRole role) &&
                     (!binding.HasPresentedRole || binding.PresentedRole != role))
            {
                binding.PresentedRole = role;
                binding.HasPresentedRole = true;
                binding.TitleText.text = role.ToString().ToUpperInvariant();
            }

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
            int index = Array.IndexOf(Slots, view.SlotId);
            if (index < 0 || bindings[index] != null || !view.HasCompleteBindings) continue;
            Image art = view.Artwork;
            bindings[index] = new CardBinding
            {
                SlotId = view.SlotId,
                View = view,
                Button = view.PurchaseButton,
                Art = art,
                TitleText = view.TitleText,
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

            ProductionSlotId slotId = binding.SlotId;
            binding.PurchaseAction = () => purchaseRequested?.Invoke(slotId);
            binding.Button.onClick.AddListener(binding.PurchaseAction);
            SoundManager.SuppressGenericClick(binding.Button);
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

        ProductionSlotId slotId = binding.SlotId;
        binding.HoverEntry = CreateInteractionEntry(
            EventTriggerType.PointerEnter,
            _ => slotSelected?.Invoke(slotId));
        binding.PointerDownEntry = CreateInteractionEntry(
            EventTriggerType.PointerDown,
            _ => slotSelected?.Invoke(slotId));
        binding.InteractionTrigger.triggers.Add(binding.HoverEntry);
        binding.InteractionTrigger.triggers.Add(binding.PointerDownEntry);
    }

    private void RefreshArt(CardBinding binding, int tier, GameManager gameManager)
    {
        if (binding.Art == null) return;

        if (binding.SlotId == ProductionSlotId.Mythic && gameManager.MythicUnitRoster != null)
        {
            MythicArtworkPresenter.Update(
                binding.Art,
                gameManager.GetSelectedMythicUnit(playerTeam),
                gameManager.MythicUnitRoster,
                GetMythicParchmentSprite());
        }
        else if (gameManager.TryGetProductionPrefab(
                     playerTeam,
                     binding.SlotId,
                     out BaseUnit prefab) &&
                 binding.PresentedPrefab != prefab)
        {
            binding.PresentedPrefab = prefab;
            MythicArtworkPresenter.HideCrossedSwords(binding.Art);
            Sprite sprite = MythicArtworkPresenter.GetUnitSprite(prefab);
            if (sprite != null)
            {
                binding.Art.sprite = sprite;
                binding.Art.preserveAspect = true;
            }
        }

        bool lockedMythic = binding.SlotId == ProductionSlotId.Mythic && tier == 0;
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
        CardBinding mythic = GetBinding(ProductionSlotId.Mythic);
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

    private CardBinding GetBinding(ProductionSlotId slotId)
    {
        foreach (CardBinding binding in bindings)
        {
            if (binding != null && binding.SlotId == slotId)
            {
                return binding;
            }
        }

        return null;
    }

}
