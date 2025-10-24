using UnityEngine;
using Photon.Pun;

public class PlayerAttack : MonoBehaviourPun
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public float fireCooldown = 0.5f;
    public float fireOffsetDistance = 0.5f; // distance from player center when firing

    private float nextFireTime;
    private Vector2 lastMoveDir = Vector2.down; // default facing down
    private PlayerMovement movementScript;
    private Animator anim;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("PlayerAttack: Animator not found in children!");
    }

    void Update()
    {
        // Only control the local player
        if (!photonView.IsMine) return;

        // Track last facing direction from movement
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput.normalized;

        // Fire when pressing Space
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }

        
    }

    void Shoot()
    {
        if (fireballPrefab == null) return;

        // Spawn position slightly in front of player
        Vector3 spawnPos = transform.position + (Vector3)(lastMoveDir * fireOffsetDistance);

        // Instantiate the fireball over the network
        GameObject fb = PhotonNetwork.Instantiate(fireballPrefab.name, spawnPos, Quaternion.identity);

        // Assign direction and owner
        Fireball fireball = fb.GetComponent<Fireball>();
        if (fireball != null)
        {
            fireball.SetDirection(lastMoveDir, gameObject);
        }
    }
}
