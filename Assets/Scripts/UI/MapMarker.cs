using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMarker : MonoBehaviour
{
    RectTransform rect;
    public RectTransform icon_pivot;
    public Image icon;
    public Text nameTag;

    public MarkerType markerType;

    // castet target //
    Ship t_Ship;
    StationEntity t_StationEnt;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Assign(Component comp)
    {
        if (comp is Ship)
        {
            markerType = MarkerType.ship;

            t_Ship = (Ship)comp;

            icon_pivot.sizeDelta = Vector2.one * 8f;
            icon.sprite = PrefabManager.marker_ship;
            icon.color = GameManager.current.team[t_Ship.teamID].teamColor;

            nameTag.enabled = t_Ship.pilot.important && MapSystem.zoomLevel == 1;
            nameTag.text = t_Ship.Name;
        }
        if (comp is StationEntity)
        {
            markerType = MarkerType.station;

            t_StationEnt = (StationEntity)comp;

            icon_pivot.sizeDelta = Vector2.one * 10f;
            icon.sprite = PrefabManager.marker_station;
            icon.color = GameManager.current.team[t_StationEnt.teamID].teamColor;

            nameTag.text = t_StationEnt.Name;
        }
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
                        icon.sprite = PrefabManager.marker_ship_far;
                        icon.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else
                    {
                        if (!nameTag.enabled && t_Ship.pilot.important) nameTag.enabled = true;
                        icon_pivot.sizeDelta = Vector2.one * 8f;
                        icon.sprite = PrefabManager.marker_ship;
                        icon.rectTransform.rotation = Quaternion.Euler(0, 0, rotation);
                    }
                    break;
                }
            case MarkerType.station:
                {
                    if (Ship.PLAYER == null) break;
                    float dist = (t_StationEnt.transform.position - Ship.PLAYER.transform.position).magnitude;
                    if(dist < 100) break;

                    if (dist < 1500f)
                    {
                        nameTag.text = $"{t_StationEnt.Name} [ {dist:0}m ]";
                    }
                    else
                    {
                        nameTag.text = $"{t_StationEnt.Name} [ {(dist * 0.001f):0.0}km ]";
                    }
                    break;
                }
        }
    }
}

public enum MarkerType
{
    ship,
    station
}