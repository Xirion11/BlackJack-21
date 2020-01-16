 using UnityEngine;
 using UnityEngine.UI;
 using UnityEngine.EventSystems;
 using System.Collections;

public class AudioButton : Button
{
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
            SFXHandler.Instance.PlayUISfx();
        }
        catch (System.Exception exception)
        {
            Debug.Log(exception, this);
            Debug.LogError(_SFX_NOT_PRESENT, this);
        }
    }
}
