using UnityEngine;
using Photon.Pun;

public class Shockwave : Spell
{
    [Header("Shockwave Settings")]
    public float speed = 6f;
    public float lifetime = 0.4f;
    public int damage = 2;
    public float knockbackForce = 8f;

    [HideInInspector]
    public GameObject owner;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        spellName = "Shockwave";
        manaCost = 20f;
        cooldown = 2f;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir, GameObject waveOwner)
    {
        direction = dir.normalized;

        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_SetOwner), RpcTarget.AllBuffered, waveOwner.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    void RPC_SetOwner(int ownerViewID)
    {
        PhotonView ownerPV = PhotonView.Find(ownerViewID);
        if (ownerPV != null)
            owner = ownerPV.gameObject;
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
                Vector2 knockDir = (other.transform.position - owner.transform.position).normalized;
                player.photonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage, knockDir, knockbackForce);
            }
        }
    }

    public override void Cast(GameObject caster)
    {
        // Handled by PlayerAttack
    }
}
