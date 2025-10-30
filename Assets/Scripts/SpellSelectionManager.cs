using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SpellSelectionManager : MonoBehaviourPunCallbacks
{
    public static SpellSelectionManager Instance;

    [System.Serializable]
    public class PlayerSpellLoadout
    {
        public string spell1;
        public string spell2;
    }

    // Each player’s loadout stored by their Photon player ID
    private Dictionary<int, PlayerSpellLoadout> playerSpells = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedSpells(string spell1, string spell2)
    {
        if (PhotonNetwork.LocalPlayer == null) return;

        int id = PhotonNetwork.LocalPlayer.ActorNumber;
        playerSpells[id] = new PlayerSpellLoadout { spell1 = spell1, spell2 = spell2 };
        Debug.Log($"🧙 Spells selected for Player {id}: {spell1}, {spell2}");
    }

    public PlayerSpellLoadout GetSelectedSpells(int playerId)
    {
        if (playerSpells.TryGetValue(playerId, out var loadout))
            return loadout;

        // Default to Fireball if nothing chosen
        return new PlayerSpellLoadout { spell1 = "Fireball", spell2 = "Shockwave" };
    }

    public void EnsureDefaults()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            int id = p.ActorNumber;
            if (!playerSpells.ContainsKey(id))
                playerSpells[id] = new PlayerSpellLoadout { spell1 = "Fireball", spell2 = "Shockwave" };
        }
    }
}
