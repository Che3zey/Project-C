using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SpellSelectionManager : MonoBehaviourPunCallbacks
{
    public static SpellSelectionManager Instance;

    // Tracks all players' selections by ActorNumber
    private Dictionary<int, List<string>> playerSelections = new Dictionary<int, List<string>>();

    // Local player's current selections
    public string chosenSpell1;
    public string chosenSpell2;

    [Header("Available Spell Prefabs (Names Must Match)")]
    public string[] allSpells = { "Fireball", "ShockwaveUpDown", "ShockwaveSides" };

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Call when player selects a spell in the shop
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

    // Call when player unselects a spell in the shop
    public void UnchooseSpell(string spellName)
    {
        if (chosenSpell1 == spellName) chosenSpell1 = null;
        else if (chosenSpell2 == spellName) chosenSpell2 = null;

        SaveLocalSelection();
    }

    // Ensures local player has a default loadout if they picked nothing
    public void EnsureDefaults()
    {
        if (string.IsNullOrEmpty(chosenSpell1)) chosenSpell1 = "Fireball";
        if (string.IsNullOrEmpty(chosenSpell2))
            chosenSpell2 = (chosenSpell1 == "Fireball") ? "ShockwaveUpDown" : "Fireball";

        SaveLocalSelection();
        Debug.Log($"✅ Finalized Spells: {chosenSpell1}, {chosenSpell2}");
    }

    // Saves the local player's selections into the dictionary
    private void SaveLocalSelection()
    {
        int id = PhotonNetwork.LocalPlayer.ActorNumber;
        if (!playerSelections.ContainsKey(id))
            playerSelections[id] = new List<string>();

        playerSelections[id].Clear();
        if (!string.IsNullOrEmpty(chosenSpell1)) playerSelections[id].Add(chosenSpell1);
        if (!string.IsNullOrEmpty(chosenSpell2)) playerSelections[id].Add(chosenSpell2);
    }

    // Returns the prefab GameObjects for the local player's selections
    public GameObject[] GetChosenSpellPrefabs()
    {
        List<GameObject> spells = new List<GameObject>();
        if (!string.IsNullOrEmpty(chosenSpell1))
        {
            GameObject s1 = Resources.Load<GameObject>(chosenSpell1);
            if (s1 != null) spells.Add(s1);
        }
        if (!string.IsNullOrEmpty(chosenSpell2))
        {
            GameObject s2 = Resources.Load<GameObject>(chosenSpell2);
            if (s2 != null) spells.Add(s2);
        }
        return spells.ToArray();
    }

    // Struct for returning two selected spell names
    public struct PlayerLoadout
    {
        public string spell1;
        public string spell2;
    }

    // Returns the saved selections for a given player by ActorNumber
    public PlayerLoadout GetSelectedSpells(int playerId)
    {
        if (playerSelections.ContainsKey(playerId))
        {
            var list = playerSelections[playerId];
            string s1 = list.Count > 0 ? list[0] : "Fireball";
            string s2 = list.Count > 1 ? list[1] : "ShockWaveUpDown";
            return new PlayerLoadout { spell1 = s1, spell2 = s2 };
        }

        // Default loadout if player never picked
        return new PlayerLoadout { spell1 = "Fireball", spell2 = "Shockwave" };
    }
}
