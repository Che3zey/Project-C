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

        PhotonView casterPV = caster.GetComponent<PhotonView>();
        if (casterPV == null || !casterPV.IsMine) return;

        // Determine facing direction
        Vector2 lookDir = GetFacingDirection(caster);
        if (lookDir == Vector2.zero)
            lookDir = Vector2.right; // fallback

        // Compute spawn position & rotation
        Vector3 spawnPos = caster.transform.position + (Vector3)lookDir * spawnDistance;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lookDir);

        // Spawn over network
        GameObject go = PhotonNetwork.Instantiate("BarrierSpellPrefab", spawnPos, rotation);
        if (go == null)
        {
            Debug.LogWarning("BarrierSpell: Instantiate returned null for prefab.");
            return;
        }

        // Setup barrier
        BarrierSpell barrierSpell = go.GetComponent<BarrierSpell>();
        if (barrierSpell == null)
        {
            Debug.LogWarning("BarrierSpell: Instantiated object missing BarrierSpell component.");
            return;
        }

        barrierSpell.owner = caster;
        SetupBarrierPhysics(go);

        // Set proper rotation (horizontal/vertical alignment)
        float angle = (Mathf.Abs(lookDir.x) > Mathf.Abs(lookDir.y)) ? 90f : 0f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        barrierSpell.StartCoroutine(barrierSpell.LifetimeRoutine());
    }

    private Vector2 GetFacingDirection(GameObject caster)
    {
        // Derive from Animator parameters
        Animator anim = caster.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            float lx = anim.GetFloat("LastMoveX");
            float ly = anim.GetFloat("LastMoveY");
            Vector2 dir = new Vector2(lx, ly);
            if (dir.sqrMagnitude > 0.01f)
                return dir.normalized;
        }

        // fallback if no animator data
        return Vector2.right;
    }

    private void SetupBarrierPhysics(GameObject barrierGO)
    {
        if (barrierGO == null) return;

        Collider2D col = barrierGO.GetComponent<Collider2D>();
        if (col == null)
            col = barrierGO.AddComponent<BoxCollider2D>();
        col.isTrigger = false;

        Rigidbody2D rb = barrierGO.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = barrierGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(duration);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (pv == null)
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy incoming fireballs
        if (collision.gameObject.CompareTag("Fireball"))
        {
            PhotonView pv = collision.gameObject.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
                PhotonNetwork.Destroy(pv.gameObject);
        }
    }
}
