using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource ambientSound; // Referencia al AudioSource para el sonido ambiente
    public AudioSource sfxSource;    // Referencia al AudioSource para los efectos de sonido
    public AudioClip ambient;
    public AudioClip bellSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener el AudioManager al cambiar de escena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Reproducir sonido ambiente
        if (ambient != null)
        {
            PlayAmbientSound(ambient);
        }
    }

    // Método para reproducir sonido ambiente
    public void PlayAmbientSound(AudioClip clip)
    {
        ambientSound.clip = clip;
        ambientSound.loop = true;
        ambientSound.Play();
    }

    // Método para reproducir un sonido específico
    public void PlaySpecificSound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
