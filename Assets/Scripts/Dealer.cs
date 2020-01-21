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
    [SerializeField] private Transform DeckTopCard = null;
    [SerializeField] private GameObject[] PlayerCardsObjects = null;
    [SerializeField] private GameObject[] DealerCardsObjects = null;
    [SerializeField] private Transform[] PlayerCardsTransforms = null;
    [SerializeField] private Transform[] DealerCardsTransforms = null;
    [SerializeField] private Image[] PlayerCardsImages = null;
    [SerializeField] private Image[] DealerCardsImages = null;

    [Header("Split Hand")]
    [SerializeField] private GameObject[] PlayerSplitCardsObjects = null;
    [SerializeField] private Transform[] PlayerSplitCardsTransforms = null;
    [SerializeField] private Image[] PlayerSplitCardsImages = null;
    [SerializeField] private HorizontalLayoutGroup baseCardsLayout = null;
    [SerializeField] private Image HandIndicatorImage = null;
    [SerializeField] private RectTransform HandIndicatorTransform = null;
    [SerializeField] private RectTransform HandIndicatorSecondPosition = null;

    private int currentPlayerCard = 0;
    private int currentPlayerSplitCard = 0;
    private int currentDealerCard = 0;
    private int currentHand = 0;

    private int m_handValue = 0;
    private bool m_aceInHand = false;
    private bool m_isInitialDeal = true;
    private bool m_isBlackJack = false;

    private const int BASE_HAND = 0;
    private const int SPLIT_HAND = 1;
    private const int DEFAULT_SPACING = 10;
    private const int SPLIT_SPACING = -90;
    private const float LONGER_DELAY = 0.5f;

    private Vector3 InitialHandIndicatorPosition;

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
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToDealer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToPlayer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToDealer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        m_isBlackJack = CheckForBlackJack();

        if (m_isBlackJack && m_hand[Constants.FIRST_CARD].value == DeckHandler.VALUES.ACE)
        {
            ShowDealerCard(Constants.SECOND_CARD);
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
        bool isForSplitHand = currentHand == SPLIT_HAND;
        StartCoroutine(DealCardToPlayer(isForSplitHand));
    }

    private void RestoreDeckTopCard()
    {
        DeckTopCard.localScale = Vector3.one;
    }

    IEnumerator DealCardToPlayer(bool forSplitHand = false)
    {
        DeckTopCard.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        int tmpIndex = forSplitHand ? currentPlayerSplitCard : currentPlayerCard;

        m_player.AddCard(DeckHandler.Instance.DrawCard(), forSplitHand);

        if (forSplitHand)
        {
            PlayerSplitCardsObjects[tmpIndex].SetActive(true);
            PlayerSplitCardsTransforms[tmpIndex].DOScale(Vector3.one, Constants.QUICK_DELAY)
                .OnComplete(() => ShowPlayerCard(tmpIndex, forSplitHand));
            currentPlayerSplitCard += 1;
        }
        else
        {
            PlayerCardsObjects[tmpIndex].SetActive(true);
            PlayerCardsTransforms[tmpIndex].DOScale(Vector3.one, Constants.QUICK_DELAY)
                .OnComplete(() => ShowPlayerCard(tmpIndex, forSplitHand));
            currentPlayerCard += 1;
        }

        RestoreDeckTopCard();        
        SFXHandler.Instance.PlayCardSound();

        if (!m_isInitialDeal)
        {
            if (currentHand == SPLIT_HAND && m_player.PlayerHasBlackJack() && m_player.PlayerHasBlackJack(true))
            {
                GUI_Handler.Instance.GUI_HidePlayerActions();
                ResetHandIndicator();

                ShowDealerCard(Constants.SECOND_CARD);
                GameHandler.Instance.OnMatchEnded();
            }
            else if (currentHand == SPLIT_HAND && m_player.PlayerHasBlackJack(true))
            {
                currentHand = BASE_HAND;

                HandIndicatorTransform.position = HandIndicatorSecondPosition.position;

                HandIndicatorImage.enabled = true;
                
                GUI_Handler.Instance.GUI_ShowPlayerActions();
            }
            else 
            {
                if (m_player.HasPlayerSplitted())
                {
                    HandIndicatorImage.enabled = true;
                }
                GameHandler.Instance.OnPlayerCardDrawn(forSplitHand);
            }            
        }
    }

    IEnumerator DealCardToDealer()
    {
        AddCard(DeckHandler.Instance.DrawCard());
        DeckTopCard.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        int tmpIndex = currentDealerCard;

        DealerCardsObjects[currentDealerCard].SetActive(true);
        DealerCardsTransforms[tmpIndex].DOScale(Vector3.one, Constants.QUICK_DELAY)
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
        if (index == Constants.SECOND_CARD && m_isInitialDeal)
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
        currentHand = SPLIT_HAND;

        PlayerCardsTransforms[SPLIT_HAND].DOScale(Vector3.zero, Constants.QUICK_DELAY)
                .OnComplete(() => PlayerCardsObjects[SPLIT_HAND].SetActive(false));

        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        GameHandler.Instance.UpdatePlayerHandValue(false);
        baseCardsLayout.spacing = SPLIT_SPACING;

        PlayerSplitCardsObjects[BASE_HAND].SetActive(true);
        PlayerSplitCardsTransforms[BASE_HAND].DOScale(Vector3.one, Constants.QUICK_DELAY)
            .OnComplete(() => ShowPlayerCard(BASE_HAND, true));

        currentPlayerCard = Constants.SECOND_CARD;
        currentPlayerSplitCard = Constants.SECOND_CARD;

        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        StartCoroutine(DealCardToPlayer(false));
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToPlayer(true));
    }

    public string UpdateHandValue()
    {
        m_handValue = CalculateHandValue(true);

        string valueString = string.Empty;

        if (m_aceInHand)
        {
            m_handValue += Constants.ACE_MIN_VALUE;

            if (m_handValue + Constants.LETTER_VALUE <= Constants.BLACKJACK)
            {
                m_handValue += Constants.LETTER_VALUE;
            }
        }

        valueString = m_handValue.ToString();

        return valueString;
    }

    public int CalculateHandValue(bool withoutAce = false)
    {
        int result = 0;
        int value = 0;
        bool aceAlreadyExcluded = false;

        for (int i = 0; i < m_hand.Count; i++)
        {
            value = m_hand[i].GetCardValue();

            result += value;

            if (value == 1 && withoutAce && !aceAlreadyExcluded)
            {
                result -= value;
                aceAlreadyExcluded = true;
            }

            //If the value is an Ace update flag
            if (value == Constants.ACE_MIN_VALUE)
            {
                m_aceInHand = true;
            }
        }

        if (!withoutAce)
        {
            if (m_aceInHand)
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

    public void OnPlayerBusted()
    {
        if (currentHand == SPLIT_HAND) //Split Hand
        {
            currentHand = BASE_HAND;

            if (m_player.PlayerHasBlackJack())
            {
                ResetHandIndicator();
                DrawHand();
            }
            else
            {
                HandIndicatorTransform.DOMoveX(HandIndicatorSecondPosition.position.x, Constants.QUICK_DELAY)
                    .OnComplete(() => GUI_Handler.Instance.GUI_ShowPlayerActions());
            }
        }
        else //Base Hand
        {
            ResetHandIndicator();

            if (IsSplitHandAvailable())
            {
                if (m_player.IsHandBusted(true))
                {
                    ShowDealerCard(SPLIT_HAND);
                    GameHandler.Instance.OnMatchEnded();
                }
                else
                {
                    DrawHand();
                }
            }
            else
            {
                ShowDealerCard(SPLIT_HAND);
                GameHandler.Instance.OnMatchEnded();
            }
        }
    }

    public void OnPlayerStand()
    {
        if (currentHand == SPLIT_HAND) //Split Hand
        {
            currentHand = BASE_HAND;

            if (m_player.PlayerHasBlackJack())
            {
                ResetHandIndicator();
                DrawHand();
            }
            else
            {
                HandIndicatorTransform.DOMoveX(HandIndicatorSecondPosition.position.x, Constants.QUICK_DELAY)
                    .OnComplete(() => GUI_Handler.Instance.GUI_ShowPlayerActions());
            }
        }
        else //Base Hand
        {
            if (!m_player.PlayerHasBlackJack())
            {
                ResetHandIndicator();
                DrawHand();
            }
            else
            {
                ShowDealerCard(Constants.SECOND_CARD);
                GameHandler.Instance.OnMatchEnded();
            }
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

        if(handValue == Constants.BLACKJACK && m_aceInHand && m_hand.Count == 2)
        {
            result = true;
        }

        return result;
    }

    IEnumerator DrawHandRoutine()
    {
        ShowDealerCard(SPLIT_HAND);

        if (!m_isBlackJack)
        {
            int handValue = m_handValue;

            if (m_aceInHand)
            {
                handValue += Constants.LETTER_VALUE;

                if(handValue > Constants.BLACKJACK)
                {
                    handValue -= Constants.LETTER_VALUE;
                }
            }

            //Stay on 17, Draw to 16
            while (handValue < Constants.DEALER_LIMIT)
            {
                StartCoroutine(DealCardToDealer());

                yield return Yielders.WaitForSeconds(LONGER_DELAY);

                handValue = CalculateHandValue();
            }

            if(handValue > Constants.BLACKJACK)
            {
                GUI_Handler.Instance.ShowDealerBusted();
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

        m_handValue = 0;
        m_aceInHand = false;
        m_isInitialDeal = true;
        m_isBlackJack = false;
        currentPlayerCard = 0;
        currentPlayerSplitCard = 0;
        currentDealerCard = 0;
        currentHand = BASE_HAND;
    }

    public bool IsHandBusted()
    {
        int handValue = CalculateHandValue();
        return handValue > Constants.BLACKJACK;
    }

    public bool IsSplitHandActive()
    {
        return currentHand == SPLIT_HAND;
    }

    public bool IsSplitHandAvailable()
    {
        return m_player.HasPlayerSplitted();
    }

    private void ClearHand()
    {
        m_hand.Clear();
    }
}
