using UnityEngine;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance;

    private string selected1 = null;
    private string selected2 = null;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        selected1 = null;
        selected2 = null;

        if (SpellSelectionManager.Instance != null)
            SpellSelectionManager.Instance.ClearSelections();

        // Unlock cursor for shop UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("🛍 Shop initialized. Cursor unlocked, selections cleared.");
    }

    public void SelectSpell(string spellName)
    {
        // Prevent selecting the same spell twice
        if (selected1 == spellName || selected2 == spellName)
        {
            Debug.Log("🔁 Spell already selected, ignoring.");
            return;
        }

        if (selected1 == null)
        {
            selected1 = spellName;
        }
        else if (selected2 == null)
        {
            selected2 = spellName;
        }
        else
        {
            // Replace oldest spell
            selected1 = selected2;
            selected2 = spellName;
        }

        // Sync to SpellSelectionManager
        if (SpellSelectionManager.Instance != null)
        {
            SpellSelectionManager.Instance.chosenSpell1 = selected1;
            SpellSelectionManager.Instance.chosenSpell2 = selected2;
        }

        Debug.Log($"Selected spells: {selected1}, {selected2}");
    }

    public void UnselectSpell(string spellName)
    {
        if (selected1 == spellName) selected1 = null;
        else if (selected2 == spellName) selected2 = null;

        if (SpellSelectionManager.Instance != null)
        {
            SpellSelectionManager.Instance.chosenSpell1 = selected1;
            SpellSelectionManager.Instance.chosenSpell2 = selected2;
        }

        Debug.Log($"Unselected spell: {spellName}");
    }
}
