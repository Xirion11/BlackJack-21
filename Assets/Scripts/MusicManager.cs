using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource m_audioSource = null;

    // Static singleton property
    public static MusicManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (PlayerPrefsManager.getIsAudioEnabled())
        {
            ActivateMusic();
        }
        else
        {
            DeactivateMusic();
        }
    }

    public void ActivateMusic()
    {
        m_audioSource.Play();
    }

    public void DeactivateMusic()
    {
        m_audioSource.Stop();
    }
}
