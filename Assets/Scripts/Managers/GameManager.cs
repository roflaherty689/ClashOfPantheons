using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private BaseUnit leftMeleePrefab;
    [SerializeField] private BaseUnit rightMeleePrefab;

    [SerializeField] private BaseUnit leftArcherPrefab;
    [SerializeField] private BaseUnit rightArcherPrefab;

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

        BaseUnit prefabToSpawn = GetRandomUnitPrefab(team);

        BaseUnit unitInstance = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            Quaternion.identity
        );

        unitInstance.Initialize(team, targetPoint);
    }

    private BaseUnit GetRandomUnitPrefab(Team team)
    {
        bool spawnMelee = Random.value < 0.5f;

        if (team == Team.Left)
        {
            return spawnMelee
                ? leftMeleePrefab
                : leftArcherPrefab;
        }

        return spawnMelee
            ? rightMeleePrefab
            : rightArcherPrefab;
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