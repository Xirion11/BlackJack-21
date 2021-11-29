using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Yodo1.MAS;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private Player m_player = null;
    [SerializeField] private Dealer m_dealer = null;
    [SerializeField] private DeckHandler m_deckHandler = null;
    [SerializeField] private GameObject m_deckContainer = null;
    [SerializeField] private GameObject m_shuffleDescription = null;
    [SerializeField] private GameObject m_splitButton = null;
    [SerializeField] private GameObject m_splitCardsContainer = null;
    [SerializeField] private GameObject m_splitValueContainer = null;
    [SerializeField] private GameObject m_splitBlackjackContainer = null;
    [SerializeField] private GameObject m_splitBustedContainer = null;
    [SerializeField] private GameObject m_playerBetContainer = null;
    [SerializeField] private GameObject m_playerSplitBetContainer = null;
    [SerializeField] private GameObject m_playerBetDoubleContainer = null;
    [SerializeField] private GameObject m_playerSplitBetDoubleContainer = null;
    [SerializeField] private GameObject m_splitWinContainer = null;
    [SerializeField] private GameObject m_splitLoseContainer = null;
    [SerializeField] private GameObject m_splitDrawContainer = null;
    [SerializeField] private Transform m_playerRewardTransform = null;
    [SerializeField] private Animator shuffleAnimator = null;
    [SerializeField] private AudioButton doubleAction = null;
    [SerializeField] private TextMeshProUGUI lbl_playerMoney = null;
    [SerializeField] private TextMeshProUGUI lbl_betStationBet = null;
    [SerializeField] private TextMeshProUGUI lbl_playerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_playerSplitHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_dealerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerBet = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerSplitBet = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerReward = null;

    private float m_currentBet = 0;
    private float m_currentSplitBet = 0;
    private float m_playerMoney = 0;
    private bool m_dealerBlackjack = false;
    private bool m_hasPlayerDoubled = false;
    private const string moneyTemplate = "${0}";
    private const string placeBetTemplate = "Place your bet: {0}";
    private const string playerBetTemplate = "Your Bet<br>${0}";
    private const string blackJack = "BlackJack";
    private const string shuffleTrigger = "Shuffle";
    private const string rewardTemplate = "+${0}";
    private const string TwoDecimalsFormat = "F2";

    private Yodo1U3dBannerAdView mBannerAdView;

    public static GameHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get player money and initiate all labels
        m_playerMoney = PlayerPrefsManager.getPlayerMoney();

        //If the player starts with less money than the minimum bet
        if (m_playerMoney < Constants.MIN_BET)
        {
            //Give the player the starting money
            PlayerPrefsManager.setPlayerMoney(Constants.STARTING_MONEY);
            m_playerMoney = Constants.STARTING_MONEY;
        }

        //Clean labels
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_playerHandValue.SetText(string.Empty);
        lbl_dealerHandValue.SetText(string.Empty);


        //MAS built-in privacy compliance dialog, will show the dialog at start until information is received
        Yodo1AdBuildConfig config = new Yodo1AdBuildConfig().enableUserPrivacyDialog(true);
        Yodo1U3dMas.SetAdBuildConfig(config);

        // Initialize MAS SDK.
        Yodo1U3dMas.InitializeSdk();
    }

    private void RequestBanner()
    {
        // Clean up banner before reusing
        if (mBannerAdView != null)
        {
            mBannerAdView.Destroy();
        }

        // Create a 320x50 banner at top of the screen
        mBannerAdView = new Yodo1U3dBannerAdView(Yodo1U3dBannerAdSize.Banner, Yodo1U3dBannerAdPosition.BannerTop | Yodo1U3dBannerAdPosition.BannerRight);

        // Load banner ads, the banner ad will be displayed automatically after loaded
        mBannerAdView.LoadAd();
    }

    //Increasing a bet deducts it from the player money
    public void IncreaseCurrentBet(float increment)
    {
        m_currentBet += increment;
        m_playerMoney -= increment;

        //We don't need to save this change to PlayerPrefs because the player can still clear the bet.

        //Update the labels
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));
    }

    //The player gets its money back and clears the bet
    public void ClearCurrentBet()
    {
        m_playerMoney += m_currentBet;
        m_currentBet = 0f;

        //Update the labels
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));
    }

    //Returns current bet
    public float GetCurrentBet()
    {
        return m_currentBet;
    }

    //Player set bet and match is starting
    public void OnBetsReady()
    {
        //Restore blackjack flag
        m_dealerBlackjack = false;
        InitializeElements();

        //Play the sound of the bet being set on the table
        SFXHandler.Instance.PlayPlaceChipsSfx();

        //Enable the bet chips
        m_playerBetContainer.SetActive(true);
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
        PlayerPrefsManager.ReducePlayerMoney(m_currentBet);
        GUI_Handler.Instance.HidePlayerBlackJack();
        GUI_Handler.Instance.HidePlayerBusted();
        GUI_Handler.Instance.HideDealerBusted();
        GUI_Handler.Instance.HidePlayerWin();
        GUI_Handler.Instance.HidePlayerLose();
        m_dealer.DealInitialCards();
    }

    /**
    * Hide all gameplay elements except the deck and bet.
    * Clear all labels.
    * Hide UI messages.
    */
    private void InitializeElements()
    {
        m_dealer.CleanTable();
        
        lbl_playerHandValue.SetText(string.Empty);
        lbl_playerSplitHandValue.SetText(string.Empty);
        lbl_dealerHandValue.SetText(string.Empty);

        m_splitCardsContainer.SetActive(false);
        m_splitValueContainer.SetActive(false);
        m_splitBlackjackContainer.SetActive(false);
        m_splitBustedContainer.SetActive(false);
        m_playerBetContainer.SetActive(false);
        m_playerSplitBetContainer.SetActive(false);
        m_playerBetDoubleContainer.SetActive(false);
        m_playerSplitBetDoubleContainer.SetActive(false);
        m_splitWinContainer.SetActive(false);
        m_splitLoseContainer.SetActive(false);
        m_splitDrawContainer.SetActive(false);

        GUI_Handler.Instance.HidePlayerBlackJack();
        GUI_Handler.Instance.HidePlayerBusted();
        GUI_Handler.Instance.HideDealerBusted();
        GUI_Handler.Instance.HidePlayerWin();
        GUI_Handler.Instance.HidePlayerLose();
        GUI_Handler.Instance.HidePlayerDraw();
    }

    //Update hand value label depending on active hand
    public void UpdatePlayerHandValue(bool forSplitHand = false)
    {
        //Does the player has blackjack?
        bool playerHasBlackJack = m_player.PlayerHasBlackJack(forSplitHand);

        //We only update the label if its not blackjack
        //Since the blackjack message will be shown
        if (!playerHasBlackJack)
        {
            string value = m_player.UpdateHandValue(forSplitHand);
            if (forSplitHand)
            {
                lbl_playerSplitHandValue.SetText(value);
            }
            else
            {
                lbl_playerHandValue.SetText(value);
            }
        }
    }

    //Update dealer hand value
    public void UpdateDealerHandValue()
    {
        //We only update the label if its not blackjack
        //Since the blackjack message will be shown
        if (!m_dealerBlackjack)
        {
            lbl_dealerHandValue.SetText(m_dealer.CalculateHandValue().ToString());
        }
    }

    //Called when the dealer finishes dealing the first 4 cards  
    public void OnInitialHandsReady()
    {
        //Does the player have blackjack in base hand? Split cannot happen on initial hand
        bool playerHasBlackJack = m_player.PlayerHasBlackJack();

        //If no one has blackjack then check for split availability and enable double down
        if (!playerHasBlackJack && !m_dealerBlackjack)
        {
            m_hasPlayerDoubled = false;
            doubleAction.interactable = true;

            if (m_player.IsSplitAvailable())
            {
                m_splitButton.SetActive(true);
            }

            //Let the player continue their turn
            GUI_Handler.Instance.GUI_ShowPlayerActions();
        }

        //If the player has blackjack it ends its turn
        else
        {
            OnPlayerStand();
        }
    }

    //Called when player gets blackjack
    public void OnPlayerBlackJack(bool forSplitHand = false)
    {
        //Clear hand value label
        if (forSplitHand)
        {
            lbl_playerSplitHandValue.SetText(string.Empty);
        }
        else
        {
            lbl_playerHandValue.SetText(string.Empty);
        }
        
        //Show blackjack message
        GUI_Handler.Instance.ShowPlayerBlackJack(forSplitHand);
    }

    //Called when the dealer gets blackjack
    public void OnDealerBlackJack()
    {
        //Update dealer blackjack flag
        m_dealerBlackjack = true;

        //Show blackjack message
        lbl_dealerHandValue.SetText(blackJack);
    }

    //Called when the player raises the bet
    public void OnPlayerDoubled()
    {
        //Get player money
        float playerMoney = PlayerPrefsManager.getPlayerMoney();

        //If the player can't affor to double the bet
        if (m_currentBet > playerMoney)
        {
            //Tell the player to look at their money
            GUI_Handler.Instance.PlayNegativeCashFeedback();
        }

        //If the player has enough money
        else
        {
            //Enable doubled down flag
            m_hasPlayerDoubled = true;

            //Deduct the new bet from player money
            PlayerPrefsManager.ReducePlayerMoney(m_currentBet);

            //Play bet sfx
            SFXHandler.Instance.PlayPlaceChipsSfx();

            //Has the player splitted this round?
            bool isSplitHandAvailable = m_dealer.IsSplitHandAvailable();

            /**
             * Although the code seems like we can get rid of this if statement; we make this 
             * distinction becase the bet container and label for the base hand change depending
             * on the player split state. If the player has not splitted, the label and container
             * are the ones on the right. When the player splits, the base label and container are
             * now the ones in the center.
             */ 
            if (isSplitHandAvailable)
            {
                //Get the active hand
                bool isSplitHandActive = m_dealer.IsSplitHandActive();

                //Update hand bet and bet label. Add chips for new bet
                if (isSplitHandActive)
                {
                    m_playerMoney -= m_currentSplitBet;
                    m_currentSplitBet += m_currentSplitBet;
                    lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentSplitBet));
                    m_playerBetDoubleContainer.SetActive(true);
                }
                else
                {
                    m_playerMoney -= m_currentBet;
                    m_currentBet += m_currentBet;
                    lbl_PlayerSplitBet.SetText(string.Format(playerBetTemplate, m_currentBet));
                    m_playerSplitBetDoubleContainer.SetActive(true);
                }
            }

            //If the player has not splitted
            else
            {
                //Update base hand bet and bet label. Add chips for new bet
                m_playerMoney -= m_currentBet;
                m_currentBet += m_currentBet;
                lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
                m_playerBetDoubleContainer.SetActive(true);
            }

            //Update player money label
            lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));

            //This should be counted as a hit since the player asks for another card.
            OnPlayerHit();
        }
    }

    //Called when player wants another card or has doubled down
    public void OnPlayerHit()
    {
        //Hide player actions
        GUI_Handler.Instance.GUI_HidePlayerActions();

        //Since the player drew, split and double are now disabled
        doubleAction.interactable = false;
        m_splitButton.SetActive(false);

        //Ask the dealer to deal a new card to the player
        m_dealer.DealNewCardToPlayer();
    }

    //Called when the player wants to split their hand
    public void OnPlayerSplit()
    {
        //Get player money
        float playerMoney = PlayerPrefsManager.getPlayerMoney();

        //If the player can't afford to split
        if (m_currentBet > playerMoney)
        {
            //Tell the player to look at their money
            GUI_Handler.Instance.PlayNegativeCashFeedback();
        }

        //If the player can afford to split
        else
        {
            //Deduct the bet from player money and update money label
            m_playerMoney -= m_currentBet;
            lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));

            //Save new player money
            PlayerPrefsManager.ReducePlayerMoney(m_currentBet);

            //Make the bet split equals to current bet
            m_currentSplitBet = m_currentBet;

            //Hide player actions
            GUI_Handler.Instance.GUI_HidePlayerActions();

            //Player can no longer split
            m_splitButton.SetActive(false);

            //Set split bet label
            lbl_PlayerSplitBet.SetText(string.Format(playerBetTemplate, m_currentSplitBet));

            //Play bet sfx
            SFXHandler.Instance.PlayPlaceChipsSfx();

            //Activate the split message containers so they are centered on their respective hands
            m_playerSplitBetContainer.SetActive(true);
            m_splitCardsContainer.SetActive(true);
            m_splitValueContainer.SetActive(true);
            m_splitBustedContainer.SetActive(true);
            m_splitBlackjackContainer.SetActive(true);
            m_splitWinContainer.SetActive(true);
            m_splitLoseContainer.SetActive(true);
            m_splitDrawContainer.SetActive(true);

            //Ask the dealer to split player's hand
            m_dealer.SplitHand();
        }
    }

    //Called when the player draws a card
    public void OnPlayerCardDrawn(bool forSplitHand = false)
    {
        //If the active hand is busted
        if (m_player.IsHandBusted(forSplitHand))
        {
            //Notify dealer
            m_dealer.OnPlayerBusted();

            //Show busted message
            GUI_Handler.Instance.ShowPlayerBusted(forSplitHand);

            //If split hand busted (Maybe from double). Let base hand double too.
            if (forSplitHand)
            {
                m_hasPlayerDoubled = false;
                doubleAction.interactable = true;
            }
        }

        //If the player's hand did not bust
        else
        {

            //If this is a draw from double
            if (m_hasPlayerDoubled)
            {
                //Player can no longer ask for cards on this hand
                OnPlayerStand();
            }

            //If this card was not drawn from double
            else
            {
                //If the dealer doesn't have blackjack
                if (!m_dealerBlackjack)
                {
                    //Player cannot split if it was dealt a new card
                    m_splitButton.SetActive(false);

                    //Let the player continue
                    GUI_Handler.Instance.GUI_ShowPlayerActions();
                }
            }
        }
    }

    //Called when the player decides or is forced to end their turn
    public void OnPlayerStand()
    {
        //Reset double and split elements
        m_hasPlayerDoubled = false;
        m_splitButton.SetActive(false);
        doubleAction.interactable = true;
        
        //Hide player actions
        GUI_Handler.Instance.GUI_HidePlayerActions();

        //Notify dealer
        m_dealer.OnPlayerStand();
    }

    //Called when the match has ended
    public void OnMatchEnded()
    {
        float reward = 0;

        //Get dealer handValue
        int dealerHandValue = m_dealer.CalculateHandValue();

        //Add the reward from base hand if any
        reward += CalculateReward(dealerHandValue, false);

        //If the player splitted previously
        if (m_dealer.IsSplitHandAvailable())
        {
            //Add the reward from split hand if any
            reward += CalculateReward(dealerHandValue, true);
        }

        //Save player money
        PlayerPrefsManager.IncreasePlayerMoney(reward);

        //Clean bets
        m_currentBet = 0;
        m_currentSplitBet = 0;
        
        //Clean bet station label
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));

        //Start reward feedback animation
        StartCoroutine(GiveRewardCoroutine(reward));
    }

    IEnumerator GiveRewardCoroutine(float reward)
    {
        //If there IS a reward
        if (reward > 0)
        {
            //Calculate 10 steps of animation 
            const float IncrementSteps = 10f;
            float increments = reward / IncrementSteps;

            //Calculate money after reward
            float targetMoney = m_playerMoney + reward;

            //Set the reward label and animate its entrance
            lbl_PlayerReward.SetText(string.Format(rewardTemplate, reward.ToString(TwoDecimalsFormat)));
            m_playerRewardTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);

            //Play reward SFX
            SFXHandler.Instance.PlayRewardChipsSfx();

            //Let the player gaze into its fruitious reward
            yield return Yielders.WaitForSeconds(Constants.SECOND_DELAY); 

            //Animate player money and reward labels in increments
            while (m_playerMoney != targetMoney)
            {
                m_playerMoney += increments;
                reward -= increments;
                lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
                lbl_PlayerReward.SetText(string.Format(rewardTemplate, reward.ToString(TwoDecimalsFormat)));
                yield return Yielders.WaitForSeconds(Constants.NANO_DELAY);
            }

            //When done, hide reward label
            m_playerRewardTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        }

        //If the separator has been found we need to shuffle the deck.
        if (m_deckHandler.IsCurrentDeckOver())
        {
            StartCoroutine(ShuffleDeckRoutine());
        }

        //If the separator has not been found 
        else
        {
            //Check if the player still has money to bet or not
            StartCoroutine(ShowBetsOrRetryRoutine());
        }
    }

    //Calculate the reward based on hand values
    private float CalculateReward(int dealerHand, bool forSplitHand)
    {
        float reward = 0f;

        //Get player hand value and blackjack status
        int playerHandValue = m_player.CalculateHandValue(forSplitHand);
        bool playerHasBlackJack = m_player.PlayerHasBlackJack(forSplitHand);

        //If the player has blackjack its value is 21
        if (playerHasBlackJack)
        {
            playerHandValue = Constants.BLACKJACK;
        }

        //If the dealer has blackjack its value is 21
        if (m_dealerBlackjack)
        {
            dealerHand = Constants.BLACKJACK;
        }

        /**
         * If the player has blackjack and the dealer does not OR
         * The dealer busted OR
         * The player has more score than the dealer without busting.
         * 
         * Player wins.
         */ 
        if ((playerHasBlackJack && !m_dealerBlackjack) || m_dealer.IsHandBusted() || ((playerHandValue > dealerHand) && !m_player.IsHandBusted(forSplitHand)))
        {
            //Return its bet to the player
            reward = m_currentBet;

            //Give the player their reward
            if (playerHasBlackJack)
            {
                //Blackjack pays 3 to 2
                reward += m_currentBet * Constants.BLACKJACK_RATIO;
            }
            else
            {
                //Regular bet
                reward += m_currentBet;
            }

            //Enable win message
            GUI_Handler.Instance.ShowPlayerWin(forSplitHand);
        }

        /**
         * If the dealer has blackjack and the player does not OR
         * The player has busted
         * The dealer has more score than the player without busting.
         * 
         * The dealer wins
         */
        else if ((!playerHasBlackJack && m_dealerBlackjack) || m_player.IsHandBusted(forSplitHand) || ((dealerHand > playerHandValue) && !m_dealer.IsHandBusted()))
        {
            if (!m_player.IsHandBusted(forSplitHand))
            {
                //Only show lose message if the player has not busted
                //Otherwise the messages will overlap and busted has priority over lose
                GUI_Handler.Instance.ShowPlayerLose(forSplitHand);
            }
        }

        //If it is a draw
        else if (playerHandValue == dealerHand)
        {
            //Give the bet back to the player
            reward = m_currentBet;

            //Show draw message
            GUI_Handler.Instance.ShowPlayerDraw(forSplitHand);
        }

        //Return the reward
        return reward;
    }

    //Called when the separator is found
    public void OnSeparatorFound()
    {
        //Show separator explanation
        m_shuffleDescription.SetActive(true);
    }

    //Decide which to show after a delay
    IEnumerator ShowBetsOrRetryRoutine()
    {
        yield return Yielders.WaitForSeconds(Constants.SECOND_DELAY);
        ShowBetsOrRetry();
    }

    //Decide whether to show bet or retry station
    private void ShowBetsOrRetry()
    {
        //If player has more money than the minimum bet
        if (m_playerMoney >= Constants.MIN_BET)
        {
            //Let the player continue
            GUI_Handler.Instance.ShowBettingStation();
        }
        
        //If the player cannot bet anymore
        else
        {
            //Animate retry station
            StartCoroutine(ShowRetryRoutine());
        }
    }

    //Show retry station after a delay
    IEnumerator ShowRetryRoutine()
    {
        yield return Yielders.WaitForSeconds(Constants.SECOND_DELAY);

        //Show retry station
        GUI_Handler.Instance.GUI_ShowRetry();
    }

    //Called when the player wants to retry
    public void OnRetry()
    {
        //Restart the game properties
        Restart();

        //Let the player start betting
        GUI_Handler.Instance.ShowBettingStation();
    }

    //Called when the player does not want to retry
    public void OnNotRetry()
    {
        //Restart the game properties
        Restart();
    }

    //Restart the game properties
    private void Restart()
    {
        //Give the player the starting money
        PlayerPrefsManager.setPlayerMoney(Constants.STARTING_MONEY);
        m_playerMoney = Constants.STARTING_MONEY;

        //Reset gameplay elements
        InitializeElements();

        //Update labels
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, 0));

        //Prepare a new play deck
        m_deckHandler.PrepareNewDeck();
    }

    //Shuffle animation
    private void PlayShuffleAnimation()
    {
        //Hide shuffle explanation
        m_shuffleDescription.SetActive(false);

        //Trigger the start of the animation
        shuffleAnimator.SetTrigger(shuffleTrigger);
    }

    //When the shuffle is ready
    public void OnShuffleComplete()
    {
        //Activate the deck again
        m_deckContainer.SetActive(true);

        //Decide which station to show
        ShowBetsOrRetry();

        //Reset animation trigger
        shuffleAnimator.ResetTrigger(shuffleTrigger);
    }

    // Shuffle routine
    IEnumerator ShuffleDeckRoutine()
    {
        //Let the player read the separator explanation for a little while
        yield return Yielders.WaitForSeconds(Constants.TWO_SECONDS_DELAY);

        //Reset gameplay elements
        InitializeElements();

        //Deactivate the deck container so it does not interferes withe the animation
        m_deckContainer.SetActive(false);

        //Prepare a new play deck
        m_deckHandler.PrepareNewDeck();

        //Start shuffle animation
        PlayShuffleAnimation();
    }

    //Callen when player starts the game
    public void OnPlayerPressedStart()
    {
        //Start by shuffling the deck
        PlayShuffleAnimation();

        // Request Banner
        RequestBanner();
    }

#if UNITY_EDITOR
    //Cheats for debugging

    [ContextMenu("Give Money")]
    public void CHEAT_GiveMoney()
    {
        PlayerPrefsManager.setPlayerMoney(Constants.STARTING_MONEY);
    }

    [ContextMenu("Delete Money")]
    public void CHEAT_DeleteMoney()
    {
        PlayerPrefsManager.setPlayerMoney(0);
    }
#endif
}
