using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class Pilot
{
    public static Pilot PLAYER;

    public bool important;
    public bool isPlayer;

    public string Name = "";
    public int teamID;
    public int skinID;
    public int weapID;

    public int baseStationID;

    [NonSerialized]
    public Station baseStation;

    [NonSerialized]
    public Ship currentShip;
    [NonSerialized]
    public Transform currShipTrans;

    // save info //
    public float[] saved_pos;

    public Pilot(bool important, bool isPlayer, string name, int teamID, int baseStationID)
    {
        Name = name;

        this.important = important;
        this.isPlayer = isPlayer;
        this.teamID = teamID;

        AssignBaseStation(baseStationID);

        if (saved_pos==null)
        {
            Vector2 spawn = baseStation.positionV2;
            for (int s = 0; s < 25; s++)
            {
                if (Physics2D.OverlapCircleAll(spawn, 1f).Length == 0)
                {
                    break;
                }
                else
                {
                    spawn = baseStation.positionV2 + UnityEngine.Random.insideUnitCircle * 5f;
                }
            }
            SavePosition(spawn, UnityEngine.Random.value * 360f);
        }
    }

    public void SetName(string newName)
    {
        Name = newName;
    }

    public void MarkAsPlayer()
    {
        if (PLAYER != null) { PLAYER.isPlayer = false; }

        PLAYER = this;
        isPlayer = true;
        important = true;
    }

    public void AssignBaseStation(int stationID)
    {
        baseStationID = stationID;
        baseStation = GameDataManager.data.GetStation(baseStationID);
    }

    public void AssignShip(Ship ship)
    {
        currentShip = ship;
        currShipTrans = ship.transform;
    }

    public void SavePosition()
    {
        saved_pos = new float[3] { currShipTrans.position.x, currShipTrans.position.y, currShipTrans.eulerAngles.z };
    }

    public void SavePosition(Vector2 pos, float rotation)
    {
        saved_pos = new float[3] { pos.x, pos.y, rotation };
    }
}

public enum PilotState
{
    roaming,
    docked
}