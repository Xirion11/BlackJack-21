﻿using UnityEngine;

public class PlayerPrefsManager
{
    const string AUDIO_TOGGLE_KEY = "AUDIO_TOGGLE_KEY";
    const string PLAYER_MONEY_KEY = "PLAYER_MONEY_KEY";
    
    public static bool getIsAudioEnabled()
    {
        bool result = PlayerPrefs.GetInt(AUDIO_TOGGLE_KEY, 1) == 1;
        return result;
    }

    public static void setAudioEnabled(bool value)
    {
        int flag = value ? 1 : 0;
        PlayerPrefs.SetInt(AUDIO_TOGGLE_KEY, flag);
        PlayerPrefs.Save();
    }

    public static int getPlayerMoney()
    {
        return PlayerPrefs.GetInt(PLAYER_MONEY_KEY, 1000);
    }

    public static void setPlayerMoney(int value)
    {
        PlayerPrefs.SetInt(PLAYER_MONEY_KEY, value);
        PlayerPrefs.Save();
    }

    public static void IncreasePlayerMoney(int increment)
    {
        int newCoinsQuantity = getPlayerMoney();
        newCoinsQuantity += increment;
        setPlayerMoney(newCoinsQuantity);
    }

    public static void ReducePlayerMoney(int decrement)
    {
        int newCoinsQuantity = getPlayerMoney();
        newCoinsQuantity -= decrement;
        setPlayerMoney(newCoinsQuantity);
    }

}
