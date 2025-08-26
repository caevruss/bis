using System.Runtime.CompilerServices;
using UnityEngine;

public class Jumping : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float slidingJumpCorrectionFactor;

    private PlayerMover playerMover;

    private bool readyToJump;
    private Rigidbody rb;

    void Start()
    {
        readyToJump = true;
        playerMover = GetComponent<PlayerMover>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Jump()
    {
        if (readyToJump && playerMover.isGrounded)
        {
            playerMover.exitingSlope = true;
            readyToJump = false;

            if(!(!playerMover.OnSlope() || rb.linearVelocity.y > -0.1f))
            {
                float slideJumpForce = slidingJumpCorrectionFactor * jumpForce * rb.linearVelocity.magnitude/playerMover.walkSpeed;
                rb.AddForce(playerMover.slopeHit.normal * slideJumpForce, ForceMode.Impulse);
            }
 
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }

    private void ResetJump()
    {
        playerMover.exitingSlope = false;
        readyToJump = true;
    }
     
}

