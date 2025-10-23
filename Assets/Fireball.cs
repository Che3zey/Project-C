using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    private Vector2 direction;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // --- This is the method PlayerAttack calls ---
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Flip or rotate sprite based on direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement
            sr.flipX = direction.x < 0;
            transform.rotation = Quaternion.identity;
        }
        else
        {
            // Vertical movement (rotate 90°)
            sr.flipX = false;
            float angle = direction.y > 0 ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Example collision logic
        Destroy(gameObject);
    }
}
