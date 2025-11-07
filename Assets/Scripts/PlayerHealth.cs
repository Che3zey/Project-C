using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    private int currentHealth;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private bool isKnocked = false;

    [Header("UI")]
    public Slider healthSlider;

    // ✅ Added: property to check if the player is alive
    public bool IsAlive => currentHealth > 0;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();

        if (photonView.IsMine)
        {
            GameObject sliderObj = GameObject.Find("HealthSlider");
            if (sliderObj != null)
                healthSlider = sliderObj.GetComponent<Slider>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int amount, Vector2 knockbackDir, float knockbackForce)
    {
        if (!photonView.IsMine) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // prevent negative HP
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();

        if (knockbackDir != Vector2.zero && rb != null)
            StartCoroutine(ApplyKnockback(knockbackDir, knockbackForce));
    }

    private IEnumerator ApplyKnockback(Vector2 dir, float force)
    {
        isKnocked = true;
        movement.enabled = false;
        rb.velocity = Vector2.zero;
        rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.15f);
        rb.velocity = Vector2.zero;
        movement.enabled = true;
        isKnocked = false;
    }

    [PunRPC]
    public void HealPlayer(int amount)
    {
        if (!photonView.IsMine) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        int actualHealed = currentHealth - oldHealth;

        Debug.Log($"{gameObject.name} healed for {actualHealed}! Current HP: {currentHealth}/{maxHealth}");

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        if (photonView.IsMine)
        {
            DeathUIManager.Instance.ShowDeathMessage("You died...");

            if (Camera.main != null)
                Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);

            // Instead of destroying immediately, we could delay slightly if needed
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
