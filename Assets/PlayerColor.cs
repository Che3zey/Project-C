using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    private SpriteRenderer sr;

    // Call this when spawning a player
    public void SetPlayerColor(int playerNumber)
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        switch (playerNumber)
        {
            case 1:
                sr.color = Color.red;
                break;
            case 2:
                sr.color = Color.blue;
                break;
            case 3:
                sr.color = Color.green;
                break;
            case 4:
                sr.color = Color.yellow;
                break;
            default:
                sr.color = Color.white;
                break;
        }
    }
}
