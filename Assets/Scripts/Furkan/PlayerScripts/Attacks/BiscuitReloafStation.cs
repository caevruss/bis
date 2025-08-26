using UnityEngine;

public class BiscuitReloafStation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if(playerStats.biscuits < playerStats.playerStartingBis)
            {
                playerStats.biscuits = playerStats.playerStartingBis;
            }
        }

    }
}
