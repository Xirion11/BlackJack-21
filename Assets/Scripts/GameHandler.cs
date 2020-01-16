using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private Player m_player = null;
    [SerializeField] private Dealer m_dealer = null;
    [SerializeField] private AudioButton doubleAction;
    [SerializeField] private TextMeshProUGUI lbl_playerMoney = null;
    [SerializeField] private TextMeshProUGUI lbl_betStationBet = null;
    [SerializeField] private TextMeshProUGUI lbl_playerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_dealerHandValue = null;

    int m_currentBet = 0;
    int m_playerMoney = 0;
    string moneyTemplate = "${0}";
    string placeBetTemplate = "Place your bet: {0}";

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
        GUI_Handler.Instance.ShowBettingStation();
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
        m_dealer.DealInitialCards();
    }

    public void UpdatePlayerHandValue()
    {
        lbl_playerHandValue.SetText(m_player.UpdateHandValue());
    }

    public void UpdateDealerHandValue()
    {
        lbl_dealerHandValue.SetText(m_dealer.UpdateHandValue());
    }

    public void OnInitialHandsReady()
    {
        doubleAction.interactable = true;
        //TODO: Splits ?
        GUI_Handler.Instance.GUI_ShowPlayerActions();
    }
}
