using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensivityX;
    [SerializeField] private float sensivityY;

    [Header("References")] 
    [SerializeField] private Transform orientation;
    [SerializeField] private InputManager inputManager;


    private float xRotation;
    private float yRotation;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseDelta = inputManager.actions.Look.ReadValue<Vector2>();
        float mouseX = mouseDelta.x*Time.deltaTime*sensivityX;
        float mouseY = mouseDelta.y*Time.deltaTime*sensivityY;

        yRotation += mouseX;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
