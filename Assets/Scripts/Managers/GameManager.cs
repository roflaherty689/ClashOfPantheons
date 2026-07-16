using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public enum SpawnPattern
{
    GlobalInterval,
    PerUnitInterval
}

public class GameManager : MonoBehaviour
{
    private static readonly UnitRole[] SpawnRoles =
    {
        UnitRole.Melee,
        UnitRole.Archer,
        UnitRole.Cavalry,
        UnitRole.Siege,
        UnitRole.Mythic
    };

    [Header("Team factions")]
    [SerializeField] private FactionData leftFactionData;
    [SerializeField] private FactionData rightFactionData;

    [Header("Spawn Points")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;

    [Header("Targets")]
    [SerializeField] private Transform leftTargetPoint;
    [SerializeField] private Transform rightTargetPoint;

    [Header("Spawn Settings")]
    [SerializeField] private SpawnPattern spawnPattern = SpawnPattern.GlobalInterval;
    [Min(0.1f)]
    [SerializeField] private float spawnInterval = 3f;
    [FormerlySerializedAs("maxMeleeUnitsPerTeam")]
    [Min(1)]
    [SerializeField] private int maxUnitsPerTeam = 5;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI victoryText;

    [Header("Game Settings")]
    [SerializeField] private float gameSpeed = 1f;
    [SerializeField] private bool randomiseSpawns = false;
    [SerializeField] public bool setTeamColour = true;
    
    private float globalSpawnTimer;
    private readonly float[] leftUnitSpawnTimers = new float[SpawnRoles.Length];
    private readonly float[] rightUnitSpawnTimers = new float[SpawnRoles.Length];
    private int leftReadySpawnIndex;
    private int rightReadySpawnIndex;
    private bool gameOver;

    public bool IsGameOver => gameOver;

    private int _leftSpawnIndex = 0;
    private int _rightSpawnIndex = 0;


    private void Start()
    {
        Time.timeScale = gameSpeed;
    }

    private void Update()
    {
        if (gameOver) return;

        if (spawnPattern == SpawnPattern.PerUnitInterval)
        {
            UpdatePerUnitSpawns(Team.Left, leftFactionData, leftUnitSpawnTimers, ref leftReadySpawnIndex);
            UpdatePerUnitSpawns(Team.Right, rightFactionData, rightUnitSpawnTimers, ref rightReadySpawnIndex);
            return;
        }

        UpdateGlobalSpawns();
    }

    private void UpdateGlobalSpawns()
    {
        globalSpawnTimer += Time.deltaTime;

        if (globalSpawnTimer >= spawnInterval)
        {
            globalSpawnTimer -= spawnInterval;

            TrySpawnUnit(Team.Left);
            TrySpawnUnit(Team.Right);
        }
    }

    private void UpdatePerUnitSpawns(
        Team team,
        FactionData factionData,
        float[] timers,
        ref int readySpawnIndex)
    {
        for (int i = 0; i < SpawnRoles.Length; i++)
        {
            timers[i] += Time.deltaTime;
        }

        int scanStartIndex = readySpawnIndex;
        int availableSpawnSlots = GetAvailableSpawnSlots(team);

        for (int offset = 0; offset < SpawnRoles.Length; offset++)
        {
            int roleIndex = (scanStartIndex + offset) % SpawnRoles.Length;
            UnitRole role = SpawnRoles[roleIndex];
            UnitData data = factionData.GetUnitData(role);

            if (data == null)
            {
                continue;
            }

            float roleSpawnInterval = Mathf.Max(data.spawnInterval, 0.1f);
            timers[roleIndex] = Mathf.Min(timers[roleIndex], roleSpawnInterval);

            if (timers[roleIndex] < roleSpawnInterval)
            {
                continue;
            }

            if (availableSpawnSlots <= 0 || !SpawnUnit(team, role))
            {
                continue;
            }

            timers[roleIndex] -= roleSpawnInterval;
            availableSpawnSlots--;
            readySpawnIndex = (roleIndex + 1) % SpawnRoles.Length;
        }
    }

    public void EndGame(string winningTeam)
    {
        gameOver = true;

        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.text = $"{winningTeam} Team Wins!";
        }
    }

