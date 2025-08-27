using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float attackDamage;

    [Header("References")]
    private StatManager statManager;

    private EnemyHealth enemy;

    void Start()
    {
        enemy = GetComponent<EnemyHealth>();
    }
    

    public void EnemyAttacker()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 8f);
        foreach(var hit in hits)
        {
            if(hit.CompareTag("Player"))
            {
                statManager.PlayerTakeDamage(attackDamage);
                Debug.Log("Enemy attacked");
                enemy.enemyPool.Release(enemy);
                break;
            }
        }
       
    }
}
