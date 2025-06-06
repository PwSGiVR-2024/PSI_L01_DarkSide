using UnityEngine;

public class MovementController : MonoBehaviour
{
    #region Public Fields
    
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    
    [Header("Jump Settings")]
    public float coyoteTime = 0.5f;
    public float jumpBufferTime = 0.1f; // Buffer dla skoku
    public AudioSource jumpSound;
    
    [Header("Audio")]
    public AudioSource walkSource;
    public AudioClip walkClip;
    
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    
    #endregion

    #region Private Fields
    
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter; // Buffer dla skoku
    private float moveInput;
    private bool facingRight = true;
    
    #endregion

    #region Unity Lifecycle
    
    void Start()
    {
        ValidateComponents();
    }

    void Update()
    {
        HandleInput();
        Move();
        Jump();
        CheckGroundStatus();
        HandleAnimations();
        HandleFootstepSound();
        HandleFacing();
    }
    
    #endregion

    #region Initialization
    
    private void ValidateComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError($"[{name}] Rigidbody2D not found!");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{name}] Animator not found!");
            }
        }

        if (groundCheck == null)
        {
            Debug.LogError($"[{name}] GroundCheck Transform not assigned!");
        }

        if (walkSource == null)
        {
            walkSource = GetComponent<AudioSource>();
            if (walkSource == null)
            {
                walkSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    #endregion

    #region Input Handling
    
    private void HandleInput()
    {
        moveInput = Input.GetAxis("Horizontal");
        
        // Jump buffer - pozwala na skok tuż przed lądowaniem
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpSound.Play();
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    #endregion

    #region Movement System
    
    void Move()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void HandleFacing()
    {
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
    #endregion

    #region Jump System
    
    void Jump()
    {
        // Skok z coyote time i jump buffer
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }

        // Kontrola wysokości skoku (krótszy skok gdy puścimy Space)
        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void CheckGroundStatus()
    {
        // Sprawdzenie, czy gracz na ziemi (zachowana oryginalna logika z tagami)
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
    }
    
    #endregion

    #region Animation System
    
    private void HandleAnimations()
    {
        if (animator == null) return;

        // Animacja biegu (zachowana oryginalna logika)
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        
    }
    
    #endregion

    #region Audio System
    
    void HandleFootstepSound()
    {
        // Zachowana oryginalna logika dźwięku kroków
        if (Mathf.Abs(moveInput) > 0.1f && isGrounded)
        {
            if (walkSource != null && !walkSource.isPlaying)
            {
                walkSource.clip = walkClip;
                walkSource.loop = true;
                walkSource.Play();
            }
        }
        else
        {
            if (walkSource != null && walkSource.isPlaying)
            {
                walkSource.Stop();
            }
        }
    }
    
    #endregion

    #region Public Methods
    
    public bool IsFacingRight()
    {
        return facingRight;
    }


    // Metoda dla zewnętrznych skryptów do zatrzymania gracza
    public void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    // Metoda do wymuszenia skoku (np. dla trampolijny)
    public void ForceJump(float force)
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, force);
        }
    }

    // Metoda do wyłączenia/włączenia kontrolera
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled && rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    // Metoda do ustawienia prędkości ruchu
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    // Metoda do ustawienia siły skoku
    public void SetJumpForce(float newJumpForce)
    {
        jumpForce = newJumpForce;
    }
    
    #endregion

    #region Debug & Visualization
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    #endregion
}