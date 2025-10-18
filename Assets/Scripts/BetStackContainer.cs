using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetStackContainer : MonoBehaviour
{
    [SerializeField] private BetStack[] _betStacks;

    public void SetValue(int value, bool isDouble = false)
    {
        var valueLeft = value;
        foreach (var betStack in _betStacks)
        {
            betStack.ResetStack();
            valueLeft = betStack.SetValue(valueLeft, isDouble);
        }
    }
}
