using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Pause Menu (auto-detected per scene)")]
    public GameObject pauseMenu;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down; // Default facing down
    private bool isPaused = false;

    // Synced variables for remote players
    private Vector2 networkMoveDir;
    private Vector2 networkLastMoveDir;
    private bool networkIsMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (anim == null) Debug.LogWarning("Animator not found on child!");
        if (sr == null) Debug.LogWarning("SpriteRenderer not found on child!");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 🔹 Called every time a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pauseMenu == null)
        {
            GameObject uiManager = GameObject.Find("UIManager");
            if (uiManager != null)
            {
                Transform pauseTransform = uiManager.transform.Find("PauseMenu");
                if (pauseTransform != null)
                {
                    pauseMenu = pauseTransform.gameObject;
                    pauseMenu.SetActive(false);
                    Debug.Log($"✅ Pause menu found under UIManager in scene: {scene.name}");
                }
            }
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 🔹 Continually retry finding pause menu if it doesn’t exist yet
        if (pauseMenu == null)
            TryFindPauseMenu();

        HandlePauseToggle();

        if (isPaused)
        {
            anim.SetBool("IsMoving", false);
            rb.velocity = Vector2.zero;
            return;
        }

        HandleLocalInput();
        UpdateAnimatorLocal();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine || isPaused) return;
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void HandleLocalInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput;
    }

    private void HandlePauseToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            // Try to find the pause menu dynamically under UIManager
            if (pauseMenu == null)
            {
                GameObject uiManager = GameObject.Find("UIManager");
                if (uiManager != null)
                {
                    Transform pauseTransform = uiManager.transform.Find("PauseMenu");
                    if (pauseTransform != null)
                        pauseMenu = pauseTransform.gameObject;
                }
            }

            if (pauseMenu != null)
                pauseMenu.SetActive(isPaused);

            // Freeze player movement and optionally cursor
            Cursor.visible = isPaused;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // 🔹 Tries to locate the pause menu by tag or name
    private void TryFindPauseMenu()
    {
        GameObject foundMenu = GameObject.FindWithTag("PauseMenu");
        if (foundMenu == null)
            foundMenu = GameObject.Find("PauseMenu");

        if (foundMenu != null)
        {
            pauseMenu = foundMenu;
            pauseMenu.SetActive(false);
            Debug.Log($"✅ Found PauseMenu in scene: {SceneManager.GetActiveScene().name}");
        }
    }

    private void UpdateAnimatorLocal()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        if (Mathf.Abs(lastMoveDir.x) > Mathf.Abs(lastMoveDir.y))
            sr.flipX = lastMoveDir.x > 0;

        Vector2 displayDir = isMoving ? moveInput : lastMoveDir;

        anim.SetFloat("MoveX", displayDir.x);
        anim.SetFloat("MoveY", displayDir.y);
        anim.SetFloat("LastMoveX", lastMoveDir.x);
        anim.SetFloat("LastMoveY", lastMoveDir.y);
        anim.SetBool("IsMoving", isMoving);
    }

    private void UpdateRemoteAnimation()
    {
        Vector2 displayDir = networkIsMoving ? networkMoveDir : networkLastMoveDir;

        if (Mathf.Abs(displayDir.x) > Mathf.Abs(displayDir.y))
            sr.flipX = displayDir.x > 0;

        anim.SetFloat("MoveX", displayDir.x);
        anim.SetFloat("MoveY", displayDir.y);
        anim.SetFloat("LastMoveX", networkLastMoveDir.x);
        anim.SetFloat("LastMoveY", networkLastMoveDir.y);
        anim.SetBool("IsMoving", networkIsMoving);
    }

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

    public bool IsPaused() => isPaused;
}
