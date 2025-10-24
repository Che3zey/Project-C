using UnityEngine;
using Photon.Pun;

public class PlayerColor : MonoBehaviourPun
{
    private SpriteRenderer sr;

    void Awake()
    {
        // Cache the SpriteRenderer from the child
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("PlayerColor: No SpriteRenderer found in children!");
        }
    }

    // Call this when spawning a player
    public void SetPlayerColor(int playerNumber)
    {
        if (sr == null) return;

        // Use RPC so the color change propagates to all clients
        photonView.RPC(nameof(RPC_SetColor), RpcTarget.AllBuffered, playerNumber);
    }

    [PunRPC]
    private void RPC_SetColor(int playerNumber)
    {
        if (sr == null) return;

        switch (playerNumber)
        {
            case 1: sr.color = Color.red; break;
            case 2: sr.color = Color.blue; break;
            case 3: sr.color = Color.green; break;
            case 4: sr.color = Color.yellow; break;
            default: sr.color = Color.white; break;
        }
    }
}
