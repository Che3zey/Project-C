using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // only needed if you’re using TextMeshProUGUI

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
        UpdateCountdownUI();
    }

    void Update()
    {
        // Only Master Client runs the timer logic
        if (!PhotonNetwork.IsMasterClient || hasEnded)
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
        {
            UpdateCountdownUI();
        }
        else
        {
            // Prevent re-triggering
            hasEnded = true;
            timer = 0f;
            UpdateCountdownUI();

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

    void UpdateCountdownUI()
    {
        int seconds = Mathf.CeilToInt(timer);
        string text = $"Time Remaining: {seconds}s";

        if (countdownText != null)
            countdownText.text = text;

        if (countdownTMP != null)
            countdownTMP.text = text;
    }
}
