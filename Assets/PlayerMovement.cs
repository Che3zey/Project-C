using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector2 lastMoveDir; // remembers last movement direction for idle

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // --- Get movement input ---
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement
        if (moveInput.magnitude > 1)
            moveInput.Normalize();

        bool isMoving = moveInput.magnitude > 0.1f;

        // --- Remember last direction while moving ---
        if (isMoving)
        {
            lastMoveDir = moveInput;

            // Flip sprite for side movement
            if (moveInput.x > 0.1f)
                sr.flipX = true;   // facing right
            else if (moveInput.x < -0.1f)
                sr.flipX = false;  // facing left
        }
        else
        {
            // When idle, for side direction, flip sprite based on last horizontal movement
            if (Mathf.Abs(lastMoveDir.x) > 0.1f)
                sr.flipX = lastMoveDir.x > 0; // right = flipped, left = normal
        }

        // --- Feed animator parameters ---
        anim.SetFloat("MoveX", isMoving ? moveInput.x : Mathf.Abs(lastMoveDir.x));
        anim.SetFloat("MoveY", isMoving ? moveInput.y : lastMoveDir.y);

        anim.SetBool("IsMoving", isMoving);
    }

    void FixedUpdate()
    {
        // --- Apply movement ---
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
