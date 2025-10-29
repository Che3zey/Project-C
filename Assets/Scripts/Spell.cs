using UnityEngine;
using Photon.Pun;

public abstract class Spell : MonoBehaviourPun
{
    public string spellName;
    public float manaCost;
    public float cooldown;

    public abstract void Cast(GameObject caster);
}
