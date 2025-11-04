using UnityEngine;

public class DeathUIManager : MonoBehaviour
{
    public static DeathUIManager Instance;
    public Canvas deathCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        deathCanvas.gameObject.SetActive(false); // hide by default
    }

    public void ShowDeathMessage(string text)
    {
        deathCanvas.gameObject.SetActive(true);
        var txt = deathCanvas.GetComponentInChildren<UnityEngine.UI.Text>();
        if (txt != null) txt.text = text;
    }
}
