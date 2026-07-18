using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class BattleEconomyUI : MonoBehaviour
{
    private const string EconomyPath = "Safe Area/Bottom HUD/Economy/";
    private const string PlayerHealthPath = "Safe Area/Top HUD/Player Stronghold/Health Bar/";
    private const string EnemyHealthPath = "Safe Area/Top HUD/Enemy Stronghold/Health Bar/";
    private const string ResultPath = "Safe Area/Game Over Overlay/Result Panel/";
    private const string ProductionPath = "Safe Area/Bottom HUD/Independent Production/";
    private const string SelectedRolePath = "Safe Area/Bottom HUD/Selected Role/";

    private static readonly UnitRole[] ProductionRoles =
    {
        UnitRole.Melee,
        UnitRole.Archer,
        UnitRole.Cavalry,
        UnitRole.Siege,
        UnitRole.Mythic
    };

    private sealed class ProductionUIBinding
    {
        public UnitRole Role;
        public Transform Card;
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

    [SerializeField] private Team playerTeam = Team.Left;

    private WorkerManager workerManager;
    private GameManager gameManager;
    private Base playerBase;
    private Base enemyBase;
    private TextMeshProUGUI goldTotalText;
    private TextMeshProUGUI goldPerTripText;
    private TextMeshProUGUI workerTotalText;
    private TextMeshProUGUI battleSummaryText;
    private Button buyWorkerButton;
    private TextMeshProUGUI playerHealthText;
    private TextMeshProUGUI enemyHealthText;
    private Image playerHealthFill;
    private Image enemyHealthFill;
    private TextMeshProUGUI timerText;
    private CanvasGroup gameOverOverlay;
    private TextMeshProUGUI resultTitleText;
    private TextMeshProUGUI resultReasonText;
    private Button restartButton;
    private readonly ProductionUIBinding[] productionBindings = new ProductionUIBinding[ProductionRoles.Length];
    private UnitRole selectedRole = UnitRole.Melee;
    private Button selectedRoleButton;
    private Image selectedRoleIcon;
    private TextMeshProUGUI selectedRoleTitleText;
    private TextMeshProUGUI selectedRoleStatusText;
    private TextMeshProUGUI selectedRoleTierText;
    private TextMeshProUGUI selectedRoleDescriptionText;
    private TextMeshProUGUI selectedRoleActionText;
    private RectTransform selectedRolePanel;
    private MythicPickerController mythicPickerController;
    private string battleSummarySuffix = string.Empty;

    private void Awake()
    {
        ResolveGameManager();
        ResolveWorkerManager();
        ResolveBases();
        ResolveUI();
        mythicPickerController = new MythicPickerController(
            selectedRolePanel,
            selectedRoleTitleText != null ? selectedRoleTitleText.font : null,
            playerTeam,
            Refresh);

        if (buyWorkerButton != null)
        {
            if (buyWorkerButton.targetGraphic != null)
            {
                buyWorkerButton.targetGraphic.raycastTarget = true;
            }

            buyWorkerButton.onClick.AddListener(BuyWorker);
        }

        if (restartButton != null)
        {
            if (restartButton.targetGraphic != null)
            {
                restartButton.targetGraphic.raycastTarget = true;
            }

            restartButton.onClick.AddListener(RestartMatch);
        }

        BindProductionButtons();

        SetResultOverlayVisible(false);

        Refresh();
    }

    private void LateUpdate()
    {
        if (gameManager == null)
        {
            ResolveGameManager();
        }

        if (workerManager == null)
        {
            ResolveWorkerManager();
        }

        if (playerBase == null || enemyBase == null)
        {
            ResolveBases();
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (buyWorkerButton != null)
        {
            buyWorkerButton.onClick.RemoveListener(BuyWorker);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartMatch);
        }


        foreach (ProductionUIBinding binding in productionBindings)
        {
            if (binding?.Button != null && binding.PurchaseAction != null)
            {
                binding.Button.onClick.RemoveListener(binding.PurchaseAction);
            }

            if (binding?.InteractionTrigger != null)
            {
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

        if (selectedRoleButton != null)
        {
            selectedRoleButton.onClick.RemoveListener(PurchaseSelectedRole);
        }

        mythicPickerController?.Close();
    }

    private void ResolveGameManager()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void ResolveWorkerManager()
    {
        WorkerManager[] managers = FindObjectsByType<WorkerManager>();
        foreach (WorkerManager manager in managers)
        {
            if (manager.Team == playerTeam)
            {
                workerManager = manager;
                return;
            }
        }
    }

    private void ResolveBases()
    {
        playerBase = null;
        enemyBase = null;

        Base[] bases = FindObjectsByType<Base>();
        foreach (Base battleBase in bases)
        {
            if (battleBase.Team == playerTeam)
            {
                playerBase = battleBase;
            }
            else
            {
                enemyBase = battleBase;
            }
        }
    }

    private void ResolveUI()
    {
        goldTotalText = FindText(EconomyPath + "Gold Total", EconomyPath + "540 Text");
        goldPerTripText = FindText(EconomyPath + "Gold Per Trip", EconomyPath + "+12 PER TRIP Text");
        workerTotalText = FindText(EconomyPath + "Worker Total", EconomyPath + "2 / 5 Text");
        battleSummaryText = FindText(
            "Safe Area/Bottom HUD/Battle Summary/Worker Battle Summary",
            "Safe Area/Bottom HUD/Battle Summary/WORKERS  2 / 5     |     FRIENDLY UNITS  8     |     ENEMY UNITS  8 Text");
        playerHealthText = FindText(PlayerHealthPath + "Stronghold Health Total", PlayerHealthPath + "50 / 50 Text");
        enemyHealthText = FindText(EnemyHealthPath + "Stronghold Health Total", EnemyHealthPath + "50 / 50 Text");
        playerHealthFill = FindImage(PlayerHealthPath + "Health Fill");
        enemyHealthFill = FindImage(EnemyHealthPath + "Health Fill");
        timerText = FindText(
            "Safe Area/Top HUD/Match Timer/Time Remaining",
            "Safe Area/Top HUD/Match Timer/03:00 Text");
        resultTitleText = FindText(ResultPath + "VICTORY Text");
        resultReasonText = FindText(ResultPath + "ENEMY STRONGHOLD DESTROYED Text");

        Transform overlayTransform = transform.Find("Safe Area/Game Over Overlay");
        gameOverOverlay = overlayTransform != null
            ? overlayTransform.GetComponent<CanvasGroup>()
            : null;

        Transform buttonTransform = transform.Find(EconomyPath + "Buy Worker");
        buyWorkerButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;

        Transform restartTransform = transform.Find(ResultPath + "Restart Match");
        restartButton = restartTransform != null ? restartTransform.GetComponent<Button>() : null;

        ResolveProductionUI();

        if (battleSummaryText != null)
        {
            int separatorIndex = battleSummaryText.text.IndexOf('|');
            battleSummarySuffix = separatorIndex >= 0
                ? "     " + battleSummaryText.text.Substring(separatorIndex)
                : string.Empty;
        }
    }

    private void ResolveProductionUI()
    {
        for (int i = 0; i < ProductionRoles.Length; i++)
        {
            UnitRole role = ProductionRoles[i];
            string roleName = role.ToString().ToUpperInvariant();
            string cardPath = ProductionPath + roleName + " Production/";
            Transform cardTransform = transform.Find(cardPath.TrimEnd('/'));
            Transform buttonTransform = transform.Find(cardPath + "Unlock " + roleName);
            Transform artTransform = transform.Find(cardPath + roleName + " Art");
            Image art = artTransform != null ? artTransform.GetComponent<Image>() : null;

            productionBindings[i] = new ProductionUIBinding
            {
                Role = role,
                Card = cardTransform,
                Button = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null,
                Art = art,
                StatusText = FindDirectTextContaining(cardTransform, "LOCKED"),
                TierText = FindDirectTextContaining(cardTransform, "STARS"),
                ActionText = buttonTransform != null
                    ? buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true)
                    : null,
                UnlockedColour = art != null ? art.color : Color.white
            };
        }

        Transform selectedRoleTransform = transform.Find(SelectedRolePath.TrimEnd('/'));
        selectedRolePanel = selectedRoleTransform as RectTransform;
        selectedRoleIcon = FindImage(SelectedRolePath + "Role Icon", SelectedRolePath + "Melee Icon");
        selectedRoleTitleText = FindText(SelectedRolePath + "MELEE Text");
        selectedRoleStatusText = FindDirectTextContaining(selectedRoleTransform, "LOCKED");
        selectedRoleTierText = FindDirectTextContaining(selectedRoleTransform, "STARS");
        selectedRoleDescriptionText = FindText(
            SelectedRolePath + "Unlock to begin recurring melee production. Upgrades affect future spawns only. Text");

        Transform actionTransform = transform.Find(SelectedRolePath + "Role Action");
        selectedRoleButton = actionTransform != null ? actionTransform.GetComponent<Button>() : null;
        selectedRoleActionText = actionTransform != null
            ? actionTransform.GetComponentInChildren<TextMeshProUGUI>(true)
            : null;
    }

    private void BindProductionButtons()
    {
        foreach (ProductionUIBinding binding in productionBindings)
        {
            if (binding == null) continue;

            BindProductionCardInteraction(binding);

            if (binding.Button == null) continue;

            if (binding.Button.targetGraphic != null)
            {
                binding.Button.targetGraphic.raycastTarget = true;
            }

            UnitRole role = binding.Role;
            binding.PurchaseAction = () => PurchaseProduction(role);
            binding.Button.onClick.AddListener(binding.PurchaseAction);
        }

        if (selectedRoleButton != null)
        {
            if (selectedRoleButton.targetGraphic != null)
            {
                selectedRoleButton.targetGraphic.raycastTarget = true;
            }

            selectedRoleButton.onClick.AddListener(PurchaseSelectedRole);
        }
    }

    private void BindProductionCardInteraction(ProductionUIBinding binding)
    {
        if (binding.Card == null) return;

        if (binding.Card.TryGetComponent(out Graphic cardGraphic))
        {
            cardGraphic.raycastTarget = true;
        }

        binding.InteractionTrigger = binding.Card.GetComponent<EventTrigger>();
        if (binding.InteractionTrigger == null)
        {
            binding.InteractionTrigger = binding.Card.gameObject.AddComponent<EventTrigger>();
        }

        UnitRole role = binding.Role;
        binding.HoverEntry = CreateInteractionEntry(
            EventTriggerType.PointerEnter,
            _ => SelectProductionRole(role));
        binding.PointerDownEntry = CreateInteractionEntry(
            EventTriggerType.PointerDown,
            _ => SelectProductionRole(role));
        binding.InteractionTrigger.triggers.Add(binding.HoverEntry);
        binding.InteractionTrigger.triggers.Add(binding.PointerDownEntry);
    }

    private static EventTrigger.Entry CreateInteractionEntry(
        EventTriggerType eventType,
        UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };
        entry.callback.AddListener(action);
        return entry;
    }

    private Image FindImage(params string[] paths)
    {
        foreach (string path in paths)
        {
            Transform target = transform.Find(path);
            if (target != null && target.TryGetComponent(out Image image))
            {
                return image;
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

    private TextMeshProUGUI FindText(params string[] paths)
    {
        foreach (string path in paths)
        {
            Transform target = transform.Find(path);
            if (target != null && target.TryGetComponent(out TextMeshProUGUI text))
            {
                return text;
            }
        }

        return null;
    }

    private void BuyWorker()
    {
        if (workerManager != null)
        {
            workerManager.TryBuyWorker();
            Refresh();
        }
    }

    private void PurchaseProduction(UnitRole role)
    {
        SelectProductionRole(role);

        if (role == UnitRole.Mythic && gameManager != null &&
            gameManager.GetProductionTier(playerTeam, UnitRole.Mythic) == 0)
        {
            mythicPickerController?.Open(gameManager, workerManager);
            return;
        }

        gameManager?.TryPurchaseProduction(playerTeam, role, workerManager);
        Refresh();
    }

    private void SelectProductionRole(UnitRole role)
    {
        if (role != UnitRole.Mythic)
        {
            mythicPickerController?.Close();
        }

        selectedRole = role;

        if (gameManager != null && workerManager != null)
        {
            RefreshSelectedRole();
        }
    }

    private void PurchaseSelectedRole()
    {
        PurchaseProduction(selectedRole);
    }

    private void RestartMatch()
    {
        gameManager?.RestartMatch();
    }

    private void Refresh()
    {
        if (workerManager != null && goldTotalText != null)
        {
            goldTotalText.text = workerManager.CurrentGold.ToString();
        }

        if (workerManager != null && goldPerTripText != null)
        {
            goldPerTripText.text = $"+{workerManager.TotalGoldPerTrip} PER TRIP";
        }

        if (workerManager != null)
        {
            string workerTotal = $"{workerManager.WorkerCount} / {workerManager.MaxWorkers}";
            if (workerTotalText != null)
            {
                workerTotalText.text = workerTotal;
            }

            if (battleSummaryText != null)
            {
                battleSummaryText.text = $"WORKERS  {workerTotal}{battleSummarySuffix}";
            }

            if (buyWorkerButton != null)
            {
                buyWorkerButton.interactable = workerManager.HasWorkerCapacity &&
                    workerManager.CurrentGold >= workerManager.WorkerCost &&
                    (gameManager == null || !gameManager.IsGameOver);
            }
        }

        RefreshProduction();

        RefreshBaseHealth(playerBase, playerHealthText, playerHealthFill);
        RefreshBaseHealth(enemyBase, enemyHealthText, enemyHealthFill);
        RefreshMatchState();
    }

    private void RefreshProduction()
    {
        if (gameManager == null || workerManager == null) return;

        foreach (ProductionUIBinding binding in productionBindings)
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

            if (binding.Art != null)
            {
                if (binding.Role == UnitRole.Mythic && gameManager.MythicUnitRoster != null)
                {
                    BaseUnit selectedMythic = gameManager.GetSelectedMythicUnit(playerTeam);
                    UpdateMythicCardArt(binding.Art, selectedMythic);
                }

                bool lockedMythic = binding.Role == UnitRole.Mythic && tier == 0;
                binding.Art.color = lockedMythic
                    ? Color.clear
                    : tier == 0
                        ? new Color(0.35f, 0.35f, 0.35f, binding.UnlockedColour.a)
                        : binding.UnlockedColour;

                SetCrossedSwordsColour(
                    binding.Art,
                    lockedMythic ? new Color(0.35f, 0.35f, 0.35f, 1f) : binding.Art.color);
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

        RefreshSelectedRole();
        mythicPickerController?.Refresh(gameManager, workerManager);
    }

    private void RefreshSelectedRole()
    {
        int tier = gameManager.GetProductionTier(playerTeam, selectedRole);
        bool hasData = gameManager.TryGetProductionData(playerTeam, selectedRole, out UnitData data);
        int cost = hasData ? data.Cost : 0;
        BaseUnit selectedMythic = selectedRole == UnitRole.Mythic
            ? gameManager.GetSelectedMythicUnit(playerTeam)
            : null;
        string roleName = selectedMythic != null
            ? GetDisplayName(selectedMythic).ToUpperInvariant()
            : selectedRole.ToString().ToUpperInvariant();

        if (selectedRoleIcon != null)
        {
            if (selectedRole == UnitRole.Mythic)
            {
                UpdateMythicCardArt(selectedRoleIcon, selectedMythic);
                selectedRoleIcon.color = selectedMythic == null ? Color.clear : Color.white;
                SetCrossedSwordsColour(
                    selectedRoleIcon,
                    selectedMythic == null ? new Color(0.35f, 0.35f, 0.35f, 1f) : Color.white);
            }
            else
            {
                HideCrossedSwords(selectedRoleIcon);
                ProductionUIBinding selectedBinding = GetProductionBinding(selectedRole);
                if (selectedBinding?.Art != null)
                {
                    selectedRoleIcon.sprite = selectedBinding.Art.sprite;
                    selectedRoleIcon.color = selectedBinding.Art.color;
                    selectedRoleIcon.preserveAspect = true;
                }
            }
        }

        if (selectedRoleTitleText != null)
        {
            selectedRoleTitleText.text = roleName;
        }

        if (selectedRoleStatusText != null)
        {
            selectedRoleStatusText.text = tier == 0 ? "LOCKED" : "PRODUCING";
        }

        if (selectedRoleTierText != null)
        {
            selectedRoleTierText.text = $"{tier} / {GameManager.MaximumProductionTier} STARS";
        }

        if (selectedRoleDescriptionText != null)
        {
            selectedRoleDescriptionText.text = selectedRole == UnitRole.Mythic && tier == 0
                ? "Choose a mythic unit before spending gold. Its own cost and production cadence will apply for this match."
                : tier == 0
                ? $"Unlock to begin recurring {roleName.ToLowerInvariant()} production. Upgrades affect future spawns only."
                : tier < GameManager.MaximumProductionTier
                    ? $"Producing {tier}-star {roleName.ToLowerInvariant()} units. Upgrade affects future spawns only."
                    : $"Producing maximum-tier {roleName.ToLowerInvariant()} units.";
        }

        if (selectedRoleActionText != null)
        {
            selectedRoleActionText.text = tier >= GameManager.MaximumProductionTier
                ? "MAXIMUM TIER"
                : selectedRole == UnitRole.Mythic && tier == 0 ? "SELECT UNIT"
                : tier == 0 ? $"UNLOCK   {cost} GOLD" : $"UPGRADE   {cost} GOLD";
        }

        if (selectedRoleButton != null)
        {
            selectedRoleButton.interactable = selectedRole == UnitRole.Mythic && tier == 0
                ? gameManager.HasMythicChoices(playerTeam)
                : hasData && tier < GameManager.MaximumProductionTier &&
                    workerManager.CurrentGold >= cost && !gameManager.IsGameOver;
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
        SpriteRenderer renderer = prefab != null ? prefab.GetComponentInChildren<SpriteRenderer>(true) : null;
        return renderer != null ? renderer.sprite : null;
    }

    private void UpdateMythicCardArt(Image art, BaseUnit selectedMythic)
    {
        if (art == null || gameManager?.MythicUnitRoster == null) return;

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
            CreateSwordImage(crossedSwords, "Sword A", 0f);
            CreateSwordImage(crossedSwords, "Sword B", 90f);
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
        parchment.sprite = GetMythicParchmentSprite();
        parchment.color = Color.white;
        parchment.preserveAspect = true;
        parchment.raycastTarget = false;
    }

    private Sprite GetMythicParchmentSprite()
    {
        ProductionUIBinding mythicBinding = GetProductionBinding(UnitRole.Mythic);
        Transform parchment = mythicBinding?.Card != null
            ? mythicBinding.Card.Find("Portrait Paper")
            : null;
        return parchment != null && parchment.TryGetComponent(out Image image)
            ? image.sprite
            : null;
    }

    private void CreateSwordImage(Transform parent, string objectName, float rotation)
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

    private ProductionUIBinding GetProductionBinding(UnitRole role)
    {
        foreach (ProductionUIBinding binding in productionBindings)
        {
            if (binding != null && binding.Role == role)
            {
                return binding;
            }
        }

        return null;
    }

    private void RefreshMatchState()
    {
        if (gameManager == null) return;

        if (timerText != null)
        {
            timerText.text = MatchResultText.GetCountdown(gameManager.TimeRemaining);
        }

        if (!gameManager.IsGameOver)
        {
            SetResultOverlayVisible(false);
            return;
        }

        SetResultOverlayVisible(true);

        bool playerWon = gameManager.HasWinner && gameManager.WinningTeam == playerTeam;
        if (resultTitleText != null)
        {
            resultTitleText.text = MatchResultText.GetTitle(
                gameManager.HasWinner,
                gameManager.WinningTeam,
                playerTeam);
        }

        if (resultReasonText != null)
        {
            Team enemyTeam = playerTeam == Team.Left ? Team.Right : Team.Left;
            resultReasonText.text = MatchResultText.GetReason(
                gameManager.EndReason,
                playerWon,
                gameManager.GetTotalUnitLossValue(playerTeam),
                gameManager.GetTotalUnitLossValue(enemyTeam));
        }
    }

    private void SetResultOverlayVisible(bool visible)
    {
        if (gameOverOverlay == null) return;

        gameOverOverlay.alpha = visible ? 1f : 0f;
        gameOverOverlay.interactable = visible;
        gameOverOverlay.blocksRaycasts = visible;
    }

    private static void RefreshBaseHealth(Base battleBase, TextMeshProUGUI healthText, Image healthFill)
    {
        if (battleBase == null) return;

        if (healthText != null)
        {
            healthText.text = $"{battleBase.CurrentHealth:0.#} / {battleBase.MaxHealth:0.#}";
        }

        if (healthFill != null)
        {
            healthFill.fillAmount = Mathf.Clamp01(battleBase.CurrentHealth / battleBase.MaxHealth);
        }
    }
}
