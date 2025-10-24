using UnityEngine;
using Photon.Pun;

public class Fireball : MonoBehaviourPun
{
    [Header("Fireball Settings")]
    public float speed = 10f;
    public float lifetime = 2f;

    [HideInInspector]
    public GameObject owner; // assigned via RPC

    private Vector2 direction;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
            Debug.LogWarning("Fireball: No SpriteRenderer found!");
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // auto-destroy after lifetime
    }

    /// <summary>
    /// Called when spawned to set flight direction and owner
    /// </summary>
    public void SetDirection(Vector2 dir, GameObject fireOwner)
    {
        direction = dir.normalized;

        // Assign owner over network
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SetOwner), RpcTarget.AllBuffered, fireOwner.GetComponent<PhotonView>().ViewID);
        }

        // Update rotation/flip locally and for all clients
        photonView.RPC(nameof(RPC_UpdateRotation), RpcTarget.AllBuffered, direction.x, direction.y);
    }

    [PunRPC]
    void RPC_SetOwner(int ownerViewID)
    {
        PhotonView ownerPV = PhotonView.Find(ownerViewID);
        if (ownerPV != null)
            owner = ownerPV.gameObject;
    }

    [PunRPC]
    void RPC_UpdateRotation(float dirX, float dirY)
    {
        direction = new Vector2(dirX, dirY);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            sr.flipX = direction.x < 0; // left = flipped
            transform.rotation = Quaternion.identity;
        }
        else
        {
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
        if (owner == null) return; // safety check
        if (other.gameObject == owner) return; // ignore hitting owner

        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            // Only the owner applies damage
            PhotonView ownerPV = owner.GetComponent<PhotonView>();
            if (ownerPV != null && ownerPV.IsMine)
            {
                Vector2 knockbackDir = (other.transform.position - owner.transform.position).normalized;
                player.photonView.RPC("TakeDamage", RpcTarget.AllBuffered, 1, knockbackDir, 6f);
            }

            // Destroy fireball for all clients
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);

            return;
        }

        // Destroy if it hits anything else solid
        if (!other.isTrigger && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
