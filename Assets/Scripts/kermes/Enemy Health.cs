using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 50f;
    private float hp;

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
        Destroy(gameObject);
    }
}