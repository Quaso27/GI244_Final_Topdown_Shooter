using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // Singleton: ใช้ชื่อเดิมที่สคริปต์อื่นเรียกหา
    public static SoundManager instance;

    [Header("Audio Sources")]
    [Tooltip("ลาก AudioSource ที่อยู่ใน Object นี้มาใส่")]
    public AudioSource sfxSource;

    [Header("Audio Configuration")]
    public AudioMixer mainMixer;

    [Header("UI Sliders (Optional)")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Awake()
    {
        // ระบบ Singleton แบบสมบูรณ์
        if (instance == null)
        {
            instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

        // โหลดค่าระดับเสียงเดิม
        LoadSettings();

        // เชื่อมต่อ Event ของ Slider
        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume");
            if (musicSlider != null) musicSlider.value = musicVol;
            SetMusicVolume(musicVol);
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume");
            if (sfxSlider != null) sfxSlider.value = sfxVol;
            SetSFXVolume(sfxVol);
        }
    }

    public void SetMusicVolume(float value)
    {
        if (mainMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mainMixer.SetFloat("MusicVol", dB);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        if (mainMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mainMixer.SetFloat("SFXVol", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void PlaySFXFixed(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f) // เพิ่ม float volume เข้าไป
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            // ใช้ PlayOneShot โดยใส่ volume ที่รับมาจากมอนสเตอร์เข้าไป
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}