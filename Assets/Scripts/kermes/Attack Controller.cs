using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private InputManager input;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private StatManager statManager;

    [Header("Numbers")]
    [SerializeField] private float projectileSpeed = 22f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseDunkTime = 1.5f;
    [SerializeField] private float baseCooldown = 0.35f;
    [SerializeField] private float overDunkPenalty = 3.0f;

    [Header("Tuning")]
    [Tooltip("Hold sırasında çay tüketim hız çarpanı. >1 daha hızlı yer.")]
    [SerializeField] private float teaConsumptionSpeed = 1.75f;

    [Tooltip("Açıksa, bir atışta çay tüketimi MaxTeaPerBiscuit ile sınırlanır. Kapatırsan plateau olmaz.")]
    [SerializeField] private bool usePerShotTeaCap = false; // <-- YENİ (default: kapalı)

    private float cooldownTimer;
    private bool blockedByCooldown;

    private bool isCharging;
    private float teaUsedThisCharge;
    private bool overDunkedThisCharge; // <-- YENİ: bu basışta threshold’u aştık mı?

    private void Awake()
    {
        if (!cam) cam = Camera.main;

#if UNITY_2023_1_OR_NEWER
        if (!input)       input       = FindFirstObjectByType<InputManager>(FindObjectsInactive.Include);
        if (!statManager) statManager = StatManager.Instance ?? FindFirstObjectByType<StatManager>(FindObjectsInactive.Include);
#else
        if (!input)       input       = FindObjectOfType<InputManager>();
        if (!statManager) statManager = StatManager.Instance ?? FindObjectOfType<StatManager>();
#endif

        if (!firePoint)
        {
            var t = transform.Find("Right Hand");
            firePoint = t ? t : transform;
        }
        if (!projectilePrefab)
            Debug.LogWarning("AttackController: Projectile Prefab atanmadı.", this);
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.OnAttackStarted  += HandleAttackStarted;
            input.OnAttackReleased += HandleAttackReleased;
        }
        if (statManager != null)
            statManager.OnResourcesChanged += HandleResourcesChanged;
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.OnAttackStarted  -= HandleAttackStarted;
            input.OnAttackReleased -= HandleAttackReleased;
        }
        if (statManager != null)
            statManager.OnResourcesChanged -= HandleResourcesChanged;
    }

    private void Update()
    {
        // Cooldown sayaç
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f && blockedByCooldown)
            {
                blockedByCooldown = false;
                input?.SetAttackBlocked(false);
            }
        }

        // Basılı tutarken çayı dinamik tüket + threshold'u aşınca ANINDA cezayı uygula
        if (isCharging && input != null && input.IsAttackHeld && statManager != null && !overDunkedThisCharge)
        {
            float dunkTime = statManager.ModifyDissolveTime(baseDunkTime);

            // 1) ÇAY TÜKETİMİ (plateau kaldırıldı)
            float maxTeaForShot = (statManager.MaxTeaPerBiscuit > 0f) ? statManager.MaxTeaPerBiscuit : statManager.TeaCapacity;
            float remainingShotCap = usePerShotTeaCap ? Mathf.Max(0f, maxTeaForShot - teaUsedThisCharge)
                                                      : float.PositiveInfinity; // sınırsız tüketim

            float baseRate = (maxTeaForShot / Mathf.Max(0.05f, dunkTime)); // tam dunkTime'da full şarj
            float rate = baseRate * Mathf.Max(0.01f, teaConsumptionSpeed);

            float want = rate * Time.deltaTime;
            float canFromTea = Mathf.Max(0f, statManager.tea);
            float delta = Mathf.Min(want, canFromTea, remainingShotCap);

            if (delta > 0f)
            {
                statManager.AddTea(-delta);
                teaUsedThisCharge += delta;
            }

            // 2) THRESHOLD AŞIMI → ANINDA SIFIRLA + CEZA
            if (input.AttackHoldTime >= dunkTime)
            {
                OverDunkNow(); // anında uygula
            }
        }
    }

    private void HandleAttackStarted()
    {
        if (IsOnCooldown)
        {
            input?.SetAttackBlocked(true);
            return;
        }

        if (statManager && statManager.biscuits < 1f)
        {
            input?.SetAttackBlocked(true);
            return;
        }

        isCharging = true;
        teaUsedThisCharge = 0f;
        overDunkedThisCharge = false; // reset
    }

    private void HandleAttackReleased(float heldSeconds)
    {
        if (!EnsureRefs()) { isCharging = false; return; }

        // Eğer threshold’u zaten ANINDA uyguladıysak, bu basışı yok say.
        if (overDunkedThisCharge)
        {
            isCharging = false;
            teaUsedThisCharge = 0f;
            overDunkedThisCharge = false;
            return;
        }

        // Bisküvi yoksa güvenlik
        if (statManager && statManager.biscuits < 1f)
        {
            isCharging = false;
            return;
        }

        float dunkTime = statManager ? statManager.ModifyDissolveTime(baseDunkTime) : baseDunkTime;

        // Normal atış (threshold aşılmadı)
        float maxTeaForShot = (statManager && statManager.MaxTeaPerBiscuit > 0f) ? statManager.MaxTeaPerBiscuit : statManager.TeaCapacity;
        float charge = Mathf.Clamp01(teaUsedThisCharge / Mathf.Max(1e-3f, maxTeaForShot));

        const float k = 4f;
        float expPart = (Mathf.Exp(k * charge) - 1f) / (Mathf.Exp(k) - 1f);
        float damageMult = 1f + expPart * 3f; // 1..4
        float damage = baseDamage * damageMult;

        Vector3 fwd = cam.transform.forward;
        var go = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(fwd));

        if (go.TryGetComponent<Projectile>(out var proj))
            proj.Launch(fwd * projectileSpeed, gameObject, damage);
        else if (go.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = fwd * projectileSpeed;

        if (statManager) statManager.AddBiscuits(-1f);

        float cd = statManager ? statManager.ModifyCooldown(baseCooldown) : baseCooldown;
        StartCooldown(cd);

        isCharging = false;
        teaUsedThisCharge = 0f;
    }

    private void OverDunkNow()
    {
        if (overDunkedThisCharge) return;

        overDunkedThisCharge = true;
        isCharging = false;

        // çayı anında bitir
        ZeroTea();

        // cezalı cooldown
        float cdPenalty = statManager ? statManager.ModifyCooldown(overDunkPenalty) : overDunkPenalty;
        StartCooldown(cdPenalty);

        // input'u blokla (parmağını/mouse'u çekene kadar)
        input?.SetAttackBlocked(true);
    }

    private void StartCooldown(float seconds)
    {
        cooldownTimer = seconds;
        blockedByCooldown = true;
        input?.SetAttackBlocked(true);
    }

    private void ZeroTea()
    {
        if (!statManager) return;
        statManager.SetTea(0f);
    }

    private bool EnsureRefs()
    {
        if (!cam)               { Debug.LogError("AttackController: Camera yok.", this); return false; }
        if (!firePoint)         { Debug.LogError("AttackController: FirePoint yok.", this); return false; }
        if (!projectilePrefab)  { Debug.LogError("AttackController: Projectile Prefab yok.", this); return false; }
        return true;
    }

    private void HandleResourcesChanged()
    {
        if (!blockedByCooldown && statManager != null && statManager.biscuits >= 1f)
            input?.SetAttackBlocked(false);
    }

    public bool IsOnCooldown => cooldownTimer > 0f;
    public float GetCooldownRemaining() => Mathf.Max(0f, cooldownTimer);
}
