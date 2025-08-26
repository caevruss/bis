using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float playerStartingHP;
    public float playerStartingTea;
    public float playerStartingBis;
    public float maxTeaPerBiscuit;

    [Header("Stats")]
    public float biscuits;
    public float tea;
    public float playerHP;
    void Start()
    {
        playerHP = playerStartingHP;
        biscuits = playerStartingBis;
        tea = playerStartingTea;
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
