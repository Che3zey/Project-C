using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BarrierSpell : Spell
{
    [Header("Barrier Settings")]
    public float duration = 5f;          // How long the barrier lasts
    public float spawnDistance = 2f;     // How far in front of the caster it spawns

    private bool isCasting = false;

    private void Awake()
    {
        // Spell base data
        spellName = "Barrier";
        manaCost = 15f;
        cooldown = 6f;
    }

    /// <summary>
    /// Cast is called by PlayerAttack/Spell system.
    /// Caster should be the player GameObject.
    /// </summary>
    public override void Cast(GameObject caster)
    {
        if (isCasting) return;
        isCasting = true;

        PhotonView casterPV = caster.GetComponent<PhotonView>();
        if (casterPV == null) return;

        // Only the local owner should spawn the networked barrier
        if (!casterPV.IsMine) 
        {
            StartCoroutine(ResetCastingFlagAfter(cooldown));
            return;
        }

        // Determine look direction from Animator if possible
        Vector2 lookDir = GetCastDirectionFromAnimator(caster);
        if (lookDir == Vector2.zero)
            lookDir = Vector2.right; // fallback

        Vector3 spawnPos = caster.transform.position + (Vector3)lookDir.normalized * spawnDistance;

        // Spawn barrier prefab (this script should be on the prefab)
        GameObject barrierObj = PhotonNetwork.Instantiate(gameObject.name, spawnPos, Quaternion.identity);
        if (barrierObj == null)
        {
            Debug.LogWarning("BarrierSpell: Failed to PhotonNetwork.Instantiate barrier prefab with name: " + gameObject.name);
            StartCoroutine(ResetCastingFlagAfter(cooldown));
            return;
        }

        BarrierSpell barrier = barrierObj.GetComponent<BarrierSpell>();
        if (barrier == null)
        {
            Debug.LogWarning("BarrierSpell: Instantiated object doesn't have BarrierSpell component.");
            StartCoroutine(ResetCastingFlagAfter(cooldown));
            return;
        }

        // Calculate rotation angle (0 = horizontal, 90 = vertical)
        float angle = CalculateAngleForDirection(lookDir);

        // Set rotation on all clients so everyone sees aligned barrier
        barrier.photonView.RPC(nameof(RPC_SetRotation), RpcTarget.AllBuffered, angle);

        // Start lifetime on the networked object
        barrier.StartCoroutine(barrier.LifetimeRoutine());

        // Start cooldown on caster side
        StartCoroutine(ResetCastingFlagAfter(cooldown));
    }

    // Read last facing direction from the caster's animator parameters set by PlayerMovement.
    // This avoids touching PlayerMovement's private fields.
    private Vector2 GetCastDirectionFromAnimator(GameObject caster)
    {
        Animator anim = caster.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            // These parameters exist in your movement code: LastMoveX / LastMoveY
            float lx = 0f, ly = 0f;
            // Protect against missing parameters by try/catch (GetFloat doesn't throw, but value will be 0)
            lx = anim.GetFloat("LastMoveX");
            ly = anim.GetFloat("LastMoveY");

            Vector2 v = new Vector2(lx, ly);
            if (v.sqrMagnitude > 0.0001f) return v.normalized;
        }

        // As a fallback, try to query a PlayerMovement public property if you add one later.
        // For now return Vector2.zero to allow caller to pick a default.
        return Vector2.zero;
    }

    private float CalculateAngleForDirection(Vector2 dir)
    {
        // If horizontal dominates => use horizontal (angle 0)
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return 0f;
        else
            return 90f; // vertical barrier
    }

    [PunRPC]
    private void RPC_SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(duration);

        // Only the owner of the barrier should call Destroy (Photon ensures it removes network object)
        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (pv == null)
            Destroy(gameObject);
    }

    private IEnumerator ResetCastingFlagAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isCasting = false;
    }

    // Optional collision behavior to block/destroy spells that hit the barrier
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the incoming object is a spell projectile and is owned by someone, optionally destroy it.
        // Tag your projectiles with "Spell" or check component.
        if (collision.gameObject.CompareTag("Spell"))
        {
            PhotonView spellPV = collision.gameObject.GetComponent<PhotonView>();
            if (spellPV != null && spellPV.IsMine)
                PhotonNetwork.Destroy(spellPV.gameObject);
        }
    }
}
