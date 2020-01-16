using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dealer : MonoBehaviour
{

    [SerializeField] Player m_player;

    [ContextMenu ("Deal Initial Cards")]
    public void DealInitialCards()
    {
        m_player.AddCard(DeckHandler.Instance.DrawCard());
        m_player.AddCard(DeckHandler.Instance.DrawCard());
        SFXHandler.Instance.PlayCardSound();
    }
}
