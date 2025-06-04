using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform player;
    public bool isFlipped = false;
    public float speed = 2.5f;
    public float attackRange = 3f;
    public int maxHealth = 20;

    public void LookAtPlayer()
    {
        if (transform.position.x > player.position.x && !isFlipped)
        {
            Flip();
        }
        else if (transform.position.x < player.position.x && isFlipped)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFlipped = !isFlipped;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
