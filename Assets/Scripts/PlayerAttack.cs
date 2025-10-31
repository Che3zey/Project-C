using UnityEngine;
using Photon.Pun;

public class PlayerAttack : MonoBehaviourPun
{
    [Header("Spell Settings")]
    public GameObject[] equippedSpellPrefabs; // store prefab references instead of Spell components
    public float fireOffsetDistance = 0.5f;

    [Header("Shockwave Prefabs")]
    public GameObject shockwavePrefabUpDown;
    public GameObject shockwavePrefabSides;

    private float[] nextCastTime;
    private Vector2 lastMoveDir = Vector2.down;
    private PlayerMovement movementScript;
    private Animator anim;

    void Start()
    {
        movementScript = GetComponent<PlayerMovement>();
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("PlayerAttack: Animator not found in children!");

        // Load equipped spells from SpellSelectionManager if not already assigned
        if ((equippedSpellPrefabs == null || equippedSpellPrefabs.Length == 0) && SpellSelectionManager.Instance != null)
        {
            SpellSelectionManager.Instance.EnsureDefaults();
            equippedSpellPrefabs = SpellSelectionManager.Instance.GetChosenSpellPrefabs();
        }

        // Safety fallback: ensure at least two spells exist
        if (equippedSpellPrefabs == null || equippedSpellPrefabs.Length == 0)
        {
            equippedSpellPrefabs = new GameObject[2];
            equippedSpellPrefabs[0] = Resources.Load<GameObject>("FireballPrefab");
            equippedSpellPrefabs[1] = Resources.Load<GameObject>("ShockWavePrefabUpDown");
            Debug.Log("⚠️ Defaulted to FireballPrefab + ShockWavePrefabUpDown");
        }

        nextCastTime = new float[equippedSpellPrefabs.Length];
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Track last facing direction
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.magnitude > 0.1f)
            lastMoveDir = moveInput.normalized;

        if (Input.GetKeyDown(KeyCode.J)) TryCastSpell(0);
        if (Input.GetKeyDown(KeyCode.K)) TryCastSpell(1);
    }

    void TryCastSpell(int index)
    {
        if (equippedSpellPrefabs == null || index >= equippedSpellPrefabs.Length) return;
        GameObject spellPrefab = equippedSpellPrefabs[index];
        if (spellPrefab == null) return;

        Spell spell = spellPrefab.GetComponent<Spell>();
        if (spell == null) return;

        if (Time.time < nextCastTime[index]) return;

        PlayerMana mana = GetComponent<PlayerMana>();
        if (mana != null && mana.CurrentMana < spell.manaCost) return;

        nextCastTime[index] = Time.time + spell.cooldown;

        if (mana != null)
            mana.UseMana(spell.manaCost);

        CastSpell(spellPrefab, spell);
    }

    void CastSpell(GameObject prefab, Spell spell)
    {
        Vector3 spawnPos = transform.position + (Vector3)(lastMoveDir * fireOffsetDistance);

        if (spell is Fireball)
        {
            GameObject go = PhotonNetwork.Instantiate(prefab.name, spawnPos, Quaternion.identity);
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
            spell.Cast(gameObject);
        }
    }

    // ✅ Called from NetworkManager when player spawns
    public void AssignSpellsByName(string spell1, string spell2)
    {
        SpellSelectionManager.Instance.EnsureDefaults();

        // ✅ Load directly from Resources folder (no "Spells/" prefix)
        string prefabName1 = spell1 == "Fireball" ? "FireballPrefab" : "ShockWavePrefabUpDown";
        string prefabName2 = spell2 == "Fireball" ? "FireballPrefab" : "ShockWavePrefabUpDown";

        GameObject prefab1 = Resources.Load<GameObject>(prefabName1);
        GameObject prefab2 = Resources.Load<GameObject>(prefabName2);

        equippedSpellPrefabs = new GameObject[2];
        equippedSpellPrefabs[0] = prefab1;
        equippedSpellPrefabs[1] = prefab2;

        nextCastTime = new float[equippedSpellPrefabs.Length];

        Debug.Log($"🪄 Equipped prefabs: {spell1} -> {(prefab1 ? prefab1.name : "❌ null")}, {spell2} -> {(prefab2 ? prefab2.name : "❌ null")}");
    }
}
