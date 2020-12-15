using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station 
{
    public StationData data;
    public StationEntity ent;

    public Station(StationData stationData)
    {
        data = stationData;
        GlobalEvents.OnActiveStationChanged += OnActiveStationChanged;
        GlobalEvents.OnShipKilled += OnShipKilled;
    }

    public void UnlinkEvents()
    {
        GlobalEvents.OnActiveStationChanged -= OnActiveStationChanged;
        GlobalEvents.OnShipKilled -= OnShipKilled;
    }

    private void OnShipKilled(Damage dmg)
    {
        var victim = dmg.victim.pilot;
        if (!victim.data.important && victim.data.baseStationID == data.ID)
        {
            RemoveFleetPilot(dmg.victim.pilot);
        }
    }

    public void AddFleetPilot(Pilot pilot)
    {
        if (!data.fleet.Contains(pilot.data.ID))
        {
            data.fleet.Add(pilot.data.ID);
        }
    }

    public void RemoveFleetPilot(Pilot pilot)
    {
        if (data.fleet.Contains(pilot.data.ID))
        {
            data.fleet.Remove(pilot.data.ID);
        }
    }

    private void OnActiveStationChanged(Station oldStation, Station newStation)
    {
        if (newStation == this)
        {
            var stGO = UnityEngine.Object.Instantiate(PrefabManager.inst.station[0], Vector3.zero, Quaternion.Euler(0, 0, data.position[2]));
            ent = stGO.GetComponent<StationEntity>();
            ent.station = this;

            GlobalEvents.StationSpawned(ent);
        }
        if (oldStation == this)
        {
            GlobalEvents.StationDespawned(ent);

            UnityEngine.Object.Destroy(ent.gameObject, 0);
            ent = null;
        }
    }

    public void AssignStationEntity(StationEntity stEnt)
    {
        ent = stEnt;
    }

    public void SetName(string newName)
    {
        data.Name = newName;
    }

    public void SetOwner(int newOwnerID)
    {
        data.ownerID = newOwnerID;
    }

    public Pilot GetOwner()
    {
        return GameDataManager.pilots[data.ownerID];
    }
    public int GetTeam()
    {
        return GameDataManager.pilots[data.ownerID].data.teamID;
    }
}

[System.Serializable]
public class StationData
{
    public string Name = "Station #";
    public int ID;
    public int ownerID;
    public List<int> fleet;
    // x,y - pos / z - rotation
    // position on global map, rotation in game
    public float[] position = new float[3];

    public int fleetQueue;  //ships on processing

    public int teamID { get => GameDataManager.data.pilotsInfo[ownerID].teamID; }
    public Vector2 positionV2 { get => new Vector2(position[0], position[1]); }
}