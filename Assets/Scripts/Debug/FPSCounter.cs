using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public RectTransform box;
    public Text label;
    int frames;

    private void Awake()
    {
        StartCoroutine(Refresh());
    }

    private void Update()
    {
        frames++;
    }

    IEnumerator Refresh()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            label.text = $"{frames} fps";
            box.sizeDelta = new Vector2(label.preferredWidth + 8, label.preferredHeight + 8);

            frames = 0;
        }
    }
}