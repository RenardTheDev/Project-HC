using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSystem : MonoBehaviour
{
    public static MapSystem inst;

    public static int maxZoomLevel = 5;

    public MapMoveInput mapMove;

    public RectTransform selection;
    public RectTransform markers_parent;
    public Image markers_field;

    public List<MapMarker> shipMarkers;
    public List<MapMarker> stationMarkers;

    public Button button_mapZoomIN;
    public Button button_mapZoomOUT;
    public Button button_warp;

    public Image grid;

    public Vector2 mapPos;

    public static int zoomLevel = 4;
    public float zoom;

    public static int inv_zoomLevel = 2;
    public float inv_zoom;

    [Header("Selection")]
    public Text selectedInfo;
    public Station selectedStation;

    private void Awake()
    {
        inst = this;

        shipMarkers = new List<MapMarker>();
        stationMarkers = new List<MapMarker>();

        GlobalEvents.OnGameStateChanged += OnGameStateChanged;
        GlobalEvents.OnShipSpawned += OnShipSpawned;
        //GlobalEvents.OnStationSpawned += OnStationSpawned;
        GlobalEvents.OnActiveStationChanged += OnActiveStationChanged;
    }
    private void Start()
    {
        button_warp.interactable = false;
    }

    private void OnActiveStationChanged(Station oldStation, Station newStation)
    {
        UpdateMarkers();
    }

    public void SpawnStationMarkers()
    {
        for (int i = 0; i < GameDataManager.stations.Count; i++)
        {
            SpawnStationMarker(GameDataManager.stations[i]);
        }
    }

    void SpawnStationMarker(Station station)
    {
        for (int i = 0; i < stationMarkers.Count; i++)
        {
            var stMark = stationMarkers[i];
            if (stMark.t_Station == null)
            {
                stMark.Assign(station);
                ToggleMarker(station, true);
                return;
            }
        }

        AddStationMarker(station);
    }

    private void OnShipSpawned(Ship ship)
    {
        /*for (int i = 0; i < shipMarkers.Count; i++)
        {
            var shMark = shipMarkers[i];
            if (shMark.t_Ship == ship)
            {
                ToggleMarker(ship,true);
                return;
            }
        }

        AddShipMarker(ship);*/
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.Pause)
        {
            OnMapUpdated();
        }

        switch (newState)
        {
            case GameState.MainMenu:
                {
                    break;
                }
            case GameState.Game:
                {
                    break;
                }
            case GameState.Pause:
                {
                    OnMapUpdated();
                    break;
                }
        }
    }

    private void Update()
    {
        if (GameManager.gameState != GameState.Pause) return;

        markers_parent.sizeDelta = Vector2.one * GameManager.inst.WorldRadius;

        mapPos += mapMove.deltaMove / inv_zoom;
        mapPos = Vector2.ClampMagnitude(mapPos, GameManager.inst.WorldRadius / zoom);

        markers_parent.anchoredPosition = mapPos * inv_zoom;

        if (mapMove.moving && mapMove.deltaMove.magnitude == 0 && selection.gameObject.activeSelf)
        {
            button_warp.interactable = false;
            selection.gameObject.SetActive(false);
            selectedStation = null;
        }
    }

    void UpdateMarkers()
    {
        if (GameManager.gameState != GameState.Pause) return;

        Vector2 itemPoint;
        foreach (var m in shipMarkers)
        {
            if (m.t_Ship.isAlive)
            {
                itemPoint = ((Vector2)m.t_Ship.transform.position - GameManager.inst.WorldCenter) / zoom;
                m.UpdatePosition(itemPoint, m.t_Ship.transform.eulerAngles.z);
            }
            else
            {
                if (m.gameObject.activeSelf)
                {
                    ToggleMarker(m.t_Ship, false);
                }
            }

            m.UpdateGraphics();
        }

        foreach (var m in stationMarkers)
        {
            if (m.t_Station != null)
            {
                itemPoint = (m.t_Station.data.positionV2 - GameManager.inst.WorldCenter) / zoom;
                m.UpdatePosition(itemPoint, 0);
            }
            else
            {
                ToggleMarker(m.t_Station, false);
            }

            m.UpdateGraphics();
        }
    }

    void AddShipMarker(Ship target)
    {
        var go = Instantiate(PrefabManager.inst.ui_mapMarker, markers_parent);
        MapMarker mark = go.GetComponent<MapMarker>();

        mark.Assign(target);
        shipMarkers.Add(mark);

        OnMapUpdated();
    }

    void AddStationMarker(Station target)
    {
        var go = Instantiate(PrefabManager.inst.ui_mapMarker, markers_parent);
        MapMarker mark = go.GetComponent<MapMarker>();

        mark.Assign(target);
        stationMarkers.Add(mark);

        OnMapUpdated();
    }

    void ToggleMarker(Ship target, bool enable)
    {
        var mark = shipMarkers.Find(x => x.t_Ship == target);
        mark.Toggle(enable);
    }

    void ToggleMarker(Station target, bool enable)
    {
        var mark = stationMarkers.Find(x => x.t_Station == target);
        mark.Toggle(enable);
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
        Vector2 pos = GameDataManager.stations[GameDataManager.data.currentActiveStation].data.positionV2;
        mapPos = ((GameManager.inst.WorldCenter - pos) / zoom) / inv_zoom;
        UpdateMarkers();
    }

    public void OnMapUpdated()
    {
        inv_zoomLevel = maxZoomLevel - zoomLevel + 1;
        inv_zoom = Mathf.Pow(2, inv_zoomLevel);

        grid.rectTransform.localScale = Vector3.one * inv_zoom;

        UpdateMarkers();
    }

    public void test_WarpToStation()
    {
        //GameManager.inst.TransferToNewLocation(selectedStation);
        GameManager.inst.TogglePause(false);

        GameManager.inst.StartJumpProcess(selectedStation);
        button_warp.interactable = false;
        selectedStation = null;
        selection.gameObject.SetActive(false);
    }

    public void OnStationSelected(Station target)
    {
        MapMarker stMarker = null;
        for (int i = 0; i < stationMarkers.Count; i++)
        {
            if (stationMarkers[i].t_Station == target)
            {
                stMarker = stationMarkers[i];
                break;
            }
        }
        if (stMarker == null) return;
        button_warp.interactable = target.data.ID != GameDataManager.data.currentActiveStation;

        selectedStation = target;
        selection.gameObject.SetActive(true);
        selection.anchoredPosition = stMarker.GetPosition();

        string ownerName = GameDataManager.pilots[selectedStation.data.ownerID].data.Name;
        string fleet = $"{selectedStation.data.fleet.Count}/20";

        if (selectedStation.data.fleetQueue > 0)
        {
            fleet += $"<color=green>(+{selectedStation.data.fleetQueue})</color>";
        }

        selectedInfo.text = $"< {ownerName} >\n {fleet}";
    }
}