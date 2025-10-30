using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour
{
    public Spell spellPrefab;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (SpellSelectionManager.Instance == null) return;

        // Toggle selection
        if (!SpellSelectionManager.Instance.ChooseSpell(spellPrefab))
        {
            // If already chosen, unchoose it
            SpellSelectionManager.Instance.UnchooseSpell(spellPrefab);
        }
    }
}
