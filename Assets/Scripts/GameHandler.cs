using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lbl_playerHandValue = null;
    [SerializeField] private TextMeshProUGUI lbl_dealerHandValue = null;

    // Start is called before the first frame update
    void Start()
    {
        lbl_playerHandValue.SetText(string.Empty);
        lbl_dealerHandValue.SetText(string.Empty);
    }

    
}
