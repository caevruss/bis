using UnityEngine;
using TMPro;
public class EnemyHungerBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] EnemyHealth enemy;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI healthText;
    // Update is called once per frame
    void Update()
    {
        healthText.text = $"Hunger: {enemy.hp}";
    }
}
