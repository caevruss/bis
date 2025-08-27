using System.Collections;
using UnityEngine;
using TMPro;

public class WaveDebugHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_Text label;

    private Coroutine countdownCo;

    private void OnEnable()
    {
        if (!levelManager)
        {
            levelManager = UnityEngine.Object.FindFirstObjectByType<LevelManager>();
            // Eğer sahnede inaktif objelerde de aramak istersen:
            // levelManager = UnityEngine.Object.FindFirstObjectByType<LevelManager>(FindObjectsInactive.Include);
        }

        if (levelManager)
        {
            levelManager.OnWaveStarted         += HandleWaveStarted;
            levelManager.OnWaveCleared         += HandleWaveCleared;
            levelManager.OnIntermissionStarted += HandleIntermissionStarted;
            levelManager.OnIntermissionEnded   += HandleIntermissionEnded;
        }
    }

    private void OnDisable()
    {
        if (!levelManager) return;
        levelManager.OnWaveStarted         -= HandleWaveStarted;
        levelManager.OnWaveCleared         -= HandleWaveCleared;
        levelManager.OnIntermissionStarted -= HandleIntermissionStarted;
        levelManager.OnIntermissionEnded   -= HandleIntermissionEnded;
    }

    private void HandleWaveStarted(int waveNumber)
    {
        ShowForSeconds($"WAVE {waveNumber}", 1.5f);
    }

    private void HandleWaveCleared(int waveNumber)
    {
        ShowForSeconds($"WAVE {waveNumber} CLEARED!", 1.8f);
    }

    private void HandleIntermissionStarted(int nextWaveNumber, float duration)
    {
        // countdown’u frame-frame güncelle
        if (countdownCo != null) StopCoroutine(countdownCo);
        countdownCo = StartCoroutine(CountdownUI(nextWaveNumber, duration));
    }

    private void HandleIntermissionEnded()
    {
        if (countdownCo != null) StopCoroutine(countdownCo);
        countdownCo = null;
        label.text = string.Empty;
    }

    private void ShowForSeconds(string msg, float seconds)
    {
        if (countdownCo != null) { StopCoroutine(countdownCo); countdownCo = null; }
        if (gameObject.activeInHierarchy)
            StartCoroutine(ShowTemp(msg, seconds));
        else
            label.text = msg;
    }

    private IEnumerator ShowTemp(string msg, float seconds)
    {
        label.text = msg;
        yield return new WaitForSeconds(seconds);
        // Eğer arada countdown başlamadıysa temizle
        if (countdownCo == null) label.text = string.Empty;
    }

    private IEnumerator CountdownUI(int nextWave, float dur)
    {
        float t = dur;
        while (t > 0f)
        {
            int sec = Mathf.CeilToInt(t);
            label.text = $"Next wave ({nextWave}) in {sec} s...";
            t -= Time.deltaTime;
            yield return null;
        }
        label.text = string.Empty;
        countdownCo = null;
    }
}
