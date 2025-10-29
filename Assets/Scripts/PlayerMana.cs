using UnityEngine;
using UnityEngine.Events;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public float maxMana = 100f;            // Maximum mana
    public float rechargeRate = 10f;        // Mana per second
    public float rechargeDelay = 1f;        // Delay after spending mana

    [Header("Events")]
    public UnityEvent<float> onManaChanged; // Pass current mana (0-1) for UI

    [HideInInspector] public float currentMana;
    private float lastManaSpentTime;

    public float CurrentMana => currentMana;

    void Start()
    {
        currentMana = maxMana;
        onManaChanged?.Invoke(currentMana / maxMana);
    }

    void Update()
    {
        RechargeMana();
    }

    void RechargeMana()
    {
        if (Time.time - lastManaSpentTime < rechargeDelay) return;

        if (currentMana < maxMana)
        {
            currentMana += rechargeRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
            onManaChanged?.Invoke(currentMana / maxMana);
        }
    }

    /// <summary>
    /// Attempt to use mana. Returns true if successful.
    /// </summary>
    public bool UseMana(float amount)
    {
        if (currentMana < amount) return false;

        currentMana -= amount;
        lastManaSpentTime = Time.time;
        onManaChanged?.Invoke(currentMana / maxMana);
        return true;
    }

    /// <summary>
    /// Fully restores mana.
    /// </summary>
    public void RefillMana()
    {
        currentMana = maxMana;
        onManaChanged?.Invoke(currentMana / maxMana);
    }
}
