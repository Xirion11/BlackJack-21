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

    private int currentPlayerCard = 0; //Current Index in player's hand
    private int currentPlayerSplitCard = 0; //Current Index in player's second hand
    private int currentDealerCard = 0; //Current Index in dealer's hand
    private int currentHand = 0; //Which player's hand are we dealing to

    private int m_handValue = 0; //Dealer's hand value
    private bool m_aceInHand = false; //Does the Dealer has an Ace in hand?
    private bool m_isInitialDeal = true;  //Are we dealing the first 4 cards?
    private bool m_isBlackJack = false; //Does the Dealer has a natural blackjack?

    private const int BASE_HAND = 0; //Default player's hand
    private const int SPLIT_HAND = 1; //Split player's hand
    private const int DEFAULT_SPACING = 10; //10 points of separation between cards
    private const int SPLIT_SPACING = -90; //This will cause the cards to overlap to save space
    private const float LONGER_DELAY = 0.5f;

    private Vector3 InitialHandIndicatorPosition;

    private void Start()
    {
        //Initialize dealer's hand
        m_hand = new List<Card>();

        //Save HandIndicator initial position
        InitialHandIndicatorPosition = HandIndicatorTransform.position;
    }
    
    public void DealInitialCards()
    {
        //Start to deal the first 4 cards
        StartCoroutine(DealInitialCardsRoutine());
    }

    IEnumerator DealInitialCardsRoutine()
    {
        //Clear blackjack flag
        m_isBlackJack = false;

        StartCoroutine(DealCardToPlayer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToDealer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToPlayer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToDealer());
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        //Does the dealer have a natural blackjack?
        m_isBlackJack = CheckForBlackJack();

        //If dealer has a black (A+LETTER) AND the visible card is an Ace
        if (m_isBlackJack && m_hand[Constants.FIRST_CARD].value == DeckHandler.VALUES.ACE)
        {
            //Reveal dealer's hand and notify GameHandler
            ShowDealerCard(Constants.SECOND_CARD);
            GameHandler.Instance.OnDealerBlackJack();

            //Even if the player has blackjack, since no one is drawing more cards, the match is over
            GameHandler.Instance.OnMatchEnded();
        }
        else
        {
            //If the dealer does not have a visible blackjack the match may continue
            GameHandler.Instance.OnInitialHandsReady();
        }
    }

    public void DealNewCardToPlayer()
    {
        //After implementing splits we need to specify which hand we are interested in
        bool isForSplitHand = currentHand == SPLIT_HAND;
        StartCoroutine(DealCardToPlayer(isForSplitHand));
    }

    private void RestoreDeckTopCard()
    {
        //When drawing cards the top card animates to disapper. This restores it.
        DeckTopCard.localScale = Vector3.one;
    }

    IEnumerator DealCardToPlayer(bool forSplitHand = false)
    {
        //Animate the deck's top card to simulate drawing
        DeckTopCard.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        //Get the index of the next card to assign depending on active hand
        int tmpIndex = forSplitHand ? currentPlayerSplitCard : currentPlayerCard;

        //Get a new card and assign it to the player's hand
        m_player.AddCard(DeckHandler.Instance.DrawCard(), forSplitHand);

        //Depending on the active hand, activate the card object and animate to simulate drawing
        //When the animation is ready, reveal the card, this will cause the hand value to be updated
        //Finally increment current index
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

        //Animate deck's top card to prepare for next draw animation
        RestoreDeckTopCard();       

        //Player card slide sfx to enhance immersion
        SFXHandler.Instance.PlayCardSound();

        //If we are dealing a card outside of the first 4 cards
        if (!m_isInitialDeal)
        {
            //If we are dealing to the second hand, and both player's hands have blackjack
            if (currentHand == SPLIT_HAND && m_player.PlayerHasBlackJack() && m_player.PlayerHasBlackJack(true))
            {
                /** 
                 * No more actions can be done. If the dealer has blackjack it cannot draw more.
                 * If not, then the player already won
                 */ 
                GUI_Handler.Instance.GUI_HidePlayerActions();
                ResetHandIndicator();

                //Reveal the dealer's hand and end the match
                ShowDealerCard(Constants.SECOND_CARD);
                GameHandler.Instance.OnMatchEnded();
            }
            //If we are dealing to the second hand, and only that player's hand has blackjack
            else if (currentHand == SPLIT_HAND && m_player.PlayerHasBlackJack(true))
            {
                //Skip this hand and change to base hand
                currentHand = BASE_HAND;

                //Update the hand indicator position to point to base hand and enable it
                HandIndicatorTransform.position = HandIndicatorSecondPosition.position;
                HandIndicatorImage.enabled = true;
                
                //The player may continue their turn
                GUI_Handler.Instance.GUI_ShowPlayerActions();
            }
            //If player does not have blackjack on any hand
            else 
            {
                //Point to split hand if split has happened
                if (m_player.HasPlayerSplitted())
                {
                    HandIndicatorImage.enabled = true;
                }

                //Notify that the player has drawn a card
                GameHandler.Instance.OnPlayerCardDrawn(forSplitHand);
            }            
        }
    }

    IEnumerator DealCardToDealer()
    {
        //Animate the deck's top card to simulate drawing
        DeckTopCard.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        //Get the index of the next card to assign
        int tmpIndex = currentDealerCard;

        ///Get a new card and assign it to the dealer's hand
        AddCard(DeckHandler.Instance.DrawCard());

        //Activate the card object and animate to simulate drawing
        //When the animation is ready, reveal the card, this will cause the hand value to be updated
        //Finally increment current index
        DealerCardsObjects[currentDealerCard].SetActive(true);
        DealerCardsTransforms[tmpIndex].DOScale(Vector3.one, Constants.QUICK_DELAY)
            .OnComplete(() => ShowDealerCard(tmpIndex));
        currentDealerCard += 1;

        //Animate deck's top card to prepare for next draw animation
        RestoreDeckTopCard();

        //Player card slide sfx to enhance immersion
        SFXHandler.Instance.PlayCardSound();
    }

    //Change the card sprite and update the hand value
    private void ShowPlayerCard(int index, bool forSplitHand = false)
    {
        //Change the card sprite
        if (forSplitHand)
        {
            PlayerSplitCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_player.GetCard(index, forSplitHand));
        }
        else
        {
            PlayerCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_player.GetCard(index, forSplitHand));
        }
        
        //Update hand value label
        GameHandler.Instance.UpdatePlayerHandValue(forSplitHand);
    }

    //Change the card sprite and update the hand value
    private void ShowDealerCard(int index)
    {
        //If we are dealing the second dealer's card.
        if (index == Constants.SECOND_CARD && m_isInitialDeal)
        {
            //We have just dealt the initial 4 cards
            m_isInitialDeal = false;

            //Do not update the hand's value at this point so the player has to guess
        }
        else
        {
            //Update the hand value
            UpdateHandValue();

            //Show the actual card
            DealerCardsImages[index].sprite = DeckHandler.Instance.GetCardSprite(m_hand[index]);

            //Update the hand value label
            GameHandler.Instance.UpdateDealerHandValue();
        }
    }

    //Add card to hand array
    private void AddCard(Card newCard)
    {
        m_hand.Add(newCard);
    }

    //Start split routine
    public void SplitHand()
    {
        StartCoroutine(SplitHandRoutine());
    }

    IEnumerator SplitHandRoutine()
    {
        //Tell the player to separate its cards in two arrays
        m_player.SplitHand();

        //Change to split hand
        currentHand = SPLIT_HAND;

        //Disappear player's second card to simulate split
        PlayerCardsTransforms[Constants.SECOND_CARD].DOScale(Vector3.zero, Constants.QUICK_DELAY)
                .OnComplete(() => PlayerCardsObjects[SPLIT_HAND].SetActive(false));

        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        //Update hand value label for first hand
        GameHandler.Instance.UpdatePlayerHandValue(false);

        //Change card spacing to save space on screen
        baseCardsLayout.spacing = SPLIT_SPACING;

        //Activate the first card in the split hand and animate it to simulate draw
        //After that update its hand value label
        PlayerSplitCardsObjects[Constants.FIRST_CARD].SetActive(true);
        PlayerSplitCardsTransforms[Constants.FIRST_CARD].DOScale(Vector3.one, Constants.QUICK_DELAY)
            .OnComplete(() => ShowPlayerCard(Constants.FIRST_CARD, true));

        //Both hands have now 1 card each. Set current index to 1
        currentPlayerCard = Constants.SECOND_CARD;
        currentPlayerSplitCard = Constants.SECOND_CARD;

        yield return Yielders.WaitForSeconds(Constants.QUICK_DELAY);

        //Deal 2 more cards to player

        StartCoroutine(DealCardToPlayer(false));
        yield return Yielders.WaitForSeconds(Constants.MID_DELAY);

        StartCoroutine(DealCardToPlayer(true));
    }

    //Update hand value
    public void UpdateHandValue()
    {
        m_handValue = CalculateHandValue();
    }

    //Calculate hand value
    public int CalculateHandValue()
    {
        int result = 0;
        int value = 0;

        //Go through each of the cards in hand
        for (int i = 0; i < m_hand.Count; i++)
        {
            //Get card value
            value = m_hand[i].GetCardValue();

            //Add it to the result
            result += value;

            //If the value is an Ace update flag
            if (value == Constants.ACE_MIN_VALUE)
            {
                m_aceInHand = true;
            }
        }

        //If there is an ace in hand
        if (m_aceInHand)
        {
            //Add 10 to the hand value
            result += Constants.LETTER_VALUE;

            //If by adding the 10 to the hand we are over 21, remove those 10
            if (result > Constants.BLACKJACK)
            {
                result -= Constants.LETTER_VALUE;
            }
        }

        return result;
    }

    //Called when a player hand has busted
    public void OnPlayerBusted()
    {
        //If we were dealing to the split hand
        if (currentHand == SPLIT_HAND)
        {
            //Change to base hand
            currentHand = BASE_HAND;

            //If the first hand has blackjack
            if (m_player.PlayerHasBlackJack())
            {
                //Skip base hand

                //Deactivate and reposition hand indicator
                ResetHandIndicator();

                //Draw dealer's hand
                DrawHand();
            }

            //If the base hand does not have blackjack
            else
            {
                //Move the hand indicator to the base hand and allow the player to continue their turn
                HandIndicatorTransform.DOMoveX(HandIndicatorSecondPosition.position.x, Constants.QUICK_DELAY)
                    .OnComplete(() => GUI_Handler.Instance.GUI_ShowPlayerActions());
            }
        }

        //If we were dealing with the base hand
        else
        {
            //Deactivate and reposition the hand activator
            ResetHandIndicator();

            //If the player splitted before
            if (IsSplitHandAvailable())
            {
                //If the split hand is also busted
                if (m_player.IsHandBusted(true))
                {
                    //Just show the dealer's hand and end the match
                    ShowDealerCard(Constants.SECOND_CARD);
                    GameHandler.Instance.OnMatchEnded();
                }

                //If the split hand did not bust
                else
                {
                    //Continue drawing dealer's hand
                    DrawHand();
                }
            }

            //The player did not split. The only player hand busted
            else
            {
                //Just show the dealer's hand and end the match
                ShowDealerCard(SPLIT_HAND);
                GameHandler.Instance.OnMatchEnded();
            }
        }
    }

    //If the player decided to not draw more cards or is unable to (IE. Double, BlackJack)
    public void OnPlayerStand()
    {
        //If we were dealing to the split hand
        if (currentHand == SPLIT_HAND)
        {
            //Change to base hand
            currentHand = BASE_HAND;

            //If the base hand has blackjack
            if (m_player.PlayerHasBlackJack())
            {
                //Skip base hand

                //Deactivate and reposition Hand Indicator
                ResetHandIndicator();

                //Draw dealer's hand
                DrawHand();
            }

            //Base hand had no blackjack
            else
            {
                //Move the hand indicator to base hand and allow the player to continue
                HandIndicatorTransform.DOMoveX(HandIndicatorSecondPosition.position.x, Constants.QUICK_DELAY)
                    .OnComplete(() => GUI_Handler.Instance.GUI_ShowPlayerActions());
            }
        }

        //If the player stood on the base hand
        else
        {
            //If it wasn't due to a blackjack
            if (!m_player.PlayerHasBlackJack())
            {
                //Deactivate and reposition hand indicator
                ResetHandIndicator();

                //Continue drawing dealer's hand
                DrawHand();
            }

            //If player had blackjack
            else
            {
                //Show dealer's hand and end the match
                ShowDealerCard(Constants.SECOND_CARD);
                GameHandler.Instance.OnMatchEnded();
            }
        }
    }

    //Deactivate the hand indicator and reposition it
    private void ResetHandIndicator()
    {
        HandIndicatorImage.enabled = false;
        HandIndicatorTransform.position = InitialHandIndicatorPosition;
    }

    //Start drawing dealer's hand
    public void DrawHand()
    {
        StartCoroutine(DrawHandRoutine());
    }

    //Check if dealer has blackjack
    private bool CheckForBlackJack()
    {
        //By default we assume we don't
        bool result = false;

        //Get hand value
        int handValue = CalculateHandValue();

        //If hand value is 21, we have an ace in hand and we only have 2 cards, then we have blackjack
        if(handValue == Constants.BLACKJACK && m_aceInHand && m_hand.Count == 2)
        {
            result = true;
        }

        return result;
    }

    IEnumerator DrawHandRoutine()
    {
        //Start by showing the second card
        ShowDealerCard(Constants.SECOND_CARD);

        //If the dealer does not have blackjack
        if (!m_isBlackJack)
        {
            //Get hand value
            int handValue = m_handValue;

            //If there is an ace in hand
            if (m_aceInHand)
            {
                //Add 10 to the hand value
                handValue += Constants.LETTER_VALUE;

                //If by adding 10 we are over 21
                if(handValue > Constants.BLACKJACK)
                {
                    //Remove those 10
                    handValue -= Constants.LETTER_VALUE;
                }
            }

            //If hand value is less than 17
            while (handValue < Constants.DEALER_LIMIT)
            {
                //Draw a card
                StartCoroutine(DealCardToDealer());

                yield return Yielders.WaitForSeconds(LONGER_DELAY);

                //Get new hand value
                handValue = CalculateHandValue();
            }

            //If the hand value is over 21
            if(handValue > Constants.BLACKJACK)
            {
                //Show dealer busted label
                GUI_Handler.Instance.ShowDealerBusted();
            }
        }

        //The match has ended
        GameHandler.Instance.OnMatchEnded();
    }

    //Clean the table and prepare everything for another match
    public void CleanTable()
    {
        //Tell the player to clear its hand
        m_player.ClearHand();

        //Clear dealer's hand
        ClearHand();

        //Go through all elements on player hand(s) and reset it
        for (int i = 0; i < PlayerCardsObjects.Length; i++)
        {
            PlayerCardsObjects[i].SetActive(false);
            PlayerCardsTransforms[i].localScale = Vector3.zero;
            PlayerCardsImages[i].sprite = m_cardBack;

            PlayerSplitCardsObjects[i].SetActive(false);
            PlayerSplitCardsTransforms[i].localScale = Vector3.zero;
            PlayerSplitCardsImages[i].sprite = m_cardBack;
        }

        //Go through all elements on dealer's hand and reset it
        for (int i = 0; i < DealerCardsObjects.Length; i++)
        {
            DealerCardsObjects[i].SetActive(false);
            DealerCardsTransforms[i].localScale = Vector3.zero;
            DealerCardsImages[i].sprite = m_cardBack;
        }

        //Return player' base hand's spacing
        baseCardsLayout.spacing = DEFAULT_SPACING;

        //Reset dealer's attributes
        m_handValue = 0;
        m_aceInHand = false;
        m_isInitialDeal = true;
        m_isBlackJack = false;
        currentPlayerCard = Constants.FIRST_CARD;
        currentPlayerSplitCard = Constants.FIRST_CARD;
        currentDealerCard = Constants.FIRST_CARD;
        currentHand = BASE_HAND;
    }

    //Is hand value above 21?
    public bool IsHandBusted()
    {
        int handValue = CalculateHandValue();
        return handValue > Constants.BLACKJACK;
    }

    //Are we dealing to the split hand?
    public bool IsSplitHandActive()
    {
        return currentHand == SPLIT_HAND;
    }

    //Did the player split?
    public bool IsSplitHandAvailable()
    {
        return m_player.HasPlayerSplitted();
    }

    //Remove all cards from dealer's hand
    private void ClearHand()
    {
        m_hand.Clear();
    }
}
