using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI current;

    [HideInInspector] public Canvas canvas;
    CanvasGroup group;

    public AudioMixer globalMixer;

    public Slider slider_music;
    public Slider slider_sfx;
    public Toggle toggle_controls;

    public Text label_music;
    public Text label_sfx;

    public GameSettings settings = new GameSettings();

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        group = GetComponentInChildren<CanvasGroup>();

        slider_music.onValueChanged.AddListener(OnMusicVolumeUpdated);
        slider_sfx.onValueChanged.AddListener(OnSFXVolumeUpdated);
        toggle_controls.onValueChanged.AddListener(OnControlsTypeChange);
    }

    private void Start()
    {
        current = this;

        LoadSettings();

        slider_music.value = settings.volume_music;
        slider_sfx.value = settings.volume_sfx;
        toggle_controls.isOn = settings.useNewControls;
    }

    private void Update()
    {

    }

    void OnMusicVolumeUpdated(float volume)
    {
        settings.volume_music = volume;
        label_music.text = $"{volume:0%}";

        globalMixer.SetFloat("vol_music", Mathf.Log(volume) * 20);

        if (settings.volume_music <= 0.01)
        {
            globalMixer.SetFloat("vol_music", -80);
            label_music.text = "OFF";
        }
    }

    void OnSFXVolumeUpdated(float volume)
    {
        settings.volume_sfx = volume;
        label_sfx.text = $"{volume:0%}";

        globalMixer.SetFloat("vol_sfx", Mathf.Log(volume) * 20);

        if (settings.volume_sfx <= 0.01)
        {
            globalMixer.SetFloat("vol_sfx", -80);
            label_sfx.text = "OFF";
        }
    }

    void OnControlsTypeChange(bool state)
    {
        settings.useNewControls = state;
        GlobalEvents.ControlsChanged(state);
    }

    public void ToggleSettingsWindow(bool toggle)
    {
        if (toggle)
        {
            StartCoroutine(ShowWindow());
        }
        else
        {
            StartCoroutine(HideWindow());
        }
    }

    float progress;
    IEnumerator ShowWindow()
    {
        progress = 0;
        canvas.enabled = true;

        while (progress < 1)
        {
            progress = Mathf.MoveTowards(progress, 1.0f, Time.unscaledDeltaTime * 4);

            group.alpha = progress;
            transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.0f, progress);

            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator HideWindow()
    {
        progress = 1;

        SaveSettings();

        while (progress > 0)
        {
            progress = Mathf.MoveTowards(progress, 0.0f, Time.unscaledDeltaTime * 4);

            group.alpha = progress;
            transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.0f, progress);

            yield return new WaitForEndOfFrame();
        }

        canvas.enabled = false;
    }

    void LoadSettings()
    {
        if (File.Exists(Application.persistentDataPath + "/settings.dat"))
        {
            Debug.Log($"Settings exists at \n\'{Application.persistentDataPath}/settings.dat\'");

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/settings.dat");

            settings = (GameSettings)bf.Deserialize(file);

            file.Close();
        }
    }

    void SaveSettings()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/settings.dat");

        bf.Serialize(file, settings);
        file.Close();
    }
}

[System.Serializable]
public class GameSettings
{
    public float volume_music;
    public float volume_sfx;
    public bool useNewControls;

    public GameSettings()
    {
        volume_music = 1;
        volume_sfx = 1;
        useNewControls = true;
    }
}