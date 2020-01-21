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

    public static GameHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_playerMoney = PlayerPrefsManager.getPlayerMoney();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_playerHandValue.SetText(string.Empty);
        lbl_dealerHandValue.SetText(string.Empty);
    }

    public void IncreaseCurrentBet(float increment)
    {
        m_currentBet += increment;
        m_playerMoney -= increment;
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));
    }

    public void ClearCurrentBet()
    {
        m_playerMoney += m_currentBet;
        m_currentBet = 0f;
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));
    }

    public float GetCurrentBet()
    {
        return m_currentBet;
    }

    public float GetCurrentSplitBet()
    {
        return m_currentSplitBet;
    }

    public void OnBetsReady()
    {
        //m_lastBet = m_currentBet;
        m_dealerBlackjack = false;
        InitializeElements();
        SFXHandler.Instance.PlayPlaceChipsSfx();
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

    private void InitializeElements()
    {
        //Hide Player's and Dealer's cards
        //m_currentBet = 0;
        //m_currentSplitBet = 0;
        //m_dealerBlackjack = false;
        //m_hasPlayerDoubled = false;

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
            lbl_dealerHandValue.SetText(m_dealer.CalculateHandValue().ToString());
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
        float playerMoney = PlayerPrefsManager.getPlayerMoney();
        float nextBet = m_currentBet + m_currentBet;

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

            SFXHandler.Instance.PlayPlaceChipsSfx();

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

            lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));

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
        float playerMoney = PlayerPrefsManager.getPlayerMoney();

        if (m_currentBet > playerMoney)
        {
            GUI_Handler.Instance.PlayNegativeCashFeedback();
        }
        else
        {
            m_playerMoney -= m_currentBet;
            lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
            PlayerPrefsManager.ReducePlayerMoney(m_currentBet);
            m_currentSplitBet = m_currentBet;
            GUI_Handler.Instance.GUI_HidePlayerActions();
            m_splitButton.SetActive(false);
            lbl_PlayerSplitBet.SetText(string.Format(playerBetTemplate, m_currentSplitBet));
            SFXHandler.Instance.PlayPlaceChipsSfx();
            m_playerSplitBetContainer.SetActive(true);
            m_splitCardsContainer.SetActive(true);
            m_splitValueContainer.SetActive(true);
            m_splitBustedContainer.SetActive(true);
            m_splitBlackjackContainer.SetActive(true);
            m_splitWinContainer.SetActive(true);
            m_splitLoseContainer.SetActive(true);
            m_splitDrawContainer.SetActive(true);
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
        m_splitButton.SetActive(false);
        doubleAction.interactable = true; 
        GUI_Handler.Instance.GUI_HidePlayerActions();
        m_dealer.OnPlayerStand();
    }

    public void OnMatchEnded()
    {
        float prize = 0;
        int dealerCardValue = m_dealer.CalculateHandValue();

        prize += CalculateReward(m_player.CalculateHandValue(), dealerCardValue, m_player.PlayerHasBlackJack(), false);

        if (m_dealer.IsSplitHandAvailable())
        {
            prize += CalculateReward(m_player.CalculateHandValue(true), dealerCardValue, m_player.PlayerHasBlackJack(true), true);
        }

        //m_currentBet = m_lastBet;
        m_currentBet = 0;
        m_currentSplitBet = 0;

        //TODO: Fade bet container

        
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, m_currentBet));

        StartCoroutine(GiveRewardCoroutine(prize));
    }

    IEnumerator GiveRewardCoroutine(float prize)
    {
        if (prize > 0)
        {
            const float IncrementSteps = 10f;
            float increments = prize / IncrementSteps;
            float targetMoney = m_playerMoney + prize;

            lbl_PlayerReward.SetText(string.Format(rewardTemplate, prize.ToString(TwoDecimalsFormat)));
            m_playerRewardTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);

            SFXHandler.Instance.PlayRewardChipsSfx();

            yield return Yielders.WaitForSeconds(Constants.SECOND_DELAY);            

            PlayerPrefsManager.IncreasePlayerMoney(prize);

            while (m_playerMoney != targetMoney)
            {
                m_playerMoney += increments;
                prize -= increments;
                lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
                lbl_PlayerReward.SetText(string.Format(rewardTemplate, prize.ToString(TwoDecimalsFormat)));
                yield return Yielders.WaitForSeconds(Constants.NANO_DELAY);
            }
        }

        m_playerRewardTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);

        if (m_deckHandler.IsCurrentDeckOver())
        {
            StartCoroutine(ShuffleDeckRoutine());
        }
        else
        {
            StartCoroutine(ShowBetsOrRetryRoutine());
        }
    }

    private float CalculateReward(int playerHand, int dealerHand, bool playerHasBlackJack, bool forSplitHand)
    {
        float prize = 0f;

        if (playerHasBlackJack)
        {
            playerHand = Constants.BLACKJACK;
        }

        if (m_dealerBlackjack)
        {
            dealerHand = Constants.BLACKJACK;
        }

        //Debug.Log("Player Money: " + m_playerMoney, this);
        if ((playerHasBlackJack && !m_dealerBlackjack) || m_dealer.IsHandBusted() || ((playerHand > dealerHand) && !m_player.IsHandBusted(forSplitHand)))
        {
            prize = m_currentBet;

            if (playerHasBlackJack)
            {
                prize += m_currentBet * Constants.BLACKJACK_RATIO;
            }
            else
            {
                prize += m_currentBet;
            }

            GUI_Handler.Instance.ShowPlayerWin(forSplitHand);

            //Debug.Log("Player win. Bet: " + prize + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }
        else if ((!playerHasBlackJack && m_dealerBlackjack) || m_player.IsHandBusted(forSplitHand) || dealerHand > playerHand)
        {
            if (!m_player.IsHandBusted(forSplitHand))
            {
                GUI_Handler.Instance.ShowPlayerLose(forSplitHand);
            }

            //Debug.Log("Dealer win. Bet: " + m_currentBet + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }
        else if (playerHand == dealerHand)
        {
            prize = m_currentBet;
            GUI_Handler.Instance.ShowPlayerDraw(forSplitHand);
            //Debug.Log("Draw. Bet: " + m_currentBet + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }

        return prize;
    }

    public void OnSeparatorFound()
    {
        m_shuffleDescription.SetActive(true);
    }

    IEnumerator ShowBetsOrRetryRoutine()
    {
        yield return Yielders.WaitForSeconds(Constants.SECOND_DELAY);
        ShowBetsOrRetry();
    }

    private void ShowBetsOrRetry()
    {
        if (m_playerMoney >= Constants.MIN_BET)
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
        yield return Yielders.WaitForSeconds(Constants.SECOND_DELAY);

        GUI_Handler.Instance.GUI_ShowRetry();
    }

    public void OnRetry()
    {
        PlayerPrefsManager.setPlayerMoney(Constants.STARTING_MONEY);
        m_playerMoney = Constants.STARTING_MONEY;
        InitializeElements();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
        m_deckHandler.PrepareNewDeck();
        GUI_Handler.Instance.ShowBettingStation();
    }

    public void OnNotRetry()
    {
        PlayerPrefsManager.setPlayerMoney(Constants.STARTING_MONEY);
        m_playerMoney = Constants.STARTING_MONEY;
        InitializeElements();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney.ToString(TwoDecimalsFormat)));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, 0));
        m_deckHandler.PrepareNewDeck();
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
        yield return Yielders.WaitForSeconds(Constants.TWO_SECONDS_DELAY);

        InitializeElements();
        m_deckContainer.SetActive(false);
        m_deckHandler.PrepareNewDeck();
        PlayShuffleAnimation();
    }

    public void OnPlayerPressedStart()
    {
        PlayShuffleAnimation();
    }

#if UNITY_EDITOR
    [ContextMenu("Give Money")]
    public void CHEAT_GiveMoney()
    {
        PlayerPrefsManager.setPlayerMoney(Constants.STARTING_MONEY*10);
    }
#endif
}
