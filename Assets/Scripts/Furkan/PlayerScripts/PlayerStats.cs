using System;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField] private float startingHP = 100f;
    [SerializeField] private float startingTea = 100f;       // yüzdelik gibi kullanıyorsan 0..100
    [SerializeField] private float startingBiscuits = 5f;
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float maxTeaPerBiscuit = 100f;
    [SerializeField] private float teaCapacity = 100f;
    
    // ileride craft/harcama için

    [Header("Core Stats")]
    [Tooltip("Karakterin anlık canı.")]
    public float playerHP;

    [Tooltip("Anlık çay miktarı (UI bu değeri gösteriyor).")]
    public float tea;

    [Tooltip("Anlık bisküvi sayısı.")]
    public float biscuits;

    [Header("Derived/Shop Stats")]
    [Tooltip("Hareket hızı çarpanı (1 = normal).")]
    [SerializeField] private float moveSpeedMultiplier = 1f;

    [Tooltip("Saldırı hızı çarpanı (1 = normal). 2 = %50 cooldown, 0.5 = 2x cooldown.")]
    [SerializeField] private float attackSpeedMultiplier = 1f;
    
    public float StartingBiscuits => startingBiscuits;
    public float MaxTeaPerBiscuit => maxTeaPerBiscuit;
    
    public float TeaCapacity => teaCapacity;

    public event Action OnResourcesChanged; // HUD buna abone

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        playerHP = Mathf.Clamp(startingHP, 0f, maxHP);
        tea       = Mathf.Max(0f, startingTea);
        biscuits  = Mathf.Max(0f, startingBiscuits);

        // HUD’ı ilk karede güncelle
        OnResourcesChanged?.Invoke();
    }

    // ---------- Public modifiers / helpers ----------

    /// <summary> Cooldown’u saldırı hızına göre ayarlar. 1x = dokunma, 2x = yarı cooldown. </summary>
    public float ModifyCooldown(float baseCooldown)
    {
        float denom = Mathf.Max(0.1f, attackSpeedMultiplier);
        return Mathf.Max(0.01f, baseCooldown / denom);
    }

    /// <summary> Dissolve (charge) süresini ayarlamak istersen burayı kullan; şimdilik dokunmuyoruz. </summary>
    public float ModifyDissolveTime(float baseDunkTime)
    {
        return baseDunkTime; // istersen: return baseDunkTime / Mathf.Max(0.1f, attackSpeedMultiplier);
    }

    // ---------- Resources (Tea/Biscuit) ----------
    public void SetTea(float value)
    {
        tea = Mathf.Clamp(value, 0f, teaCapacity);
        OnResourcesChanged?.Invoke();
    }

    public void AddTea(float delta) => SetTea(tea + delta);

    public void SetBiscuits(float value)
    {
        biscuits = Mathf.Max(0f, value);
        OnResourcesChanged?.Invoke();
    }

    public void AddBiscuits(float delta)
    {
        SetBiscuits(biscuits + delta);
    }

    // ---------- HP ----------
    public void PlayerTakeDamage(float damage)
    {
        if (damage <= 0f) return;

        playerHP -= damage;
        if (playerHP <= 0f)
        {
            playerHP = 0f;
            Debug.Log("Player is dead");
            // TODO: Ölüm akışı
        }
        OnResourcesChanged?.Invoke();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        playerHP = Mathf.Min(maxHP, playerHP + amount);
        OnResourcesChanged?.Invoke();
    }

    // ---------- Shop erişimi için basit getter'lar ----------
    public float GetMoveSpeedMultiplier()  => moveSpeedMultiplier;
    public float GetAttackSpeedMultiplier() => attackSpeedMultiplier;

    public void SetMoveSpeedMultiplier(float mul)
    {
        moveSpeedMultiplier = Mathf.Max(0.1f, mul);
        // İstersen burada ayrı bir OnStatsChanged event’i tanımlayıp invoke edebilirsin.
    }

    public void SetAttackSpeedMultiplier(float mul)
    {
        attackSpeedMultiplier = Mathf.Max(0.1f, mul);
    }
}
