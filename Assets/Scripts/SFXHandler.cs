using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private AudioClip SFX_UI_Positive = null;
    [SerializeField] private AudioClip SFX_UI_Negative = null;
    [SerializeField] private AudioClip SFX_PlayChips = null;
    [SerializeField] private AudioClip SFX_RewardChips = null;
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
            //If there are pendind sfx
            if (AudioClipQueue.Count > 0)
            {
                //Play next sfx in queue
                audioSource.PlayOneShot(AudioClipQueue.Dequeue());
            }
        }

        //If the player muted the game while there still were pendind sfx
        else if (AudioClipQueue.Count > 0)
        {
            //Clear queue
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

    public void PlayNegativeUISfx()
    {
        if (IsAudioEnabled())
        {
            AudioClipQueue.Enqueue(SFX_UI_Negative);
        }
    }

    public void PlayPlaceChipsSfx()
    {
        if (IsAudioEnabled())
        {
            AudioClipQueue.Enqueue(SFX_PlayChips);
        }
    }

    public void PlayRewardChipsSfx()
    {
        if (IsAudioEnabled())
        {
            AudioClipQueue.Enqueue(SFX_RewardChips);
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