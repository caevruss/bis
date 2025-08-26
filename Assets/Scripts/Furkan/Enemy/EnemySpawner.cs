using UnityEngine;
using UnityEngine.Pool;
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float maxEnemyCount;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenSpawns;

    [SerializeField] private EnemyHealth enemyPrefab;
    private IObjectPool<EnemyHealth> enemyPool;

    private float timeSinceLastSpawn;
    private float enemyCount;
    private void Awake()
    {
        enemyPool = new ObjectPool<EnemyHealth>(CreateEnemy, OnGet, OnRelease);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > timeSinceLastSpawn && enemyCount < maxEnemyCount)
        {
            enemyPool.Get();
            timeSinceLastSpawn = Time.time + timeBetweenSpawns;
        }
    }

    private EnemyHealth CreateEnemy()
    {
        EnemyHealth enemy = Instantiate(enemyPrefab);
        enemy.SetPool(enemyPool);
        return enemy;
    }

    private void OnGet(EnemyHealth enemy)
    {
        enemyCount++;
        enemy.gameObject.SetActive(true);
        enemy.hp = enemy.maxHealth;
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemy.transform.position = randomSpawnPoint.position;
    }
    private void OnRelease(EnemyHealth enemy)
    {
        enemyCount--;
        enemy.gameObject.SetActive(false);
    }
}
