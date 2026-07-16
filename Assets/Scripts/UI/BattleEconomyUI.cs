using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEconomyUI : MonoBehaviour
{
    private const string EconomyPath = "Safe Area/Bottom HUD/Economy/";

    [SerializeField] private Team playerTeam = Team.Left;

    private WorkerManager workerManager;
    private TextMeshProUGUI goldTotalText;
    private TextMeshProUGUI goldPerTripText;
    private TextMeshProUGUI workerTotalText;
    private TextMeshProUGUI battleSummaryText;
    private Button buyWorkerButton;
    private string battleSummarySuffix = string.Empty;

    private void Awake()
    {
        ResolveWorkerManager();
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

    private void ResolveUI()
    {
        goldTotalText = FindText(EconomyPath + "Gold Total", EconomyPath + "540 Text");
        goldPerTripText = FindText(EconomyPath + "Gold Per Trip", EconomyPath + "+12 PER TRIP Text");
        workerTotalText = FindText(EconomyPath + "Worker Total", EconomyPath + "2 / 5 Text");
        battleSummaryText = FindText(
            "Safe Area/Bottom HUD/Battle Summary/Worker Battle Summary",
            "Safe Area/Bottom HUD/Battle Summary/WORKERS  2 / 5     |     FRIENDLY UNITS  8     |     ENEMY UNITS  8 Text");

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
        if (workerManager == null) return;

        if (goldTotalText != null)
        {
            goldTotalText.text = workerManager.CurrentGold.ToString();
        }

        if (goldPerTripText != null)
        {
            goldPerTripText.text = $"+{workerManager.TotalGoldPerTrip} PER TRIP";
        }

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
}
