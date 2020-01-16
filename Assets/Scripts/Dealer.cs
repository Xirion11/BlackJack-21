using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Dealer : MonoBehaviour
{
    [SerializeField] private Player m_player = null;
    [SerializeField] private List<Card> m_hand = null;
    [SerializeField] private Sprite m_cardBack = null;

    [Header("Cards References")]
    [SerializeField] private Transform DeckTopCard;
    [SerializeField] private GameObject[] PlayerCardsObjects;
    [SerializeField] private GameObject[] DealerCardsObjects;
    [SerializeField] private Transform[] PlayerCardsTransforms;
    [SerializeField] private Transform[] DealerCardsTransforms;
    [SerializeField] private Image[] PlayerCardsImages;
    [SerializeField] private Image[] DealerCardsImages;

    private int currentPlayerCard = 0;
    private int currentDealerCard = 0;

    private int m_handValue = 0;
    private bool m_aceInHand = false;
    private bool m_isInitialDeal = true;

    string templateAceValue = "{0}/{1}";

    const int BLACKJACK = 21;
    const int LETTER_VALUE = 10;
    const int LIMIT_VALUE = 17;

    private void Start()
    {
        m_hand = new List<Card>();
    }
    
    public void DealInitialCards()
    {
        StartCoroutine(DealCardToPlayer());
        StartCoroutine(DealCardToDealer(0.4f));
        StartCoroutine(DealCardToPlayer(0.8f));
        StartCoroutine(DealCardToDealer(1.2f));
    }

    public void DealNewCardToPlayer()
    {
        StartCoroutine(DealCardToPlayer());
    }

    private void RestoreDeckTopCard()
    {
        DeckTopCard.localScale = Vector3.one;
    }

    IEnumerator DealCardToPlayer(float delay = 0f)
    {
        if(delay > 0f)
        {
            yield return Yielders.WaitForSeconds(delay);
        }

        DeckTopCard.DOScale(Vector3.zero, 0.2f);
        yield return Yielders.WaitForSeconds(0.2f);

        int tmpIndex = currentPlayerCard;

        m_player.AddCard(DeckHandler.Instance.DrawCard());
        PlayerCardsObjects[currentPlayerCard].SetActive(true);
        PlayerCardsTransforms[tmpIndex].DOScale(Vector3.one, 0.2f)
            .OnComplete(() => ShowPlayerCard(tmpIndex));
        RestoreDeckTopCard();        
        SFXHandler.Instance.PlayCardSound();

        currentPlayerCard += 1;

        if (!m_isInitialDeal)
        {
            GameHandler.Instance.OnPlayerCardDrawn();
        }
    }

    IEnumerator DealCardToDealer(float delay = 0f)
    {
        if (delay > 0f)
        {
            yield return Yielders.WaitForSeconds(delay);
        }

        DeckTopCard.DOScale(Vector3.zero, 0.2f);
        yield return Yielders.WaitForSeconds(0.2f);

        int tmpIndex = currentDealerCard;

        AddCard(DeckHandler.Instance.DrawCard());
        DealerCardsObjects[currentDealerCard].SetActive(true);
        DealerCardsTransforms[tmpIndex].DOScale(Vector3.one, 0.2f)
            .OnComplete(() => ShowDealerCard(tmpIndex));
        RestoreDeckTopCard();
        SFXHandler.Instance.PlayCardSound();

        currentDealerCard += 1;
    }

    private void ShowPlayerCard(int index)
    {
        PlayerCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_player.GetCard(index));
        GameHandler.Instance.UpdatePlayerHandValue();
    }

    private void ShowDealerCard(int index)
    {
        if (index == 1 && m_isInitialDeal)
        {
            m_isInitialDeal = false;
            GameHandler.Instance.OnInitialHandsReady();
        }
        else
        {
            DealerCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_hand[index]);
            GameHandler.Instance.UpdateDealerHandValue();
        }
    }

    public void AddCard(Card newCard)
    {
        m_hand.Add(newCard);
        UpdateHandValue();
    }

    public string UpdateHandValue()
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

    public int CalculateHandValue()
    {
        int result = 0;
        int value = 0;

        for (int i = 0; i < m_hand.Count; i++)
        {
            value = m_hand[i].GetCardValue();
            result += value;

            //If the value is an Ace update flag
            if (value == 1)
            {
                m_aceInHand = true;
            }
        }

        return result;
    }

    public void OnPlayerBusted()
    {
        //Reveal cards
        ShowDealerCard(1);
    }

    public void DrawHand()
    {
        StartCoroutine(DrawHandRoutine());
    }

    IEnumerator DrawHandRoutine()
    {
        ShowDealerCard(1);

        //Stay on 17, Draw to 16
        while (m_handValue < LIMIT_VALUE)
        {
            StartCoroutine(DealCardToDealer());

            yield return Yielders.WaitForSeconds(0.4f);
        }

        GameHandler.Instance.OnMatchEnded();
    }

    public void CleanTable()
    {
        m_player.ClearHand();
        ClearHand();

        for (int i = 0; i < PlayerCardsObjects.Length; i++)
        {
            PlayerCardsObjects[i].SetActive(false);
            PlayerCardsTransforms[i].localScale = Vector3.zero;
            PlayerCardsImages[i].sprite = m_cardBack;
        }

        for (int i = 0; i < DealerCardsObjects.Length; i++)
        {
            DealerCardsObjects[i].SetActive(false);
            DealerCardsTransforms[i].localScale = Vector3.zero;
            DealerCardsImages[i].sprite = m_cardBack;
        }

        m_isInitialDeal = true;
        currentPlayerCard = 0;
        currentDealerCard = 0;
    }

    public bool IsHandBusted()
    {
        int handValue = CalculateHandValue();
        return handValue > BLACKJACK;
    }

    private void ClearHand()
    {
        m_hand.Clear();
    }
}
