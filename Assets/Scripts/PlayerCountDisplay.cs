using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCountDisplay : MonoBehaviour
{
    public TMP_Text playerCountText;

    void OnEnable()
    {
        // Subscribe to Photon callbacks manually
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
        UpdatePlayerCount();
    }

    void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
    }

    // Called when Photon sends any event
    private void OnPhotonEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        // We only care about join/leave updates
        if (!PhotonNetwork.InRoom || playerCountText == null)
            return;

        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        if (!PhotonNetwork.InRoom || playerCountText == null)
        {
            playerCountText.text = "Not connected...";
            return;
        }

        int connectedPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        playerCountText.text = $"Players connected: {connectedPlayers} / {maxPlayers}";

        if (PhotonNetwork.IsMasterClient)
            playerCountText.text += "\nYou are the Master Client";
        else
            playerCountText.text += "\nWaiting for Master Client...";
    }
}
