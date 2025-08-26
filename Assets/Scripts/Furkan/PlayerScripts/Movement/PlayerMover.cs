using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Settings")]
    public float walkSpeed;
    //public float slideSpeed;
    //[SerializeField] private float dashSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airMultiplier;
    //[SerializeField] private float slideIncreaseMultiplier;
    //[SerializeField] private float slopeIncreaseMultiplier;
    //[SerializeField] private float dashSpeedChangeFactor; 

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    public bool isGrounded;

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform orientation;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    [HideInInspector] public RaycastHit slopeHit;
    public bool exitingSlope;
    public bool sliding;
    public bool dashing;

    private float moveSpeed;
    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private float speedChangeFactor;

    private Rigidbody rb;

    private Vector3 moveDirection;

    public MovementState state;

    public float horizantalInput;
    public float verticalInput;

    public enum MovementState
    {
        walking,
        //dashing,
        //sliding,
        air
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        moveSpeed = walkSpeed;
    }
    private void Update()
    {
        horizantalInput = inputManager.actions.Move.ReadValue<Vector2>().x;
        verticalInput = inputManager.actions.Move.ReadValue<Vector2>().y;

        SpeedControl();

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if(state == MovementState.walking)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        //StateHandler();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Move(inputManager.actions.Move.ReadValue<Vector2>());
    }




    private void Move(Vector2 playerInput)
    {
        //if (state == MovementState.dashing) return;
        moveDirection = orientation.forward * playerInput.y + orientation.right * playerInput.x;
        if(OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            if(rb.linearVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if(!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f*airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if(OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
        }
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight*0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    //ABI BUNLAR KENÝ OYUNUMDA OLAN KLODLAR BU OYUNDA KENDÝ OYNUMDA OLAN DASH SLÝDE GÝBÝ MEKANÝKLER OLAMADIÐI ÝÇÝN COMMENTDE DURUYOR ÞU AN
    /*private void StateHandler()
    {
        if(dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if(sliding)
        {
            state = MovementState.sliding;
            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                speedChangeFactor = slideIncreaseMultiplier;
            }
            else
            {
                desiredMoveSpeed = walkSpeed;
            }
        }
        else if(isGrounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
            desiredMoveSpeed = walkSpeed;
        }
        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing || lastState == MovementState.sliding) keepMomentum = true;

        if(desiredMoveSpeedHasChanged)
        {
            if(keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while(time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if(OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedChangeFactor * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedChangeFactor;
            }
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }*/
}
