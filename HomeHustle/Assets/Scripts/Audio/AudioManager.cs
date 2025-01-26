using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource ambientSound;
    public AudioSource sfxSource;

    [Header("Ambient")]
    public AudioClip ambient;

    [Header("Sounds")]
    public AudioClip bellSound;
    public AudioClip openDoor;
    public AudioClip lockDoor;
    public AudioClip lockedDoor;
    public AudioClip evilLaugh;

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
}
