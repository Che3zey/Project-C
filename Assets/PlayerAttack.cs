using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public float fireCooldown = 0.5f;
    public float fireOffsetDistance = 0.5f; // how far in front of the player it spawns

    private float nextFireTime;
    private Vector2 lastMoveDir = Vector2.down; // default facing
    private PlayerMovement movementScript;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // --- Update last facing direction from movement input ---
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput.normalized;

        // --- Fire when pressing Space ---
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        if (fireballPrefab == null) return;

        // Calculate spawn position based on facing direction
        Vector3 spawnPos = transform.position + (Vector3)(lastMoveDir * fireOffsetDistance);

        // Spawn and set direction
        GameObject fb = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        Fireball fireball = fb.GetComponent<Fireball>();

        if (fireball != null)
            fireball.SetDirection(lastMoveDir);
    }
}
