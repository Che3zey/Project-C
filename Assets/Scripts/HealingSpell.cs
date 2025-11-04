using UnityEngine;
using Photon.Pun;

public class HealingSpell : Spell
{
    [Header("Healing Settings")]
    public int healAmount = 20;         // how much health to restore
    public float healRadius = 1.5f;     // optional (if you want AoE later)
    public float duration = 1.5f;       // lifetime for the animation object

    [HideInInspector]
    public GameObject owner;

    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();

        // Base Spell setup
        spellName = "Healing";      // must match name in shop system
        manaCost = 15f;
        cooldown = 4f;
    }

    void Start()
    {
        // Auto destroy after animation ends
        Destroy(gameObject, duration);
    }

    public override void Cast(GameObject caster)
    {
        if (caster == null) return;

        owner = caster;

        if (!caster.TryGetComponent(out PlayerHealth health))
        {
            Debug.LogWarning("HealingSpell: No PlayerHealth found on caster!");
            return;
        }

        // Only the local owner handles healing
        PhotonView casterPV = caster.GetComponent<PhotonView>();
        if (casterPV != null && casterPV.IsMine)
        {
            int newHealth = Mathf.Min(health.GetCurrentHealth() + healAmount, health.maxHealth);
            int healDiff = newHealth - health.GetCurrentHealth();

            // Heal via RPC so everyone sees consistent health
            health.photonView.RPC("HealPlayer", RpcTarget.AllBuffered, healDiff);

            // Spawn healing animation effect for all players
            photonView.RPC(nameof(RPC_PlayEffect), RpcTarget.AllBuffered, casterPV.ViewID);
        }
    }

    [PunRPC]
    void RPC_PlayEffect(int casterViewID)
    {
        PhotonView casterPV = PhotonView.Find(casterViewID);
        if (casterPV == null) return;

        Transform casterT = casterPV.transform;

        // Move spell to caster's position for visual effect
        transform.position = casterT.position;

        if (anim != null)
            anim.SetTrigger("Cast");
    }
}
