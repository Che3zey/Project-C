using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviourPunCallbacks
{
    public static PlayerColorManager Instance;

    // Define your four colors (order matters: 0=Red,1=Blue,2=Green,3=Yellow)
    public Color[] availableColors = { Color.red, Color.blue, Color.green, Color.yellow };

    // Track which colors are currently assigned
    private Dictionary<int, int> playerColorMap = new(); // Key: ActorNumber, Value: color index

    // Live list of connected players
    public List<Player> connectedPlayers = new List<Player>();

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

    #region Public Methods

    // Assign a color to a player when they join
    public int AssignColor(Player player)
    {
        if (player == null) return -1;

        // Already assigned? Return existing index
        if (playerColorMap.TryGetValue(player.ActorNumber, out int existingIndex))
            return existingIndex;

        // Find first available color
        for (int i = 0; i < availableColors.Length; i++)
        {
            if (!playerColorMap.ContainsValue(i))
            {
                playerColorMap[player.ActorNumber] = i;
                UpdateConnectedPlayers();
                return i;
            }
        }

        Debug.LogWarning("No available colors left for player " + player.ActorNumber);
        return -1; // All colors in use
    }

    // Release color when a player leaves
    public void ReleaseColor(Player player)
    {
        if (player == null) return;

        if (playerColorMap.ContainsKey(player.ActorNumber))
            playerColorMap.Remove(player.ActorNumber);

        UpdateConnectedPlayers();
    }

    // Get the assigned color for a player
    public Color GetPlayerColor(Player player)
    {
        if (player != null && playerColorMap.TryGetValue(player.ActorNumber, out int colorIndex))
            return availableColors[colorIndex];

        return Color.white; // fallback
    }

    // Get the assigned color index (1-based for your PlayerColor script)
    public int GetPlayerColorIndex(Player player)
    {
        if (player != null && playerColorMap.TryGetValue(player.ActorNumber, out int colorIndex))
            return colorIndex + 1; // match PlayerColor enum
        return -1;
    }

    #endregion

    #region Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AssignColor(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ReleaseColor(otherPlayer);
    }

    #endregion

    private void UpdateConnectedPlayers()
    {
        connectedPlayers.Clear();
        connectedPlayers.AddRange(PhotonNetwork.PlayerList);
    }
}
