using UnityEngine;
using Photon.Pun;

public class PlayerAttack : MonoBehaviourPun
{
    [Header("Spell Settings")]
    public Spell[] equippedSpells; // Max 2 spells equipped
    public float fireOffsetDistance = 0.5f; // distance from player center when casting

    private float[] nextCastTime;
    private Vector2 lastMoveDir = Vector2.down; // default facing down
    private PlayerMovement movementScript;
    private Animator anim;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("PlayerAttack: Animator not found in children!");

        // Initialize cooldown trackers
        nextCastTime = new float[equippedSpells.Length];
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Track last facing direction from movement
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput.normalized;

        // Example input: cast first spell with Space, second with LeftControl
        if (equippedSpells.Length > 0 && Input.GetKeyDown(KeyCode.Space))
            TryCastSpell(0);

        if (equippedSpells.Length > 1 && Input.GetKeyDown(KeyCode.LeftControl))
            TryCastSpell(1);
    }

    void TryCastSpell(int index)
    {
        Spell spell = equippedSpells[index];
        if (spell == null) return;

        // Check cooldown
        if (Time.time < nextCastTime[index]) return;

        // Check mana
        PlayerMana mana = GetComponent<PlayerMana>();
        if (mana != null && mana.CurrentMana < spell.manaCost) return;

        nextCastTime[index] = Time.time + spell.cooldown;

        // Deduct mana
        if (mana != null)
            mana.UseMana(spell.manaCost);

        CastSpell(spell);
    }

    void CastSpell(Spell spell)
    {
        // Spawn the spell prefab at an offset
        Vector3 spawnPos = transform.position + (Vector3)(lastMoveDir * fireOffsetDistance);

        GameObject go = PhotonNetwork.Instantiate(spell.gameObject.name, spawnPos, Quaternion.identity);
        Fireball fb = go.GetComponent<Fireball>();
        if (fb != null)
        {
            fb.SetDirection(lastMoveDir, gameObject);
        }

        // TODO: add other spell types here (melee, blockade, heal)
    }
}
