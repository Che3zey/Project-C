using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class NetworkManagerPUN : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Main Camera")]
    public Camera mainCamera;

    private Transform[] spawnPoints;

    void Awake()
    {
        // Find all spawn points in the scene by tag
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .Select(go => go.transform)
            .OrderBy(t => t.name) // optional: sort by name for consistency
            .ToArray();

        if (spawnPoints.Length == 0)
            Debug.LogError("No spawn points found! Make sure they are tagged 'SpawnPoint'.");
    }

    void Start()
    {
        // Connect to Photon if not already connected
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server!");

        // Join or create a room
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("WizardDuelRoom", options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room! Spawning local player...");
        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        if (playerPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Cannot spawn player: missing prefab or spawn points.");
            return;
        }

        // Determine spawn index based on Photon actor number
        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length)
            spawnIndex = 0;

        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        // Spawn the networked player
        GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

        // Assign color based on index
        PlayerColor colorScript = newPlayer.GetComponent<PlayerColor>();
        if (colorScript != null)
            colorScript.SetPlayerColor(spawnIndex + 1);

        // Set camera to follow the local player only
        PhotonView pv = newPlayer.GetComponent<PhotonView>();
        if (mainCamera != null && pv != null && pv.IsMine)
        {
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(newPlayer); // ensures Z stays fixed and smooth follow
            }
        }
    }
}
