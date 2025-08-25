using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float playerStartingHP;

    [Header("Stats")]
    public float playerHP;
    void Start()
    {
        playerHP = playerStartingHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerTakeDamage(float damage)
    {        
        playerHP -= damage;
        if(playerHP<= 0)
        {
            Debug.Log("Player is dead");
            // ölme mantýðý eklenecek
            return;
        }
        Debug.Log($"Player took damage {damage}, health: {playerHP}");
    }
}
