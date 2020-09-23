using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Monetization;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade curr;
    public Image fader;

    public Color fadeColor;

    float fade = 1;

    private void Awake()
    {
        curr = this;
        SetColor();

        StartCoroutine(FadeOUTCor(1f, 1f));
    }

    public void FadeIN(float delay, float time)
    {
        StartCoroutine(FadeINCor(delay, time));
    }

    public void FadeOUT(float delay, float time)
    {
        StartCoroutine(FadeOUTCor(delay, time));
    }

    public void FadeINOUT(float delayIN, float timeIN, float delayOUT, float timeOUT)
    {
        StartCoroutine(FadeINOUTCor(delayIN, timeIN, delayOUT, timeOUT));
    }

    IEnumerator FadeINCor(float delay, float time)
    {
        if (delay > 0) yield return new WaitForSecondsRealtime(delay);

        fader.fillOrigin = 1;

        if (time > 0)
        {
            fade = 0;
            SetColor();
            while (fade < 1f)
            {
                fade = Mathf.MoveTowards(fade, 1f, Time.deltaTime / time);
                SetColor();
                yield return new WaitForEndOfFrame();
            }
            fade = 1;
            SetColor();
        }
        else
        {
            fade = 1;
            SetColor();
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeOUTCor(float delay, float time)
    {
        if (delay > 0) yield return new WaitForSecondsRealtime(delay);

        fader.fillOrigin = 0;

        if (time > 0)
        {
            fade = 1;
            SetColor();
            while (fade > 0f)
            {
                fade = Mathf.MoveTowards(fade, 0f, Time.deltaTime / time);
                SetColor();
                yield return new WaitForEndOfFrame();
            }
            fade = 0;
            SetColor();
        }
        else
        {
            fade = 0;
            SetColor();
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeINOUTCor(float delayIN, float timeIN, float delayOUT, float timeOUT)
    {
        yield return StartCoroutine(FadeINCor(delayIN, timeIN));
        yield return StartCoroutine(FadeOUTCor(delayOUT, timeOUT));
    }

    void SetColor()
    {
        fadeColor.a = fade;
        fader.color = fadeColor;
    }
}