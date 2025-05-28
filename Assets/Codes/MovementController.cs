using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform groundCheck;
    public Animator animator;

    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float groundCheckRadius = 0.2f;

    public AudioSource walkSource;
    public AudioSource attackSource;
    public AudioClip walkClip;
    public AudioClip attackClip;

    private bool isGrounded;
    private float coyoteTime = 0.5f;
    private float coyoteTimeCounter;

    private float moveInput;

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");

        // Ruch i skok
        Move();
        Jump();

        // Sprawdzenie czy gracz na ziemi
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Animacja biegu
        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // DŸwiêk kroków
        HandleFootstepSound();

        // Atak pod klawiszem E
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");

            if (attackSource && attackClip)
            {
                attackSource.PlayOneShot(attackClip);
            }
        }
    }

    void Move()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
        }
    }

    void HandleFootstepSound()
    {
        if (Mathf.Abs(moveInput) > 0.1f && isGrounded)
        {
            if (!walkSource.isPlaying)
            {
                walkSource.clip = walkClip;
                walkSource.loop = true;
                walkSource.Play();
            }
        }
        else
        {
            if (walkSource.isPlaying)
            {
                walkSource.Stop();
            }
        }
    }
}
