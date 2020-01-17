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
    [SerializeField] private Animator shuffleAnimator = null;
    [SerializeField] private AudioButton doubleAction;
    [SerializeField] private TextMeshProUGUI lbl_playerMoney = null;
    [SerializeField] private TextMeshProUGUI lbl_betStationBet = null;
    [SerializeField] private TextMeshProUGUI lbl_playerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_dealerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerBet = null;

    int m_currentBet = 0;
    int m_playerMoney = 0;
    const int BLACKJACK = 21;
    bool m_playerBlackjack = false;
    bool m_dealerBlackjack = false;
    bool m_hasPlayerDoubled = false;
    const string moneyTemplate = "${0}";
    const string placeBetTemplate = "Place your bet: {0}";
    const string playerBetTemplate = "Your Bet<br>${0}";
    const string blackJack = "BlackJack";
    const string shuffleTrigger = "Shuffle";

    const int STARTING_MONEY = 1000;

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

    public void OnBetsReady()
    {
        m_playerBlackjack = false;
        m_dealerBlackjack = false;
        InitializeElements();
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
        PlayerPrefsManager.ReducePlayerMoney(m_currentBet);
        GUI_Handler.Instance.HidePlayerBlackJack();
        m_dealer.DealInitialCards();
    }

    private void InitializeElements()
    {
        //Hide Player's and Dealer's cards
        m_dealer.CleanTable();
        lbl_playerHandValue.SetText(string.Empty);
        lbl_dealerHandValue.SetText(string.Empty);
    }

    public void UpdatePlayerHandValue()
    {
        if (!m_playerBlackjack)
        {
            lbl_playerHandValue.SetText(m_player.UpdateHandValue());
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
        if (!m_playerBlackjack && !m_dealerBlackjack)
        {
            m_hasPlayerDoubled = false;
            doubleAction.interactable = true;
            //TODO: Splits ?
            GUI_Handler.Instance.GUI_ShowPlayerActions();
        }
        else
        {
            OnPlayerStand();
        }
    }

    public void OnPlayerBlackJack()
    {
        m_playerBlackjack = true;
        lbl_playerHandValue.SetText(string.Empty);
        GUI_Handler.Instance.ShowPlayerBlackJack();
    }

    public void OnDealerBlackJack()
    {
        m_dealerBlackjack = true;
        lbl_dealerHandValue.SetText(blackJack);
    }

    public void OnPlayerDoubled()
    {
        GUI_Handler.Instance.GUI_HidePlayerActions();
        m_hasPlayerDoubled = true;
        m_playerMoney -= m_currentBet;
        m_currentBet += m_currentBet;
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_PlayerBet.SetText(string.Format(playerBetTemplate, m_currentBet));
        OnPlayerHit();
    }

    public void OnPlayerHit()
    {
        GUI_Handler.Instance.GUI_HidePlayerActions();
        doubleAction.interactable = false;
        m_dealer.DealNewCardToPlayer();
    }

    public void OnPlayerCardDrawn()
    {
        if (m_player.IsHandBusted())
        {
            m_dealer.OnPlayerBusted();
            //TODO: Show Busted Message
            OnMatchEnded();
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
                    doubleAction.interactable = false;
                    GUI_Handler.Instance.GUI_ShowPlayerActions();
                }
            }
        }
    }

    public void OnPlayerStand()
    {
        GUI_Handler.Instance.GUI_HidePlayerActions();
        m_dealer.DrawHand();
    }

    public void OnMatchEnded()
    {
        int playerHand = m_player.CalculateHandValue();
        int dealerHand = m_dealer.CalculateHandValue();

        if (m_playerBlackjack)
        {
            playerHand = BLACKJACK;
        }

        if (m_dealerBlackjack)
        {
            dealerHand = BLACKJACK;
        }

        Debug.Log("Player Money: " + m_playerMoney, this);
        if ((m_playerBlackjack && !m_dealerBlackjack) || m_dealer.IsHandBusted() || ((playerHand > dealerHand) && !m_player.IsHandBusted()))
        {
            float prize = m_currentBet;

            if (m_playerBlackjack)
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
        else if ((!m_playerBlackjack && m_dealerBlackjack) || m_player.IsHandBusted() || dealerHand > playerHand)
        {
            Debug.Log("Dealer win. Bet: " + m_currentBet + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }
        else if (playerHand == dealerHand)
        {
            PlayerPrefsManager.IncreasePlayerMoney(m_currentBet);
            Debug.Log("Draw. Bet: " + m_currentBet + " Hands P: " + playerHand + " H:" + dealerHand, this);
        }

        m_currentBet = 0;

        //TODO: Update player money label
        //TODO: Show Loser/Winner
        //TODO: Fade bet container

        m_playerMoney = PlayerPrefsManager.getPlayerMoney();
        lbl_playerMoney.SetText(string.Format(moneyTemplate, m_playerMoney));
        lbl_betStationBet.SetText(string.Format(placeBetTemplate, 0));

        if (m_deckHandler.IsCurrentDeckOver())
        {

        }
        else
        {
            StartCoroutine(ShowBetsOrRetry());
        }
    }

    IEnumerator ShowBetsOrRetry()
    {
        yield return Yielders.WaitForSeconds(1);

        if (m_playerMoney > 0)
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
        shuffleAnimator.SetTrigger(shuffleTrigger);
    }

    public void OnShuffleComplete()
    {
        m_deckContainer.SetActive(true);
        GUI_Handler.Instance.ShowBettingStation();
        shuffleAnimator.ResetTrigger(shuffleTrigger);
    }

    [ContextMenu("Give Money")]
    public void CHEAT_GiveMoney()
    {
        PlayerPrefsManager.setPlayerMoney(5);
    }
}
