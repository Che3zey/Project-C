using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BarrierSpell : Spell
{
    [Header("Barrier Settings")]
    public float duration = 5f;          // How long the barrier lasts
    public float spawnDistance = 2f;     // How far in front of the caster it spawns

    [HideInInspector] public GameObject owner;

    void Awake()
    {
        spellName = "Barrier";
        manaCost = 15f;
        cooldown = 6f;
    }

    public override void Cast(GameObject caster)
    {
        if (caster == null) return;

        owner = caster;

        // Only the owner should instantiate the networked barrier prefab
        PhotonView casterPV = caster.GetComponent<PhotonView>();
        if (casterPV == null || !casterPV.IsMine) return;

        // Prevent multiple barriers for the same player:
        // (we check instantiated barriers in the scene that carry BarrierSpell and match owner)
        foreach (BarrierSpell b in FindObjectsOfType<BarrierSpell>())
        {
            if (b.owner == caster)
            {
                Debug.Log("BarrierSpell: You already have an active barrier.");
                return;
            }
        }

        // Determine facing direction using animator parameters (same logic as Fireball)
        Vector2 lookDir = Vector2.right; // default
        Animator anim = caster.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            float lx = anim.GetFloat("LastMoveX");
            float ly = anim.GetFloat("LastMoveY");
            Vector2 v = new Vector2(lx, ly);
            if (v.sqrMagnitude > 0.0001f)
                lookDir = v.normalized;
        }

        // Compute spawn position in front of the caster
        Vector3 spawnPos = caster.transform.position + (Vector3)(lookDir * spawnDistance);

        // Instantiate the barrier prefab across the network (prefab must be placed in Resources and have this BarrierSpell on it)
        GameObject inst = PhotonNetwork.Instantiate(gameObject.name, spawnPos, Quaternion.identity);
        if (inst == null)
        {
            Debug.LogWarning("BarrierSpell: Instantiate returned null for prefab: " + gameObject.name);
            return;
        }

        // Configure the instantiated barrier (only the spawning owner needs to set owner & start lifetime)
        BarrierSpell instSpell = inst.GetComponent<BarrierSpell>();
        if (instSpell == null)
        {
            Debug.LogWarning("BarrierSpell: Instantiated object missing BarrierSpell component.");
            return;
        }

        // Set the owner reference on the instantiated copy (so future checks know who it belongs to)
        instSpell.owner = caster;

        // Ensure the instantiated object has a blocking Collider2D and a static Rigidbody2D
        SetupBarrierPhysics(inst);

        // Determine rotation angle so barrier is *perpendicular* to facing direction:
        // - If player faces left/right (horizontal dominates): barrier should be vertical → angle = 90
        // - If player faces up/down (vertical dominates): barrier should be horizontal → angle = 0
        float angle = (Mathf.Abs(lookDir.x) > Mathf.Abs(lookDir.y)) ? 90f : 0f;

        // Set rotation on the instantiated object directly (it is networked, so transform will be visible to others).
        // Use the instantiated object's photonView to call the RPC if you prefer strict syncing; setting transform on the instanced object is fine.
        inst.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Start lifetime coroutine on the instantiated barrier (only the owner of the instantiated barrier should destroy it)
        instSpell.StartCoroutine(instSpell.LifetimeRoutine());
    }

    // Ensures collider & rigidbody block physics
    private void SetupBarrierPhysics(GameObject barrierGO)
    {
        if (barrierGO == null) return;

        // Ensure a Collider2D exists and is not a trigger
        Collider2D col = barrierGO.GetComponent<Collider2D>();
        if (col == null)
        {
            // Add a BoxCollider2D as a sensible default (adjust in prefab if needed)
            col = barrierGO.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = false;

        // Ensure it has a Rigidbody2D set to Static so it blocks dynamic rigidbodies (player/projectiles)
        Rigidbody2D rb = barrierGO.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = barrierGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Optional: set a layer so you can control Layer Collision Matrix (recommended)
        // barrierGO.layer = LayerMask.NameToLayer("Barrier"); // create and configure "Barrier" layer in Unity editor
    }

    // Lifetime coroutine that destroys networked object after duration
    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(duration);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else if (pv == null)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Optional: destroy incoming spell projectiles (tagged "Spell")
        if (collision.gameObject.CompareTag("Fireball"))
        {
            PhotonView pv = collision.gameObject.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
                PhotonNetwork.Destroy(pv.gameObject);
        }
    }
}
