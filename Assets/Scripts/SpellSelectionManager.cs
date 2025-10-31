using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class SpellSelectionManager : MonoBehaviourPunCallbacks
{
    public static SpellSelectionManager Instance;

    // Tracks all players' selections by ActorNumber
    private Dictionary<int, List<string>> playerSelections = new Dictionary<int, List<string>>();

    // Local player's current selections (names)
    public string chosenSpell1;
    public string chosenSpell2;

    [Header("Available Spell Prefabs (Names Must Match Spell Name field in Spell.cs)")]
    public GameObject[] availableSpellPrefabs; // e.g. FireballPrefab, ShockWavePrefabUpDown, ShockWavePrefabSides

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Called when the player selects a spell button in the shop
    public void ChooseSpell(string spellName)
    {
        if (string.IsNullOrEmpty(chosenSpell1))
        {
            chosenSpell1 = spellName;
            Debug.Log($"🧙 Spell 1 chosen: {spellName}");
        }
        else if (string.IsNullOrEmpty(chosenSpell2))
        {
            if (chosenSpell1 == spellName) return; // prevent duplicates
            chosenSpell2 = spellName;
            Debug.Log($"🔥 Spell 2 chosen: {spellName}");
        }
        else
        {
            Debug.Log("⚠️ Already have two spells selected!");
        }

        SaveLocalSelection();
    }

    // Unselect a spell
    public void UnchooseSpell(string spellName)
    {
        if (chosenSpell1 == spellName) chosenSpell1 = null;
        else if (chosenSpell2 == spellName) chosenSpell2 = null;

        SaveLocalSelection();
    }

    // ✅ Ensures default spells exist if none selected
    public void EnsureDefaults()
    {
        if (string.IsNullOrEmpty(chosenSpell1)) chosenSpell1 = "Fireball";
        if (string.IsNullOrEmpty(chosenSpell2)) chosenSpell2 = "Shockwave";

        SaveLocalSelection();
        Debug.Log($"✅ Finalized spells: {chosenSpell1}, {chosenSpell2}");
    }

    // Saves this player’s chosen spell names to the dictionary
    private void SaveLocalSelection()
    {
        int id = PhotonNetwork.LocalPlayer.ActorNumber;
        if (!playerSelections.ContainsKey(id))
            playerSelections[id] = new List<string>();

        playerSelections[id].Clear();
        if (!string.IsNullOrEmpty(chosenSpell1)) playerSelections[id].Add(chosenSpell1);
        if (!string.IsNullOrEmpty(chosenSpell2)) playerSelections[id].Add(chosenSpell2);
    }

    // ✅ Returns actual prefab references for the player's chosen spells
    public GameObject[] GetChosenSpellPrefabs()
    {
        EnsureDefaults();

        List<GameObject> result = new List<GameObject>();

        foreach (string spellName in new[] { chosenSpell1, chosenSpell2 })
        {
            if (string.IsNullOrEmpty(spellName)) continue;

            // Find the first prefab whose Spell component’s name matches
            GameObject prefab = availableSpellPrefabs.FirstOrDefault(p =>
            {
                Spell spell = p.GetComponent<Spell>();
                return spell != null && spell.spellName == spellName;
            });

            if (prefab != null)
            {
                result.Add(prefab);
                Debug.Log($"🧩 Matched prefab for {spellName}: {prefab.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ No matching prefab found for {spellName}!");
            }
        }

        return result.ToArray();
    }

    // Struct used by NetworkManagerPUN to query loadouts by player ID
    public struct PlayerLoadout
    {
        public string spell1;
        public string spell2;
    }

    public PlayerLoadout GetSelectedSpells(int playerId)
    {
        if (playerSelections.ContainsKey(playerId))
        {
            var list = playerSelections[playerId];
            string s1 = list.Count > 0 ? list[0] : "Fireball";
            string s2 = list.Count > 1 ? list[1] : "Shockwave";
            return new PlayerLoadout { spell1 = s1, spell2 = s2 };
        }

        // Default loadout if player never picked
        return new PlayerLoadout { spell1 = "Fireball", spell2 = "Shockwave" };
    }
}
