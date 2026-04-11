using UnityEngine;
public class AudioSystem : MonoBehaviour
{
    private static AudioSystem _instance;

    public AudioClip Music;

    public static AudioSystem Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnDisable()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        if (Music != null)
        {
            PlayMusic(Music, 1.0f);
        }
    }

    public static AudioSource PlayMusic(AudioClip clip, float volume)
    {
        var musicSourcePrefab = Resources.Load<AudioSource>("Music Source");
        var audioSource = Instantiate(musicSourcePrefab);
        audioSource.gameObject.SetActive(true);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        return audioSource;
    }

    public static AudioSource PlaySFX(AudioClip clip, float volume)
    {
        var sfxSourcePrefab = Resources.Load<AudioSource>("SFX Source");
        var audioSource = Instantiate(sfxSourcePrefab);
        audioSource.gameObject.SetActive(true);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(0.75f, 1.25f);
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
        return audioSource;
    }

    public static AudioSource PlaySFXSpatial(AudioClip clip, float volume, Transform parent)
    {
        var sfxSourcePrefab = Resources.Load<AudioSource>("SFX Source (Spatial)");
        var audioSource = Instantiate(sfxSourcePrefab, parent);
        audioSource.gameObject.SetActive(true);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = Random.Range(0.75f, 1.25f);
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
        return audioSource;
    }
}