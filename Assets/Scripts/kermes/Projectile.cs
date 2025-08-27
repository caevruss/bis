using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float ownerIgnoreSeconds = 0.1f;

    private Rigidbody rb;
    private Collider col;
    private GameObject owner;
    private float damage; // dinamik hasar

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.useGravity = useGravity;
    }

    private void OnEnable()
    {
        CancelInvoke(nameof(SelfDestruct));
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    /// <summary>
    /// Mermiyi başlatır: hız, sahibi ve hasarı set edilir.
    /// </summary>
    public void Launch(Vector3 velocity, GameObject owner, float damage)
    {
        this.owner = owner;
        this.damage = damage;
        rb.linearVelocity = velocity;

        if (ownerTryGetColliders(owner, out var ownerCols))
            StartCoroutine(TemporarilyIgnoreOwner(ownerCols));
    }

    private bool ownerTryGetColliders(GameObject go, out Collider[] cols)
    {
        cols = go ? go.GetComponentsInChildren<Collider>() : null;
        return cols != null && cols.Length > 0;
    }

    private IEnumerator TemporarilyIgnoreOwner(Collider[] ownerCols)
    {
        foreach (var oc in ownerCols) if (oc) Physics.IgnoreCollision(col, oc, true);
        yield return new WaitForSeconds(ownerIgnoreSeconds);
        foreach (var oc in ownerCols) if (oc) Physics.IgnoreCollision(col, oc, false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // önce çarpan collider'da ara, yoksa parent'ında ara
        var dmg = collision.collider.GetComponent<IDamageable>()
                  ?? collision.collider.GetComponentInParent<IDamageable>();

        if (dmg != null)
            dmg.TakeDamage(damage);

        SelfDestruct();
    }

    private void SelfDestruct() => Destroy(gameObject);
}
