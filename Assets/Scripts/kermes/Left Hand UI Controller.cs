using UnityEngine;

public class LeftHandUIController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private InputManager input;
    [SerializeField] private StatManager statManager;

    [Header("Timings")]
    [Tooltip("Çaya batırmada baz çözünme süresi (StatManager ModifyDissolveTime ile ölçeklenir).")]
    [SerializeField] private float baseDunkTime = 1.5f;

    [Tooltip("Basılı tutulmuyor ve cooldown yokken, şu aralıkla Idle animasyonu tetiklenir.")]
    [SerializeField] private float idleIntervalSeconds = 15f;

    // Animator parametre adları (animatordaki isimlerle birebir aynı olmalı)
    [Header("Animator Param Names")]
    [SerializeField] private string p_IsCharging   = "IsCharging";
    [SerializeField] private string p_Charge01     = "Charge01";
    [SerializeField] private string p_IdleTrigger  = "Idle";
    [SerializeField] private string p_OverDunkTrig = "OverDunk";

    // Cache edilmiş hash'ler (performans)
    private int h_IsCharging, h_Charge01, h_Idle, h_OverDunk;

    private float idleTimer;
    private bool overDunkedThisHold;

    private void Awake()
    {
        // Referansları otomatik bul (boşsa)
#if UNITY_2023_1_OR_NEWER
        if (!animator)    animator    = GetComponent<Animator>();
        if (!input)       input       = FindFirstObjectByType<InputManager>(FindObjectsInactive.Include);
        if (!statManager) statManager = StatManager.Instance ?? FindFirstObjectByType<StatManager>(FindObjectsInactive.Include);
#else
        if (!animator)    animator    = GetComponent<Animator>();
        if (!input)       input       = FindObjectOfType<InputManager>();
        if (!statManager) statManager = StatManager.Instance ?? FindObjectOfType<StatManager>();
#endif
        h_IsCharging = Animator.StringToHash(p_IsCharging);
        h_Charge01   = Animator.StringToHash(p_Charge01);
        h_Idle       = Animator.StringToHash(p_IdleTrigger);
        h_OverDunk   = Animator.StringToHash(p_OverDunkTrig);
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.OnAttackStarted  += OnAttackStarted;
            input.OnAttackReleased += OnAttackReleased;
        }
        ResetChargingVisuals();
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.OnAttackStarted  -= OnAttackStarted;
            input.OnAttackReleased -= OnAttackReleased;
        }
    }

    private void Update()
    {
        if (input == null || animator == null) return;

        // CHARGING (basılı tutarken)
        if (input.IsAttackHeld)
        {
            animator.SetBool(h_IsCharging, true);

            float dunkTime = GetEffectiveDunkTime();
            float t = Mathf.Clamp01(input.AttackHoldTime / Mathf.Max(0.05f, dunkTime));
            animator.SetFloat(h_Charge01, t);

            // Over-dunk anı: eşik aşıldığında bir kere tetikle
            if (!overDunkedThisHold && input.AttackHoldTime >= dunkTime)
            {
                animator.ResetTrigger(h_Idle);
                animator.SetTrigger(h_OverDunk);
                overDunkedThisHold = true;
            }

            // Charging sırasında idle sayacı çalışmasın
            idleTimer = 0f;
            return;
        }

        // IDLE (basılı tutulmuyor)
        animator.SetBool(h_IsCharging, false);
        animator.SetFloat(h_Charge01, 0f);

        // Cooldown veya başka bloklar InputManager tarafından handle ediliyor;
        // burada sadece periyodik idle tetikliyoruz.
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleIntervalSeconds)
        {
            idleTimer = 0f;
            animator.SetTrigger(h_Idle);
        }
    }

    private float GetEffectiveDunkTime()
    {
        if (statManager == null) return baseDunkTime;
        // StatManager içindeki ModifyDissolveTime gibi bir fonksiyon kullanıyoruz.
        // Yoksa direkt baseDunkTime döner.
        try { return statManager.ModifyDissolveTime(baseDunkTime); }
        catch { return baseDunkTime; }
    }

    private void OnAttackStarted()
    {
        // Yeni charge başlarken
        overDunkedThisHold = false;
        idleTimer = 0f;
        animator.ResetTrigger(h_Idle);
        // IsCharging true/Charge01 güncellemeleri Update’te yapılacak.
    }

    private void OnAttackReleased(float heldSeconds)
    {
        // Bırakınca charging visuals sıfırlansın
        ResetChargingVisuals();
    }

    private void ResetChargingVisuals()
    {
        overDunkedThisHold = false;
        if (animator == null) return;
        animator.SetBool(h_IsCharging, false);
        animator.SetFloat(h_Charge01, 0f);
    }
}
