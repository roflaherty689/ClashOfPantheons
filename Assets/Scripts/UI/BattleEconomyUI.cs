using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEconomyUI : MonoBehaviour
{
    private const string EconomyPath = "Safe Area/Bottom HUD/Economy/";
    private const string PlayerHealthPath = "Safe Area/Top HUD/Player Stronghold/Health Bar/";
    private const string EnemyHealthPath = "Safe Area/Top HUD/Enemy Stronghold/Health Bar/";

    [SerializeField] private Team playerTeam = Team.Left;

    private WorkerManager workerManager;
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
    private string battleSummarySuffix = string.Empty;

    private void Awake()
    {
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

        Refresh();
    }

    private void LateUpdate()
    {
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

        Transform buttonTransform = transform.Find(EconomyPath + "Buy Worker");
        buyWorkerButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;

        if (battleSummaryText != null)
        {
            int separatorIndex = battleSummaryText.text.IndexOf('|');
            battleSummarySuffix = separatorIndex >= 0
                ? "     " + battleSummaryText.text.Substring(separatorIndex)
                : string.Empty;
        }
    }

    private Image FindImage(string path)
    {
        Transform target = transform.Find(path);
        return target != null ? target.GetComponent<Image>() : null;
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
                buyWorkerButton.interactable = workerManager.HasWorkerCapacity;
            }
        }

        RefreshBaseHealth(playerBase, playerHealthText, playerHealthFill);
        RefreshBaseHealth(enemyBase, enemyHealthText, enemyHealthFill);
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
