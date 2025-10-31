using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour
{
    public string spellName; // e.g. "Fireball"
    private Button button;
    private bool isSelected = false;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (isSelected)
        {
            SpellSelectionManager.Instance.UnchooseSpell(spellName);
            isSelected = false;
            // (optional) change button color to unselected
        }
        else
        {
            SpellSelectionManager.Instance.ChooseSpell(spellName);
            isSelected = true;
            // (optional) change button color to selected
        }
    }
}
