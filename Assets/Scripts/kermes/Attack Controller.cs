using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class AttackController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform rightHandMuzzle;
    [SerializeField] private Projectile biscuitProjectile; // prefab
    private PlayerStats playerStats; //Çay ve bisküvi sayısını almak için

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
    private bool isDipping = false; // çayı updatede düzenli azaltabilmek için bool
    private float dippingTime;

    private void Awake()
    {
        input = GetComponent<InputManager>();
        if (!playerCamera) playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        playerStats = GetComponent<PlayerStats>();
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
        /*if (inCooldown)       // Bisküvi eriyince reloalamak zorundasın o yüzden cooldown mantığını çıkarıyorum
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                inCooldown = false;
                input.SetAttackBlocked(false);
            }
        }*/
        if(isDipping) // çaya dipliyosa süre saymaya başlıyor ve çay azalmaya başlıyor
        {
            dippingTime += Time.deltaTime;
            playerStats.tea -= playerStats.maxTeaPerBiscuit/1.8f*Time.deltaTime;
            if(playerStats.tea <= 0.2) // diplerken çay biterse otamatikmen fırlatıyor bisküviyi
            {
                OnAttackReleased(dippingTime);
                playerStats.tea = 0;
                return;
            }

            else if(dippingTime > dissolveTime)
            {
                playerStats.tea = 0; //çayda eridiğinde çay ziyan olcak ve bisküvi azalacak
                playerStats.biscuits--;
                isDipping = false;
                dippingTime = 0;
                //StartCooldown();
                // İleride: dissolve VFX / ses
                return;
            }
        }
    }

    private void OnAttackStarted()
    {
        // İleride: VFX/anim başlat
        //if (inCooldown) return;
        if (playerStats.tea <= 0 || playerStats.biscuits <= 0) return; // eğer çay ve ya bisküvi yoksa saldırıyı gerçekleştiremiyor
        isDipping = true;
    }

    private void OnAttackReleased(float holdSeconds)
    {
        if (playerStats.tea <= 0 || playerStats.biscuits <= 0) return; // eğer çay ve ya bisküvi yoksa saldırıyı gerçekleştiremiyor
        if (/*inCooldown || */!biscuitProjectile || !playerCamera) return;
        playerStats.biscuits--; //attığında bisküvi azalcak
        // Dissolve eşiğini geçtiysek: ateş yok, cooldown
        isDipping = false;
        dippingTime = 0;
        /* if (holdSeconds >= dissolveTime)
        {
            playerStats.tea = 0; //çayda eridiğinde çay ziyan olcak ve bisküvi azalacak
            playerStats.biscuits--;
            StartCooldown();
            // İleride: dissolve VFX / ses
            return;
        } */ //ŞİMDİLİK COMMENTOUTLUYORUM BU KISMI ÇAY ERİME SÜRESİ DOLDUĞU GİBİ KIRILMALI ÇAYDA DİSOLVELANMAK İÇİN BUTTONU RELEASELEMEYİ BEKLEMEMELİ

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
