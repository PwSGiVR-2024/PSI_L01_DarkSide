using UnityEngine;

public class EnemyPatrolAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform player;
    public float detectionRadius = 5f;
    public float moveSpeed = 0.1f;
    public float losePlayerDelay = 2f;

    public GameObject walkSoundObject;   // GameObject z AudioSource – chodzenie
    public GameObject attackSoundObject; // GameObject z AudioSource – atak

    private Vector3 currentTarget;
    private bool chasingPlayer = false;
    private float timeSinceLastSeen = 0f;

    private Animator animator;
    private Vector2 lastPosition;

    private AudioSource walkAudioSource;
    private AudioSource attackAudioSource;

    void Start()
    {
        currentTarget = pointA.position;
        animator = GetComponent<Animator>();
        lastPosition = transform.position;

        walkAudioSource = walkSoundObject.GetComponent<AudioSource>();
        attackAudioSource = attackSoundObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            chasingPlayer = true;
            timeSinceLastSeen = 0f;
        }
        else if (chasingPlayer)
        {
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen >= losePlayerDelay)
            {
                chasingPlayer = false;
                currentTarget = (Vector2.Distance(transform.position, pointA.position) < Vector2.Distance(transform.position, pointB.position)) ? pointA.position : pointB.position;
            }
        }

        Vector2 oldPos = transform.position;

        if (chasingPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
            {
                currentTarget = (currentTarget == pointA.position) ? pointB.position : pointA.position;
            }
        }

        // Ustawienie animacji
        float movementSpeed = ((Vector2)transform.position - lastPosition).magnitude / Time.deltaTime;
        animator.SetFloat("Speed", movementSpeed);
        lastPosition = transform.position;

        // Obsługa dźwięku chodzenia
        if (movementSpeed > 0.01f && !walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();
        }
        else if (movementSpeed <= 0.01f && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            animator.SetTrigger("Attack");

            if (!attackAudioSource.isPlaying)
            {
                attackAudioSource.Play();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        if (pointA != null) Gizmos.DrawWireSphere(pointA.position, 0.1f);
        if (pointB != null) Gizmos.DrawWireSphere(pointB.position, 0.1f);
    }
}
