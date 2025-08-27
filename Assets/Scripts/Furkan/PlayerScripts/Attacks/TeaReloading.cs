using UnityEngine;

public class TeaReloading : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("0..TeaCapacity aralığını kaç saniyede tamamen doldursun?")]
    [SerializeField] private float fillSeconds = 1.5f;

    private StatManager sm;

    private void Awake()
    {
        sm = StatManager.Instance;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (sm == null) sm = StatManager.Instance;
        if (sm == null) return;

        float cap = Mathf.Max(1f, sm.TeaCapacity);                 // depo kapasitesi
        float perSecond = cap / Mathf.Max(0.05f, fillSeconds);     // saniyede dolan miktar

        if (sm.tea < cap)
        {
            float want = perSecond * Time.deltaTime;
            float remain = cap - sm.tea;
            float delta = Mathf.Min(want, remain);
            if (delta > 0f) sm.AddTea(delta);                      // HUD event
        }
        else if (sm.tea > cap)
        {
            sm.SetTea(cap); // olası taşmaları toparla
        }
    }
}