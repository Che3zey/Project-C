using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using System.Collections;

public class GameEndManager : MonoBehaviourPunCallbacks
{
    [Header("Timer Settings")]
    public float matchDuration = 60f;
    public Text countdownText;
    public TextMeshProUGUI countdownTMP;

    private double startTime;
    private bool hasEnded = false;
    private Coroutine checkCoroutine;

    void Start()
    {
        // Only master sets the start time and begins the match check
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            photonView.RPC(nameof(RPC_SetStartTime), RpcTarget.AllBuffered, startTime);
            checkCoroutine = StartCoroutine(CheckMatchEnd());
        }
    }

    [PunRPC]
    void RPC_SetStartTime(double networkStartTime)
    {
        startTime = networkStartTime;
        StartCoroutine(UpdateCountdownUIRoutine());
    }

    private IEnumerator UpdateCountdownUIRoutine()
    {
        while (!hasEnded)
        {
            UpdateCountdownUI();
            yield return null;
        }
    }

    private IEnumerator CheckMatchEnd()
    {
        yield return new WaitForSeconds(1f);

        while (!hasEnded)
        {
            double elapsed = PhotonNetwork.Time - startTime;
            double remaining = matchDuration - elapsed;

            int aliveCount = CountAlivePlayers();

            Debug.Log($"[GameEndManager] Alive players: {aliveCount}");

            if (aliveCount <= 1)
            {
                EndMatch("Only one player alive!");
                yield break;
            }

            if (remaining <= 0)
            {
                EndMatch("Time’s up!");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateCountdownUI()
    {
        double elapsed = PhotonNetwork.Time - startTime;
        double remaining = Mathf.Max(0, (float)(matchDuration - elapsed));

        int seconds = Mathf.CeilToInt((float)remaining);
        string text = $"Match Ends In: {seconds}s";

        if (countdownText != null)
            countdownText.text = text;

        if (countdownTMP != null)
            countdownTMP.text = text;
    }

    private int CountAlivePlayers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        int alive = 0;

        foreach (var player in players)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null && health.IsAlive)
                alive++;
        }

        return alive;
    }

    private void EndMatch(string reason)
    {
        if (hasEnded) return;

        Debug.Log($"🏁 Match ending: {reason}");

        if (checkCoroutine != null)
            StopCoroutine(checkCoroutine);

        // Only send the RPC; let the RPC handle hasEnded and scene loading
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_EndMatch), RpcTarget.AllBufferedViaServer, reason);
        }
        else
        {
            StartCoroutine(LoadShopScene());
        }
    }

    [PunRPC]
    private void RPC_EndMatch(string reason)
    {
        // All clients, including master, should execute this
        if (hasEnded) return;

        hasEnded = true;

        Debug.Log($"➡️ Match Ended: {reason}");
        StartCoroutine(LoadShopScene());
    }

    private IEnumerator LoadShopScene()
    {
        Debug.Log($"🕒 Loading ShopScene in 1.5 seconds... Master? {PhotonNetwork.IsMasterClient}");
        yield return new WaitForSeconds(1.5f);

        // Only master actually loads the scene; others sync automatically
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("🟢 Master loading ShopScene now!");
            PhotonNetwork.LoadLevel("ShopScene");
        }
    }
}
