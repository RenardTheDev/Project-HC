using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager current;

    public MusicTheme[] theme_normal;

    public MusicTheme currTheme;

    public AudioSource src_ambient;
    public AudioSource[] src_action;
    public AudioSource src_endpoint;

    public float musicVolume = 0.25f;

    bool startinActionLoop;
    double actionLoopChangeTime;

    bool isAction;

    Coroutine ambient;
    //Coroutine action;
    //Coroutine themeChange;

    [Header("Blend timings")]
    public float ambientStartUpTime = 5f;
    public float actionStartUpTime = 5f;
    int flip;
    private void Awake()
    {
        current = this;

        GlobalEvents.OnGameOver += OnGameOver;
        GlobalEvents.OnShipGetHit += OnShipGetHit;
        GlobalEvents.OnShipKilled += OnShipKilled;
        GlobalEvents.OnGameStateChanged += OnGameStateChanged;

        currTheme = theme_normal[0];
    }
    private void Start()
    {
        StartCoroutine(ChangeTheme(false, theme_normal[Random.Range(0, theme_normal.Length)]));
    }

    private void OnGameStateChanged(GameState oldstate, GameState newstate)
    {
        switch (newstate)
        {
            case GameState.MainMenu:
                StartCoroutine(StopAction(true, true));
                break;
            case GameState.Game:
                if (oldstate == GameState.MainMenu) StartCoroutine(ChangeTheme(true, theme_normal[Random.Range(0, theme_normal.Length)]));
                break;
            case GameState.Pause:
                break;
            default:
                break;
        }
    }

    private void OnShipKilled(Damage data)
    {

    }

    private void OnShipGetHit(Damage data)
    {

    }

    private void OnGameOver()
    {

    }

    public void StopAction()
    {
        StartCoroutine(StopAction(true, true));
    }

    private void Update()
    {
        if (startinActionLoop)
        {
            double time = AudioSettings.dspTime;
            if (time + 1.0f > actionLoopChangeTime)
            {
                flip = 1 - flip;

                currTheme.currActionLoop = Random.Range(0, currTheme.action_loop.Length);

                src_action[flip].clip = currTheme.action_loop[currTheme.currActionLoop].loopBody;
                src_action[flip].PlayScheduled(actionLoopChangeTime);

                actionLoopChangeTime += src_action[flip].clip.length;
            }
        }
    }

    IEnumerator StartAmbient()
    {
        yield return new WaitForEndOfFrame();

        src_ambient.clip = currTheme.ambient_loop[Random.Range(0, currTheme.ambient_loop.Length)];
        src_ambient.Play();

        while (src_ambient.volume < musicVolume)
        {
            src_ambient.volume = Mathf.MoveTowards(src_ambient.volume, musicVolume, (Time.deltaTime * musicVolume) / ambientStartUpTime);
            yield return new WaitForEndOfFrame();
        }

        src_ambient.volume = musicVolume;
    }

    IEnumerator StopAmbient()
    {
        while (src_ambient.volume > 0)
        {
            src_ambient.volume = Mathf.MoveTowards(src_ambient.volume, 0, Time.deltaTime * musicVolume);
            yield return new WaitForEndOfFrame();
        }

        src_ambient.volume = 0;
        src_ambient.Stop();
    }

    IEnumerator StartAction(bool useStarter)
    {
        if (ambient != null) StopCoroutine(ambient);
        StartCoroutine(StopAmbient());

        if (useStarter)
        {
            currTheme.currActionLoop = Random.Range(0, currTheme.action_loop.Length);
            src_endpoint.clip = currTheme.action_loop[currTheme.currActionLoop].loopStart;
            src_endpoint.Play();

            actionLoopChangeTime = AudioSettings.dspTime + src_endpoint.clip.length;

            //src_action[flip].Play();

            startinActionLoop = true;

            src_action[0].volume = musicVolume;
            src_action[1].volume = musicVolume;

            src_endpoint.volume = 0;
            while (src_endpoint.volume < musicVolume)
            {
                src_endpoint.volume = Mathf.MoveTowards(src_endpoint.volume, musicVolume, (Time.deltaTime * musicVolume) / actionStartUpTime);
                yield return new WaitForEndOfFrame();
            }
            src_endpoint.volume = musicVolume;
        }
        else
        {
            startinActionLoop = true;

            src_action[0].volume = musicVolume;
            src_action[1].volume = musicVolume;

            src_action[flip].volume = 0;

            currTheme.currActionLoop = Random.Range(0, currTheme.action_loop.Length);
            src_action[flip].clip = currTheme.action_loop[currTheme.currActionLoop].loopBody;
            src_action[flip].Play();

            actionLoopChangeTime = AudioSettings.dspTime + src_action[flip].clip.length;
            startinActionLoop = true;

            while (src_action[flip].volume < musicVolume)
            {
                src_action[flip].volume = Mathf.MoveTowards(src_action[flip].volume, musicVolume, (Time.deltaTime * musicVolume) / 1f);
                yield return new WaitForEndOfFrame();
            }

            src_action[flip].volume = musicVolume;
        }
    }

    IEnumerator StopAction(bool startAmbient, bool playEndpoint)
    {
        while (src_action[flip].volume > 0)
        {
            src_action[flip].volume = Mathf.MoveTowards(src_action[flip].volume, 0, Time.deltaTime * musicVolume);

            if (src_action[flip].volume / musicVolume < 0.5f && !src_endpoint.isPlaying && playEndpoint)
            {
                src_endpoint.clip = currTheme.action_loop[currTheme.currActionLoop].loopEnd;
                src_endpoint.Play();
                startinActionLoop = false;
            }

            yield return new WaitForEndOfFrame();
        }

        src_action[flip].volume = 0;
        src_action[flip].Stop();

        if (startAmbient)
        {
            yield return new WaitForSeconds(1f);

            if (ambient != null) StopCoroutine(ambient);
            ambient = StartCoroutine(StartAmbient());
        }
    }

    IEnumerator ChangeTheme(bool startAction, MusicTheme newTheme)
    {
        currTheme = newTheme;
        if (startAction)
        {
            yield return StartCoroutine(StartAction(true));
        }
        else
        {
            yield return ambient = StartCoroutine(StartAmbient());
        }
    }
}
