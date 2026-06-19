using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private BaseUnit leftMeleePrefab;
    [SerializeField] private BaseUnit rightMeleePrefab;

    [SerializeField] private BaseUnit leftArcherPrefab;
    [SerializeField] private BaseUnit rightArcherPrefab;

    [SerializeField] private BaseUnit leftCavalryPrefab;
    [SerializeField] private BaseUnit rightCavalryPrefab;

    [SerializeField] private BaseUnit leftSiegePrefab;
    [SerializeField] private BaseUnit rightSiegePrefab;

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

private int _leftSpawnIndex = 0;
private int _rightSpawnIndex = 0;

private BaseUnit GetNextUnitPrefab(Team team)
{
    int index = team == Team.Left ? _leftSpawnIndex : _rightSpawnIndex;

    BaseUnit prefab = index switch
    {
        0 => team == Team.Left ? leftMeleePrefab : rightMeleePrefab,
        1 => team == Team.Left ? leftArcherPrefab : rightArcherPrefab,
        2 => team == Team.Left ? leftCavalryPrefab : rightCavalryPrefab,
        _ => team == Team.Left ? leftSiegePrefab : rightSiegePrefab
    };

    if (team == Team.Left)
        _leftSpawnIndex = (_leftSpawnIndex + 1) % 4;
    else
        _rightSpawnIndex = (_rightSpawnIndex + 1) % 4;

    return prefab;
}
    private float spawnTimer;
    private bool gameOver;

    public bool IsGameOver => gameOver;

    [SerializeField] private float gameSpeed = 1f;

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

            TrySpawnUnit(Team.Left, leftSpawnPoint, leftTargetPoint);
            TrySpawnUnit(Team.Right, rightSpawnPoint, rightTargetPoint);
        }
    }

    private void TrySpawnUnit(Team team, Transform spawnPoint, Transform targetPoint)
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

        BaseUnit prefabToSpawn = GetNextUnitPrefab(team);

        BaseUnit unitInstance = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            Quaternion.identity
        );

        unitInstance.Initialize(team, targetPoint);
    }

    private BaseUnit GetRandomUnitPrefab(Team team)
    {
        float randomValue = Random.value;
        
        bool spawnMelee = randomValue < 0.25f;
        bool spawnArcher = randomValue >= 0.25f && randomValue < 0.5f;
        bool spawnCavalry = randomValue >= 0.5f && randomValue < 0.75f;


        if (team == Team.Left)
        {
            return spawnMelee ? leftMeleePrefab : spawnArcher ? leftArcherPrefab : spawnCavalry ? leftCavalryPrefab : leftSiegePrefab;
        }

        return spawnMelee ? rightMeleePrefab : spawnArcher ? rightArcherPrefab : spawnCavalry ? rightCavalryPrefab : rightSiegePrefab;
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
}