using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards")]
public class Card : ScriptableObject
{
    public DeckHandler.SUITS suit;
    public DeckHandler.VALUES value;

    public DeckHandler.VALUES GetValue()
    {
        return value;
    }

    public DeckHandler.SUITS GetSuit()
    {
        return suit;
    }

    public int GetCardValue()
    {
        int result;

        result = 1 + (int)value;

        switch (value)
        {
            case DeckHandler.VALUES.TEN:
            case DeckHandler.VALUES.JACK:
            case DeckHandler.VALUES.QUEEN:
            case DeckHandler.VALUES.KING:
                result = 10;
                break;
        }

        return result;
    }
}
