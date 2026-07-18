using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEconomyUI : MonoBehaviour
{
    private const string EconomyPath = "Safe Area/Bottom HUD/Economy/";
    private const string PlayerHealthPath = "Safe Area/Top HUD/Player Stronghold/Health Bar/";
    private const string EnemyHealthPath = "Safe Area/Top HUD/Enemy Stronghold/Health Bar/";
    private const string ResultPath = "Safe Area/Game Over Overlay/Result Panel/";

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
    private ProductionCardPresenter productionCardPresenter;
    private SelectedRolePresenter selectedRolePresenter;
    private MythicPickerController mythicPickerController;
    private string battleSummarySuffix = string.Empty;

    private void Awake()
    {
        ResolveGameManager();
        ResolveWorkerManager();
        ResolveBases();
        ResolveUI();
        productionCardPresenter = new ProductionCardPresenter(
            transform,
            playerTeam,
            PurchaseProduction,
            SelectProductionRole);
        selectedRolePresenter = new SelectedRolePresenter(
            transform,
            playerTeam,
            productionCardPresenter,
            PurchaseProduction);
        mythicPickerController = new MythicPickerController(
            selectedRolePresenter.Panel,
            selectedRolePresenter.Font,
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

        productionCardPresenter?.Dispose();

        selectedRolePresenter?.Dispose();
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

        if (battleSummaryText != null)
        {
            int separatorIndex = battleSummaryText.text.IndexOf('|');
            battleSummarySuffix = separatorIndex >= 0
                ? "     " + battleSummaryText.text.Substring(separatorIndex)
                : string.Empty;
        }
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

        selectedRolePresenter?.Select(role, gameManager, workerManager);
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

        productionCardPresenter?.Refresh(gameManager, workerManager);
        selectedRolePresenter?.Refresh(gameManager, workerManager);
        mythicPickerController?.Refresh(gameManager, workerManager);
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
