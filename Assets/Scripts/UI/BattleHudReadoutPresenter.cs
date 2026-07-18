using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleHudReadoutPresenter
{
    private const string EconomyPath = "Safe Area/Bottom HUD/Economy/";
    private const string PlayerHealthPath = "Safe Area/Top HUD/Player Stronghold/Health Bar/";
    private const string EnemyHealthPath = "Safe Area/Top HUD/Enemy Stronghold/Health Bar/";
    private const string ResultPath = "Safe Area/Game Over Overlay/Result Panel/";

    private readonly Team playerTeam;
    private readonly Action buyWorkerRequested;
    private readonly Action restartRequested;
    private readonly TextMeshProUGUI goldTotalText;
    private readonly TextMeshProUGUI goldPerTripText;
    private readonly TextMeshProUGUI workerTotalText;
    private readonly TextMeshProUGUI battleSummaryText;
    private readonly TextMeshProUGUI playerHealthText;
    private readonly TextMeshProUGUI enemyHealthText;
    private readonly Image playerHealthFill;
    private readonly Image enemyHealthFill;
    private readonly TextMeshProUGUI timerText;
    private readonly CanvasGroup gameOverOverlay;
    private readonly TextMeshProUGUI resultTitleText;
    private readonly TextMeshProUGUI resultReasonText;
    private readonly Button buyWorkerButton;
    private readonly Button restartButton;
    private readonly string battleSummarySuffix;

    public BattleHudReadoutPresenter(
        Transform root,
        Team playerTeam,
        Action buyWorkerRequested,
        Action restartRequested)
    {
        this.playerTeam = playerTeam;
        this.buyWorkerRequested = buyWorkerRequested;
        this.restartRequested = restartRequested;

        goldTotalText = FindText(root, EconomyPath + "Gold Total", EconomyPath + "540 Text");
        goldPerTripText = FindText(root, EconomyPath + "Gold Per Trip", EconomyPath + "+12 PER TRIP Text");
        workerTotalText = FindText(root, EconomyPath + "Worker Total", EconomyPath + "2 / 5 Text");
        battleSummaryText = FindText(root,
            "Safe Area/Bottom HUD/Battle Summary/Worker Battle Summary",
            "Safe Area/Bottom HUD/Battle Summary/WORKERS  2 / 5     |     FRIENDLY UNITS  8     |     ENEMY UNITS  8 Text");
        playerHealthText = FindText(root, PlayerHealthPath + "Stronghold Health Total", PlayerHealthPath + "50 / 50 Text");
        enemyHealthText = FindText(root, EnemyHealthPath + "Stronghold Health Total", EnemyHealthPath + "50 / 50 Text");
        playerHealthFill = FindImage(root, PlayerHealthPath + "Health Fill");
        enemyHealthFill = FindImage(root, EnemyHealthPath + "Health Fill");
        timerText = FindText(root,
            "Safe Area/Top HUD/Match Timer/Time Remaining",
            "Safe Area/Top HUD/Match Timer/03:00 Text");
        resultTitleText = FindText(root, ResultPath + "VICTORY Text");
        resultReasonText = FindText(root, ResultPath + "ENEMY STRONGHOLD DESTROYED Text");

        Transform overlay = root.Find("Safe Area/Game Over Overlay");
        gameOverOverlay = overlay != null ? overlay.GetComponent<CanvasGroup>() : null;
        Transform buyWorker = root.Find(EconomyPath + "Buy Worker");
        buyWorkerButton = buyWorker != null ? buyWorker.GetComponent<Button>() : null;
        Transform restart = root.Find(ResultPath + "Restart Match");
        restartButton = restart != null ? restart.GetComponent<Button>() : null;

        int separatorIndex = battleSummaryText != null ? battleSummaryText.text.IndexOf('|') : -1;
        battleSummarySuffix = separatorIndex >= 0
            ? "     " + battleSummaryText.text.Substring(separatorIndex)
            : string.Empty;

        BindButton(buyWorkerButton, BuyWorker);
        BindButton(restartButton, Restart);
        SetResultOverlayVisible(false);
    }

    public void Refresh(
        GameManager gameManager,
        WorkerManager workerManager,
        Base playerBase,
        Base enemyBase)
    {
        RefreshEconomy(gameManager, workerManager);
        RefreshBaseHealth(playerBase, playerHealthText, playerHealthFill);
        RefreshBaseHealth(enemyBase, enemyHealthText, enemyHealthFill);
        RefreshMatchState(gameManager);
    }

    public void Dispose()
    {
        if (buyWorkerButton != null) buyWorkerButton.onClick.RemoveListener(BuyWorker);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
    }

    private void RefreshEconomy(GameManager gameManager, WorkerManager workerManager)
    {
        if (workerManager == null) return;

        if (goldTotalText != null) goldTotalText.text = workerManager.CurrentGold.ToString();
        if (goldPerTripText != null) goldPerTripText.text = $"+{workerManager.TotalGoldPerTrip} PER TRIP";

        string workerTotal = $"{workerManager.WorkerCount} / {workerManager.MaxWorkers}";
        if (workerTotalText != null) workerTotalText.text = workerTotal;
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

    private void RefreshMatchState(GameManager gameManager)
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
                gameManager.HasWinner, gameManager.WinningTeam, playerTeam);
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

    private static void RefreshBaseHealth(Base battleBase, TextMeshProUGUI text, Image fill)
    {
        if (battleBase == null) return;
        if (text != null) text.text = $"{battleBase.CurrentHealth:0.#} / {battleBase.MaxHealth:0.#}";
        if (fill != null) fill.fillAmount = Mathf.Clamp01(battleBase.CurrentHealth / battleBase.MaxHealth);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        if (button.targetGraphic != null) button.targetGraphic.raycastTarget = true;
        button.onClick.AddListener(action);
    }

    private void BuyWorker() => buyWorkerRequested?.Invoke();
    private void Restart() => restartRequested?.Invoke();

    private static TextMeshProUGUI FindText(Transform root, params string[] paths)
    {
        foreach (string path in paths)
        {
            Transform target = root.Find(path);
            if (target != null && target.TryGetComponent(out TextMeshProUGUI text)) return text;
        }
        return null;
    }

    private static Image FindImage(Transform root, params string[] paths)
    {
        foreach (string path in paths)
        {
            Transform target = root.Find(path);
            if (target != null && target.TryGetComponent(out Image image)) return image;
        }
        return null;
    }
}
