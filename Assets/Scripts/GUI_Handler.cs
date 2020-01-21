﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GUI_Handler : MonoBehaviour
{
    [SerializeField] private GameObject LogoContainer = null;
    [SerializeField] private GameObject PlayerActionsContainer = null;
    [SerializeField] private Transform BettingStation = null;
    [SerializeField] private Transform RetryStation = null;
    [SerializeField] private Transform PlayerMoneyTransform = null;
    [SerializeField] private Transform PlayerBetTransform = null;
    [SerializeField] private Transform[] bettingChipsTransform = null;
    [SerializeField] private Transform DealerBustedTransform = null;
    [SerializeField] private Transform PlayerBlackJackTransform = null;
    [SerializeField] private Transform PlayerSplitBlackJackTransform = null;
    [SerializeField] private Transform PlayerBustedTransform = null;
    [SerializeField] private Transform PlayerSplitBustedTransform = null;
    [SerializeField] private Transform PlayerWinTransform = null;
    [SerializeField] private Transform PlayerSplitWinTransform = null;
    [SerializeField] private Transform PlayerLoseTransform = null;
    [SerializeField] private Transform PlayerSplitLoseTransform = null;
    [SerializeField] private Transform PlayerDrawTransform = null;
    [SerializeField] private Transform PlayerSplitDrawTransform = null;
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

    private float[] bettingValues =
    { 
        5f,
        10f,
        50f,
        100f,
        500f,
        1000f
    };

    public static GUI_Handler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    
    public void ShowBettingStation()
    {
        BettingStation.DOScale(Vector3.one, Constants.QUICK_DELAY);
    }

    public void ShowPlayerBlackJack(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitBlackJackTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
        else
        {
            PlayerBlackJackTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
    }

    public void HidePlayerBlackJack()
    {
        PlayerBlackJackTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        PlayerSplitBlackJackTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
    }

    public void ShowDealerBusted()
    {
        DealerBustedTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
    }

    public void HideDealerBusted()
    {
        DealerBustedTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
    }

    public void ShowPlayerBusted(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitBustedTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
        else
        {
            PlayerBustedTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
    }

    public void HidePlayerBusted()
    {
        PlayerBustedTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        PlayerSplitBustedTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
    }

    public void ShowPlayerWin(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitWinTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
        else
        {
            PlayerWinTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
    }

    public void HidePlayerWin(bool forSplitHand = false)
    {
        PlayerSplitWinTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        PlayerWinTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
    }

    public void ShowPlayerLose(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitLoseTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
        else
        {
            PlayerLoseTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
    }

    public void HidePlayerLose(bool forSplitHand = false)
    {
        PlayerSplitLoseTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        PlayerLoseTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
    }

    public void ShowPlayerDraw(bool forSplitHand = false)
    {
        if (forSplitHand)
        {
            PlayerSplitDrawTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
        else
        {
            PlayerDrawTransform.DOScale(Vector3.one, Constants.QUICK_DELAY);
        }
    }

    public void HidePlayerDraw(bool forSplitHand = false)
    {
        PlayerSplitDrawTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
        PlayerDrawTransform.DOScale(Vector3.zero, Constants.QUICK_DELAY);
    }

    public void GUI_IncreaseBet(int index)
    {
        bettingChipsTransform[index].DOPunchScale(bettingPunch, bettingPunchDuration, bettingPunchVibrato, bettingPunchElasticity)
            .OnComplete(() => bettingChipsTransform[index].DOScale(Vector3.one, Constants.QUICK_DELAY));

        float playerMoney = PlayerPrefsManager.getPlayerMoney();
        float nextBet = GameHandler.Instance.GetCurrentBet() + bettingValues[index];

        if (nextBet <= playerMoney)
        {
            SFXHandler.Instance.PlayUISfx();
            GameHandler.Instance.IncreaseCurrentBet(bettingValues[index]);
        }
        else
        {
            PlayNegativeCashFeedback();
        }
    }

    public void PlayNegativeCashFeedback()
    {
        SFXHandler.Instance.PlayNegativeUISfx();

        PlayerMoneyTransform.DOPunchPosition(negativePunch, negativePunchDuration, negativePunchVibrato, negativePunchElasticity)
            .OnComplete(() => PlayerMoneyTransform.DOMove(PlayerMoneyTransform.position, Constants.QUICK_DELAY));

        lbl_PlayerMoney.DOColor(Color.red, negativePunchDuration)
            .SetEase(Ease.Flash, Constants.FEEDBACK_AMPLITUDE, Constants.SECOND_DELAY)
            .OnComplete(() => lbl_PlayerMoney.DOColor(Color.white, Constants.QUICK_DELAY));
    }

    public void PlayInvalidBetFeedback()
    {
        SFXHandler.Instance.PlayNegativeUISfx();

        PlayerBetTransform.DOPunchPosition(negativePunch, negativePunchDuration, negativePunchVibrato, negativePunchElasticity)
            .OnComplete(() => PlayerBetTransform.DOMove(PlayerBetTransform.position, Constants.QUICK_DELAY));

        lbl_PlayerBet.DOColor(Color.red, negativePunchDuration)
            .SetEase(Ease.Flash, Constants.FEEDBACK_AMPLITUDE, Constants.SECOND_DELAY)
            .OnComplete(() => lbl_PlayerBet.DOColor(Color.white, Constants.QUICK_DELAY));
    }

    public void GUI_ClearBet()
    {
        GameHandler.Instance.ClearCurrentBet();
    }

    public void GUI_BetReady()
    {
        if (GameHandler.Instance.GetCurrentBet() != 0)
        {
            BettingStation.DOScale(Vector3.zero, Constants.QUICK_DELAY)
                .OnComplete(() => GameHandler.Instance.OnBetsReady());
        }
        else
        {
            PlayInvalidBetFeedback();
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
        RetryStation.DOScale(Vector3.one, Constants.QUICK_DELAY);
    }

    public void GUI_HideRetry()
    {
        RetryStation.DOScale(Vector3.zero, Constants.QUICK_DELAY);
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
        GameHandler.Instance.OnNotRetry();
        ShowLogo();
    }

    public void ShowLogo()
    {
        LogoContainer.SetActive(true);
    }

    public void HideLogo()
    {
        LogoContainer.SetActive(false);
    }

    public void GUI_Start()
    {
        HideLogo();
        GameHandler.Instance.OnPlayerPressedStart();
    }
}
