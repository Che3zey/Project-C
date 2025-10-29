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
    private Vector2 lastMoveDir = Vector2.down; // Default facing down

    // Synced variables for remote players
    private Vector2 networkMoveDir;
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
        if (!photonView.IsMine) return;

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    // -------------------
    // LOCAL INPUT HANDLING
    // -------------------
    private void HandleLocalInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput;
    }

    private void UpdateAnimatorLocal()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        // Flip only when moving sideways
        if (Mathf.Abs(lastMoveDir.x) > Mathf.Abs(lastMoveDir.y))
            sr.flipX = lastMoveDir.x > 0;

        // Use last move dir when idle so we don't face up by default
        Vector2 displayDir = isMoving ? moveInput : lastMoveDir;

        anim.SetFloat("MoveX", displayDir.x);
        anim.SetFloat("MoveY", displayDir.y);
        anim.SetFloat("LastMoveX", lastMoveDir.x);
        anim.SetFloat("LastMoveY", lastMoveDir.y);
        anim.SetBool("IsMoving", isMoving);
    }

    // -------------------
    // REMOTE ANIMATION DISPLAY
    // -------------------
    private void UpdateRemoteAnimation()
    {
        Vector2 displayDir = networkIsMoving ? networkMoveDir : networkLastMoveDir;

        // Flip side-facing animations
        if (Mathf.Abs(displayDir.x) > Mathf.Abs(displayDir.y))
            sr.flipX = displayDir.x > 0;

        anim.SetFloat("MoveX", displayDir.x);
        anim.SetFloat("MoveY", displayDir.y);
        anim.SetFloat("LastMoveX", networkLastMoveDir.x);
        anim.SetFloat("LastMoveY", networkLastMoveDir.y);
        anim.SetBool("IsMoving", networkIsMoving);
    }

    // -------------------
    // PHOTON SYNC
    // -------------------
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool isMoving = moveInput.magnitude > 0.1f;

            stream.SendNext(isMoving);
            stream.SendNext(moveInput);
            stream.SendNext(lastMoveDir);
        }
        else
        {
            networkIsMoving = (bool)stream.ReceiveNext();
            networkMoveDir = (Vector2)stream.ReceiveNext();
            networkLastMoveDir = (Vector2)stream.ReceiveNext();
        }
    }
}
