using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Monetization;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade inst;
    public Image fader;

    public Color fadeColor;

    //float fade = 1;

    Coroutine fade;

    private void Awake()
    {
        inst = this;
        //fade = 1;
        SetColor(1);
    }

    private void Start()
    {
        FadeINOUT(0f, 0f, 1f, 1f);
    }

    public void FadeIN(float delay, float time)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeINCor(delay, time));
    }

    public void FadeOUT(float delay, float time)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeOUTCor(delay, time));
    }

    public void FadeINOUT(float delayIN, float timeIN, float delayOUT, float timeOUT)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeINOUTCor(delayIN, timeIN, delayOUT, timeOUT));
    }

    public void CancelFader()
    {
        if (fade != null) StopCoroutine(fade);
        SetColor(0);
    }

    IEnumerator FadeINCor(float delay, float time)
    {
        if (delay > 0) yield return new WaitForSecondsRealtime(delay);

        fader.fillOrigin = 1;

        if (time > 0)
        {
            float fade = 0;
            SetColor(fade);
            while (fade < 1f)
            {
                fade = Mathf.MoveTowards(fade, 1f, Time.deltaTime / time);
                SetColor(fade);
                yield return new WaitForEndOfFrame();
            }
            fade = 1;
            SetColor(fade);
        }
        else
        {
            SetColor(1);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeOUTCor(float delay, float time)
    {
        if (delay > 0) yield return new WaitForSecondsRealtime(delay);

        fader.fillOrigin = 0;

        if (time > 0)
        {
            float fade = 1;
            SetColor(fade);
            while (fade > 0f)
            {
                fade = Mathf.MoveTowards(fade, 0f, Time.deltaTime / time);
                SetColor(fade);
                yield return new WaitForEndOfFrame();
            }
            fade = 0;
            SetColor(fade);
        }
        else
        {
            SetColor(0);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeINOUTCor(float delayIN, float timeIN, float delayOUT, float timeOUT)
    {
        fade = StartCoroutine(FadeINCor(delayIN, timeIN));
        yield return fade;
        fade = StartCoroutine(FadeOUTCor(delayOUT, timeOUT));
        yield return fade;
    }

    void SetColor(float fade)
    {
        fadeColor.a = fade;
        fader.color = fadeColor;
    }
}