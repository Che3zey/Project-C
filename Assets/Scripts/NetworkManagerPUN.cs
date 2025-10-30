using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class NetworkManagerPUN : MonoBehaviourPunCallbacks
{
    public static NetworkManagerPUN Instance;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Main Camera")]
    public Camera mainCamera;

    private Transform[] spawnPoints;
    private GameObject localPlayerInstance;

    private bool quittingToMenu = false;

    void Awake()
    {
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📜 Scene Loaded: {scene.name}");

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .Select(go => go.transform)
            .OrderBy(t => t.name)
            .ToArray();

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        if (!PhotonNetwork.InRoom)
        {
            JoinOrCreateRoom();
            return;
        }

        if (spawnPoints.Length > 0)
        {
            SpawnLocalPlayer();
        }

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

        // 🔹 Multiplayer-safe mana slider assignment
        SetupManaSlider(localPlayerInstance);
    }

    private void SetupManaSlider(GameObject player)
    {
        if (player == null) return;
        PlayerMana pm = player.GetComponent<PlayerMana>();
        if (pm == null) return;

        // Only assign for local player
        if (!pm.photonView.IsMine) return;

        // Find slider in scene (assumes one local HUD slider)
        Slider slider = GameObject.FindObjectOfType<Slider>();
        if (slider != null)
        {
            pm.SetupSlider(slider);
        }
    }

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
            Debug.Log("Reconnecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            quittingToMenu = false;
        }
    }
}
