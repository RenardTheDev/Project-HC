using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager inst;

    [Header("World")]
    public GameObject[] station;
    public GameObject ship;

    [Header("UI")]
    public GameObject ui_mapMarker;

    [Header("UI Sprites")]
    public Sprite marker_ship;
    public Sprite marker_ship_far;
    public Sprite marker_station;

    public Sprite tag_ship;
    public Sprite tag_station;

    private void Awake()
    {
        inst = this;
    }

    public static GameObject InstatiatePrefab(GameObject go)
    {
        return Instantiate(go);
    }
}
