using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSystem : MonoBehaviour
{
    public static int maxZoomLevel = 5;

    public MapMoveInput mapMove;

    public RectTransform markers_parent;
    public Image markers_field;

    public Dictionary<Ship, MapMarker> shipMarkers;
    public Dictionary<StationEntity, MapMarker> stationMarkers;

    public Button button_mapZoomIN;
    public Button button_mapZoomOUT;

    public Image grid;

    public Vector2 mapPos;

    public static int zoomLevel = 4;
    public float zoom;

    public static int inv_zoomLevel = 2;
    public float inv_zoom;

    private void Awake()
    {
        shipMarkers = new Dictionary<Ship, MapMarker>();
        stationMarkers = new Dictionary<StationEntity, MapMarker>();

        GlobalEvents.OnGameStateChanged += OnGameStateChanged;
        GlobalEvents.OnShipSpawned += OnShipSpawned;
        GlobalEvents.OnStationSpawned += OnStationSpawned;
    }

    private void Start()
    {
        StartCoroutine(MapUpdate());
    }

    private void OnStationSpawned(StationEntity stationEnt)
    {
        if (stationMarkers.ContainsKey(stationEnt))
        {
            ShowMarker(stationEnt);
        }
        else
        {
            AddStationMarker(stationEnt);
        }
    }

    private void OnShipSpawned(Ship ship)
    {
        if (shipMarkers.ContainsKey(ship))
        {
            ShowMarker(ship);
        }
        else
        {
            AddShipMarker(ship);
        }
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"{this}>OnGameStateChanged({oldState}, {newState})");

        if (newState == GameState.Pause)
        {
            OnMapUpdated();
        }
    }

    private void Update()
    {
        if (GameManager.gameState != GameState.Pause) return;

        markers_parent.sizeDelta = Vector2.one * GameManager.current.WorldRadius;

        mapPos += mapMove.deltaMove / inv_zoom;
        mapPos = Vector2.ClampMagnitude(mapPos, GameManager.current.WorldRadius / zoom);

        markers_parent.anchoredPosition = mapPos * inv_zoom;
    }

    IEnumerator MapUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            OnMapUpdated();
        }
    }

    void UpdateMarkers()
    {
        if (GameManager.gameState != GameState.Pause) return;

        Vector2 itemPoint;
        foreach (var m in shipMarkers)
        {
            if (m.Key.isAlive)
            {
                itemPoint = ((Vector2)m.Key.transform.position - GameManager.current.WorldCenter) / zoom;
                m.Value.UpdatePosition(itemPoint, m.Key.transform.eulerAngles.z);
            }
            else
            {
                if (m.Value.gameObject.activeSelf)
                {
                    HideMarker(m.Key);
                }
            }
        }

        foreach (var m in stationMarkers)
        {
            itemPoint = ((Vector2)m.Key.transform.position - GameManager.current.WorldCenter) / zoom;
            m.Value.UpdatePosition(itemPoint, 0);
        }
    }

    void AddShipMarker(Ship target)
    {
        if (!shipMarkers.ContainsKey(target))
        {
            var go = Instantiate(PrefabManager.ui_mapMarker, markers_parent);
            MapMarker mark = go.GetComponent<MapMarker>();

            mark.Assign(target);
            shipMarkers.Add(target, mark);
        }

        OnMapUpdated();
    }

    void AddStationMarker(StationEntity target)
    {
        if (!stationMarkers.ContainsKey(target))
        {
            var go = Instantiate(PrefabManager.ui_mapMarker, markers_parent);
            MapMarker mark = go.GetComponent<MapMarker>();

            mark.Assign(target);
            stationMarkers.Add(target, mark);
        }

        OnMapUpdated();
    }

    void ShowMarker(Ship target)
    {
        if (shipMarkers.ContainsKey(target)) shipMarkers[target].gameObject.SetActive(false);
    }

    void HideMarker(Ship target)
    {
        if (shipMarkers.ContainsKey(target)) shipMarkers[target].gameObject.SetActive(false);
    }

    void ShowMarker(StationEntity target)
    {
        if (stationMarkers.ContainsKey(target)) stationMarkers[target].gameObject.SetActive(false);
    }

    void HideMarker(StationEntity target)
    {
        if (stationMarkers.ContainsKey(target)) stationMarkers[target].gameObject.SetActive(false);
    }

    public void Map_ZoomOUT()
    {
        if (zoomLevel < maxZoomLevel)
        {
            zoomLevel++;
            zoom = Mathf.Pow(2, zoomLevel);

            if (zoomLevel >= maxZoomLevel)
            {
                zoomLevel = maxZoomLevel;
                button_mapZoomOUT.interactable = false;
            }

            if (zoomLevel > 1 && !button_mapZoomIN.interactable)
            {
                button_mapZoomIN.interactable = true;
            }

            OnMapUpdated();
        }
    }

    public void Map_ZoomIN()
    {
        if (zoomLevel > 1)
        {
            zoomLevel--;
            zoom = Mathf.Pow(2, zoomLevel);

            if (zoomLevel <= 1)
            {
                zoomLevel = 1; 
                button_mapZoomIN.interactable = false;
            }

            if (zoomLevel < maxZoomLevel && !button_mapZoomOUT.interactable)
            {
                button_mapZoomOUT.interactable = true;
            }
            OnMapUpdated();
        }
    }

    public void Map_CenterOnPlayer()
    {
        mapPos = ((GameManager.current.WorldCenter - (Vector2)Ship.PLAYER.transform.position) / zoom) / inv_zoom;
        UpdateMarkers();
    }

    void OnMapUpdated()
    {
        inv_zoomLevel = maxZoomLevel - zoomLevel + 1;
        inv_zoom = Mathf.Pow(2, inv_zoomLevel);

        grid.rectTransform.localScale = Vector3.one * inv_zoom;

        UpdateMarkers();
    }
}