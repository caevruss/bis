using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public IObjectPool<EnemyHealth> enemyPool;
    public float maxHealth = 50f;
    public float hp;

    private void Awake() => hp = maxHealth;

    public void TakeDamage(float amount)
    {
        hp -= amount;
        Debug.Log($"{name} took {amount} dmg. HP: {hp}");

        if (hp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        enemyPool.Release(this);
    }

    public void SetPool(IObjectPool<EnemyHealth> pool)
    {
        enemyPool = pool;
    }
}