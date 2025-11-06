using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ShopTimer : MonoBehaviourPun
{
    public float selectionTime = 60f;
    public Text countdownText;              // For legacy UI
    public TextMeshProUGUI countdownTMP;    // Optional TMP version

    private float timer;
    private bool hasEnded = false;

    void Start()
    {
        timer = selectionTime;
        UpdateCountdownUI(timer);
    }

    void Update()
    {
        if (hasEnded) return;

        // Only Master Client updates the timer
        if (PhotonNetwork.IsMasterClient)
        {
            timer -= Time.deltaTime;

            if (timer > 0f)
            {
                // Broadcast the current timer to all players
                photonView.RPC(nameof(RPC_UpdateTimer), RpcTarget.All, timer);
            }
            else
            {
                hasEnded = true;
                timer = 0f;
                photonView.RPC(nameof(RPC_UpdateTimer), RpcTarget.All, timer);

                // Ensure all players have spells
                if (SpellSelectionManager.Instance != null)
                    SpellSelectionManager.Instance.EnsureDefaults();

                // Pick random scene once
                int sceneIndex = Random.Range(0, 3);
                string sceneName = "GameScene";
                if (sceneIndex == 1) sceneName = "GameScene1";
                else if (sceneIndex == 2) sceneName = "GameScene2";

                Debug.Log($"⏰ Time up! Loading {sceneName}");
                PhotonNetwork.LoadLevel(sceneName);
            }
        }
    }

    [PunRPC]
    void RPC_UpdateTimer(float currentTime)
    {
        UpdateCountdownUI(currentTime);
    }

    void UpdateCountdownUI(float currentTime)
    {
        int seconds = Mathf.CeilToInt(currentTime);
        string text = $"Time Remaining: {seconds}s";

        if (countdownText != null)
            countdownText.text = text;

        if (countdownTMP != null)
            countdownTMP.text = text;
    }
}
