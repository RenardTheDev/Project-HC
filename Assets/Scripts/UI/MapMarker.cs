using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapMarker : MonoBehaviour, IPointerDownHandler
{
    RectTransform rect;
    public RectTransform icon_pivot;
    public Image icon;
    public Image nameTagBG;
    public Text nameTag;

    public MarkerType markerType;

    // castet target //
    public Ship t_Ship;
    public Station t_Station;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Assign(Ship shipTarget)
    {
        markerType = MarkerType.ship;
        t_Ship = shipTarget;
        UpdateGraphics();
    }

    public void Assign(Station stationTarget)
    {
        markerType = MarkerType.station;
        t_Station = stationTarget;
        UpdateGraphics();
    }

    public void UpdateGraphics()
    {
        switch (markerType)
        {
            case MarkerType.ship:
                {
                    icon_pivot.sizeDelta = Vector2.one * 8f;
                    icon.sprite = PrefabManager.inst.marker_ship;
                    icon.color = GameManager.inst.team[t_Ship.teamID].teamColor;

                    nameTag.enabled = t_Ship.pilot.data.important && MapSystem.zoomLevel == 1;
                    nameTag.text = t_Ship.Name;
                    break;
                }
            case MarkerType.station:
                {
                    icon_pivot.sizeDelta = Vector2.one * 10f;
                    icon.sprite = PrefabManager.inst.marker_station;
                    icon.color = GameManager.inst.team[t_Station.data.teamID].teamColor;

                    nameTag.enabled = true;
                    nameTag.text = t_Station.data.Name;

                    bool isActive = t_Station.data.ID == GameDataManager.data.currentActiveStation;

                    nameTag.color = isActive ? Color.black : Color.white;
                    nameTagBG.enabled = isActive;
                    nameTagBG.rectTransform.sizeDelta = new Vector2(nameTag.preferredWidth + 8, nameTag.preferredHeight + 6);

                    break;
                }
        }
    }

    public void Toggle(bool enable)
    {
        if (enable)
        {
            switch (markerType)
            {
                case MarkerType.ship:
                    UpdateGraphics();
                    break;
                case MarkerType.station:
                    UpdateGraphics();
                    break;
            }
        }
        gameObject.SetActive(enable);
    }

    public Vector2 GetPosition()
    {
        return rect.anchoredPosition;
    }

    public void UpdatePosition(Vector2 position, float rotation)
    {
        rect.anchoredPosition = position;

        switch (markerType)
        {
            case MarkerType.ship:
                {
                    if (MapSystem.zoomLevel > 2 && !t_Ship.isPlayer)
                    {
                        if (nameTag.enabled) nameTag.enabled = false;
                        icon_pivot.sizeDelta = Vector2.one * 2f;
                        icon.sprite = PrefabManager.inst.marker_ship_far;
                        icon.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else
                    {
                        if (!nameTag.enabled && t_Ship.pilot.data.important) nameTag.enabled = true;
                        icon_pivot.sizeDelta = Vector2.one * 8f;
                        icon.sprite = PrefabManager.inst.marker_ship;
                        icon.rectTransform.rotation = Quaternion.Euler(0, 0, rotation);
                    }
                    break;
                }
            case MarkerType.station:
                {
                    break;
                }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MapSystem.inst.OnStationSelected(t_Station);
    }
}

public enum MarkerType
{
    ship,
    station
}