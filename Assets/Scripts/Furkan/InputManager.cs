using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInputs playerInputs;
    public PlayerInputs.PlayerActions actions;

    [Header("References")]
    [SerializeField] private Jumping jumping;
    
    public event Action OnAttackStarted;
    public event Action<float> OnAttackReleased;

    public bool IsAttackHeld { get; private set; }
    public float AttackHoldTime { get; private set; }
    
    private bool attackBlocked;

    void Awake()
    {
        playerInputs = new PlayerInputs();
        actions = playerInputs.Player;
    }

    private void OnEnable()
    {
        actions.Enable();
        
        actions.Jump.performed += OnJumpPerformed;
        
        actions.Attack.started += OnAttackStartedCtx;
        actions.Attack.canceled += OnAttackCanceledCtx;
    }

    private void OnDisable()
    {
        actions.Jump.performed -= OnJumpPerformed;
        
        actions.Attack.started -= OnAttackStartedCtx;
        actions.Attack.canceled -= OnAttackCanceledCtx;

        actions.Disable();
    }

    private void Update()
    {
        if (IsAttackHeld)
            AttackHoldTime += Time.deltaTime;
    }
    
    public void SetAttackBlocked(bool blocked)
    {
        attackBlocked = blocked;
        if (attackBlocked && IsAttackHeld)
        {
            IsAttackHeld = false;
            AttackHoldTime = 0f;
        }
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumping?.Jump();
    }

    private void OnAttackStartedCtx(InputAction.CallbackContext ctx)
    {
        if (attackBlocked) return;

        IsAttackHeld = true;
        AttackHoldTime = 0f;
        OnAttackStarted?.Invoke();
    }

    private void OnAttackCanceledCtx(InputAction.CallbackContext ctx)
    {
        if (attackBlocked) return;

        if (IsAttackHeld)
        {
            IsAttackHeld = false;
            float held = AttackHoldTime;
            AttackHoldTime = 0f;
            OnAttackReleased?.Invoke(held);
        }
    }
}
