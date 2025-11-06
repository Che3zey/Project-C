using UnityEngine;
using Photon.Pun;

public class HealingSpell : Spell
{
    [Header("Healing Settings")]
    public int healAmount = 20;            // How much to heal
    public float duration = 1f;            // Lifetime of the healing effect
    public float verticalOffset = 1.2f;    // How high above player to spawn

    [HideInInspector]
    public GameObject owner;

    private SpriteRenderer sr;

    void Awake()
    {
        // Base Spell setup
        spellName = "HealingSpell";
        manaCost = 15f;
        cooldown = 3f;

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
            Debug.LogWarning("HealingSpell: No SpriteRenderer found!");
    }

    void Start()
    {
        // Auto-destroy after duration
        Destroy(gameObject, duration);
    }

    /// <summary>
    /// Called by PlayerAttack via PhotonNetwork.Instantiate
    /// </summary>
    public override void Cast(GameObject caster)
    {
        owner = caster;

        if (owner == null)
        {
            Debug.LogWarning("HealingSpell: caster is null!");
            return;
        }

        if (photonView.IsMine)
        {
            // Heal the player via RPC
            PlayerHealth playerHealth = caster.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.photonView.RPC("HealPlayer", RpcTarget.AllBuffered, healAmount);
            }

            // Position this healing prefab above the player
            transform.position = caster.transform.position + Vector3.up * verticalOffset;
        }
    }

    /// <summary>
    /// Optional: call this if you want the effect to follow the player during the animation
    /// </summary>
    void Update()
    {
        if (owner != null)
        {
            transform.position = owner.transform.position + Vector3.up * verticalOffset;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Healing spell has no collisions; destroy on lifetime expiry only
    }
}
