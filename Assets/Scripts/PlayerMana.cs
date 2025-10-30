using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerMana : MonoBehaviourPun
{
    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float rechargeRate = 10f;
    public float rechargeDelay = 1f;

    [Header("Events")]
    public UnityEvent<float> onManaChanged; // passes normalized mana 0-1

    [HideInInspector] public float currentMana;
    private float lastManaSpentTime;

    [Header("UI")]
    public Slider manaSlider;

    public float CurrentMana => currentMana;

    void Awake()
    {
        // Start with full mana
        currentMana = maxMana;
    }

    void Start()
    {
        // Update UI if slider is already assigned
        UpdateUI();
    }

    void Update()
    {
        // Only local player handles recharge
        if (!photonView.IsMine) return;

        RechargeMana();
    }

    private void RechargeMana()
    {
        if (Time.time - lastManaSpentTime < rechargeDelay) return;

        if (currentMana < maxMana)
        {
            currentMana += rechargeRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
            UpdateUI();
        }
    }

    /// <summary>
    /// Attempt to spend mana. Returns true if successful.
    /// </summary>
    public bool UseMana(float amount)
    {
        if (!photonView.IsMine) return false;
        if (currentMana < amount) return false;

        currentMana -= amount;
        lastManaSpentTime = Time.time;
        UpdateUI();
        return true;
    }

    /// <summary>
    /// Instantly refill mana to max.
    /// </summary>
    public void RefillMana()
    {
        currentMana = maxMana;
        UpdateUI();
    }

    private void UpdateUI()
    {
        float normalized = currentMana / maxMana;
        onManaChanged?.Invoke(normalized);

        if (manaSlider != null)
            manaSlider.value = normalized;
    }

    /// <summary>
    /// Assigns the UI slider for the local player after spawn.
    /// </summary>
    public void SetupSlider(Slider slider)
    {
        if (!photonView.IsMine) return;
        manaSlider = slider;
        UpdateUI(); // ensure full bar immediately
    }
}
