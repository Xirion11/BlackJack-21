using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FloatBehavior : MonoBehaviour
{
    [SerializeField] private float floatDistance = 1f;
    [SerializeField] private float floatDuration = 1f;

    //This is equals to duration * 2. Used as delay to allow a full animation cycle to complete
    private float doubleDuration = 0f;
    private Transform m_transform;

    private Sequence floatSequence;

    void Start()
    {
        //Caching this object's transform
        m_transform = this.transform;
        doubleDuration = floatDuration + floatDuration;

        //Start float animation
        StartCoroutine(FloatRoutine());
    }

    //Float up and down
    IEnumerator FloatRoutine()
    {
        while (true)
        {
            floatSequence = DOTween.Sequence();
            floatSequence.Append(m_transform.DOMoveY(m_transform.position.y + floatDistance, floatDuration))
                .Append(m_transform.DOMoveY(m_transform.position.y, floatDuration));
            yield return Yielders.WaitForSeconds(floatDuration * doubleDuration);
        }
    }
}
