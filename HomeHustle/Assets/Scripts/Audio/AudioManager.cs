using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource ambientSound;
    public AudioSource sfxSource;

    [Header("UI")]
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    public Slider sfxSlider;

    [Header("Ambient")]
    public AudioClip ambient;

    [Header("Sounds")]
    public AudioClip bellSound;
    public AudioClip openDoor;
    public AudioClip lockDoor;
    public AudioClip lockedDoor;
    public AudioClip evilLaugh;
    public AudioClip sadTrombone;
    public AudioClip yeah;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (ambient != null)
        {
            PlayAmbientSound(ambient);
        }
    }

    void Update()
    {
        AdjustVolume();
    }

    public void PlayAmbientSound(AudioClip clip)
    {
        ambientSound.clip = clip;
        ambientSound.loop = true;
        ambientSound.Play();
    }

    public void PlaySpecificSound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    void AdjustVolume()
    {
        ambientSound.volume = musicSlider.normalizedValue;
        sfxSource.volume = sfxSlider.normalizedValue;
    }
}
