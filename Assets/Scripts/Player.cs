using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private List<Card> m_hand = null;
    [SerializeField] private List<Card> m_splitHand = null;

    private bool m_aceInHand = false;
    private bool m_aceInSplitHand = false;

    private string templateAceValue = "{0}/{1}";

    private void Start()
    {
        //Initialize hand arrays
        m_hand = new List<Card>();
        m_splitHand = new List<Card>();
    }

    //Called from dealer
    public void AddCard(Card newCard, bool forSplitHand = false)
    {
        //Add a new card to the correct hand
        if (forSplitHand)
        {
            m_splitHand.Add(newCard);
        }
        else
        {
            m_hand.Add(newCard);
        }

        //Update hand value label
        UpdateHandValue(forSplitHand);

        //If we have blackjack
        if (PlayerHasBlackJack(forSplitHand))
        {
            //Norify game handler
            GameHandler.Instance.OnPlayerBlackJack(forSplitHand);
        }
    }

    //Move second card to split hand
    public void SplitHand()
    {
        m_splitHand.Add(m_hand[Constants.SECOND_CARD]);
        m_hand.RemoveAt(Constants.SECOND_CARD);
    }

    //Return the card in index "index" for the correct hand
    public Card GetCard(int index, bool forSplitHand = false)
    {
        Card result;

        if (forSplitHand)
        {
            result = m_splitHand[index];
        }
        else
        {
            result = m_hand[index];
        }

        return result;
    }

    //Update hand value label
    public string UpdateHandValue(bool forSplitHand = false)
    {
        //Get the hand value without taking one ace into account
        int handValueWithoutAce = CalculateHandValue(forSplitHand, true);

        //Is there an ace in hand
        bool aceInHand = IsAceInHand(forSplitHand);

        string valueString = string.Empty;

        //If there is an ace in hand
        if (aceInHand)
        {
            //Add the omitted ace value again
            int aceHandValue = handValueWithoutAce + Constants.ACE_MIN_VALUE;

            //If by increasing the letter value we are still within 21
            if (aceHandValue + Constants.LETTER_VALUE <= Constants.BLACKJACK)
            {
                //Show possible values. IE 9/19
                int lowerLimit = aceHandValue;
                int higherLimit = aceHandValue + Constants.LETTER_VALUE;
                valueString = string.Format(templateAceValue, lowerLimit, higherLimit);
            }

            //If that would cause the hand to bust just set the hand value as normal
            else
            {
                valueString = aceHandValue.ToString();
            }
        }
        else
        {
            valueString = handValueWithoutAce.ToString();
        }

        return valueString;
    }

    //Calculate hand value 
    public int CalculateHandValue(bool forSplitHand = false, bool withoutAce = false)
    {
        int result = 0;
        int value = 0;

        //Get how many cards there are in this hand
        int length = forSplitHand ? m_splitHand.Count : m_hand.Count;

        //This will let us omit one ace in hand
        bool aceAlreadyExcluded = false;

        //Add each card value to the result
        for (int i = 0; i < length; i++)
        {
            if (forSplitHand)
            {
                value = m_splitHand[i].GetCardValue();
            }
            else
            {
                value = m_hand[i].GetCardValue();
            }

            result += value;

            //If this is an ace and we have not omitted any ace yet
            if (value == 1 && withoutAce && !aceAlreadyExcluded)
            {
                //Omit this value and update the flag
                result -= value;
                aceAlreadyExcluded = true;
            }

            //If the value is an Ace update the ace in hand flag
            if(value == Constants.ACE_MIN_VALUE)
            {
                if (forSplitHand)
                {
                    m_aceInSplitHand = true;
                }
                else
                {
                    m_aceInHand = true;
                }
            }
        }

        //If this is not being calculated for the label we can return the whole value
        if (!withoutAce)
        {
            if ((forSplitHand && m_aceInSplitHand) || !forSplitHand && m_aceInHand)
            {
                result += Constants.LETTER_VALUE;

                if (result > Constants.BLACKJACK)
                {
                    result -= Constants.LETTER_VALUE;
                }
            }
        }

        return result;
    }

    //Is there an ace in hand?
    public bool IsAceInHand(bool forSplitHand = false)
    {
        bool result = false;

        if (forSplitHand)
        {
            result = m_aceInSplitHand;
        }
        else
        {
            result = m_aceInHand;
        }

        return result;
    }

    //Is a split option available
    public bool IsSplitAvailable()
    {
        bool result = false;

        //If both cards have the same value and we have not splitted before
        result = (m_hand[Constants.FIRST_CARD].GetCardValue() == m_hand[Constants.SECOND_CARD].GetCardValue()) && m_splitHand.Count == 0;

        return result;
    }

    //Does player has black jack?
    public bool PlayerHasBlackJack(bool forSplitHand = false)
    {
        bool result = false;

        //Get player hand value, ace status and cards in hand
        int handValue = CalculateHandValue(forSplitHand);
        bool aceInHand = IsAceInHand(forSplitHand);
        int cardsInHand = forSplitHand ? m_splitHand.Count : m_hand.Count;

        //If player has 21, an ace in hand and only two cards then yes
        result = handValue == Constants.BLACKJACK && aceInHand && cardsInHand == 2;

        return result;
    }

    //Is hand busted?
    public bool IsHandBusted(bool forSplitHand = false)
    {
        int handValue = CalculateHandValue(forSplitHand);

        //If hand value is more than 21 then yes
        return handValue > Constants.BLACKJACK;
    }

    //Remove all cards in hand and reset flags
    public void ClearHand()
    {
        m_aceInHand = false;
        m_aceInSplitHand = false;
        m_hand.Clear();
        m_splitHand.Clear();
    }

    //If there is at least one card in split hand then yes
    public bool HasPlayerSplitted()
    {
        return m_splitHand.Count > 0;
    }
}
