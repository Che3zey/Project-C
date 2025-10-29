using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NetworkSceneLoader : MonoBehaviourPun
{
    public Button button;

    void Start()
    {
        if (button == null) return;
        button.interactable = PhotonNetwork.IsMasterClient;
    }

    void Update()
    {
        if (button == null) return;
        button.interactable = PhotonNetwork.IsMasterClient;
    }

    public void LoadScene(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (string.IsNullOrEmpty(sceneName)) return;

        PhotonNetwork.LoadLevel(sceneName);
    }
}
