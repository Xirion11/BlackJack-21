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

    [Header("Split Hand")]
    [SerializeField] private GameObject[] PlayerSplitCardsObjects;
    [SerializeField] private Transform[] PlayerSplitCardsTransforms;
    [SerializeField] private Image[] PlayerSplitCardsImages;
    [SerializeField] private HorizontalLayoutGroup baseCardsLayout;
    [SerializeField] private Image HandIndicatorImage;
    [SerializeField] private RectTransform HandIndicatorTransform;
    [SerializeField] private RectTransform HandIndicatorSecondPosition;

    private int currentPlayerCard = 0;
    private int currentPlayerSplitCard = 0;
    private int currentDealerCard = 0;
    private int currentHand = 0;

    private int m_handValue = 0;
    private bool m_aceInHand = false;
    private bool m_isInitialDeal = true;
    private bool m_isBlackJack = false;
    private bool m_isSplitActive = false;

    string templateAceValue = "{0}/{1}";

    const int BASEJACK = 11;
    const int BLACKJACK = 21;
    const int LETTER_VALUE = 10;
    const int LIMIT_VALUE = 17;
    const int FIRST = 0;
    const int SECOND = 1;
    private const int DEFAULT_SPACING = 10;
    private const int SPLIT_SPACING = -90;

    Vector3 InitialHandIndicatorPosition;

    private void Start()
    {
        m_hand = new List<Card>();
        InitialHandIndicatorPosition = HandIndicatorTransform.position;
    }
    
    public void DealInitialCards()
    {
        StartCoroutine(DealInitialCardsRoutine());
    }

    IEnumerator DealInitialCardsRoutine()
    {
        m_isBlackJack = false;

        StartCoroutine(DealCardToPlayer());
        yield return Yielders.WaitForSeconds(0.4f);

        StartCoroutine(DealCardToDealer());
        yield return Yielders.WaitForSeconds(0.4f);

        StartCoroutine(DealCardToPlayer());
        yield return Yielders.WaitForSeconds(0.4f);

        StartCoroutine(DealCardToDealer());
        yield return Yielders.WaitForSeconds(0.4f);

        m_isBlackJack = CheckForBlackJack();

        if (m_isBlackJack && m_hand[0].value == DeckHandler.VALUES.ACE)
        {
            ShowDealerCard(1);
            GameHandler.Instance.OnDealerBlackJack();
            GameHandler.Instance.OnMatchEnded();
        }
        else
        {
            GameHandler.Instance.OnInitialHandsReady();
        }
    }

    public void DealNewCardToPlayer()
    {
        bool isForSplitHand = currentHand == SECOND;
        StartCoroutine(DealCardToPlayer(isForSplitHand));
    }

    private void RestoreDeckTopCard()
    {
        DeckTopCard.localScale = Vector3.one;
    }

    IEnumerator DealCardToPlayer(bool forSplitHand = false)
    {
        DeckTopCard.DOScale(Vector3.zero, 0.2f);
        yield return Yielders.WaitForSeconds(0.2f);

        int tmpIndex = forSplitHand ? currentPlayerSplitCard : currentPlayerCard;

        m_player.AddCard(DeckHandler.Instance.DrawCard(), forSplitHand);

        if (forSplitHand)
        {
            PlayerSplitCardsObjects[tmpIndex].SetActive(true);
            PlayerSplitCardsTransforms[tmpIndex].DOScale(Vector3.one, 0.2f)
                .OnComplete(() => ShowPlayerCard(tmpIndex, forSplitHand));
            currentPlayerSplitCard += 1;
        }
        else
        {
            PlayerCardsObjects[tmpIndex].SetActive(true);
            PlayerCardsTransforms[tmpIndex].DOScale(Vector3.one, 0.2f)
                .OnComplete(() => ShowPlayerCard(tmpIndex, forSplitHand));
            currentPlayerCard += 1;
        }

        RestoreDeckTopCard();        
        SFXHandler.Instance.PlayCardSound();

        if (!m_isInitialDeal)
        {
            if (currentHand == SECOND && m_player.PlayerHasBlackJack() && m_player.PlayerHasBlackJack(true))
            {
                DrawHand();
            }
            else
            {
                HandIndicatorImage.enabled = true;
                GameHandler.Instance.OnPlayerCardDrawn(forSplitHand);
            }            
        }
    }

    IEnumerator DealCardToDealer()
    {
        AddCard(DeckHandler.Instance.DrawCard());
        DeckTopCard.DOScale(Vector3.zero, 0.2f);
        yield return Yielders.WaitForSeconds(0.2f);

        int tmpIndex = currentDealerCard;

        DealerCardsObjects[currentDealerCard].SetActive(true);
        DealerCardsTransforms[tmpIndex].DOScale(Vector3.one, 0.2f)
            .OnComplete(() => ShowDealerCard(tmpIndex));
        RestoreDeckTopCard();
        SFXHandler.Instance.PlayCardSound();

        currentDealerCard += 1;
    }

    private void ShowPlayerCard(int index, bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_player.GetCard(index, forSplitHand));
        }
        else
        {
            PlayerCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_player.GetCard(index, forSplitHand));
        }
        
        GameHandler.Instance.UpdatePlayerHandValue(forSplitHand);
    }

    private void ShowDealerCard(int index)
    {
        if (index == 1 && m_isInitialDeal)
        {
            m_isInitialDeal = false;
        }
        else
        {
            UpdateHandValue();
            DealerCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_hand[index]);
            GameHandler.Instance.UpdateDealerHandValue();
        }
    }

    private void AddCard(Card newCard)
    {
        m_hand.Add(newCard);
    }

    public void SplitHand()
    {
        StartCoroutine(SplitHandRoutine());
    }

    IEnumerator SplitHandRoutine()
    {
        m_player.SplitHand();
        currentHand = SECOND;

        PlayerCardsTransforms[SECOND].DOScale(Vector3.zero, 0.2f)
                .OnComplete(() => PlayerCardsObjects[SECOND].SetActive(false));

        yield return Yielders.WaitForSeconds(0.2f);

        GameHandler.Instance.UpdatePlayerHandValue(false);
        baseCardsLayout.spacing = SPLIT_SPACING;

        PlayerSplitCardsObjects[FIRST].SetActive(true);
        PlayerSplitCardsTransforms[FIRST].DOScale(Vector3.one, 0.2f)
            .OnComplete(() => ShowPlayerCard(FIRST, true));

        currentPlayerCard = 1;
        currentPlayerSplitCard = 1;

        yield return Yielders.WaitForSeconds(0.2f);

        StartCoroutine(DealCardToPlayer(false));
        yield return Yielders.WaitForSeconds(0.4f);

        StartCoroutine(DealCardToPlayer(true));
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
        if (currentHand == SECOND) //Split Hand
        {
            currentHand = FIRST;

            if (m_player.PlayerHasBlackJack())
            {
                ResetHandIndicator();
                DrawHand();
            }
            else
            {
                HandIndicatorTransform.DOMoveX(HandIndicatorSecondPosition.position.x, 0.2f)
                    .OnComplete(() => GUI_Handler.Instance.GUI_ShowPlayerActions());
            }
        }
        else //Base Hand
        {
            ResetHandIndicator();

            if (m_player.IsHandBusted(true))
            {
                ShowDealerCard(1);
                GameHandler.Instance.OnMatchEnded();
            }
            else
            {
                DrawHand();
            }
        }
    }

    public void OnPlayerStand()
    {
        if (currentHand == SECOND) //Split Hand
        {
            currentHand = FIRST;

            if (m_player.PlayerHasBlackJack())
            {
                ResetHandIndicator();
                DrawHand();
            }
            else
            {
                HandIndicatorTransform.DOMoveX(HandIndicatorSecondPosition.position.x, 0.2f)
                    .OnComplete(() => GUI_Handler.Instance.GUI_ShowPlayerActions());
            }
        }
        else //Base Hand
        {
            ResetHandIndicator();
            DrawHand();
        }
    }

    private void ResetHandIndicator()
    {
        HandIndicatorImage.enabled = false;
        HandIndicatorTransform.position = InitialHandIndicatorPosition;
    }

    public void DrawHand()
    {
        StartCoroutine(DrawHandRoutine());
    }

    private bool CheckForBlackJack()
    {
        bool result = false;

        int handValue = CalculateHandValue();

        if(handValue == BASEJACK && m_aceInHand)
        {
            result = true;
        }

        return result;
    }

    IEnumerator DrawHandRoutine()
    {
        ShowDealerCard(SECOND);

        if (!m_isBlackJack)
        {
            int handValue = m_handValue;

            if (m_aceInHand)
            {
                handValue += 10;
            }

            //Stay on 17, Draw to 16
            while (handValue < LIMIT_VALUE)
            {
                StartCoroutine(DealCardToDealer());

                yield return Yielders.WaitForSeconds(0.4f);

                handValue = m_handValue;

                if (m_aceInHand)
                {
                    handValue += LETTER_VALUE;

                    if(handValue > BLACKJACK)
                    {
                        handValue -= LETTER_VALUE;
                    }
                }
            }
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

            PlayerSplitCardsObjects[i].SetActive(false);
            PlayerSplitCardsTransforms[i].localScale = Vector3.zero;
            PlayerSplitCardsImages[i].sprite = m_cardBack;
        }

        for (int i = 0; i < DealerCardsObjects.Length; i++)
        {
            DealerCardsObjects[i].SetActive(false);
            DealerCardsTransforms[i].localScale = Vector3.zero;
            DealerCardsImages[i].sprite = m_cardBack;
        }

        baseCardsLayout.spacing = DEFAULT_SPACING;

        m_isInitialDeal = true;
        currentPlayerCard = 0;
        currentPlayerSplitCard = 0;
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
