using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 3;
    private int currentHealth;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private bool isKnocked = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    // Called via RPC from other clients or local fireballs
    [PunRPC]
    public void TakeDamage(int amount, Vector2 knockbackDir, float knockbackForce)
    {
        // Only the owner of this player handles health locally
        if (!photonView.IsMine) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took damage! Current HP: {currentHealth}");

        // Apply knockback if applicable
        if (knockbackDir != Vector2.zero && rb != null)
        {
            StartCoroutine(ApplyKnockback(knockbackDir, knockbackForce));
        }

        // Death check
        if (currentHealth <= 0)
        {
            Die();
        }
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

        // Destroy networked object for all clients
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
