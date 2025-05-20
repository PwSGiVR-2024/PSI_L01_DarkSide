using UnityEngine;

public class EnemyPatrolAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform player;
    public float detectionRadius = 5f; // Promień wykrycia gracza
    public float moveSpeed = 0.1f; // Prędkość przeciwnika
    public float losePlayerDelay = 2f; // po ilu sekundach wraca do patrolu

    private Vector3 currentTarget;
    private bool chasingPlayer = false;
    private float timeSinceLastSeen = 0f;

    void Start()
    {
        currentTarget = pointA.position;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position); //Obliczanie dystansu do gracza 

        if (distanceToPlayer < detectionRadius)
        {
            // Widzimy gracza
            chasingPlayer = true;
            timeSinceLastSeen = 0f; //Reset czasu 
        }
        else if (chasingPlayer)
        {
            // Gracz uciekł, odliczamy czas
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen >= losePlayerDelay) // Po przekroczenu wraca do punktu patrolowego
            {
                chasingPlayer = false;
                // Opcjonalnie: wybierz najbliższy punkt patrolowy jako nowy cel
                currentTarget = (Vector2.Distance(transform.position, pointA.position) < Vector2.Distance(transform.position, pointB.position)) ? pointA.position : pointB.position;
            }
        }

        if (chasingPlayer)
        {
            // Ścigaj gracza
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Patrolowanie
            transform.position = Vector2.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
            {
                // Zmień cel patrolowania na drugi punkt
                currentTarget = (currentTarget == pointA.position) ? pointB.position : pointA.position;
            }
        }
    }

    private void OnDrawGizmosSelected() // Rysuje promień wykrywania oraz pozycje punktów patrolowych w edytorze Unity, co pomaga w debugowaniu i ustawianiu parametrów.
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        if (pointA != null) Gizmos.DrawWireSphere(pointA.position, 0.1f);
        if (pointB != null) Gizmos.DrawWireSphere(pointB.position, 0.1f);
    }
}