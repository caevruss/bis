using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float maxEnemyCount;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenSpawns;

    [Header("Eski periyodik sistem (opsiyonel)")]
    [SerializeField] private bool autoSpawn = false;  // LevelManager kullanırken kapalı kalsın

    [SerializeField] private EnemyHealth enemyPrefab;
    private IObjectPool<EnemyHealth> enemyPool;

    private float timeSinceLastSpawn;
    private float enemyCount;

    // LevelManager'ın dinleyeceği event'ler
    public event Action<EnemyHealth> OnEnemySpawned;
    public event Action<EnemyHealth> OnEnemyReleased;

    public int AliveCount => Mathf.RoundToInt(enemyCount);

    private void Awake()
    {
        enemyPool = new ObjectPool<EnemyHealth>(CreateEnemy, OnGet, OnRelease);
    }

    private void Update()
    {
        if (!autoSpawn) return; // LevelManager varken kapalı

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

        OnEnemySpawned?.Invoke(enemy);
    }

    private void OnRelease(EnemyHealth enemy)
    {
        enemyCount--;
        enemy.gameObject.SetActive(false);
        OnEnemyReleased?.Invoke(enemy);
    }

    /// <summary>
    /// LevelManager'ın çağıracağı batch spawn. 
    /// count adet düşmanı interval arayla üretir. 
    /// Kapasite (maxEnemyCount) doluysa boşalmasını bekler.
    /// </summary>
    public IEnumerator SpawnBatch(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            // kapasite doluysa yer açılana kadar bekle
            while (enemyCount >= maxEnemyCount)
                yield return null;

            enemyPool.Get();

            if (interval > 0f) yield return new WaitForSeconds(interval);
            else yield return null;
        }
    }

    // İstersen eski davranışı aç/kapat
    public void StartAutoSpawn() => autoSpawn = true;
    public void StopAutoSpawn()  => autoSpawn = false;
}
