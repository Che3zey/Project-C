using UnityEngine;
using Photon.Pun;

public class PlayerAttack : MonoBehaviourPun
{
    [Header("Spell Settings")]
    public Spell[] equippedSpells; // Max 2 spells equipped
    public float fireOffsetDistance = 0.5f; // distance from player center when casting

    [Header("Shockwave Prefabs")]
    public GameObject shockwavePrefabUpDown;
    public GameObject shockwavePrefabSides;

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

        // Assign spells chosen in the shop (multiplayer-safe)
        if (SpellSelectionManager.Instance != null)
        {
            equippedSpells = new Spell[2];
            equippedSpells[0] = SpellSelectionManager.Instance.chosenSpell1;
            equippedSpells[1] = SpellSelectionManager.Instance.chosenSpell2;
        }

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
        if (equippedSpells.Length > 0 && Input.GetKeyDown(KeyCode.J))
            TryCastSpell(0);

        if (equippedSpells.Length > 1 && Input.GetKeyDown(KeyCode.K))
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
        Vector3 spawnPos = transform.position + (Vector3)(lastMoveDir * fireOffsetDistance);

        if (spell is Fireball)
        {
            GameObject go = PhotonNetwork.Instantiate(spell.gameObject.name, spawnPos, Quaternion.identity);
            Fireball fb = go.GetComponent<Fireball>();
            if (fb != null)
                fb.SetDirection(lastMoveDir, gameObject);
        }
        else if (spell is Shockwave)
        {
            if (shockwavePrefabUpDown == null || shockwavePrefabSides == null)
            {
                Debug.LogWarning("Shockwave prefabs not assigned!");
                return;
            }

            // Choose correct prefab based on player facing
            GameObject prefabToUse = Mathf.Abs(lastMoveDir.y) > Mathf.Abs(lastMoveDir.x)
                ? shockwavePrefabUpDown
                : shockwavePrefabSides;

            GameObject go = PhotonNetwork.Instantiate(prefabToUse.name, spawnPos, Quaternion.identity);
            Shockwave wave = go.GetComponent<Shockwave>();
            if (wave != null)
                wave.SetDirection(lastMoveDir, gameObject);
        }
        else
        {
            // Future spells like heal, shield, etc.
            spell.Cast(gameObject);
        }
    }
}
