using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Preloader : MonoBehaviour
{
    public CanvasGroup loadingGroup;
    public Image loading_fill;
    public Image ring;

    AsyncOperation loading;

    private void Start()
    {
        StartCoroutine(Preloading());
    }

    private void Update()
    {
        ring.rectTransform.Rotate(Vector3.forward, 360 * Time.deltaTime);
        if (loading != null) loading_fill.fillAmount = Mathf.Clamp01(loading.progress / 0.9f);
    }

    IEnumerator Preloading()
    {
        yield return new WaitForEndOfFrame();

        while (loadingGroup.alpha < 1)
        {
            loadingGroup.alpha = Mathf.MoveTowards(loadingGroup.alpha, 1, Time.deltaTime * 2f);
            yield return new WaitForEndOfFrame();
        }
        loadingGroup.alpha = 1;

        while (ring.fillAmount < 0.5f)
        {
            ring.fillAmount = Mathf.MoveTowards(ring.fillAmount, 0.5f, Time.deltaTime * 2f);
            yield return new WaitForEndOfFrame();
        }
        ring.fillAmount = 0.5f;

        yield return new WaitForSeconds(1f);

        loading = SceneManager.LoadSceneAsync(1);
        loading.allowSceneActivation = false;

        while (!loading.isDone)
        {
            yield return new WaitForEndOfFrame();

            if (loading.progress >= 0.9f)
            {
                while (loadingGroup.alpha > 0)
                {
                    loadingGroup.alpha = Mathf.MoveTowards(loadingGroup.alpha, 0, Time.deltaTime * 2f);
                    yield return new WaitForEndOfFrame();
                }
                loadingGroup.alpha = 0;

                loading.allowSceneActivation = true;
            }
        }
    }
}
