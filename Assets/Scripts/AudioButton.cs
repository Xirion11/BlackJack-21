 using UnityEngine;
 using UnityEngine.UI;
 using UnityEngine.EventSystems;
 using System.Collections;

public class AudioButton : Button
{
    public bool m_PositiveFeedback = true;
    const string _SFX_NOT_PRESENT = "SFX Handler is not present";

    protected override void Start()
    {
        onClick.AddListener(delegate { PlayAudioFeedback(); });
        base.Start();
    }

    private void PlayAudioFeedback()
    {
        try
        {
            SFXHandler.Instance.SoundsUIPlay(m_PositiveFeedback);
        }
        catch (System.Exception exception)
        {
            Debug.Log(exception, this);
            Debug.LogError(_SFX_NOT_PRESENT, this);
        }
    }
}
