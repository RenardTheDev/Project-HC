using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    // static //

    public static GameObject[] station;
    public static GameObject ship;

    public static GameObject ui_mapMarker;

    public static Sprite marker_ship;
    public static Sprite marker_ship_far;
    public static Sprite marker_station;

    // assignables //

    [Header("World")]
    public GameObject[] _station;
    public GameObject _ship;

    [Header("UI")]
    public GameObject _ui_mapMarker;

    [Header("UI Sprites")]
    public Sprite _marker_ship;
    public Sprite _marker_ship_far;
    public Sprite _marker_station;


    private void OnEnable()
    {
        station = _station;
        ship = _ship;

        ui_mapMarker = _ui_mapMarker;

        marker_ship = _marker_ship;
        marker_ship_far = _marker_ship_far;
        marker_station = _marker_station;
    }

    public static GameObject InstatiatePrefab(GameObject go)
    {
        return Instantiate(go);
    }
}
