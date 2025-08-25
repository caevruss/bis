using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float attackDamage;

    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    private EnemyHealth enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy = GetComponent<EnemyHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnemyAttacker()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 8f);
        foreach(var hit in hits)
        {
            if(hit.CompareTag("Player"))
            {
                playerStats.PlayerTakeDamage(attackDamage);
                Debug.Log("Enemy attacked");
                enemy.enemyPool.Release(enemy);
                break;
            }
        }
       
    }
}
