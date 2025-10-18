using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DeckHandler : MonoBehaviour
{
    [SerializeField] private Sprite[] ClubSprites = null;
    [SerializeField] private Sprite[] HeartSprites = null;
    [SerializeField] private Sprite[] SpadeSprites = null;
    [SerializeField] private Sprite[] DiamondSprites = null;
    [SerializeField] private Card[] DefaultDeck = null;
    [SerializeField] private int separatorIndex = 0;
    [SerializeField] private GameObject Separator = null;
    [SerializeField] private Transform SeparatorTransform = null;
    [SerializeField] private List<Card> Deck;

    private const int MAX_SUITS = 4;
    private const int MAX_VALUE = 13; //Number of cards per suit
    private const int MIN_OFFSET = 78; //Offset of cards from the center of the deck
    private const int MAX_OFFSET = 104;  //To accomodate the deck separator (2 decks from bottom max)
    private Vector3 SeparatorPosition; //Separator initial position

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

        //Save the separator initial position
        SeparatorPosition = SeparatorTransform.position;

        //Prepare a new deck for the game
        PrepareNewDeck();
    }

    //Fill with default deck
    private void FillDeck()
    {
        /** 
         * We clear the current play deck and add 2 decks.
         * This avoids the case where the deck runs out of cards during a match.
         */ 
        Deck.Clear();
        Deck.AddRange(DefaultDeck);
        Deck.AddRange(DefaultDeck);
        Deck.AddRange(DefaultDeck);
        Deck.AddRange(DefaultDeck);
        Deck.AddRange(DefaultDeck);
        Deck.AddRange(DefaultDeck);
    }

    private void ShuffleDeck()
    {        
        //We shuffle the play deck
        Deck.Shuffle();

        //We get a random position for the separator, close to the center but within the allowed offset
        int randomOffset = Random.Range(MIN_OFFSET, MAX_OFFSET);
        separatorIndex = Deck.Count - randomOffset;
    }

    //Get the top card on the deck and remove it
    public Card DrawCard()
    {
        Card result = null;

        //If there is at least a card in the deck
        if (Deck.Count > 0)
        {
            //Get the top card on the deck
            result = Deck[0];

            //Remove it from the deck
            Deck.RemoveAt(0);

            //The separator index moves one step closer to the top
            separatorIndex -= 1;

            //If the separator is in position 0 it means that it is the "next card"
            if(separatorIndex == 0)
            {
                //Notify the game handler. An explanation for the separator will be shown
                GameHandler.Instance.OnSeparatorFound();

                //Activate the separator so it is visible below the card to be drawn.
                Separator.SetActive(true);

                //Move the separator aside
                SetSeparatorAside();
            }
        }
        else
        {
            //It should never get to this case since we implemented 2 decks
            Debug.LogError("Deck is empty", this);
        }

        return result;
    }

    //Obtain the card sprite from a card
    public Sprite GetCardSprite(Card card)
    {
        Sprite result = null;

        //Get the correct sprite from a card value and suit
        result = GetCardSprite(card.GetValue(), card.GetSuit());

        return result;
    }

    /**
     * Get the correct sprite from a card value and suit.
     * The value will give us the index and the suit will indicate the right array
     */ 
    public Sprite GetCardSprite(VALUES _value, SUITS _suit)
    {
        Sprite result = null;

        int value = (int)_value;

        //Get the sprite from the right array
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

    //Prepare a new play deck
    public void PrepareNewDeck()
    {
        //Deactivate the separator and move it to its initial position
        Separator.SetActive(false);
        SeparatorTransform.position = SeparatorPosition;

        //Form a deck from 2 decks
        FillDeck();

        //Shuffle the deck
        ShuffleDeck();
    }

    //Animate the separator to move aside
    public void SetSeparatorAside()
    {
        const float separatorOffset = 2.0f;
        float newX = SeparatorTransform.position.x - separatorOffset;
        SeparatorTransform.DOMoveX(newX, Constants.QUICK_DELAY);
    }

    //If the separator has been found the deck needs to be shuffled again
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
