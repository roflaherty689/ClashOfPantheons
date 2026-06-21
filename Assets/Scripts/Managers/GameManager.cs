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
        float randomValue = Random.value;
        
        bool spawnMelee = randomValue < 0.25f;
        bool spawnArcher = randomValue >= 0.25f && randomValue < 0.5f;
        bool spawnCavalry = randomValue >= 0.5f && randomValue < 0.75f;
        bool spawnSiege = randomValue >= 0.5f && randomValue < 0.75f;


        if (team == Team.Left)
        {
            if (spawnMelee)
                SpawnLeftUnit(UnitRole.Melee);
            else if (spawnArcher)
                SpawnLeftUnit(UnitRole.Archer);
            else if (spawnCavalry)
                SpawnLeftUnit(UnitRole.Cavalry);            
            else if (spawnSiege)
                SpawnLeftUnit(UnitRole.Siege);
            else
                SpawnLeftUnit(UnitRole.Mythic);
        }
        else
        {
            if (spawnMelee)
                SpawnRightUnit(UnitRole.Melee);
            else if (spawnArcher)
                SpawnRightUnit(UnitRole.Archer);
            else if (spawnCavalry)
                SpawnRightUnit(UnitRole.Cavalry);            
            else if (spawnSiege)
                SpawnRightUnit(UnitRole.Siege);
            else
                SpawnRightUnit(UnitRole.Mythic);
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