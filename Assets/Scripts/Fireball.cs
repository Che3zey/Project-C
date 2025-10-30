using UnityEngine;
using Photon.Pun;

public class Fireball : Spell
{
    [Header("Fireball Settings")]
    public float speed = 10f;
    public float lifetime = 2f;
    public int damage = 1;
    public float knockbackForce = 6f;

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

        // Base Spell setup
        spellName = "Fireball";
        manaCost = 10f;
        cooldown = 1.5f;
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // auto-destroy after lifetime
    }

    public void SetDirection(Vector2 dir, GameObject fireOwner)
    {
        direction = dir.normalized;

        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SetOwner), RpcTarget.AllBuffered, fireOwner.GetComponent<PhotonView>().ViewID);
        }

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
            sr.flipX = direction.x < 0;
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
        if (owner == null) return;
        if (other.gameObject == owner) return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();

        if (player != null)
        {
            PhotonView ownerPV = owner.GetComponent<PhotonView>();
            if (ownerPV != null && ownerPV.IsMine)
            {
                Vector2 knockbackDir = (other.transform.position - owner.transform.position).normalized;
                player.photonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage, knockbackDir, knockbackForce);
            }

            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);

            return;
        }

        if (!other.isTrigger && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public override void Cast(GameObject caster)
    {
        // Fireball spawns are handled externally
    }
}
