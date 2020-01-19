using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

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
    [SerializeField] private Animator shuffleAnimator = null;
    [SerializeField] private AudioButton doubleAction;
    [SerializeField] private TextMeshProUGUI lbl_playerMoney = null;
    [SerializeField] private TextMeshProUGUI lbl_betStationBet = null;
    [SerializeField] private TextMeshProUGUI lbl_playerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_playerSplitHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_dealerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerBet = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerSplitBet = null;

    int m_currentBet = 0;
    int m_currentSplitBet = 0;
    int m_playerMoney = 0;
    const int BLACKJACK = 21;
    //bool m_playerBlackjack = false;
    bool m_dealerBlackjack = false;
    bool m_hasPlayerDoubled = false;
    const string moneyTemplate = "${0}";
    const string placeBetTemplate = "Place your bet: {0}";
    const string playerBetTemplate = "Your Bet<br>${0}";
    const string blackJack = "BlackJack";
    const string shuffleTrigger = "Shuffle";

    const int STARTING_MONEY = 1000;
    const int MINIMUM_BET = 5;

    public static GameHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_playerMoney = PlayerPrefsManager.getPlayerMoney();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_playerHandValue.SetText(string.Empty);
        lbl_dealerHandValue.SetText(string.Empty);
        PlayShuffleAnimation();
    }

    public void IncreaseCurrentBet(int increment)
    {
        m_currentBet += increment;
        m_playerMoney -= increment;
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));
    }

    public void ClearCurrentBet()
    {
        m_playerMoney += m_currentBet;
        m_currentBet = 0;
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));
    }

    public int GetCurrentBet()
    {
        return m_currentBet;
    }

    public int GetCurrentSplitBet()
    {
        return m_currentSplitBet;
    }

    public void OnBetsReady()
    {
        m_dealerBlackjack = false;
        InitializeElements();
        m_playerBetContainer.SetActive(true);
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
        PlayerPrefsManager.ReducePlayerMoney(m_currentBet);
        GUI_Handler.Instance.HidePlayerBlackJack();
        GUI_Handler.Instance.HidePlayerBusted();
        m_dealer.DealInitialCards();
    }

    private void InitializeElements()
    {
        //Hide Player's and Dealer's cards
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
        GUI_Handler.Instance.HidePlayerBlackJack();
        GUI_Handler.Instance.HidePlayerBusted();
    }

    public void UpdatePlayerHandValue(bool forSplitHand = false)
    {
        bool playerHasBlackJack = m_player.PlayerHasBlackJack(forSplitHand);

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

    public void UpdateDealerHandValue()
    {
        if (!m_dealerBlackjack)
        {
            lbl_dealerHandValue.SetText(m_dealer.UpdateHandValue());
        }
    }

    public void OnInitialHandsReady()
    {
        bool playerHasBlackJack = m_player.PlayerHasBlackJack();

        if (!playerHasBlackJack && !m_dealerBlackjack)
        {
            m_hasPlayerDoubled = false;
            doubleAction.interactable = true;

            if (m_player.IsSplitAvailable())
            {
                m_splitButton.SetActive(true);
            }

            GUI_Handler.Instance.GUI_ShowPlayerActions();
        }
        else
        {
            OnPlayerStand();
        }
    }

    public void OnPlayerBlackJack(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            lbl_playerSplitHandValue.SetText(string.Empty);
        }
        else
        {
            lbl_playerHandValue.SetText(string.Empty);
        }
        
        GUI_Handler.Instance.ShowPlayerBlackJack(forSplitHand);
    }

    public void OnDealerBlackJack()
    {
        m_dealerBlackjack = true;
        lbl_dealerHandValue.SetText(blackJack);
    }

    public void OnPlayerDoubled()
    {
        int playerMoney = PlayerPrefsManager.getPlayerMoney();
        int nextBet = m_currentBet + m_currentBet;

        if (nextBet > playerMoney)
        {
            GUI_Handler.Instance.PlayNegativeCashFeedback();
        }
        else
        {
            GUI_Handler.Instance.GUI_HidePlayerActions();

            m_hasPlayerDoubled = true;

            bool isSplitHandAvailable = m_dealer.IsSplitHandAvailable();

            PlayerPrefsManager.ReducePlayerMoney(m_currentBet);

            if (isSplitHandAvailable)
            {
                bool isSplitHandActive = m_dealer.IsSplitHandActive();

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
            else
            {
                m_playerMoney -= m_currentBet;
                m_currentBet += m_currentBet;
                lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
                m_playerBetDoubleContainer.SetActive(true);
            }

            lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));

            OnPlayerHit();
        }
    }

    public void OnPlayerHit()
    {
        GUI_Handler.Instance.GUI_HidePlayerActions();
        doubleAction.interactable = false;
        m_splitButton.SetActive(false);
        m_dealer.DealNewCardToPlayer();
    }

    public void OnPlayerSplit()
    {
        int playerMoney = PlayerPrefsManager.getPlayerMoney();
        int nextBet = m_currentBet + m_currentBet;

        if (nextBet > playerMoney)
        {
            GUI_Handler.Instance.PlayNegativeCashFeedback();
        }
        else
        {
            m_playerMoney -= m_currentBet;
            lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
            PlayerPrefsManager.ReducePlayerMoney(m_currentBet);
            m_currentSplitBet = m_currentBet;
            GUI_Handler.Instance.GUI_HidePlayerActions();
            m_splitButton.SetActive(false);
            lbl_PlayerSplitBet.SetText(string.Format(playerBetTemplate, m_currentSplitBet));
            m_playerSplitBetContainer.SetActive(true);
            m_splitCardsContainer.SetActive(true);
            m_splitValueContainer.SetActive(true);
            m_splitBustedContainer.SetActive(true);
            m_splitBlackjackContainer.SetActive(true);
            m_dealer.SplitHand();
        }
    }

    public void OnPlayerCardDrawn(bool forSplitHand = false)
    {
        if (m_player.IsHandBusted(forSplitHand))
        {
            m_dealer.OnPlayerBusted();
            GUI_Handler.Instance.ShowPlayerBusted(forSplitHand);
        }
        else
        {
            if (m_hasPlayerDoubled)
            {
                OnPlayerStand();
            }
            else
            {
                if (!m_dealerBlackjack)
                {
                    //doubleAction.interactable = false;
                    m_splitButton.SetActive(false);
                    GUI_Handler.Instance.GUI_ShowPlayerActions();
                }
            }
        }
    }

    public void OnPlayerStand()
    {
        m_hasPlayerDoubled = false;
        doubleAction.interactable = true; 
        GUI_Handler.Instance.GUI_HidePlayerActions();
        m_dealer.OnPlayerStand();
    }

    public void OnMatchEnded()
    {
        int dealerCardValue = m_dealer.CalculateHandValue();

        CalculateReward(m_player.CalculateHandValue(), dealerCardValue, m_player.PlayerHasBlackJack());

        if (m_dealer.IsSplitHandAvailable())
        {
            CalculateReward(m_player.CalculateHandValue(true), dealerCardValue, m_player.PlayerHasBlackJack(true));
        }

        m_currentBet = 0;
        m_currentSplitBet = 0;

        //TODO: Show Loser/Winner
        //TODO: Fade bet container

        m_playerMoney = PlayerPrefsManager.getPlayerMoney();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));

        if (m_deckHandler.IsCurrentDeckOver())
        {
            StartCoroutine(ShuffleDeckRoutine());
        }
        else
        {
            StartCoroutine(ShowBetsOrRetryRoutine());
        }
    }

    private void CalculateReward(int playerHand, int dealerHand, bool playerHasBlackJack)
    {
        if (playerHasBlackJack)
        {
            playerHand = BLACKJACK;
        }

        if (m_dealerBlackjack)
        {
            dealerHand = BLACKJACK;
        }

        Debug.Log("Player Money: " + m_playerMoney, this);
        if ((playerHasBlackJack && !m_dealerBlackjack) || m_dealer.IsHandBusted() || ((playerHand > dealerHand) && !m_player.IsHandBusted()))
        {
            float prize = m_currentBet;

            if (playerHasBlackJack)
            {
                prize += m_currentBet * 1.5f;
            }
            else
            {
                prize += m_currentBet;
            }

            PlayerPrefsManager.IncreasePlayerMoney((int)prize);
            Debug.Log("Player win. Bet: " + prize + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }
        else if ((!playerHasBlackJack && m_dealerBlackjack) || m_player.IsHandBusted() || dealerHand > playerHand)
        {
            Debug.Log("Dealer win. Bet: " + m_currentBet + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }
        else if (playerHand == dealerHand)
        {
            PlayerPrefsManager.IncreasePlayerMoney(m_currentBet);
            Debug.Log("Draw. Bet: " + m_currentBet + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }
    }

    public void OnSeparatorFound()
    {
        m_shuffleDescription.SetActive(true);
    }

    IEnumerator ShowBetsOrRetryRoutine()
    {
        yield return Yielders.WaitForSeconds(1);
        ShowBetsOrRetry();
    }

    private void ShowBetsOrRetry()
    {
        if (m_playerMoney >= MINIMUM_BET)
        {
            GUI_Handler.Instance.ShowBettingStation();
        }
        else
        {
            StartCoroutine(ShowRetryRoutine());
        }
    }

    IEnumerator ShowRetryRoutine()
    {
        yield return Yielders.WaitForSeconds(1f);

        GUI_Handler.Instance.GUI_ShowRetry();
    }

    public void OnRetry()
    {
        PlayerPrefsManager.setPlayerMoney(STARTING_MONEY);
        m_playerMoney = STARTING_MONEY;
        InitializeElements();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
        m_deckHandler.PrepareNewDeck();
        GUI_Handler.Instance.ShowBettingStation();
    }

    private void PlayShuffleAnimation()
    {
        m_shuffleDescription.SetActive(false);
        shuffleAnimator.SetTrigger(shuffleTrigger);
    }

    public void OnShuffleComplete()
    {
        m_deckContainer.SetActive(true);
        ShowBetsOrRetry();
        shuffleAnimator.ResetTrigger(shuffleTrigger);
    }

    IEnumerator ShuffleDeckRoutine()
    {
        yield return Yielders.WaitForSeconds(2f);

        InitializeElements();
        m_deckContainer.SetActive(false);
        m_deckHandler.PrepareNewDeck();
        PlayShuffleAnimation();
    }

    [ContextMenu("Give Money")]
    public void CHEAT_GiveMoney()
    {
        PlayerPrefsManager.setPlayerMoney(1000);
    }
}
