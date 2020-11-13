using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Station 
{
    public string Name = "Station #";
    public int ownerID;

    public int teamID { get => GameDataManager.data.pilots[ownerID].teamID; }

    public List<int> pilots;

    public Vector2 positionV2 { get => new Vector2(position[0], position[1]); }
    public float[] position = new float[3];    // x,y - pos / z - rotation

    public Station(string name, int ownerID)
    {
        Name = name;
        this.ownerID = ownerID;

        //pilots = new List<int>();
    }

    public void SetName(string newName)
    {
        Name = newName;
    }

    public void SetOwner(int newOwnerID)
    {
        ownerID = newOwnerID;
    }

    public void PilotEnterStation(Pilot pilot)
    {
        pilots.Add(GameDataManager.data.GetPilotID(pilot));
    }

    public void PilotLeaveStation(Pilot pilot)
    {
        pilots.Remove(GameDataManager.data.GetPilotID(pilot));
    }
}
