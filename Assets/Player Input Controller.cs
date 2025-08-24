using UnityEngine;

[RequireComponent(typeof(QuakeLR.QuakeCharacterController))]
public class PlayerInputController : MonoBehaviour
{
    private QuakeLR.QuakeCharacterController quakeController;

    private void Awake()
    {
        quakeController = GetComponent<QuakeLR.QuakeCharacterController>();
    }

    private void Update()
    {
        // 1. Input oku (WASD + Space)
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        Vector3 inputDir = new Vector3(h, 0, v);

        // 2. Kamera yönüne göre hareket
        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0;
            camRight.y = 0;

            Vector3 moveDir = (camForward.normalized * v + camRight.normalized * h).normalized;

            quakeController.Move(moveDir);
        }

        // 3. Zıplama
        if (Input.GetButtonDown("Jump"))
        {
            quakeController.TryJump();
        }

        // 4. Hareket motorunu her frame çalıştır
        quakeController.ControllerThink(Time.deltaTime);
    }
}