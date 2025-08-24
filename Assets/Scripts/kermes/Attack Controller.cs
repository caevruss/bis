using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class AttackController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform rightHandMuzzle;
    [SerializeField] private Projectile biscuitProjectile; // prefab

    [Header("Damage / Charge")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float dissolveTime = 1.8f;     // çaya batırma eşiği (sn)
    [SerializeField] private float maxDamageMultiplier = 4f; // eşikteki çarpan

    [Header("Ballistics")]
    [SerializeField] private float muzzleVelocity = 40f;
    [SerializeField] private float maxAimRayDistance = 200f;
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Cooldown")]
    [SerializeField] private float cooldownSeconds = 3f;

    private InputManager input;
    private bool inCooldown;
    private float cooldownTimer;

    private void Awake()
    {
        input = GetComponent<InputManager>();
        if (!playerCamera) playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        input.OnAttackStarted  += OnAttackStarted;
        input.OnAttackReleased += OnAttackReleased;
    }

    private void OnDisable()
    {
        input.OnAttackStarted  -= OnAttackStarted;
        input.OnAttackReleased -= OnAttackReleased;
    }

    private void Update()
    {
        if (inCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                inCooldown = false;
                input.SetAttackBlocked(false);
            }
        }
    }

    private void OnAttackStarted()
    {
        // İleride: VFX/anim başlat
        if (inCooldown) return;
    }

    private void OnAttackReleased(float holdSeconds)
    {
        if (inCooldown || !biscuitProjectile || !playerCamera) return;

        // Dissolve eşiğini geçtiysek: ateş yok, cooldown
        if (holdSeconds >= dissolveTime)
        {
            StartCooldown();
            // İleride: dissolve VFX / ses
            return;
        }

        // Üstel hasar çarpanı (tNorm 0..1)
        float tNorm = Mathf.Clamp01(holdSeconds / dissolveTime);
        float mult = Mathf.Exp(Mathf.Log(maxDamageMultiplier) * tNorm); // 0->1 arası üstel
        float finalDamage = baseDamage * mult;

        // Spawn yönü
        Vector3 spawnPos = rightHandMuzzle ? rightHandMuzzle.position : playerCamera.transform.position;
        Vector3 dir = playerCamera.transform.forward;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                            out var hit, maxAimRayDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            dir = (hit.point - spawnPos).normalized;
        }

        // Spawn + launch (damage parametresi ile)
        var proj = Instantiate(biscuitProjectile, spawnPos, Quaternion.LookRotation(dir));
        proj.Launch(dir * muzzleVelocity, gameObject, finalDamage);
    }

    private void StartCooldown()
    {
        inCooldown = true;
        cooldownTimer = cooldownSeconds;
        input.SetAttackBlocked(true);
        // İstersen UI'ya bildirim: “Bisküvi eridi! 3 sn bekle...”
        Debug.Log("Bisküvi çayda eridi. Cooldown başladı.");
    }
}
