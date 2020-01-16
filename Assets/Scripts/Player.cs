using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private List<Card> m_hand = null;    

    private int m_handValue = 0;
    private bool m_aceInHand = false;

    string templateAceValue = "{0}/{1}";

    const int BLACKJACK = 21;
    const int LETTER_VALUE = 10;

    private void Start()
    {

    }

    public void AddCard(Card newCard)
    {
        m_hand.Add(newCard);
        UpdateHandValue();
    }

    private string UpdateHandValue()
    {
        m_handValue = CalculateHandValue();
        
        string valueString = m_handValue.ToString();

        if (m_aceInHand)
        {
            int aceHandValue = m_handValue + LETTER_VALUE;

            if (aceHandValue <= BLACKJACK)
            {
                valueString = string.Format(templateAceValue, m_handValue, aceHandValue);
            }
        }

        return valueString;
    }

    private int CalculateHandValue()
    {
        int result = 0;
        int value = 0;

        for (int i = 0; i < m_hand.Count; i++)
        {
            value = m_hand[i].GetCardValue();
            result += value;

            //If the value is an Ace update flag
            if(value == 1)
            {
                m_aceInHand = true;
            }
        }

        return result;
    }
}
