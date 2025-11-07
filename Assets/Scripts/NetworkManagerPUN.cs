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

        if (PlayerPrefs.GetInt("ReturningToMainMenu", 0) == 1)
        {
            PlayerPrefs.DeleteKey("ReturningToMainMenu");
            Debug.Log("Skipping auto Photon reconnect (returned to Main Menu).");
            return;
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (!PhotonNetwork.InRoom)
        {
            JoinOrCreateRoom();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.name}");

        // Spawn points, player, camera setup...
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

        //  Cursor visibility and lock mode based on scene
        UpdateCursorForScene(scene.name);
    }

    
    // Shows cursor in shop, hides and locks it in gameplay
    
    private void UpdateCursorForScene(string sceneName)
    {
        if (sceneName == "ShopScene" || sceneName == "MainMenu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Cursor unlocked and visible (Shop/MainMenu).");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Cursor locked and hidden (Gameplay).");
        }
    }

    private void JoinOrCreateRoom()
    {
        Debug.Log("Joining or creating room...");
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("WizardDuelRoom", options, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        JoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name} | Players: {PhotonNetwork.CurrentRoom.PlayerCount}");

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
            Debug.LogError("Player prefab not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points in this scene.");
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
        Debug.Log($"Spawned player at {spawnPos}");

        //Ensure spells are finalized before applying
        if (SpellSelectionManager.Instance != null)
            SpellSelectionManager.Instance.EnsureDefaults();

        //Assign the selected spells (or defaults) to this player
        var spellManager = SpellSelectionManager.Instance;
        if (spellManager != null)
        {
            var loadout = spellManager.GetSelectedSpells(PhotonNetwork.LocalPlayer.ActorNumber);
            PlayerAttack attack = localPlayerInstance.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                // Assign using prefab-based method
                string spell1Name = loadout.spell1;
                string spell2Name = loadout.spell2;

                if (spell1Name == "Fireball") spell1Name = "FireballPrefab";
                if (spell2Name == "Fireball") spell2Name = "FireballPrefab";

                if (spell1Name == "Shockwave") spell1Name = "ShockWavePrefabUpDown";
                if (spell2Name == "Shockwave") spell2Name = "ShockWavePrefabUpDown";

                if (spell1Name == "HealingSpell") spell1Name = "HealingSpellPrefab";
                if (spell2Name == "HealingSpell") spell2Name = "HealingSpellPrefab";

                if (spell1Name == "Barrier") spell1Name = "BarrierSpellPrefab";
                if (spell2Name == "Barrier") spell2Name = "BarrierSpellPrefab";
                
                attack.AssignSpellsByName(loadout.spell1, loadout.spell2);

                // Debug the assigned prefabs
                if (attack.equippedSpellPrefabs != null)
                {
                    foreach (var s in attack.equippedSpellPrefabs)
                        if (s != null)
                            Debug.Log($"Loaded Spell Prefab: {s.name}");
                        else
                            Debug.LogWarning("Equipped spell prefab is null!");
                }
            }
        }

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

        // Multiplayer-safe mana slider setup
        SetupManaSlider(localPlayerInstance);
    }

    private void SetupManaSlider(GameObject player)
    {
        if (player == null) return;
        PlayerMana pm = player.GetComponent<PlayerMana>();
        if (pm == null || !pm.photonView.IsMine) return;

        
        Slider manaSlider = GameObject.FindWithTag("ManaBar")?.GetComponent<Slider>();
        if (manaSlider != null)
            pm.SetupSlider(manaSlider);
        else
            Debug.LogWarning("Could not find a Slider with the tag 'ManaBar'");
    }

    public void QuitToMenu()
    {
        quittingToMenu = true;
        Debug.Log("Returning to Main Menu...");
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");

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
