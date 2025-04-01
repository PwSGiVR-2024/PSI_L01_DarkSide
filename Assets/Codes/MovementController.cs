using UnityEngine;

public class MovementController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform groundCheck; 

    public float moveSpeed = 3f; // Prędkość poruszania się 
    public float jumpForce = 8f; // Siła skoku
    public float groundCheckRadius = 0.2f; // Zasięg łapania podłoza 

    private bool isGrounded;
    private float coyoteTime = 0.5f; // Czas w którym mozna skoczyć po oderwaniu gracza od podłoza
    private float coyoteTimeCounter;

    void Update()
    {
        Move();
        Jump();

        Debug.Log("IsGrounded: " + isGrounded); // Wyświetlanie statusu zmiennej isGrounded
        

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false; // Sprawdza czy gracz znajduje się na podłou



        foreach (Collider2D collider in colliders) // Szukanie GameObjectow o tagu 'Ground'
        {
            if (collider.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }

        // Aktualizacja coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        // Pozwala skoczyć gdy gracz znajduje się na podłozu lub mieści się w czasie po oderwaniu od podłoza
        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f; // Resetowanie czasu po skoku 
        }
    }

}