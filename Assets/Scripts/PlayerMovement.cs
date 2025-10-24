using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down; // default facing down

    // Synced for remote players
    private Vector2 networkLastMoveDir;
    private bool networkIsMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (anim == null) Debug.LogWarning("Animator not found on child!");
        if (sr == null) Debug.LogWarning("SpriteRenderer not found on child!");
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleLocalInput();
            UpdateAnimatorLocal();
        }
        else
        {
            UpdateRemoteAnimation();
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleLocalInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.magnitude > 1f) moveInput.Normalize();

        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput;
    }

    private void UpdateAnimatorLocal()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        // Flip sprite for side movement
        if (Mathf.Abs(lastMoveDir.x) > Mathf.Abs(lastMoveDir.y))
            sr.flipX = lastMoveDir.x > 0;

        // Update animator parameters
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);
        anim.SetFloat("LastMoveX", lastMoveDir.x);
        anim.SetFloat("LastMoveY", lastMoveDir.y);
        anim.SetBool("IsMoving", isMoving);
    }

    private void UpdateRemoteAnimation()
    {
        // Use synced facing direction for idle
        bool isMoving = anim.GetBool("IsMoving");

        Vector2 displayDir = isMoving
            ? new Vector2(anim.GetFloat("MoveX"), anim.GetFloat("MoveY"))
            : networkLastMoveDir;

        // Flip side sprite for right-facing
        if (Mathf.Abs(displayDir.x) > Mathf.Abs(displayDir.y))
            sr.flipX = displayDir.x > 0;
    }

    // --- Photon sync for last facing direction ---
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send last facing direction and moving state
            stream.SendNext(lastMoveDir);
            stream.SendNext(moveInput.magnitude > 0.1f);
        }
        else
        {
            // Receive from network
            networkLastMoveDir = (Vector2)stream.ReceiveNext();
            networkIsMoving = (bool)stream.ReceiveNext();
        }
    }
}
