using UnityEngine;
using UnityEngine.UI;

public class ManaSliderUI : MonoBehaviour
{
    [Header("References")]
    public PlayerMana playerMana;   // The PlayerMana script on the player
    public Slider manaSlider;       // The UI Slider component

    void Start()
    {
        if (manaSlider != null)
        {
            manaSlider.minValue = 0f;
            manaSlider.maxValue = 1f;  // Keep normalized 0–1 range
        }
    }

    void Update()
    {
        if (playerMana != null && manaSlider != null)
        {
            // Convert 0–maxMana → 0–1
            float normalizedMana = playerMana.currentMana / playerMana.maxMana;
            manaSlider.value = normalizedMana;
        }
    }
}
