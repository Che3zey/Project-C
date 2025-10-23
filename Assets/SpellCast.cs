using UnityEngine;

public class SpellCast : MonoBehaviour
{
    public GameObject spellPrefab;
    public float spellSpeed = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject spell = Instantiate(spellPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = spell.GetComponent<Rigidbody2D>();
            rb.velocity = transform.up * spellSpeed;
        }
    }
}
