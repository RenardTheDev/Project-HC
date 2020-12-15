using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipTagManager : MonoBehaviour
{
    public static ShipTagManager current;

    public GameObject prefab_ship;
    public GameObject prefab_station;

    public Dictionary<ScreenTagType, List<ScreenTag>> freeTags = new Dictionary<ScreenTagType, List<ScreenTag>>();
    public Dictionary<ScreenTagType, List<ScreenTag>> occuTags = new Dictionary<ScreenTagType, List<ScreenTag>>();

    private void Awake()
    {
        current = this;

        freeTags.Add(ScreenTagType.ship, new List<ScreenTag>());
        freeTags.Add(ScreenTagType.station, new List<ScreenTag>());

        occuTags.Add(ScreenTagType.ship, new List<ScreenTag>());
        occuTags.Add(ScreenTagType.station, new List<ScreenTag>());
    }

    private void Start()
    {
        GlobalEvents.OnShipSpawned += OnShipSpawned;
        GlobalEvents.OnShipKilled += OnShipKilled;
        GlobalEvents.OnActiveStationChanged += OnActiveStationChanged;

        GlobalEvents.OnStationSpawned += OnStationSpawned;
        GlobalEvents.OnStationDespawned += OnStationDespawned;
    }

    private void OnStationSpawned(StationEntity stationEnt)
    {
        STStation tag = getFreeTag(ScreenTagType.station) as STStation;
        tag.gameObject.SetActive(true);
        tag.Assign(stationEnt.transform, stationEnt.station);
    }

    private void OnStationDespawned(StationEntity stationEnt)
    {
        var tag = occuTags[ScreenTagType.station].Find(x => ((STStation)x).target == stationEnt.station);
        if (tag != null)
        {
            DisableTag(tag, ScreenTagType.station);
        }
    }

    private void OnActiveStationChanged(Station oldStation, Station newStation)
    {
        var tag = occuTags[ScreenTagType.station].Find(x => ((STStation)x).target == oldStation);
        if (tag != null)
        {
            DisableTag(tag, ScreenTagType.station);
        }

        /*for (int i = 0; i < occuTags[ScreenTagType.station].Count; i++)
        {
            STStation tag = occuTags[ScreenTagType.station][i] as STStation;

            if (tag.target.data.ID == oldStation.data.ID)
            {
                tag.Disable();

                occuTags[ScreenTagType.station].Remove(tag);
                freeTags[ScreenTagType.station].Add(tag);
                break;
            }
        }*/
    }

    private void OnShipSpawned(Ship ship)
    {
        if (ship.isPlayer) return;

        STShip tag = getFreeTag(ScreenTagType.ship) as STShip;
        tag.gameObject.SetActive(true);
        tag.Assign(ship.transform, ship);
    }

    private void OnShipKilled(Damage dmg)
    {
        var tag = occuTags[ScreenTagType.ship].Find(x => ((STShip)x).target == dmg.victim);
        if (tag != null)
        {
            DisableTag(tag, ScreenTagType.ship);
        }

        /*
        for (int i = 0; i < occuTags[ScreenTagType.ship].Count; i++)
        {
            var tag = occuTags[ScreenTagType.ship][i];
            if ((tag as STShip).target == dmg.victim)
            {
                DisableTag(tag, ScreenTagType.ship);
                break;
            }
        }*/
    }

    ScreenTag getFreeTag(ScreenTagType type)
    {
        if (freeTags[type].Count > 0)
        {
            var tag = freeTags[type][0];
            freeTags[type].Remove(tag);
            occuTags[type].Add(tag);

            return tag;
        }
        else
        {
            var tag = createNewTag(type);
            return tag;
        }
    }

    void DisableTag(ScreenTag tag, ScreenTagType type)
    {
        tag.Disable();
        occuTags[type].Remove(tag);
        freeTags[type].Add(tag);
    }

    GameObject spawnTag;
    ScreenTag createNewTag(ScreenTagType type)
    {
        spawnTag = null;

        switch (type)
        {
            case ScreenTagType.ship:
                {
                    spawnTag = Instantiate(prefab_ship, transform);
                    break;
                }
            case ScreenTagType.station:
                {
                    spawnTag = Instantiate(prefab_station, transform);
                    break;
                }
        }

        ScreenTag tag = spawnTag.GetComponent<ScreenTag>();
        occuTags[type].Add(tag);

        return tag;
    }

    private void Update()
    {
        
    }
}

public enum ScreenTagType
{
    ship,
    station
}