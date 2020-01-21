using UnityEngine;

public class ShuffleAnimation : MonoBehaviour
{
    /**
     * This script is only used to catch an animation event
     * and call the appropiate function
     */ 

    public void ANIM_ShuffleComplete()
    {
        GameHandler.Instance.OnShuffleComplete();
    }
}
