using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManagerPUN : MonoBehaviourPunCallbacks
{
    public static NetworkManagerPUN Instance;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Main Camera")]
    public Camera mainCamera;

    private Transform[] spawnPoints;
    private GameObject localPlayerInstance;

    // To detect intentional disconnections (e.g., when going to main menu)
    private bool quittingToMenu = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Auto-connect & join room when the manager appears
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("🔌 Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InRoom)
        {
            JoinOrCreateRoom();
        }
    }

    // Called every time a new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📜 Scene Loaded: {scene.name}");

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .Select(go => go.transform)
            .OrderBy(t => t.name)
            .ToArray();

        // If not connected, try again
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        // Auto-join a room if not already in one
        if (!PhotonNetwork.InRoom)
        {
            JoinOrCreateRoom();
            return;
        }

        // Spawn player only if the scene has spawn points
        if (spawnPoints.Length > 0)
        {
            SpawnLocalPlayer();
        }

        // Reattach camera if needed
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && localPlayerInstance != null)
        {
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            if (camFollow != null)
                camFollow.SetTarget(localPlayerInstance);
        }
    }

    private void JoinOrCreateRoom()
    {
        Debug.Log("🏠 Joining or creating room...");
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("WizardDuelRoom", options, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connected to Photon Master Server");
        JoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"🎮 Joined room: {PhotonNetwork.CurrentRoom.Name} | Players: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // Spawn only if in a scene that can spawn
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .Select(go => go.transform)
            .OrderBy(t => t.name)
            .ToArray();

        if (spawnPoints.Length > 0)
            SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ Player prefab not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ No spawn points in this scene.");
            return;
        }

        if (localPlayerInstance != null)
        {
            Debug.Log("Local player already exists, skipping spawn.");
            return;
        }

        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length)
            spawnIndex = 0;

        Vector3 spawnPos = spawnPoints[spawnIndex].position;
        localPlayerInstance = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        Debug.Log($"🧙 Spawned player at {spawnPos}");

        // Assign player color
        if (PlayerColorManager.Instance != null)
        {
            int colorIndex = PlayerColorManager.Instance.AssignColor(PhotonNetwork.LocalPlayer);
            PlayerColor colorScript = localPlayerInstance.GetComponent<PlayerColor>();
            if (colorScript != null && colorIndex != -1)
                colorScript.SetPlayerColor(colorIndex + 1);
        }

        // Set up camera
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            if (camFollow != null)
                camFollow.SetTarget(localPlayerInstance);
        }
    }

    // Called when quitting to main menu
    public void QuitToMenu()
    {
        quittingToMenu = true;
        Debug.Log("🏁 Returning to Main Menu...");
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"🔴 Disconnected from Photon: {cause}");

        if (!quittingToMenu)
        {
            // Attempt auto-reconnect if it was an unexpected disconnection
            Debug.Log("Reconnecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            quittingToMenu = false; // reset flag
        }
    }
}
