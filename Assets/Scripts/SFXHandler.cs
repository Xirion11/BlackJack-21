using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private AudioClip SFX_UI_Positive = null;
    [SerializeField] private AudioClip[] cardSounds = null;
    private Queue<AudioClip> AudioClipQueue;

    public static SFXHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AudioClipQueue = new Queue<AudioClip>();
    }

    private void Update()
    {
        if (IsAudioEnabled())
        {
            if (AudioClipQueue.Count > 0)
            {
                audioSource.PlayOneShot(AudioClipQueue.Dequeue());
            }
        }
        else if (AudioClipQueue.Count > 0)
        {
            AudioClipQueue.Clear();
        }
    }

    private bool IsAudioEnabled()
    {
        return PlayerPrefsManager.getIsAudioEnabled();
    }

    public void PlayUISfx()
    {
        if (IsAudioEnabled())
        {
            AudioClipQueue.Enqueue(SFX_UI_Positive);
        }
    }

    public void PlayCardSound()
    {
        if (IsAudioEnabled())
        {
            int randomIndex = Random.Range(0, cardSounds.Length);
            AudioClipQueue.Enqueue(cardSounds[randomIndex]);
        }
    }
}