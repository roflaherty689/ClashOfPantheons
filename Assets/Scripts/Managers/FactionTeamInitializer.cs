using UnityEngine;
using UnityEngine.UI;

public sealed class FactionTeamInitializer
{
    private readonly Image leftCastleIcon;
    private readonly Image rightCastleIcon;

    public FactionTeamInitializer(
        FactionData authoredLeftFaction,
        FactionData authoredRightFaction,
        Image leftCastleIcon,
        Image rightCastleIcon)
    {
        LeftFaction = authoredLeftFaction;
        RightFaction = authoredRightFaction;
        this.leftCastleIcon = leftCastleIcon;
        this.rightCastleIcon = rightCastleIcon;
    }

    public FactionData LeftFaction { get; private set; }
    public FactionData RightFaction { get; private set; }
    public Base LeftBase { get; private set; }
    public Base RightBase { get; private set; }

    public void Initialize()
    {
        ApplySessionFactions();
        ResolveBases();
        ApplyTeamPresentation(Team.Left, LeftFaction, LeftBase, leftCastleIcon);
        ApplyTeamPresentation(Team.Right, RightFaction, RightBase, rightCastleIcon);
    }

    public void ResolveBases()
    {
        LeftBase = null;
        RightBase = null;

        foreach (Base battleBase in
            Object.FindObjectsByType<Base>(FindObjectsSortMode.None))
        {
            if (battleBase.Team == Team.Left)
            {
                LeftBase = battleBase;
            }
            else
            {
                RightBase = battleBase;
            }
        }
    }

    private void ApplySessionFactions()
    {
        if (FactionSelectionSession.PlayerFaction != null)
        {
            LeftFaction = FactionSelectionSession.PlayerFaction;
        }

        if (FactionSelectionSession.EnemyFaction != null)
        {
            RightFaction = FactionSelectionSession.EnemyFaction;
        }
    }

    private static void ApplyTeamPresentation(
        Team team,
        FactionData factionData,
        Base battleBase,
        Image castleIcon)
    {
        if (battleBase == null)
        {
            Debug.LogError($"Cannot apply {team} faction presentation without a stronghold.");
            return;
        }

        BasePresentation presentation = battleBase.GetComponent<BasePresentation>();
        if (presentation == null)
        {
            Debug.LogError($"{battleBase.name}: Missing BasePresentation.", battleBase);
            return;
        }

        presentation.ValidateReferences();
        presentation.Apply(factionData, team);

        WorkerManager workerManager = battleBase.GetComponent<WorkerManager>();
        if (workerManager == null)
        {
            Debug.LogError($"{battleBase.name}: Missing WorkerManager.", battleBase);
        }
        else if (factionData != null && factionData.WorkerPrefab != null)
        {
            workerManager.ApplyWorkerPrefab(factionData.WorkerPrefab);
        }
        else
        {
            string factionName = factionData == null ? "<missing>" : factionData.FactionName;
            Debug.LogWarning(
                $"{team} faction '{factionName}' has no worker prefab; the existing WorkerManager fallback will be retained.",
                factionData);
        }

        if (castleIcon != null)
        {
            castleIcon.color = Color.white;
            if (factionData != null && factionData.CastleSprite != null)
            {
                castleIcon.sprite = factionData.CastleSprite;
            }
        }

        if (factionData == null || !factionData.HasBuildingPresentation)
        {
            string factionName = factionData == null ? "<missing>" : factionData.FactionName;
            Debug.LogWarning(
                $"{team} faction '{factionName}' has incomplete building presentation; existing sprites will be retained.",
                factionData);
        }

        if (castleIcon == null)
        {
            Debug.LogWarning($"{team} castle HUD icon is not assigned.");
        }
    }
}