    private void TrySpawnUnit(Team team)
    {
        if (!TeamHasSpawnCapacity(team)) return;

        if (randomiseSpawns)
            GetRandomUnitPrefab(team);
        else
            GetNextUnitPrefab(team);
    }

    private bool TeamHasSpawnCapacity(Team team)
    {
        return GetAvailableSpawnSlots(team) > 0;
    }

    private int GetAvailableSpawnSlots(Team team)
    {
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        int teamUnitCount = 0;

        foreach (BaseUnit unit in allUnits)
        {
            if (unit.Team == team)
            {
                teamUnitCount++;
            }
        }

        return Mathf.Max(0, maxUnitsPerTeam - teamUnitCount);
    }

    private void GetRandomUnitPrefab(Team team)
    {
        float meleeWeight = 1f / 50f;
        float archerWeight = 1f / 60f;
        float cavalryWeight = 1f / 100f;
        float siegeWeight = 1f / 130f;
        float mythicWeight = 1f / 220f;

        float totalWeight =
            meleeWeight +
            archerWeight +
            cavalryWeight +
            siegeWeight +
            mythicWeight;

        float roll = Random.Range(0f, totalWeight);

        UnitRole selectedRole;

        if (roll < meleeWeight)
        {
            selectedRole = UnitRole.Melee;
        }
        else if ((roll -= meleeWeight) < archerWeight)
        {
            selectedRole = UnitRole.Archer;
        }
        else if ((roll -= archerWeight) < cavalryWeight)
        {
            selectedRole = UnitRole.Cavalry;
        }
        else if ((roll -= cavalryWeight) < siegeWeight)
        {
            selectedRole = UnitRole.Siege;
        }
        else
        {
            selectedRole = UnitRole.Mythic;
        }

        if (team == Team.Left)
        {
            SpawnLeftUnit(selectedRole);
        }
        else
        {
            SpawnRightUnit(selectedRole);
        }
    }

    private void GetNextUnitPrefab(Team team)
    {
        int index = team == Team.Left ? _leftSpawnIndex : _rightSpawnIndex;

        switch (index)
        {
            case 0: 
                if (team == Team.Left) 
                    SpawnLeftUnit(UnitRole.Melee);
                else
                    SpawnRightUnit(UnitRole.Melee);
                break;
            case 1:
                if (team == Team.Left) 
                    SpawnLeftUnit(UnitRole.Archer);
                else
                    SpawnRightUnit(UnitRole.Archer);
                break;
            case 2: 
                if (team == Team.Left) 
                    SpawnLeftUnit(UnitRole.Cavalry); 
                else
                    SpawnRightUnit(UnitRole.Cavalry);
                break;
            case 3: 
                if (team == Team.Left) 
                    SpawnLeftUnit(UnitRole.Siege);
                else
                    SpawnRightUnit(UnitRole.Siege);
                break;
            case 4: 
                if (team == Team.Left) 
                    SpawnLeftUnit(UnitRole.Mythic);
                else
                    SpawnRightUnit(UnitRole.Mythic);
                break;
        };

        if (team == Team.Left)
            _leftSpawnIndex = (_leftSpawnIndex + 1) % 5;
        else
            _rightSpawnIndex = (_rightSpawnIndex + 1) % 5;
    }

    public void SpawnLeftUnit(UnitRole role)
    {
        SpawnUnit(Team.Left, role);
    }

    public void SpawnRightUnit(UnitRole role)
    {
        SpawnUnit(Team.Right, role);
    }

    private bool SpawnUnit(Team team, UnitRole role)
    {
        FactionData factionData = team == Team.Left ? leftFactionData : rightFactionData;
        Transform spawnPoint = team == Team.Left ? leftSpawnPoint : rightSpawnPoint;
        Transform targetPoint = team == Team.Left ? leftTargetPoint : rightTargetPoint;
        BaseUnit prefab = factionData.GetUnitPrefab(role);

        if (prefab == null)
            return false;

        BaseUnit unitInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        unitInstance.Initialize(team, targetPoint);

        return true;
    }
}
