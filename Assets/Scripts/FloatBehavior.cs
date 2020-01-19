using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FloatBehavior : MonoBehaviour
{
    [SerializeField] private float floatDistance = 1f;
    [SerializeField] private float floatDuration = 1f;

    float doubleDuration = 0f;
    Transform m_transform;

    Sequence floatSequence;

    void Start()
    {
        m_transform = this.transform;
        doubleDuration = floatDuration + floatDuration;
        StartCoroutine(FloatRoutine());
    }

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
