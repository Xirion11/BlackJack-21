using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GUI_Handler : MonoBehaviour
{
    [SerializeField] private GameObject PlayerBetContainer = null;
    [SerializeField] private Transform BettingStation = null;
    [SerializeField] private Transform PlayerMoneyTransform = null;
    [SerializeField] private Transform[] bettingChipsTransform = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerMoney = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerBet = null;


    [Header("Feedback Parameters")]
    [SerializeField] private Vector3 bettingPunch;
    [SerializeField] private float bettingPunchDuration = 1f;
    [SerializeField] private int bettingPunchVibrato = 1;
    [SerializeField] private float bettingPunchElasticity = 1f;
    [SerializeField] private Vector3 negativePunch;
    [SerializeField] private float negativePunchDuration = 1f;
    [SerializeField] private int negativePunchVibrato = 1;
    [SerializeField] private float negativePunchElasticity = 1f;

    int[] bettingValues =
    { 
        5,
        10,
        50,
        100,
        500,
        1000
    };

    string playerBetTemplate = "Your Bet<br>${0}";

    public static GUI_Handler Instance { get; private set; }

    enum State
    {
        ON = 0,
        OFF
    }

    private void Awake()
    {
        Instance = this;
    }
    
    public void ShowBettingStation()
    {
        //BettingStation.SetActive(true);
        BettingStation.DOScale(Vector3.one, 0.2f);
    }

    public void GUI_IncreaseBet(int index)
    {
        bettingChipsTransform[index].DOPunchScale(bettingPunch, bettingPunchDuration, bettingPunchVibrato, bettingPunchElasticity)
            .OnComplete(() => bettingChipsTransform[index].DOScale(Vector3.one, 0.2f));

        int playerMoney = PlayerPrefsManager.getPlayerMoney();
        int nextBet = GameHandler.Instance.GetCurrentBet() + bettingValues[index];

        if (nextBet <= playerMoney)
        {
            SFXHandler.Instance.PlayUISfx();
            GameHandler.Instance.IncreaseCurrentBet(bettingValues[index]);
        }
        else
        {
            SFXHandler.Instance.PlayNegativeUISfx();

            PlayerMoneyTransform.DOPunchPosition(negativePunch, negativePunchDuration, negativePunchVibrato, negativePunchElasticity)
                .OnComplete(()=> PlayerMoneyTransform.DOMove(PlayerMoneyTransform.position, 0.2f));

            lbl_PlayerMoney.DOColor(Color.red, negativePunchDuration)
                .SetEase(Ease.Flash, 6f, 1f)
                .OnComplete(()=> lbl_PlayerMoney.DOColor(Color.white, 0.2f));
        }
    }

    public void GUI_ClearBet()
    {
        GameHandler.Instance.ClearCurrentBet();
    }

    public void GUI_BetReady()
    {
        if (GameHandler.Instance.GetCurrentBet() != 0)
        {
            BettingStation.DOScale(Vector3.zero, 0.2f);
            lbl_PlayerBet.SetText(string.Format(playerBetTemplate, GameHandler.Instance.GetCurrentBet()));
            PlayerBetContainer.SetActive(true);
        }
        else
        {
            //TODO: Negative feedback
        }
    }

}
