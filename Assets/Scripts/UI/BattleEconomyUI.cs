using TMPro;
using UnityEngine;
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
        public Button Button;
        public Image Art;
        public TextMeshProUGUI StatusText;
        public TextMeshProUGUI TierText;
        public TextMeshProUGUI ActionText;
        public Color UnlockedColour;
        public UnityAction PurchaseAction;
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
    private TextMeshProUGUI selectedRoleTitleText;
    private TextMeshProUGUI selectedRoleStatusText;
    private TextMeshProUGUI selectedRoleTierText;
    private TextMeshProUGUI selectedRoleDescriptionText;
    private TextMeshProUGUI selectedRoleActionText;
    private string battleSummarySuffix = string.Empty;

    private void Awake()
    {
        ResolveGameManager();
        ResolveWorkerManager();
        ResolveBases();
        ResolveUI();

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
        }

        if (selectedRoleButton != null)
        {
            selectedRoleButton.onClick.RemoveListener(PurchaseSelectedRole);
        }
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
            if (binding?.Button == null) continue;

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

    private Image FindImage(string path)
    {
        Transform target = transform.Find(path);
        return target != null ? target.GetComponent<Image>() : null;
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
        selectedRole = role;
        gameManager?.TryPurchaseProduction(playerTeam, role, workerManager);
        Refresh();
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
            bool canPurchase = hasData && tier < GameManager.MaximumProductionTier &&
                workerManager.CurrentGold >= cost && !gameManager.IsGameOver;

            if (binding.Art != null)
            {
                binding.Art.color = tier == 0
                    ? new Color(0.35f, 0.35f, 0.35f, binding.UnlockedColour.a)
                    : binding.UnlockedColour;
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
                    : tier == 0 ? $"UNLOCK {cost}" : $"UPGRADE {cost}";
            }

            if (binding.Button != null)
            {
                binding.Button.interactable = canPurchase;
            }
        }

        RefreshSelectedRole();
    }

    private void RefreshSelectedRole()
    {
        int tier = gameManager.GetProductionTier(playerTeam, selectedRole);
        bool hasData = gameManager.TryGetProductionData(playerTeam, selectedRole, out UnitData data);
        int cost = hasData ? data.Cost : 0;
        string roleName = selectedRole.ToString().ToUpperInvariant();

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
            selectedRoleDescriptionText.text = tier == 0
                ? $"Unlock to begin recurring {roleName.ToLowerInvariant()} production. Upgrades affect future spawns only."
                : tier < GameManager.MaximumProductionTier
                    ? $"Producing {tier}-star {roleName.ToLowerInvariant()} units. Upgrade affects future spawns only."
                    : $"Producing maximum-tier {roleName.ToLowerInvariant()} units.";
        }

        if (selectedRoleActionText != null)
        {
            selectedRoleActionText.text = tier >= GameManager.MaximumProductionTier
                ? "MAXIMUM TIER"
                : tier == 0 ? $"UNLOCK   {cost} GOLD" : $"UPGRADE   {cost} GOLD";
        }

        if (selectedRoleButton != null)
        {
            selectedRoleButton.interactable = hasData &&
                tier < GameManager.MaximumProductionTier &&
                workerManager.CurrentGold >= cost &&
                !gameManager.IsGameOver;
        }
    }

    private void RefreshMatchState()
    {
        if (gameManager == null) return;

        if (timerText != null)
        {
            int totalSeconds = Mathf.CeilToInt(gameManager.TimeRemaining);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = $"{minutes}:{seconds:00}";
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
            resultTitleText.text = !gameManager.HasWinner
                ? "DRAW"
                : playerWon ? "VICTORY" : "DEFEAT";
        }

        if (resultReasonText != null)
        {
            resultReasonText.text = GetResultReason(playerWon);
        }
    }

    private string GetResultReason(bool playerWon)
    {
        Team enemyTeam = playerTeam == Team.Left ? Team.Right : Team.Left;

        return gameManager.EndReason switch
        {
            MatchEndReason.StrongholdDestroyed => playerWon
                ? "ENEMY STRONGHOLD DESTROYED"
                : "YOUR STRONGHOLD WAS DESTROYED",
            MatchEndReason.TimeoutHealth => playerWon
                ? "TIME EXPIRED · YOUR STRONGHOLD HAD MORE HEALTH"
                : "TIME EXPIRED · ENEMY STRONGHOLD HAD MORE HEALTH",
            MatchEndReason.TimeoutUnitLossValue =>
                $"TIME EXPIRED · LOSSES {gameManager.GetTotalUnitLossValue(playerTeam)} vs " +
                $"{gameManager.GetTotalUnitLossValue(enemyTeam)} GOLD",
            MatchEndReason.TimeoutDraw =>
                $"TIME EXPIRED · HEALTH AND LOSSES TIED AT " +
                $"{gameManager.GetTotalUnitLossValue(playerTeam)} GOLD",
            _ => "MATCH COMPLETE"
        };
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
