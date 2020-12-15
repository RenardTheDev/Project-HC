using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Pilot
{
    public static Pilot PLAYER;

    public PilotData data;

    public Station baseStation;
    public Ship currentShip;
    public Transform currShipTrans;

    public Pilot(bool important, bool isPlayer, string name, int teamID, int baseStationID)
    {
        data = new PilotData()
        {
            Name = name,
            important = important,
            isPlayer = isPlayer,
            teamID = teamID,
            baseStationID = baseStationID,
            skinID = UnityEngine.Random.Range(0, GameManager.inst.team[teamID].teamSkins.Length)
        };
        Instantiation();
    }

    public Pilot(PilotData pilotData)
    {
        data = pilotData;
        Instantiation();
    }

    void Instantiation()
    {
        //AssignBaseStation(data.baseStationID);

        if (data.saved_pos == null)
        {
            Vector2 spawn = Vector2.zero;
            for (int s = 0; s < 25; s++)
            {
                if (Physics2D.OverlapCircleAll(spawn, 1f).Length == 0)
                {
                    break;
                }
                else
                {
                    spawn = /*baseStation.positionV2 +*/ UnityEngine.Random.insideUnitCircle * 5f;
                }
            }
            data.SavePosition(spawn, UnityEngine.Random.value * 360f);
        }

        GlobalEvents.OnActiveStationChanged += OnActiveStationChanged;
    }

    public void UnlinkEvents()
    {
        GlobalEvents.OnActiveStationChanged -= OnActiveStationChanged;
    }

    private void OnActiveStationChanged(Station oldStation, Station newStation)
    {
        if (data.isPlayer)
        {
            data.currStationID = newStation.data.ID;
        }
        else
        {
            if (newStation.data.ID == data.currStationID)
            {
                ShipPool.current.SpawnShip(this);
            }
            else
            {
                if (oldStation.data.ID == data.currStationID)
                {
                    data.SavePosition(currShipTrans);
                    currentShip.Hide();
                }
            }
        }
    }

    public void AssignBaseStation(int stationID)
    {
        data.baseStationID = stationID;
        baseStation = GameDataManager.GetStation(data.baseStationID);

        if (!data.important) baseStation.AddFleetPilot(this);
    }

    public void AssignShip(Ship ship)
    {
        currentShip = ship;
        currShipTrans = ship.transform;
    }

    public void TransferToNewLocation(int stationID, Vector2 spawn, float angle)
    {
        if (data.currStationID == stationID) return;

        if (stationID == GameDataManager.data.currentActiveStation)
        {
            ShipPool.current.SpawnShip(spawn, angle, this);
        }
    }

    public float[] GetPositionAsFloats()
    {
        return new float[] { currShipTrans.position.x, currShipTrans.position.y, currShipTrans.eulerAngles.z };
    }
}

public enum PilotState
{
    roaming,
    docked
}

[Serializable]
public class PilotData
{
    public bool important;
    public bool isPlayer;

    public string Name;
    public int teamID;
    public int skinID;
    public int weapID;
    public int ID;

    public int baseStationID;   // spawn origin //
    public int currStationID;   // current location //

    public float[] saved_pos;

    public void SavePosition(Transform trans)
    {
        saved_pos = new float[3] { trans.position.x, trans.position.y, trans.eulerAngles.z };
    }
    public void SavePosition(Vector2 pos, float rotation) { saved_pos = new float[3] { pos.x, pos.y, rotation }; }
    public Vector2 GetPosition() { return new Vector2(saved_pos[0], saved_pos[1]); }
    public float GetRotation() { return saved_pos[2]; }
}