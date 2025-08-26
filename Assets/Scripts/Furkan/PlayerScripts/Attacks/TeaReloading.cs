using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class TeaReloading : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float teaPerSecond;
    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats.tea < 100)
            {
                playerStats.tea += teaPerSecond * Time.deltaTime;
                
            }
            else if(playerStats.tea >= 100)
            {
                playerStats.tea = 100;
            }
        }
    }
}
