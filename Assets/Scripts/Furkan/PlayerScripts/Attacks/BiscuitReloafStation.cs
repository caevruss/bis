using UnityEngine;

public class BiscuitReloafStation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var sm = StatManager.Instance;
        if (sm == null) return;

        // Başlangıç bisküvi sayısına kadar doldur
        if (sm.biscuits < sm.StartingBiscuits)
        {
            sm.SetBiscuits(sm.StartingBiscuits); // HUD olayı tetiklenir
        }
    }
}