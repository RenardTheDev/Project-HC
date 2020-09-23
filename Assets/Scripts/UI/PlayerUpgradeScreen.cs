using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeScreen : MonoBehaviour
{
    public static PlayerUpgradeScreen current;
    [HideInInspector] public Canvas canvas;
    public CanvasGroup group;

    //public Text label_cash;

    public UpgradeItemUI[] upgradeItems;
    public UpgradeWeaponUI[] upgradeWeapons;

    public GameObject[] lists;

    private void Awake()
    {
        current = this;
        canvas = GetComponent<Canvas>();

        for (int i = 0; i < lists.Length; i++)
        {
            lists[i].SetActive(true);
        }
    }

    private void Start()
    {
        for (int i = 1; i < lists.Length; i++)
        {
            lists[i].SetActive(false);
        }
    }

    public bool test_autoUpdate;
    private void Update()
    {
        if (test_autoUpdate)
        {
            UpdateLabels();
        }
    }

    public void ToggleUpgradeScreen(bool toggle)
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

    public void UpdateLabels()
    {
        for (int i = 0; i < upgradeItems.Length; i++)
        {
            upgradeItems[i].UpdateLabels();
        }
        for (int i = 0; i < upgradeWeapons.Length; i++)
        {
            upgradeWeapons[i].UpdateLabels();
        }
    }

    float progress;
    IEnumerator ShowWindow()
    {
        progress = 0;
        canvas.enabled = true;
        PlayerShipUI.current.ToggleControls(false);

        UpdateLabels();

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

        while (progress > 0)
        {
            progress = Mathf.MoveTowards(progress, 0.0f, Time.unscaledDeltaTime * 4);

            group.alpha = progress;
            transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.0f, progress);

            yield return new WaitForEndOfFrame();
        }

        canvas.enabled = false;
        PlayerShipUI.current.ToggleControls(true);
    }
}