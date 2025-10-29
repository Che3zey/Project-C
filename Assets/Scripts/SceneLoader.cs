using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Name of the scene to load when this button is clicked.")]
    public string sceneName;

    /// <summary>
    /// Call this method from the button's OnClick event
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneLoader: No scene name set for this button!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
