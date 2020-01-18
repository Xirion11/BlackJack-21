using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] Sprite CardBack = null;
    [SerializeField] Sprite[] ClubSprites = null;
    [SerializeField] Sprite[] HeartSprites = null;
    [SerializeField] Sprite[] SpadeSprites = null;
    [SerializeField] Sprite[] DiamondSprites = null;
    [SerializeField] Card[] DefaultDeck = null;
    [SerializeField] int separatorIndex = 0;
    [SerializeField] GameObject Separator = null;
    [SerializeField] Transform SeparatorTransform = null;
    [SerializeField] List<Card> Deck;

    const int MAX_SUITS = 4;
    const int MAX_VALUE = 13;
    private Vector3 SeparatorPosition;

    public enum SUITS
    {
        CLUBS = 0,
        HEARTS,
        SPADES,
        DIAMONDS
    }

    public enum VALUES
    {
        ACE = 0,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING
    }

    public static DeckHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Deck = new List<Card>();
        SeparatorPosition = SeparatorTransform.position;
        PrepareNewDeck();
    }

    //Fill with default deck
    private void FillDeck()
    {
        Deck.Clear();
        Deck.AddRange(DefaultDeck);
        Deck.AddRange(DefaultDeck);
    }

    private void ShuffleDeck()
    {        
        Deck.Shuffle();
        int randomOffset = Random.Range(-5, 6);
        separatorIndex = DefaultDeck.Length + randomOffset;
    }

    public Card DrawCard()
    {
        Card result = null;

        if (Deck.Count > 0)
        {
            result = Deck[0];
            Deck.RemoveAt(0);
            separatorIndex -= 1;
            if(separatorIndex == 0)
            {
                GameHandler.Instance.OnSeparatorFound();
                Separator.SetActive(true);
                SetSeparatorAside();
            }
        }
        else
        {
            Debug.LogError("Deck is empty", this);
        }

        return result;
    }

    public Sprite GetCardSprite(Card card)
    {
        Sprite result = null;

        result = GetCardSprite(card.GetValue(), card.GetSuit());

        return result;
    }

    public Sprite GetCardSprite(VALUES _value, SUITS _suit)
    {
        Sprite result = null;

        int value = (int)_value;

        switch (_suit)
        {
            case SUITS.CLUBS:
                result = ClubSprites[value];
                break;

            case SUITS.HEARTS:
                result = HeartSprites[value];
                break;

            case SUITS.SPADES:
                result = SpadeSprites[value];
                break;

            case SUITS.DIAMONDS:
                result = DiamondSprites[value];
                break;
        }

        return result;
    }

    public void PrepareNewDeck()
    {
        Separator.SetActive(false);
        SeparatorTransform.position = SeparatorPosition;
        FillDeck();
        ShuffleDeck();
    }

    public void SetSeparatorAside()
    {
        float newY = SeparatorTransform.position.y - 2f;
        SeparatorTransform.DOMoveY(newY, 0.2f);
    }

    public bool IsCurrentDeckOver()
    {
        bool result = false;

        result = separatorIndex <= 0;

        return result;
    }
}

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
