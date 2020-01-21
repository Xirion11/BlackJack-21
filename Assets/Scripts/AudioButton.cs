 using UnityEngine;
 using UnityEngine.UI;
 using UnityEngine.EventSystems;
 using System.Collections;

public class AudioButton : Button
{
    const string _SFX_NOT_PRESENT = "SFX Handler is not present";

    /**
     * This is an extension from Button. Basically it lets us 
     * add this component to basic UI buttons so we don't need
     * to call SFX handler in each of their functions or add the
     * callback to the button's OnClick.
     * 
     * For the correct behavior of this script, SFXHandler must also
     * be active on the hierarchy.
     */

    protected override void Start()
    {
        //Add this function to the default OnClick callback
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
