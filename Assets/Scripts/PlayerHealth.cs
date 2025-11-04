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
        if (healthSlider != null)
            healthSlider.value = currentHealth;

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

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        if (photonView.IsMine)
        {
            // Show the death UI before destroying player
            DeathUIManager.Instance.ShowDeathMessage("You died...");

            // Optionally move camera to center of map
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
