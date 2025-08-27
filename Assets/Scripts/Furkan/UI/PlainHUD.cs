using UnityEngine;
using TMPro;

public class PlainHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StatManager statManager;
    [Header("TEXTS")]
    [SerializeField] private TextMeshProUGUI teaText;
    [SerializeField] private TextMeshProUGUI bisText;

    private void Awake()
    {
        // Inspector boşsa otomatik bul
        if (!statManager)
        {
#if UNITY_2023_1_OR_NEWER
            statManager = FindFirstObjectByType<StatManager>(FindObjectsInactive.Include);
#else
            statManager = FindObjectOfType<StatManager>();
#endif
        }

        // İsimle bulma (opsiyonel)
        if (!teaText) teaText = GameObject.Find("TeaText")?.GetComponent<TextMeshProUGUI>();
        if (!bisText) bisText = GameObject.Find("BisText")?.GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (statManager) statManager.OnResourcesChanged += UpdateTexts;
        UpdateTexts();
    }

    private void OnDisable()
    {
        if (statManager) statManager.OnResourcesChanged -= UpdateTexts;
    }

    private void UpdateTexts()
    {
        if (!statManager || !teaText || !bisText) return;

        teaText.text = $"Tea: {statManager.tea:0.0}%";
        bisText.text = $"Biscuits: {statManager.biscuits:0}";
    }
}