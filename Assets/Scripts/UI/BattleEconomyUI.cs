using UnityEngine;

public class BattleEconomyUI : MonoBehaviour
{
    [SerializeField] private Team playerTeam = Team.Left;

    private WorkerManager workerManager;
    private GameManager gameManager;
    private Base playerBase;
    private Base enemyBase;
    private BattleHudReadoutPresenter hudPresenter;
    private ProductionCardPresenter productionCardPresenter;
    private SelectedRolePresenter selectedRolePresenter;
    private MythicPickerController mythicPickerController;

    private void Awake()
    {
        ResolveDependencies();
        hudPresenter = new BattleHudReadoutPresenter(
            transform,
            playerTeam,
            BuyWorker,
            RestartMatch);
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
        Refresh();
    }

    private void LateUpdate()
    {
        ResolveMissingDependencies();
        Refresh();
    }

    private void OnDestroy()
    {
        hudPresenter?.Dispose();
        productionCardPresenter?.Dispose();
        selectedRolePresenter?.Dispose();
        mythicPickerController?.Close();
    }

    private void ResolveDependencies()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        ResolveWorkerManager();
        ResolveBases();
    }

    private void ResolveMissingDependencies()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (workerManager == null) ResolveWorkerManager();
        if (playerBase == null || enemyBase == null) ResolveBases();
    }

    private void ResolveWorkerManager()
    {
        foreach (WorkerManager manager in FindObjectsByType<WorkerManager>())
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
        foreach (Base battleBase in FindObjectsByType<Base>())
        {
            if (battleBase.Team == playerTeam) playerBase = battleBase;
            else enemyBase = battleBase;
        }
    }

    private void BuyWorker()
    {
        workerManager?.TryBuyWorker();
        Refresh();
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
        if (role != UnitRole.Mythic) mythicPickerController?.Close();
        selectedRolePresenter?.Select(role, gameManager, workerManager);
    }

    private void RestartMatch() => gameManager?.RestartMatch();

    private void Refresh()
    {
        hudPresenter?.Refresh(gameManager, workerManager, playerBase, enemyBase);
        if (gameManager == null || workerManager == null) return;
        productionCardPresenter?.Refresh(gameManager, workerManager);
        selectedRolePresenter?.Refresh(gameManager, workerManager);
        mythicPickerController?.Refresh(gameManager, workerManager);
    }
}
