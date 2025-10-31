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

    public void SelectSpell(string spellName)
    {
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
            // Replace the oldest or implement unselect logic
            selected1 = selected2;
            selected2 = spellName;
        }

        Debug.Log($"Selected spells: {selected1}, {selected2}");
        
    }
}
