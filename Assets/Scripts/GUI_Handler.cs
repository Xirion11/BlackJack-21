using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GUI_Handler : MonoBehaviour
{
    [SerializeField] private GameObject PlayerBetContainer = null;
    [SerializeField] private GameObject PlayerActionsContainer = null;
    [SerializeField] private Transform BettingStation = null;
    [SerializeField] private Transform RetryStation = null;
    [SerializeField] private Transform PlayerMoneyTransform = null;
    [SerializeField] private Transform[] bettingChipsTransform = null;
    [SerializeField] private Transform PlayerBlackJackTransform = null;
    [SerializeField] private Transform PlayerSplitBlackJackTransform = null;
    [SerializeField] private TextMeshProUGUI lbl_PlayerMoney = null;

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

    public void ShowPlayerBlackJack(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitBlackJackTransform.DOScale(Vector3.one, 0.2f);
        }
        else
        {
            PlayerBlackJackTransform.DOScale(Vector3.one, 0.2f);
        }
    }

    public void HidePlayerBlackJack()
    {
        PlayerBlackJackTransform.DOScale(Vector3.zero, 0.2f);
        PlayerSplitBlackJackTransform.DOScale(Vector3.zero, 0.2f);
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
            BettingStation.DOScale(Vector3.zero, 0.2f)
                .OnComplete(() => GameHandler.Instance.OnBetsReady());
            PlayerBetContainer.SetActive(true);
        }
        else
        {
            //TODO: Negative feedback
        }
    }

    public void GUI_ShowPlayerActions()
    {
        PlayerActionsContainer.SetActive(true);
    }

    public void GUI_HidePlayerActions()
    {
        PlayerActionsContainer.SetActive(false);
    }

    public void GUI_ShowRetry()
    {
        RetryStation.DOScale(Vector3.one, 0.2f);
    }

    public void GUI_HideRetry()
    {
        RetryStation.DOScale(Vector3.zero, 0.2f);
    }

    public void GUI_DoubleBet()
    {
        GameHandler.Instance.OnPlayerDoubled();
    }

    public void GUI_Hit()
    {
        GameHandler.Instance.OnPlayerHit();
    }

    public void GUI_Split()
    {
        GameHandler.Instance.OnPlayerSplit();
    }

    public void GUI_Stand()
    {
        GameHandler.Instance.OnPlayerStand();
    }

    public void GUI_Retry()
    {
        GUI_HideRetry();
        GameHandler.Instance.OnRetry();
    }

    public void GUI_NoRetry()
    {
        GUI_HideRetry();
    }
}
