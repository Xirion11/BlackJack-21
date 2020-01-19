using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private List<Card> m_hand = null;
    [SerializeField] private List<Card> m_splitHand = null;

    private int m_handValue = 0;
    private int m_splitHandValue = 0;
    private bool m_aceInHand = false;
    private bool m_aceInSplitHand = false;

    string templateAceValue = "{0}/{1}";

    const int BASEJACK = 11;
    const int BLACKJACK = 21;
    const int LETTER_VALUE = 10;
    const int FIRST_CARD = 0;
    const int SECOND_CARD = 1;

    private void Start()
    {
        m_hand = new List<Card>();
        m_splitHand = new List<Card>();
    }

    public void AddCard(Card newCard, bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            m_splitHand.Add(newCard);
        }
        else
        {
            m_hand.Add(newCard);
        }

        UpdateHandValue(forSplitHand);

        if (PlayerHasBlackJack(forSplitHand))
        {
            GameHandler.Instance.OnPlayerBlackJack(forSplitHand);
        }
    }

    public void SplitHand()
    {
        m_splitHand.Add(m_hand[SECOND_CARD]);
        m_hand.RemoveAt(SECOND_CARD);
    }

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

    public string UpdateHandValue(bool forSplitHand = false)
    {
        int handValueWithoutAce = CalculateHandValue(forSplitHand, true);

        bool aceInHand = IsAceInHand(forSplitHand);

        string valueString = string.Empty;

        if (aceInHand)
        {
            int aceHandValue = handValueWithoutAce + 1;

            if (aceHandValue + LETTER_VALUE <= BLACKJACK)
            {
                int lowerLimit = aceHandValue;
                int higherLimit = aceHandValue + LETTER_VALUE;
                valueString = string.Format(templateAceValue, lowerLimit, higherLimit);
            }
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

    public int CalculateHandValue(bool forSplitHand = false, bool withoutAce = false)
    {
        int result = 0;
        int value = 0;
        int length = forSplitHand ? m_splitHand.Count : m_hand.Count;
        bool aceAlreadyExcluded = false;

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

            if (value == 1 && withoutAce && !aceAlreadyExcluded)
            {
                result -= value;
                aceAlreadyExcluded = true;
            }

            //If the value is an Ace update flag
            if(value == 1)
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

        if (!withoutAce)
        {
            if ((forSplitHand && m_aceInSplitHand) || !forSplitHand && m_aceInHand)
            {
                result += LETTER_VALUE;

                if (result > BLACKJACK)
                {
                    result -= LETTER_VALUE;
                }
            }
        }

        return result;
    }

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

    public bool IsSplitAvailable()
    {
        bool result = false;

        result = (m_hand[0].GetCardValue() == m_hand[1].GetCardValue()) && m_splitHand.Count == 0;

        return result;
    }

    public bool PlayerHasBlackJack(bool forSplitHand = false)
    {
        bool result = false;
        int handValue = CalculateHandValue(forSplitHand);
        bool aceInHand = IsAceInHand(forSplitHand);
        int cardsInHand = forSplitHand ? m_splitHand.Count : m_hand.Count;

        result = handValue == BLACKJACK && aceInHand && cardsInHand == 2;

        return result;
    }

    public bool IsHandBusted(bool forSplitHand = false)
    {
        int handValue = CalculateHandValue(forSplitHand);
        return handValue > BLACKJACK;
    }

    public void ClearHand()
    {
        m_aceInHand = false;
        m_aceInSplitHand = false;
        m_hand.Clear();
        m_splitHand.Clear();
    }

    public bool HasPlayerSplitted()
    {
        return m_splitHand.Count > 0;
    }
}
