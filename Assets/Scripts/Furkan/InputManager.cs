using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    private PlayerInputs playerInputs;
    public PlayerInputs.PlayerActions actions;

    [Header("References")]
    [SerializeField] private Jumping jumping;
    void Awake()
    {
        playerInputs = new PlayerInputs();
        actions = playerInputs.Player;
    }

    private void OnEnable()
    {
        actions.Enable();
        actions.Jump.performed += ctx => jumping.Jump();
    }
    private void OnDisable()
    {
        actions.Disable();
        actions.Jump.performed -= ctx => jumping.Jump();
    }
}
