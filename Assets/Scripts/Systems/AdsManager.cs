using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    public static AdsManager current;

    string gameId = "3747123";
    string justAdID = "video";
    string respawnRewardID = "rewardedVideo";
    public bool testMode = true;

    float lastTimeShowed;
    public float minTimeBetweenAds = 120;

    public bool respawnRewardReady;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
    }

    public void ShowAds()
    {
        if (Time.time > lastTimeShowed + minTimeBetweenAds)
        {
            Debug.Log("ShowAds()");
            Advertisement.Show(justAdID);
            lastTimeShowed = Time.time;
        }
    }

    public void ShowAdForRespawn()
    {
        Advertisement.Show(respawnRewardID);
    }

    public void OnUnityAdsReady(string placementId)
    {
        Debug.Log($"OnUnityAdsReady({placementId})");

        respawnRewardReady = placementId == respawnRewardID;
    }

    public void OnUnityAdsDidError(string message)
    {
        Debug.Log($"OnUnityAdsDidError({message})");
        //--- error ---
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        Debug.Log($"OnUnityAdsDidStart({placementId})");
        //--- ad started ---
        Time.timeScale = 0;
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        Debug.Log($"OnUnityAdsDidFinish({placementId}, {showResult})");

        switch (showResult)
        {
            case ShowResult.Failed:
                {
                    //--- error ---
                    break;
                }
            case ShowResult.Skipped:
                {
                    //--- alarm ad skip ---
                    break;
                }
            case ShowResult.Finished:
                {
                    //--- reward ---
                    if (placementId == respawnRewardID)
                    {

                    }
                    break;
                }
        }
        Time.timeScale = 1.0f;
    }
}
