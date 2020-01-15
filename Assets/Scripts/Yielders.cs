using UnityEngine;
using System.Collections.Generic;

public static class Yielders
{
    static Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(100);

    static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();

    public static WaitForEndOfFrame EndOfFrame
    {
        get { return _endOfFrame; }
    }

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!_timeInterval.ContainsKey(seconds))
            _timeInterval.Add(seconds, new WaitForSeconds(seconds));
        return _timeInterval[seconds];
    }
}