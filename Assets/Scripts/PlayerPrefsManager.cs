using UnityEngine;

public class PlayerPrefsManager
{
    const string AUDIO_TOGGLE_KEY = "AUDIO_TOGGLE_KEY";
    const string PLAYER_COINS_KEY = "PLAYER_COINS_KEY";
    
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

    public static int getPlayerCoins()
    {
        return PlayerPrefs.GetInt(PLAYER_COINS_KEY, 0);
    }

    public static void setPlayerCoins(int value)
    {
        PlayerPrefs.SetInt(PLAYER_COINS_KEY, value);
        PlayerPrefs.Save();
    }

    public static void IncreasePlayerCoins(int increment)
    {
        int newCoinsQuantity = getPlayerCoins();
        newCoinsQuantity += increment;
        setPlayerCoins(newCoinsQuantity);
    }

}
