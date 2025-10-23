using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Spawn Points (1-4)")]
    public Transform[] spawnPoints = new Transform[4];

    [Header("Main Camera")]
    public Camera mainCamera;

    private int playerCount = 0;

    void Start()
    {
        for (int i = 0; i < 4; i++)
            SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (playerCount >= 4)
        {
            Debug.LogWarning("Max 4 players reached!");
            return;
        }

        // Spawn the player
        GameObject newPlayer = Instantiate(playerPrefab, spawnPoints[playerCount].position, Quaternion.identity);

        // Assign color
        PlayerColor colorScript = newPlayer.GetComponent<PlayerColor>();
        if (colorScript != null)
            colorScript.SetPlayerColor(playerCount + 1);

        // 👇 Assign camera follow target for the first player only (for testing)
        if (playerCount == 0 && mainCamera != null)
        {
            CameraFollow camFollow = mainCamera.GetComponent<CameraFollow>();
            camFollow.target = newPlayer.transform;
        }

        playerCount++;
    }
}
