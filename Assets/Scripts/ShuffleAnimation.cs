using UnityEngine;

public class ShuffleAnimation : MonoBehaviour
{
    public void ANIM_ShuffleComplete()
    {
        GameHandler.Instance.OnShuffleComplete();
    }
}
