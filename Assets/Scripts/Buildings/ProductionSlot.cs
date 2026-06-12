using UnityEngine;

public class ProductionSlot : MonoBehaviour
{
    public Team team;
    public Unit unitPrefab;
    public UnitData unitData;
    public Transform spawnPoint;
    public float spawnInterval = 5f;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            SpawnUnit();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnUnit()
    {
        Unit unit = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        unit.team = team;
        unit.data = unitData;
    }
}