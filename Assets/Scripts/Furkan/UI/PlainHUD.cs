using UnityEngine;
using TMPro;
public class PlainHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerStats playerStats;
    [Header("TEXTS")]
    [SerializeField] TextMeshProUGUI teaText;
    [SerializeField] TextMeshProUGUI bisText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        teaText.text = $"Tea: {playerStats.tea.ToString("0.0")}%";
        bisText.text = $"Biscuits: {playerStats.biscuits}";
    }
}
