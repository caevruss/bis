using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelManager : MonoBehaviour
{
    [Header("Sahnedeki Spawner'lar (Inspector'dan sırayla ver)")]
    [SerializeField] private EnemySpawner[] spawners;

    [Header("Dalgalar (Wave)")]
    [SerializeField] private List<Wave> waves = new List<Wave>();

    [Header("Dalgalar arası bekleme (shop için kullanacağız)")]
    [SerializeField] private float intermissionSeconds = 0f;

    public int CurrentWaveIndex { get; private set; } = -1;
    public int AliveEnemies { get; private set; } = 0;

    private int _spawnersFinished;
    private bool _isSpawning;

    public event Action<int> OnWaveStarted;                     // param: waveNumber (1-based)
    public event Action<int> OnWaveCleared;                     // param: waveNumber (1-based)
    public event Action<int, float> OnIntermissionStarted;      // nextWaveNumber, duration
    public event Action OnIntermissionEnded;
    
    private void OnEnable()
    {
        foreach (var sp in spawners)
        {
            if (!sp) continue;
            sp.OnEnemySpawned  += HandleEnemySpawned;
            sp.OnEnemyReleased += HandleEnemyReleased;
        }
    }

    private void OnDisable()
    {
        foreach (var sp in spawners)
        {
            if (!sp) continue;
            sp.OnEnemySpawned  -= HandleEnemySpawned;
            sp.OnEnemyReleased -= HandleEnemyReleased;
        }
    }

    private void Start()
    {
        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogError("LevelManager: Spawner bulunamadı.");
            return;
        }
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("LevelManager: Wave tanımı yok.");
            return;
        }

        // Otomatik sistemleri kapat (emin olmak için)
        foreach (var sp in spawners) sp.StopAutoSpawn();

        StartCoroutine(RunWaves());
    }
    

    private IEnumerator RunSingleWave(Wave wave)
    {
        _isSpawning = true;
        _spawnersFinished = 0;

        int activeSpawnerCount = Mathf.Min(spawners.Length, wave.countsPerSpawner.Length);

        for (int s = 0; s < activeSpawnerCount; s++)
        {
            StartCoroutine(SpawnFromSpawner(spawners[s], wave.countsPerSpawner[s], wave.spawnInterval));
        }

        // Tüm batche’ler bitti ve sahnede canlı kalmadıysa wave biter
        while (_isSpawning || AliveEnemies > 0)
            yield return null;
    }

    private IEnumerator SpawnFromSpawner(EnemySpawner spawner, int count, float interval)
    {
        yield return spawner.SpawnBatch(count, interval);
        _spawnersFinished++;
        if (_spawnersFinished >= ActiveSpawnerCountThisWave())
            _isSpawning = false;
    }

    private int ActiveSpawnerCountThisWave()
    {
        var wave = waves[Mathf.Clamp(CurrentWaveIndex, 0, waves.Count - 1)];
        return Mathf.Min(spawners.Length, wave.countsPerSpawner.Length);
    }

    private void HandleEnemySpawned(EnemyHealth _)
    {
        AliveEnemies++;
        // İstersen UI’ı burada güncelle
    }

    private void HandleEnemyReleased(EnemyHealth _)
    {
        AliveEnemies = Mathf.Max(AliveEnemies - 1, 0);
        // İstersen UI’ı burada güncelle
    }

    [System.Serializable]
    public class Wave
    {
        [Tooltip("Her spawner için üretilecek adet (spawner sayısı kadar değer).")]
        public int[] countsPerSpawner;

        [Tooltip("Bu dalgada her spawn arasındaki süre (s).")]
        public float spawnInterval = 0.5f;
    }
    
    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            CurrentWaveIndex = i;
    
            // >>> EKRAN: Wave başladı
            OnWaveStarted?.Invoke(i + 1);
    
            var wave = waves[i];
            yield return StartCoroutine(RunSingleWave(wave));
    
            // >>> EKRAN: Wave bitti
            OnWaveCleared?.Invoke(i + 1);
    
            // Son wave değilse ve arada bekleme varsa
            if (i < waves.Count - 1 && intermissionSeconds > 0f)
            {
                // >>> EKRAN: Aralık başladı (shop öncesi)
                OnIntermissionStarted?.Invoke(i + 2, intermissionSeconds);
                yield return new WaitForSeconds(intermissionSeconds);
                OnIntermissionEnded?.Invoke();
            }
        }
    
        Debug.Log("Tüm dalgalar tamam!");
    }
}
