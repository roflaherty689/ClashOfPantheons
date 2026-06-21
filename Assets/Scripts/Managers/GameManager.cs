using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
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
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxMeleeUnitsPerTeam = 5;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI victoryText;

    [Header("Game Settings")]
    [SerializeField] private float gameSpeed = 1f;
    [SerializeField] private bool randomiseSpawns = false;
    [SerializeField] public bool setTeamColour = true;
    
    private float spawnTimer;
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

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            TrySpawnUnit(Team.Left);
            TrySpawnUnit(Team.Right);
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
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);

        int teamUnitCount = 0;

        foreach (BaseUnit unit in allUnits)
        {
            if (unit.Team == team)
            {
                teamUnitCount++;
            }
        }

        if (teamUnitCount >= maxMeleeUnitsPerTeam) return;

        if (randomiseSpawns)
            GetRandomUnitPrefab(team);
        else
            GetNextUnitPrefab(team);
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
        BaseUnit prefab = leftFactionData.GetUnitPrefab(role);

        if (prefab == null)
            return;

        BaseUnit unitInstance = Instantiate(prefab, leftSpawnPoint.position, leftSpawnPoint.rotation);

        unitInstance.Initialize(Team.Left, leftTargetPoint);
    }

    public void SpawnRightUnit(UnitRole role)
    {
        BaseUnit prefab = rightFactionData.GetUnitPrefab(role);

        if (prefab == null)
            return;

        BaseUnit unitInstance = Instantiate(prefab, rightSpawnPoint.position, rightSpawnPoint.rotation);

        unitInstance.Initialize(Team.Right, rightTargetPoint);
    }
}