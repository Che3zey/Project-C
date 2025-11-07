using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class SpellSelectionManager : MonoBehaviourPunCallbacks
{
    public static SpellSelectionManager Instance;

    // Tracks all players' selections by ActorNumber
    private Dictionary<int, List<string>> playerSelections = new Dictionary<int, List<string>>();

    // Local player's current selections
    public string chosenSpell1;
    public string chosenSpell2;

    [Header("Available Spell Prefabs (Names Must Match Spell Name field in Spell.cs)")]
    public GameObject[] availableSpellPrefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
    // Clears spell selections for the local player (or all players)
    
    public void ClearSelections(bool onlyLocalPlayer = true)
    {
        if (onlyLocalPlayer)
        {
            int id = PhotonNetwork.LocalPlayer.ActorNumber;
            if (playerSelections.ContainsKey(id))
                playerSelections[id].Clear();

            chosenSpell1 = null;
            chosenSpell2 = null;
        }
        else
        {
            playerSelections.Clear();
            chosenSpell1 = null;
            chosenSpell2 = null;
        }

        Debug.Log("Spell selections cleared.");
    }

    
    // Player chooses a spell (prevents duplicates)
    
    public void ChooseSpell(string spellName)
    {
        // Prevent duplicate selection
        if (spellName == chosenSpell1 || spellName == chosenSpell2)
        {
            Debug.Log("Spell already selected, ignoring.");
            return;
        }

        if (string.IsNullOrEmpty(chosenSpell1))
        {
            chosenSpell1 = spellName;
            Debug.Log($"Spell 1 chosen: {spellName}");
        }
        else if (string.IsNullOrEmpty(chosenSpell2))
        {
            chosenSpell2 = spellName;
            Debug.Log($"Spell 2 chosen: {spellName}");
        }
        else
        {
            // Replace the oldest spell (chosenSpell1) with the new one
            chosenSpell1 = chosenSpell2;
            chosenSpell2 = spellName;
            Debug.Log($"Replaced oldest spell. New selections: {chosenSpell1}, {chosenSpell2}");
        }

        SaveLocalSelection();
    }

    public void UnchooseSpell(string spellName)
    {
        if (chosenSpell1 == spellName) chosenSpell1 = null;
        else if (chosenSpell2 == spellName) chosenSpell2 = null;

        SaveLocalSelection();
    }

    public void EnsureDefaults()
    {
        if (string.IsNullOrEmpty(chosenSpell1)) chosenSpell1 = "Fireball";
        if (string.IsNullOrEmpty(chosenSpell2)) chosenSpell2 = "Shockwave";

        SaveLocalSelection();
        Debug.Log($"Finalized spells: {chosenSpell1}, {chosenSpell2}");
    }

    private void SaveLocalSelection()
    {
        int id = PhotonNetwork.LocalPlayer.ActorNumber;
        if (!playerSelections.ContainsKey(id))
            playerSelections[id] = new List<string>();

        playerSelections[id].Clear();
        if (!string.IsNullOrEmpty(chosenSpell1)) playerSelections[id].Add(chosenSpell1);
        if (!string.IsNullOrEmpty(chosenSpell2) && chosenSpell2 != chosenSpell1)
            playerSelections[id].Add(chosenSpell2);
    }

    public GameObject[] GetChosenSpellPrefabs()
    {
        EnsureDefaults();

        List<GameObject> result = new List<GameObject>();

        foreach (string spellName in new[] { chosenSpell1, chosenSpell2 })
        {
            if (string.IsNullOrEmpty(spellName)) continue;

            GameObject prefab = availableSpellPrefabs.FirstOrDefault(p =>
            {
                Spell spell = p.GetComponent<Spell>();
                return spell != null && spell.spellName == spellName;
            });

            if (prefab != null)
                result.Add(prefab);
            else
                Debug.LogWarning($"No prefab found for {spellName}");
        }

        return result.ToArray();
    }

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

        return new PlayerLoadout { spell1 = "Fireball", spell2 = "Shockwave" };
    }
}
